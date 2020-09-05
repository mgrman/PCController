silentcmd, run a command-line without showing the cmd window
http://miffthefox.googlepages.com/

silentcmd will run the cmd program indicated in %COMSPEC%, but not show any window.  This is good for automation batch and powershell scripts that will run in the background.

Usage:
	silentcmd COMMAND

Requires:
	Windows XP/Vista, Microsoft .NET Framework 2.0 or later

License:
	GNU General Public License version 3
	See the enclosed COPYING.txt for the text of the license
	This readme file is public domain

Notes:
	If building from source, make sure csc.exe is in your %PATH%.  See buildit.bat for details.
	You can place findexe in your %PATH% (such as C:\WINDOWS\) to make it easier to run.
	%COMSPEC% must accept commands in the form of "exepath /c COMMAND", such as cmd.exe does.

Icon (terminal.ico) by Umut Pulat as part of the Tulliana 2 icon set.  Used under the terms of the GNU Lesser General Public License.
http://www.iconarchive.com/category/system/tulliana-2-icons-by-umut-pulat.html