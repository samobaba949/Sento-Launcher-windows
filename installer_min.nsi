Unicode true
SetCompressor /SOLID lzma

!include "FileFunc.nsh"

!define APPNAME "Sento Launcher"
!define APPEXE "SentoLauncher.exe"
!define APPVERSION "1.0.0"
!define UNINSTKEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\SentoLauncher"

Name "${APPNAME}"
OutFile "G:\Sento Launcher\dist\SentoLauncher-Setup.exe"
Icon "G:\Sento Launcher\Assets\icon.ico"
InstallDir "$LOCALAPPDATA\Programs\Sento Launcher"
InstallDirRegKey HKCU "Software\SentoLauncher" "InstallDir"
RequestExecutionLevel user
ShowInstDetails show

Section
  SetOutPath "$INSTDIR"
  File /r "G:\Sento Launcher\publish\*.*"

  WriteUninstaller "$INSTDIR\Uninstall.exe"
  WriteRegStr HKCU "Software\SentoLauncher" "InstallDir" "$INSTDIR"
  WriteRegStr HKCU "${UNINSTKEY}" "DisplayName" "${APPNAME}"
  WriteRegStr HKCU "${UNINSTKEY}" "DisplayVersion" "${APPVERSION}"
  WriteRegStr HKCU "${UNINSTKEY}" "Publisher" "Sento"
  WriteRegStr HKCU "${UNINSTKEY}" "DisplayIcon" "$INSTDIR\${APPEXE}"
  WriteRegStr HKCU "${UNINSTKEY}" "UninstallString" '"$INSTDIR\Uninstall.exe"'
  WriteRegStr HKCU "${UNINSTKEY}" "InstallLocation" "$INSTDIR"
  WriteRegDWORD HKCU "${UNINSTKEY}" "NoModify" 1
  WriteRegDWORD HKCU "${UNINSTKEY}" "NoRepair" 1
  ${GetSize} "$INSTDIR" "/S=0K" $0 $1 $2
  IntFmt $0 "0x%08X" $0
  WriteRegDWORD HKCU "${UNINSTKEY}" "EstimatedSize" "$0"

  CreateDirectory "$SMPROGRAMS\Sento Launcher"
  CreateShortcut "$SMPROGRAMS\Sento Launcher\Sento Launcher.lnk" "$INSTDIR\${APPEXE}"
  CreateShortcut "$SMPROGRAMS\Sento Launcher\Uninstall.lnk" "$INSTDIR\Uninstall.exe"
  CreateShortcut "$DESKTOP\Sento Launcher.lnk" "$INSTDIR\${APPEXE}"
SectionEnd

Section "Uninstall"
  RMDir /r "$INSTDIR"
  Delete "$SMPROGRAMS\Sento Launcher\Sento Launcher.lnk"
  Delete "$SMPROGRAMS\Sento Launcher\Uninstall.lnk"
  RMDir "$SMPROGRAMS\Sento Launcher"
  Delete "$DESKTOP\Sento Launcher.lnk"
  DeleteRegKey HKCU "${UNINSTKEY}"
  DeleteRegKey HKCU "Software\SentoLauncher"
SectionEnd
