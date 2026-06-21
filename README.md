# Görsel Yöneticisi — Image Manager

**Görsel Yöneticisi** is a desktop image management application for Windows developed by Pixel Neon Entertainment. It scans your entire computer or selected drives and lets you browse, filter, select, delete, and copy images — all from a single clean interface.

---

## ✨ Features

- 🔍 **Full Computer Scan** — Scans all files on your system to find images automatically
- 💾 **Drive Selection** — Choose specific drives (e.g. C:\, D:\) to scan instead of the whole system
- 🖼️ **Grid View** — Browse all found images in a paginated thumbnail grid
- ☑️ **Multi-Select** — Select all, deselect, or pick individual images
- 🔁 **Duplicate Detection** — Find and review duplicate images
- 🗑️ **Delete Selected** — Permanently delete selected images
- 📋 **Copy Selected** — Copy selected images to another location
- 🎨 **Theme Support** — Multiple UI themes including "Şeker Pembe" (Sugar Pink) and more
- 🔎 **Filters** — Filter images by type, size, and date

---

## 🖥️ System Requirements

- Windows 10 or later
- .NET 8.0 Runtime (or later)

---

## 🚀 Installation

1. Go to the [Releases](https://github.com/pixelneonentertainment/Image-Manager/releases) page
2. Download the latest `GorselYoneticisi_Kurulum_vX.X.X.exe`
3. Run the installer and follow the setup steps
4. Launch **Görsel Yöneticisi** from the Start Menu or desktop shortcut

---

## 🛠️ Build from Source

### Requirements

- Visual Studio 2022+
- .NET 8.0 SDK
- Windows

### Steps

```bash
git clone https://github.com/pixelneonentertainment/Image-Manager.git
cd Image-Manager
```

Open `GorselYoneticisi.sln` in Visual Studio and press **F5** to build and run.

---

## 📁 Project Structure

| File | Description |
|------|-------------|
| `App.xaml` / `App.xaml.cs` | Application entry point and global resources |
| `MainWindow.xaml` / `MainWindow.xaml.cs` | Main image browser window |
| `DiskSelectionWindow.xaml` / `DiskSelectionWindow.xaml.cs` | Drive selection dialog |
| `GorselYoneticisi.csproj` | Project configuration file |
| `app_logo.ico` | Application icon |
| `Properties/` | Assembly info and application settings |

---

## 📄 License

This project is licensed under the **MIT License**.  
© 2026 Pixel Neon Entertainment — Görkem Akkaya. All rights reserved.
