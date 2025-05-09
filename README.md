# ScreenshotHook
截图工具水印Hook

## 项目简介

ScreenshotHook是一个用于在截图时自动添加水印的工具。通过Hook截图API（如BitBlt），能够在截图过程中自动添加自定义水印

## ⚠️ 重要编译说明

**注意**：本项目需要同时编译x86和x64两个版本才能正常工作！

默认的"Any CPU"配置**无法正常工作**！推荐使用以下方法之一进行编译：

1.  **使用Git Bash脚本 (推荐)**:
    *   确保你已安装 [Git for Windows](https://git-scm.com/download/win)，它包含了 Git Bash。
    *   在项目根目录下，运行 `build_all.sh` 脚本。
    *   **编译Debug版本 (默认)**:
        ```bash
        bash build_all.sh
        ```
    *   **编译Release版本**:
        ```bash
        bash build_all.sh release
        ```
    *   此脚本会自动尝试编译x86和x64两个平台。

2.  **手动在Visual Studio中编译**:
    *   打开 `ScreenshotHook.sln`。
    *   选择所需的配置 (Debug 或 Release)。
    *   将解决方案平台配置切换为 "x86" 并生成解决方案。
    *   然后将解决方案平台配置切换为 "x64" 并再次生成解决方案。

## 项目结构

- **ScreenshotHook.Presentation**: 主程序，基于.NET 8.0开发的WPF应用
- **ScreenshotHook.HookLibrary**: Hook实现库，基于.NET Framework开发
- **ScreenshotHook.Injector**: 注入器，基于.NET Framework开发

## 技术说明

### 为什么使用混合框架？

本项目使用了混合框架开发：
- 主程序（ScreenshotHook.Presentation）使用.NET 8.0
- 其他库使用.NET Framework

因为写到一半发现.NET8.0无法正常使用EasyHook：
- EasyHook需要使用System.Runtime.Remoting
- 从.NET 5开始已移除System.Runtime.Remoting，因此无法在.NET 8.0中直接使用EasyHook
- 懒得改GUI框架了，所以采用DllImport方式

### 跨框架通信

主程序通过P/Invoke（DllImport）方式调用注入器中的静态Hook方法。

### 跨位数进程注入

项目支持32位和64位进程注入：
- 根据目标进程的位数（32位或64位）自动选择对应版本的DLL进行注入
- 编译时会生成x86和x64两个版本的HookLibrary
- 支持从64位应用注入32位进程，反之亦然

### 钩子管理与进程隔离

- 每个被注入的进程都会加载独立的HookLibrary副本
- 每个进程拥有自己独立的应用程序域(AppDomain)
- `_hooks`在每个应用程序域中是独立的
- 不同进程的钩子互不影响，各自管理

在卸载钩子时，新注入的实例会在目标进程的同一应用程序域中访问`_hooks`，并卸载该进程中所有已注册的钩子。

## 使用方法

1. 运行ScreenshotHook.exe
2. 选择需要添加水印的目标进程
3. 点击"Hook"按钮进行注入
4. 目标进程的所有截图操作都会自动添加水印
5. 需要移除水印时，选择相同进程，点击"UnHook"按钮移除钩子

## 开发环境配置

### DllExport配置

项目中使用了DllExport来实现从.NET程序集导出函数供其他程序调用，需要按以下步骤配置：

1. 双击运行解决方案根目录下的`DllExport.bat`文件
2. 在弹出的配置窗口中选择需要应用DllExport的程序集（如ScreenshotHook.Injector）
3. 点击"Apply"按钮应用设置
4. 成功应用后关闭配置工具，然后重新加载解决方案

> 注意：每次修改项目引用或更新项目结构后可能需要重新配置DllExport

### 多平台编译配置 (通过Git Bash脚本)

项目根目录下的 `build_all.sh` 脚本用于一次性编译所有必需的平台 (x86 和 x64)，并支持Debug和Release配置。

1.  **确保已安装Git Bash**: Git Bash 通常随 [Git for Windows](https://git-scm.com/download/win) 一起安装。
2.  **将msbuild所在目录添加至PAHT环境变量中**
3.  **运行脚本**:
    打开 Git Bash，导航到项目根目录，然后执行相应的命令：
    *   **编译Debug版本 (默认)**:
        ```bash
        bash build_all.sh
        ```
    *   **编译Release版本**:
        ```bash
        bash build_all.sh release
        ```
4.  **检查输出**: 
    *   Debug版本的DLL文件将分别位于 `builds\Debug\x86` 和 `builds\Debug\x64` 目录下。
    *   Release版本的DLL文件将分别位于 `builds\Release\x86` 和 `builds\Release\x64` 目录下。

## 开源引用

本项目使用了以下开源项目：
- [EasyHook](https://github.com/EasyHook/EasyHook): 用于Windows API钩子
- [DllExport](https://github.com/3F/DllExport): 用于.NET DLL导出（使用其中的DllExport.bat文件）

## 许可证

[MIT License](LICENSE.md)
