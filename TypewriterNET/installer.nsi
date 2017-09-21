; Based on http://nsis.sourceforge.net/Sample_installation_script_for_an_application
; ----------------------------------------
; Start
!define APPNAME "Typewriter.NET"
!define DESCRIPTION "Plain text editor"
!define VERSION 0
; This is the size (in kB) of all the files copied into "Program Files"
!define INSTALLSIZE 17408
!include "MUI2.nsh"
; ----------------------------------------
; General
Name "Typewriter.NET"
OutFile "typewriter-net-installer.exe"
ShowInstDetails "hide"
ShowUninstDetails "hide"
InstallDir "$PROGRAMFILES\${APPNAME}"
; ----------------------------------------
; Modern UI Configuration
!define MUI_ICON "TypewriterNET.ico"
!define MUI_WELCOMEFINISHPAGE_BITMAP "nsis.bmp"
!define MUI_UNWELCOMEFINISHPAGE_BITMAP "nsis_uninstall.bmp"
!define MUI_WELCOMEPAGE_TEXT "Setup will guide you through the installation of Typewriter.NET$\r$\n$\r$\n\
It is expected that you close all Typewriter.NET instances before starting Setup.$\r$\n$\r$\n\
Click Next to continue"
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_UNPAGE_WELCOME
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!define MUI_FINISHPAGE_RUN "$INSTDIR\TypewriterNET.exe"
!insertmacro MUI_PAGE_FINISH
!insertmacro MUI_LANGUAGE "English"
; ----------------------------------------
; Installer Sections
Section "install"
	SetShellVarContext all
	;# Files for the install directory - to build the installer, these should be in the same directory as the install script (this file)
	SetOutPath $INSTDIR
	# Files added here should be removed by the uninstaller (see section "uninstall")
	File "bin\TypewriterNET.exe"
	File "bin\MulticaretEditor.dll"
	File "bin\*.xml"
	File "TypewriterNET.ico"
	File /r "bin\ctags"
	File /r "bin\schemes"
	File /r "bin\syntax"
	File /r "bin\templates"
	File /r "bin\snippets"
	File /r "bin\omnisharp_server"
	# Uninstaller - See function un.onInit and section "uninstall" for configuration
	WriteUninstaller "$INSTDIR\uninstall.exe"
	# Start Menu
	CreateDirectory "$SMPROGRAMS\${APPNAME}"
	CreateShortCut "$SMPROGRAMS\${APPNAME}\${APPNAME}.lnk" "$INSTDIR\TypewriterNET.exe" "" ""
	CreateShortCut "$SMPROGRAMS\${APPNAME}\uninstall.lnk" "$INSTDIR\uninstall.exe" "" ""
	# Registry information for add/remove programs
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "DisplayName" "${APPNAME} - ${DESCRIPTION}"
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "UninstallString" "$\"$INSTDIR\uninstall.exe$\""
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "QuietUninstallString" "$\"$INSTDIR\uninstall.exe$\" /S"
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "InstallLocation" "$\"$INSTDIR$\""
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "DisplayIcon" "$\"$INSTDIR\TypewriterNET.ico$\""
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "DisplayVersion" "${VERSION}"
	# There is no option for modifying or repairing the install
	WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "NoModify" 1
	WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "NoRepair" 1
	# Set the INSTALLSIZE constant (!defined at the top of this script) so Add/Remove Programs can accurately report the size
	WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "EstimatedSize" ${INSTALLSIZE}
SectionEnd
; ----------------------------------------
; Uninstaller Section
Section "uninstall"
	SetShellVarContext all
	# Remove Start Menu launcher
	Delete "$SMPROGRAMS\${APPNAME}\*.*"
	RMDir "$SMPROGRAMS\${APPNAME}"
	# Remove files
	Delete $INSTDIR\TypewriterNET.ico
	Delete $INSTDIR\TypewriterNET.exe
	Delete $INSTDIR\MulticaretEditor.dll
	Delete $INSTDIR\*.xml
	RMDir /r /rebootok $INSTDIR\ctags
	RMDir /r /rebootok $INSTDIR\schemes
	RMDir /r /rebootok $INSTDIR\syntax
	RMDir /r /rebootok $INSTDIR\templates
	RMDir /r /rebootok $INSTDIR\snippets
	RMDir /r /rebootok $INSTDIR\omnisharp_server
	# Always delete uninstaller as the last action
	Delete $INSTDIR\uninstall.exe
	# Try to remove the install directory - this will only happen if it is empty
	RMDir $INSTDIR
	# Remove uninstaller information from the registry
	DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}"
SectionEnd
