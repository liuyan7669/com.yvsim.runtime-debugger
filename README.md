# Runtime Debugger

用于 Unity 运行时调试和功能测试的快捷工具包，提供时间变速快捷键和可直接触发 UI 交互的运行时检查器。

## 功能

### 时间变速（TimeScaleHotkey）

- 通过快捷键修改 `Time.timeScale`
- 支持按住生效、松开恢复
- 支持每次按键在默认和目标速度之间切换
- 默认只在 Windows Editor 中监听
- 可选择在 Player 构建中启用
- 自动跨场景保留
- 自动销毁重复实例

### UIRuntimeInspector

- 使用快捷键打开运行时 IMGUI 检查面板
- 检查最前方 UI 或所有已打开 UI
- 查找 Button、Toggle 和自定义快捷目标
- 为交互组件自动分配键盘快捷键
- 从面板或快捷键直接触发交互
- 显示组件层级路径和交互状态
- 绘制交互区域边框和快捷键标识
- 闪烁定位指定 UI 组件
- 可过滤被前层 UI 遮挡的组件
- 可选择包含未激活组件
- 支持自动追踪界面打开与关闭

## 示例

在 Package Manager 的 Samples 中导入 **UI and Time Debugging**，新建空场景并将 `RuntimeDebuggerExample` 添加到空物体，进入 Play 模式即可操作。自动创建 Button、Toggle 和计时 UI，演示 F2 运行时检查、快捷触发和可恢复的时间变速。

详见 [示例说明](Samples~/UI%20and%20Time%20Debugging/README.md)。示例只创建并清理自己的运行时对象。

## 安装

当前源码版本为 `1.0.0`。先准备同版本的 **Inspector Groups** 和本包源码，使用本地 UPM 安装：

1. 在 `Window > Package Manager > + > Add package from disk...` 选择 Inspector Groups 根目录的 `package.json`。
2. 用同一方式选择本包根目录的 `package.json`。
3. 等待 Unity 解析清单中的 Unity 模块依赖并完成编译。

也可以将两个完整包目录分别放入项目的 `Packages/com.yvsim.inspector-groups` 和 `Packages/com.yvsim.runtime-debugger`，作为 Embedded Packages 使用。不要再为这两个嵌入包添加指向不存在远端版本的依赖项。

本说明不代表 Git Tag 或 OpenUPM 的 `1.0.0` 已发布；发布完成前请使用上述本地源码安装方式。

## 时间变速快捷键

### 基本配置

1. 在 Hierarchy 右键选择 `Yvsim > 调试 > 创建时间变速控制器`，创建或定位 `[时间变速]` 对象。
2. 也可通过 Add Component 的 `Yvsim > 调试 > 时间变速` 添加 `TimeScaleHotkey`。
3. 配置默认时间缩放、目标时间缩放、快捷键和触发模式。

默认配置：

```text
默认时间缩放：1
目标时间缩放：5
快捷键：U
触发模式：KeepPress
```

### 触发模式

| 模式 | 行为 |
| --- | --- |
| `KeepPress` | 按住快捷键使用目标时间缩放，松开后恢复 |
| `Press` | 每次按下快捷键时在默认值和目标值之间切换 |

组件默认只在 Windows Editor 中生效。需要在构建版本中使用时启用 `Build Enable`。

## 运行时 UI 检查器

### 标记 UI 根节点

让每个需要检查的 UI 根组件实现空接口 `IUIRuntimeInspectorRoot`，不需要修改已有基类：

```csharp
using Yvsim.RuntimeDebugger;
using UnityEngine;

public class ExampleUI : MonoBehaviour, IUIRuntimeInspectorRoot
{
}
```

### 添加检查器

1. 将 `UIRuntimeInspector` 添加到 Canvas 或包含 UI 根节点的父对象。
2. 进入 Play Mode。
3. 按 `F2` 打开检查面板。
4. 在面板中选择检查范围并触发 Button、Toggle 或自定义组件。

层级中位置越靠后的已打开 UI 根节点会被视为显示在更前方。

### 检查范围

- `仅最前界面`：只显示最前方已打开 UI 的交互组件。
- `所有打开界面`：显示全部已打开 UI 的交互组件。

### 面板选项

- 显示未激活组件
- 自动刷新界面列表
- 显示快捷键标识
- 显示交互区域边框
- 隐藏被前层遮挡的组件

面板还提供刷新、展开/折叠、关闭和组件闪烁定位操作。

### 接入自定义交互组件

非 Button、Toggle 组件可以实现 `IUIRuntimeInspectorShortcutTarget`：

```csharp
using Yvsim.RuntimeDebugger;
using UnityEngine;

public class CustomShortcutTarget : MonoBehaviour, IUIRuntimeInspectorShortcutTarget
{
    public bool CanRuntimeInspectorTrigger()
    {
        return isActiveAndEnabled;
    }

    public string GetRuntimeInspectorName()
    {
        return "执行自定义操作";
    }

    public void TriggerRuntimeInspector()
    {
        Debug.Log("自定义操作已触发");
    }
}
```

## 使用建议

- 工程中只保留一个 `TimeScaleHotkey` 实例。
- `UIRuntimeInspector` 面向开发与测试流程，不建议作为正式玩家 UI 使用。
- 自动刷新和遮挡检测会产生额外运行时开销，复杂界面中可按需关闭。
- 快捷键基于传统 `Input.GetKeyDown` 输入接口。
- 闪烁间隔使用不受 `Time.timeScale` 影响的时间。

## 主要类型

| 类型 | 说明 |
| --- | --- |
| `TimeScaleHotkeyMode` | 时间变速快捷键触发模式 |
| `TimeScaleHotkey` | 全局时间变速组件 |
| `IUIRuntimeInspectorRoot` | UI 检查边界标记接口 |
| `IUIRuntimeInspectorShortcutTarget` | 自定义运行时快捷目标接口 |
| `UIRuntimeInspector` | 运行时 UI 查找、显示和触发工具 |

## 环境要求

- Unity 2021.3 或更高版本
- Unity UI（`com.unity.ugui`）
- 使用传统 Input Manager 快捷键输入

## 许可证

本包使用 MIT License，详情参见仓库中的许可证文件。

## Inspector 统一依赖

本包依赖 `com.yvsim.inspector-groups` `1.0.0`，Inspector 的绘制与分组样式统一由该包提供。除该基础包外不依赖其他 Yvsim 包；Samples 中的可选组合需按示例说明准备依赖。

## 从 GitHub 安装

仓库：[com.yvsim.runtime-debugger](https://github.com/liuyan7669/com.yvsim.runtime-debugger)。仓库名与 UPM 包名一致。

先通过 Package Manager 的 Add package from git URL 安装以下 Yvsim 依赖，再安装本包：

- `https://github.com/liuyan7669/com.yvsim.inspector-groups.git#main`（1.0.0）

在 Unity Package Manager 选择 Add package from git URL，输入：

```text
https://github.com/liuyan7669/com.yvsim.runtime-debugger.git#main
```

此地址指向 main 分支，当前包版本为 1.0.0。本次上传不创建版本 tag 或 GitHub Release，也不代表已在 OpenUPM 登记；Unity 官方依赖仍按 package.json 解析。
