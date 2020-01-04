#!/bin/sh

# Generate the doc files
docfx docfx/docfx.json

# Remove cache files from docfx generated in the source code
rm -rf ../Assets/com.alelievr.NodeGraphProcessor/Editor/obj/
