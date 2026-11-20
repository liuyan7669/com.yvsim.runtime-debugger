using UnityEngine;

namespace Yvsim.RuntimeDebugger.Samples
{
    /// <summary>将示例面板标记为运行时检查边界，无需依赖 UI Framework。</summary>
    public sealed class RuntimeDebuggerSamplePanel : MonoBehaviour, IUIRuntimeInspectorRoot
    {
    }
}
