#!/bin/bash

docfx metadata "docfx/docfx.json"
python "docfx/ymlToMd.py"
mv -f docfx/md/* docs/Scripting/
