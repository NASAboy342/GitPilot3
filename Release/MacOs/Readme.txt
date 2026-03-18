Run this:
hdiutil create -volname "GitPilot3 Installer" \
  -srcfolder dmg-temp \
  -ov -format UDZO GitPilot3.dmg


run this after install
xattr -d com.apple.quarantine /Applications/GitPilot3.app
