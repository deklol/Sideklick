<p align="center">
  <img src="assets/icon.png" alt="Sideklick icon" width="96" />
</p>

<h1 align="center">Sideklick</h1>

<p align="center">Push to talk. Stay on the page.</p>

<p align="center">
  <img src="https://img.shields.io/badge/platform-Windows-0078D4" alt="Windows" />
  <img src="https://img.shields.io/badge/language-C%23-68217A" alt="C#" />
  <img src="https://img.shields.io/badge/portable-no_install-2667E0" alt="Portable" />
  <img src="https://img.shields.io/badge/tray_icon-none-25B5A5" alt="No tray icon" />
</p>

<p align="center">
  <a href="https://github.com/deklol/Sideklick/raw/refs/heads/main/downloads/Sideklick.zip"><strong>Download for Windows</strong></a>
  &nbsp;&bull;&nbsp;
  <a href="Sideklick.cs"><strong>Source code</strong></a>
</p>

## Why Sideklick?

Your mouse side button is a handy Discord push-to-talk bind. It's less handy when every press also sends your browser **Back** or **Forward**.

Sideklick disables the button's normal Windows behaviour so you can keep using it for Discord push to talk without accidentally leaving the page. If your Discord setup doesn't pick up the disabled button, the optional **F13 / F14** replacement gives you a key to bind instead.

## Set it once

1. Download and extract the ZIP, then open **Sideklick.exe**.
2. Press the side button you want to configure.
3. Choose **Disable button**, then **Save & run quietly**.
4. Keep your Discord PTT binding and check it works. Repeat for your other button if needed.

No installer, admin rights, or tray icon. Each button's choice is saved. **Start Sideklick when I sign in** is optional and starts it quietly.

Open Sideklick again to change settings or exit. It needs to stay running to block buttons. Settings live beside the app in `settings.txt`; keep the folder somewhere writable. Turn startup off before moving or removing it.

Side buttons and wheel click are supported; left and right click stay available. Disabling a button applies across Windows. F-key mode maps side buttons to **F13/F14** and wheel click to **F15**; rebind Discord PTT if you choose this mode.

## Build

Run `Build.cmd` on Windows with .NET Framework 4.x installed. It uses the Windows C# compiler and produces `Sideklick.exe`. Source, the Windows icon, and its PNG image are included.
