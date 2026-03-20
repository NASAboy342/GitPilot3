run this:
dotnet publish -c Release -r osx-x64 --self-contained true
dotnet publish -c Release -r osx-arm64 --self-contained true


Run this:
hdiutil create -volname "GitPilot3 Installer" \
  -srcfolder dmg-temp \
  -ov -format UDZO GitPilot3.dmg


run this after install
xattr -d com.apple.quarantine /Applications/GitPilot3.app
