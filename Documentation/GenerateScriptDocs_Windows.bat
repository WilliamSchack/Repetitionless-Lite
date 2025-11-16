@echo off

start /b /wait cmd /c "docfx metadata "docfx/docfx.json""
start /b /wait cmd /c "py docfx/ymlToMd.py"
start /b /wait cmd /c "move /y .\docfx\md\*.md .\docs\Scripting\"

pause