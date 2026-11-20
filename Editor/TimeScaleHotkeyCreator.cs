using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Yvsim.RuntimeDebugger.Editor
{
    /// <summary>
    /// 提供在 Hierarchy 中定位或创建时间变速的菜单。
    /// </summary>
    public static class TimeScaleHotkeyCreator
    {
        private const string MenuPath = "GameObject/Yvsim/调试/创建时间变速控制器";
        private const string ObjectName = "[时间变速]";
        private const int MenuPriority = 120;

        /// <summary>
        /// 定位目标场景中的现有控制器；不存在时在场景根节点创建一个。
        /// </summary>
        /// <param name="menuCommand">Unity 菜单命令上下文。</param>
        [MenuItem(MenuPath, false, -10000)]
        private static void LocateOrCreate(MenuCommand menuCommand)
        {
            Scene targetScene = GetTargetScene(menuCommand.context as GameObject);
            if (!targetScene.IsValid() || !targetScene.isLoaded)
            {
                return;
            }

            TimeScaleHotkey existingHotkey = FindHotkey(targetScene);
            if (existingHotkey != null)
            {
                SelectAndPing(existingHotkey.gameObject);
                return;
            }

            GameObject hotkeyObject = new GameObject(ObjectName, typeof(TimeScaleHotkey));
            if (hotkeyObject.scene != targetScene)
            {
                SceneManager.MoveGameObjectToScene(hotkeyObject, targetScene);
            }

            Undo.RegisterCreatedObjectUndo(hotkeyObject, "Create Time Scale Hotkey");
            SelectAndPing(hotkeyObject);
            EditorSceneManager.MarkSceneDirty(targetScene);
        }

        /// <summary>获取右键上下文物体所在场景，未提供上下文时使用当前活动场景。</summary>
        private static Scene GetTargetScene(GameObject contextObject)
        {
            return contextObject != null && contextObject.scene.IsValid()
                ? contextObject.scene
                : SceneManager.GetActiveScene();
        }

        /// <summary>查找指定场景中包含未激活物体在内的首个时间变速。</summary>
        private static TimeScaleHotkey FindHotkey(Scene scene)
        {
            GameObject[] rootObjects = scene.GetRootGameObjects();
            for (int i = 0; i < rootObjects.Length; i++)
            {
                TimeScaleHotkey hotkey = rootObjects[i].GetComponentInChildren<TimeScaleHotkey>(true);
                if (hotkey != null)
                {
                    return hotkey;
                }
            }

            return null;
        }

        /// <summary>在 Hierarchy 中选中并高亮指定物体。</summary>
        private static void SelectAndPing(GameObject targetObject)
        {
            Selection.activeGameObject = targetObject;
            EditorGUIUtility.PingObject(targetObject);
        }
    }
}
