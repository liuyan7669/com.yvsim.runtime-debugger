# UI and Time Debugging 示例

运行时创建独立的 uGUI 面板，包含 Button、Toggle、点击计数及缩放/非缩放计时。通过 `IUIRuntimeInspectorRoot` 标记示例面板，不需要 UI Framework。

## 导入与运行

1. 安装 Runtime Debugger 1.0.0 及其声明的 Inspector Groups、uGUI 依赖，在 Package Manager 的 Samples 中导入 **UI and Time Debugging**。
2. 新建空场景，在空物体上添加 `RuntimeDebuggerExample`，进入 Play 模式。
3. 点击 `Count a click` 和 `Reset counters`，检查计数变化；`Advance counters` 控制两个计时器是否继续累计。
4. 聚焦 Game 窗口，按 **F2** 打开运行时检查器，点击其展开按钮。检查器应列出示例的两个 Button 和一个 Toggle，可通过列表内按钮或显示的快捷键触发，并使用闪烁定位。
5. 默认按 **U** 在启用示例前的时间缩放与 0.25 倍速之间切换；观察 `Scaled seconds` 与 `Unscaled seconds` 的速度差异。
6. 停用示例组件，确认生成 UI 与演示控制器被清理，时间缩放恢复原值。

## 依赖和边界

- 示例不依赖其他 Yvsim 业务包，程序集只直接引用 Runtime Debugger 和 UnityEngine.UI。
- 使用 Unity 的旧输入 API；Active Input Handling 需为 Input Manager (Old) 或 Both。示例不会修改项目输入设置。运行时检查器本身使用 Unity IMGUI。
- 场景已有 EventSystem 时直接复用，否则创建并清理自己的 EventSystem。
- 场景已有任何 `TimeScaleHotkey`（包括未激活对象）时，不创建或修改时间变速控制器，也不恢复其他控制器的时间设置；界面会明确提示。
- 自建的 `TimeScaleHotkey` 由包按正常机制跨场景保留，示例停用或随场景卸载时会负责销毁它，并恢复原 `Time.timeScale`。不修改 `Time.fixedDeltaTime`。
- 未指定字体时使用 Unity 2021.3 内置 Arial；示例界面采用英文，说明与 Inspector 悬停文字采用中文。
