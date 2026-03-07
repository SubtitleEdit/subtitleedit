#!/bin/bash

# fix-unsigned-app.sh
# Removes the macOS quarantine flag and applies an ad-hoc code signature
# so that the unsigned Subtitle Edit app can be launched normally.

APP="/Applications/Subtitle Edit.app"

echo "Fixing unsigned Subtitle Edit app..."
echo "You may be prompted for your macOS password."
echo ""

sudo xattr -rd com.apple.quarantine "$APP"
sudo codesign --force --deep --sign - "$APP"

echo ""
echo "Done. You can now launch Subtitle Edit from your Applications folder."
