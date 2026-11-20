using UnityEditor;
using Yvsim.InspectorGroups.Editor;

namespace Yvsim.RuntimeDebugger.Editor
{
    /// <summary>使用 Inspector Groups 绘制字段与条件分组。</summary>
    [CustomEditor(typeof(TimeScaleHotkey), true)]
    [CanEditMultipleObjects]
    public sealed class TimeScaleHotkeyEditor : InspectorGroupsEditor<TimeScaleHotkey>
    {
    }
}
