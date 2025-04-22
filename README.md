# ScreenshotHook
截图工具水印Hook

## 项目简介

ScreenshotHook是一个用于在截图时自动添加水印的工具。通过Hook截图API（如BitBlt），能够在截图过程中自动添加自定义水印

## 项目结构

- **ScreenshotHook.Presentation**: 主程序，基于.NET 8.0开发的WPF应用
- **ScreenshotHook.Framework**: 公共框架库，基于.NET Framework开发
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

## 使用方法

1. 运行ScreenshotHook.exe
2. 选择需要添加水印的目标进程
3. 点击"Hook"按钮进行注入
4. 目标进程的所有截图操作都会自动添加水印

## 开发环境配置

### DllExport配置

项目中使用了DllExport来实现从.NET程序集导出函数供其他程序调用，需要按以下步骤配置：

1. 双击运行解决方案根目录下的`DllExport.bat`文件
2. 在弹出的配置窗口中选择需要应用DllExport的程序集（如ScreenshotHook.Injector）
3. 点击"Apply"按钮应用设置
4. 成功应用后关闭配置工具，然后重新加载解决方案

> 注意：每次修改项目引用或更新项目结构后可能需要重新配置DllExport

## 开源引用

本项目使用了以下开源项目：
- [EasyHook](https://github.com/EasyHook/EasyHook): 用于Windows API钩子
- [DllExport](https://github.com/3F/DllExport): 用于.NET DLL导出（使用其中的DllExport.bat文件）

## 许可证

[MIT License](LICENSE.md)
