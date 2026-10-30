@echo off
setlocal
set "compiler=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%compiler%" set "compiler=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"
"%compiler%" /nologo /target:winexe /optimize+ /win32icon:"%~dp0Sideklick.ico" /out:"%~dp0Sideklick.exe" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll "%~dp0Sideklick.cs"
exit /b %errorlevel%
