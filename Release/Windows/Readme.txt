================================================================================
  GitPilot3 — Windows Release
  Publisher : Pin Sopheaktra
  GitHub    : https://github.com/NASAboy342/GitPilot3
================================================================================

REQUIREMENTS
------------
- Windows 10 / 11 (64-bit)
- Git must be installed: https://git-scm.com/download/win
- No .NET installation required (runtime is bundled)


INSTALLATION
------------
1. Run GitPilot3Setup.exe
2. Follow the wizard (Next → Next → Install)
3. Optionally tick "Create a desktop shortcut"
4. Click Finish — the app launches automatically


UNINSTALLATION
--------------
Go to Windows Settings → Apps → search "GitPilot3" → Uninstall
Or use: Start Menu → GitPilot3 → Uninstall GitPilot3


--------------------------------------------------------------------------------
  FOR DEVELOPERS — How to rebuild the installer
--------------------------------------------------------------------------------

STEP 1 — Publish the app
  Run in the project root (D:\Clones\GitPilot3):

    dotnet publish -c Release -r win-x64 --self-contained true

  Output will be placed in:
    GitPilot3\bin\Release\net9.0\win-x64\publish\

STEP 2 — Install Inno Setup (first time only)
  Download from: https://jrsoftware.org/isdl.php
  Install with default settings.

STEP 3 — Compile the installer script
  Open Inno Setup Compiler from the Start Menu, then:
    File → Open → select installer.iss (in the project root)
  Click Build → Compile  (or press Ctrl+F9)

STEP 4 — Get the output
  The installer will be generated at:
    Release\Windows\GitPilot3Setup.exe

OPTIONAL TWEAKS IN installer.iss
  AppVersion=1.0.0         → Update the version number before each release
  AppPublisher=...         → Your name or company
  AppPublisherURL=...      → Your GitHub or website URL
  Flags: unchecked         → Change to "checked" to enable desktop shortcut by default


RELEASING ON GITHUB
--------------------
1. Build and compile the installer (steps above).
2. Create a new GitHub Release at:
     https://github.com/NASAboy342/GitPilot3/releases/new
3. Upload GitPilot3Setup.exe as a release asset.
4. Write release notes and publish.