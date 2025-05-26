# FastLink

## 🚀 Introduction

With **FastLink**, you can assign global hotkeys, search, organize, and launch everything you need—
faster and more flexibly than any built-in bookmark or shortcut system.

**Tired of scattered browser bookmarks and clunky file system shortcuts?**  
Traditional browser favorites and file/folder shortcuts are often limited to a single browser or device, hard to organize, and slow to access—<br>
especially when you use multiple browsers, cloud drives, or work across different folders and apps. Managing dozens of bookmarks, desktop shortcuts, and favorites can quickly become a mess.

**FastLink** is an open-source, modern Windows launcher and hotkey manager that brings all your favorite files, folders, websites, and commands together in one place—always just a click or hotkey away.

- No more hunting through browser menus or desktop icons.
- No more duplicate bookmarks across browsers or complex folder structures.
- No more limits: launch anything, from anywhere, instantly.

---

## 🏆 Main Features

- **Add Hotkey:**  
  - (Default) Ctrl + Shift + F1
  - Instantly add the current Explorer or browser path as a new link.
  - When adding a web page, we recommend using the 'Microsoft Edge' browser.
  - The latest 'Google Chrome' browser has enhanced security, so some web pages do not recognize the path properly or are recognized properly only during page transitions.
    <p align="center">
      <img src="https://github.com/user-attachments/assets/03afcf4a-0f96-455e-9402-ab9504c4169d" alt="On Web Browser" width="500"/>
      <img src="https://github.com/user-attachments/assets/41223682-5980-454b-9c4b-6d94ba4d437d" alt="On File System" width="400"/>
    </p>

- **QuickView Hotkey:**  
  - (Default) Ctrl + Shift + Space
  - Show the full link list for quick selection.
    <p align="left">
      <img src="https://github.com/user-attachments/assets/5d128265-4600-4ab5-9c83-84192312c0a1" alt="On Web Browser" width="400"/>
    </p>

- **Per-link Hotkeys:**  
  - Assign your own hotkey to any link for instant launch.

- **System Tray Icon:**  
  - Running in the background. Quick add, auto-start toggle, and exit functions.
    <p align="left">
      <img src="https://github.com/user-attachments/assets/addc1017-9f08-4d97-8083-3e04a5d4a1df" alt="On Web Browser" width="400"/>
    </p>
---

## ⚙️ Other Features
- **Drag & Drop / Sorting:** Reorder your list easily.
- **Search/Filter:** Find links instantly with live search.
- **Windows Auto-Start:** Enable or disable launch on Windows startup with a single click.
- **Automatic Data & Settings Save:**  
  - All links and settings (hotkeys, auto-start, etc.) are saved to your local AppData folder.

---

## 💻 Installation & Usage

### 1. Using the Installer

- Download the latest setup.exe from [publish page](https://fastxlink.netlify.app/publish) and run it.
- Prerequisites : .NET 8 or higher, Windows 10 or higher

### 2. Build from Source

(1) git clone https://github.com/pjy0121/FastLink.git<br>
(2) cd FastLink<br>
(3) Open FastLink.sln in Visual Studio 2022 or later<br>
(4) Restore NuGet packages (MahApps.Metro, GongSolutions.Wpf.DragDrop, NHotkey.Wpf, etc.)<br>
(5) Build and run FastLink.exe

---

## 📁 Data & Settings Location

- Links(rows) list:
  `%LOCALAPPDATA%\FastLink\fastlink_rows.json`
- User settings:
  `%LOCALAPPDATA%\FastLink\settings.json`

---

## 🛠️ Contributing

- Built with .NET8 WPF, C#, MahApps.Metro.
- Pull requests and issues are welcome!
- See [CONTRIBUTING.md](CONTRIBUTING.md) for code style and folder structure.

---

## 📜 License

Apache 2.0 License<br>
Free for personal use(Not commercial), modification, and distribution.

---

## 🙏 Special Thanks

- [MahApps.Metro](https://github.com/MahApps/MahApps.Metro)
- [GongSolutions.Wpf.DragDrop](https://github.com/punker76/gong-wpf-dragdrop)
- [NHotkey.Wpf](https://github.com/thomaslevesque/NHotkey)

---

**Questions, suggestions, or bug reports? Please use the [Issues](https://github.com/pjy0121/FastLink/issues) tab!**
