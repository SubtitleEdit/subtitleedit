RUNNING SUBTITLE EDIT ON macOS (UNSIGNED APP)
===============================================

Because Subtitle Edit is not signed with an Apple developer certificate,
macOS will block it by default. You can still install and run it by
following these steps:

1. Double-click the .dmg file to mount it (if you have not already).

2. In the window that appears, drag "Subtitle Edit.app" into your
   Applications folder.

3. Open the Terminal app (you can find it via Spotlight or in
   /Applications/Utilities/).

4. In Terminal, run the following two commands to remove macOS's security
   quarantine flag and add an ad-hoc code signature:

      sudo xattr -rd com.apple.quarantine "/Applications/Subtitle Edit.app"

      sudo codesign --force --deep --sign - "/Applications/Subtitle Edit.app"

   You will be prompted for your macOS password. This is normal.

5. You can now launch Subtitle Edit from your Applications folder.

TIP: A script file "fix-unsigned-app.sh" is included in this DMG.
     You can also run it in Terminal to execute both commands at once:

      bash /Volumes/SubtitleEdit*/fix-unsigned-app.sh
