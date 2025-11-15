import yaml
from pathlib import Path

INPUT_FOLDER_PATH = "yml"
OUTPUT_FOLDER_PATH = "md"

EXCLUDE_FILES = [
    "Repetitionless.CustomWindows.ConfigureArrayWindowLimited.yml",
    "Repetitionless.Inspectors.RepetitionlessMasterGUI.yml",
    "Repetitionless.Inspectors.RepetitionlessTerrainGUI.yml"
]

def HandleClass(data):
    print (f"> Converting class {data["uid"]}")

    mdText = ""

    # Header
    mdText += f"# {data["uid"].removeprefix("Repetitionless.")}\n\n"

    # Description
    mdText += "## Description\n\n"

    if ("/Editor/" in data["source"]["path"]):
        mdText += "`Unity Editor Only`\n\n"

    mdText += data["summary"].replace("<br />", "\n") # Double new line
    mdText += "\n\n"

    return mdText

def HandleVariable(data):
    print (f"  > Found variable {data["name"]}")

    return f"| {data["name"]} | {data["summary"].replace("\n", "")} |\n"

def HandleFunction(data):
    print (f"> Converting function {data["name"]}")

    syntax = data["syntax"]

    mdText = ""

    # Header
    mdText += f"## {data["name"]}\n\n"

    # Declaration
    mdText += "### Declaration\n\n"
    mdText += f"``` {data["langs"][0]}\n"
    mdText += syntax["content"]
    mdText += "\n```\n\n"

    # Parameters
    if "parameters" in syntax:
        parameters = syntax["parameters"]
        
        mdText += "### Parameters\n\n"
        mdText += "| Parameter | Description |\n"
        mdText += "|-----------|-------------|\n"
        
        for param in parameters:
            mdText += f"| {param["id"]} | {param["description"].replace("\n", "")} |\n"

        mdText += "\n"

    # Returns
    if "return" in syntax:
        mdText += "### Returns\n\n"
        mdText += syntax["return"]["description"]
        mdText += "\n\n"

    # Description
    mdText += "### Description\n\n"
    mdText += data["summary"].replace("<br />", "\n") # Double new line
    mdText += "\n\n"

    mdText += "---\n\n"

    return mdText

def ConvertYmlToMd(ymlData):
    items = ymlData["items"]
    mdText = ""

    # If only one item, its the namespace category
    if (len(items) <= 1):
        return ""

    # First element is class
    mdText += HandleClass(items[0])

    # Sequential elements are variables until a method
    variablesText = "Variable" if items[0]["type"] == "Class" else "Member" # Members for enums

    mdText += f"## {variablesText}s\n\n"
    mdText += f"| {variablesText} | Description |\n"
    mdText += f"|{'-' * (len(variablesText) + 2)}|-------------|\n"

    methodStartIndex = 1;
    for item in items[1:]:
        if "type" not in item or item["type"] == "Method":
            break;

        mdText += HandleVariable(item)
        methodStartIndex += 1

    mdText += "\n---\n\n"

    # Handle methods
    for item in items[methodStartIndex:]:
        if "type" not in item:
            continue;

        mdText += HandleFunction(item)

    return mdText

# Paths
inputFolder = Path(INPUT_FOLDER_PATH)
outputFolder = Path(OUTPUT_FOLDER_PATH)
outputFolder.mkdir(exist_ok=True)

#for ymlFile in inputFolder.glob("Repetitionless*.yml"):
for ymlFile in inputFolder.glob("Repetitionless.*.yml"):
    if ymlFile.name in EXCLUDE_FILES:
        continue

    print(f"Generating file {ymlFile.name}...")

    # Get and convert yml data
    ymlData = yaml.safe_load(ymlFile.read_text())
    mdText = ConvertYmlToMd(ymlData)

    if (mdText == ""):
        continue

    # Write md file
    mdFileName = f"{ymlFile.stem}.md"
    mdFileOut = outputFolder / mdFileName

    mdFileOut.write_text(mdText);