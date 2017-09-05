# Based on http://nsis.sourceforge.net/A_simple_installer_with_start_menu_shortcut_and_uninstaller

!define APPNAME "Typewriter.NET"
!define DESCRIPTION "Plain text editor"
# These three must be integers
!define VERSION 0
# This is the size (in kB) of all the files copied into "Program Files"
!define INSTALLSIZE 5000
 
RequestExecutionLevel admin ;Require admin rights on NT6+ (When UAC is turned on)
 
InstallDir "$PROGRAMFILES\${APPNAME}"
 
# This will be in the installer/uninstaller's title bar
Name "${APPNAME}"
Icon "TypewriterNET.ico"
OutFile "typewriter-net-installer.exe"
 
!include LogicLib.nsh
 
# Just three pages - license agreement, install location, and installation
Page directory
Page instfiles
 
!macro VerifyUserIsAdmin
UserInfo::GetAccountType
Pop $0
${If} $0 != "admin" ;Require admin rights on NT4+
        MessageBox mb_iconstop "Administrator rights required!"
        SetErrorLevel 740 ;ERROR_ELEVATION_REQUIRED
        Quit
${EndIf}
!macroend
 
Function .onInit
	SetShellVarContext all
	!insertmacro VerifyUserIsAdmin
FunctionEnd
 
Section "install"
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
 
# Uninstaller
 
Function un.onInit
	SetShellVarContext all
 
	#Verify the uninstaller - last chance to back out
	MessageBox MB_OKCANCEL "Permanantly remove ${APPNAME}?" IDOK next
		Abort
	Next:
	!insertmacro VerifyUserIsAdmin
FunctionEnd
 
Section "uninstall"
 
	# Remove Start Menu launcher
	Delete "$SMPROGRAMS\${APPNAME}\${APPNAME}.lnk"
	# Try to remove the Start Menu folder - this will only happen if it is empty
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
	RMDir /r /rebootok $INSTDIR\omnisharp_server
 
	# Always delete uninstaller as the last action
	Delete $INSTDIR\uninstall.exe
 
	# Try to remove the install directory - this will only happen if it is empty
	RMDir $INSTDIR
 
	# Remove uninstaller information from the registry
	DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}"
SectionEnd
