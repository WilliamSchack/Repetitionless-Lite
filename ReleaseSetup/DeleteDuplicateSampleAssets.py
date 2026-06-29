import os
import shutil

__location__ = os.path.realpath(
    os.path.join(os.getcwd(), os.path.dirname(__file__)))

samplesFolder = os.path.realpath(os.path.join(__location__, "../True Seamless Texturing/Packages/com.williamschack.repetitionless/Samples~/"))

renderPipelines = ["BIRP", "URP", "HDRP"]
materialFolders = [
    ("Comparison", "Repetitionless"),
    ("Comparison", "Repetitionless 1"),
    ("Comparison", "Repetitionless 2"),
    ("Comparison", "Repetitionless 3"),
    ("Flat", "Repetitionless"),
    ("Forest", "Terrain"),
]

for renderPipeline in renderPipelines:
    for folders in materialFolders:
        sampleName = folders[0]
        materialName = folders[1]

        materialFolder = "_" + sampleName + "_" + materialName + "_RepetitionlessData"
        targetFolderPath = renderPipeline + "/" + sampleName + "/Materials/Repetitionless_" + renderPipeline + materialFolder
        targetFolderPath = os.path.join(samplesFolder, targetFolderPath)

        shutil.rmtree(targetFolderPath)