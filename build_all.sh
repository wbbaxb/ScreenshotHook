#!/bin/bash

# 默认配置为 Debug，如需编译Release版本,可以执行bash build_all.sh release
CONFIGURATION="Debug" 

# 检查是否有命令行参数，并据此设置配置
if [ ! -z "$1" ]; then
    if [[ "${1,,}" == "release" ]]; then # 转为小写比较
        CONFIGURATION="Release"
    fi
fi

echo "开始编译所有平台 (x86 和 x64) - 配置: $CONFIGURATION..."

# 检查解决方案文件是否存在
SOLUTION_FILE="ScreenshotHook.sln"
if [ ! -f "$SOLUTION_FILE" ]; then
    echo "错误：解决方案文件 $SOLUTION_FILE 未在当前目录中找到！"
    echo "请确保此脚本在解决方案的根目录下运行。"
    exit 1
fi

MSBUILD_CMD="msbuild.exe" 

echo ""
echo "开始编译 x86 平台 ($CONFIGURATION)..."
"$MSBUILD_CMD" "$SOLUTION_FILE" "//p:Configuration=$CONFIGURATION" //p:Platform=x86
if [ $? -ne 0 ]; then
    echo "错误：编译 x86 平台 ($CONFIGURATION) 失败！"
else
    echo "x86 平台 ($CONFIGURATION) 编译成功。"
fi

echo ""
echo "开始编译 x64 平台 ($CONFIGURATION)..."
"$MSBUILD_CMD" "$SOLUTION_FILE" "//p:Configuration=$CONFIGURATION" //p:Platform=x64
if [ $? -ne 0 ]; then
    echo "错误：编译 x64 平台 ($CONFIGURATION) 失败！"
else
    echo "x64 平台 ($CONFIGURATION) 编译成功。"
fi

echo ""
echo "所有平台 ($CONFIGURATION) 编译尝试完毕。"
echo "请检查 'builds/$CONFIGURATION/x86' 和 'builds/$CONFIGURATION/x64' 目录中的输出。" 