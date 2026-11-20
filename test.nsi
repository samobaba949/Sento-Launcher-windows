Unicode true
SetCompressor /SOLID lzma

Name "Sento Launcher Test"
OutFile "G:\Sento Launcher\dist\TestSetup.exe"
RequestExecutionLevel user
InstallDir "$LOCALAPPDATA\Programs\Sento Launcher Test"

Section
  SetOutPath "$INSTDIR"
  File /r "G:\Sento Launcher\publish"
SectionEnd