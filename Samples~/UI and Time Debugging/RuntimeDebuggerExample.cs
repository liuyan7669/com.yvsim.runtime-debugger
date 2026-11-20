using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Yvsim.RuntimeDebugger.Samples
{
    /// <summary>运行时构建测试 UI，演示检查器快捷触发和可恢复的时间变速。</summary>
    public sealed class RuntimeDebuggerExample : MonoBehaviour
    {
        [SerializeField, Tooltip("示例 UI 使用的字体；未指定时使用 Unity 内置 Arial 字体，界面文字采用英文以免缺少中文字形。")]
        private Font sampleFont;

        [SerializeField, Tooltip("按示例快捷键时切换到的 Time.timeScale；0 为暂停，1 为正常速度。仅在场景中没有其他时间变速组件时生效。"), Range(0f, 5f)]
        private float targetTimeScale = 0.25f;

        [SerializeField, Tooltip("在原时间缩放和目标时间缩放之间切换的快捷键。示例停用后恢复启用前的时间缩放。")]
        private KeyCode timeScaleKey = KeyCode.U;

        private GameObject ownedRoot;
        private GameObject ownedHotkeyObject;
        private Text statusText;
        private Toggle counterToggle;
        private float previousTimeScale;
        private bool ownsTimeScale;
        private float scaledSeconds;
        private float realSeconds;
        private int clickCount;
        private string timeScaleStatus;

        private void OnEnable()
        {
            if (!Application.isPlaying) return;
            scaledSeconds = 0f;
            realSeconds = 0f;
            clickCount = 0;
            ownedRoot = new GameObject("UI and Time Debugging Sample (Generated)");
            ownedRoot.transform.SetParent(transform, false);

            if (EventSystem.current == null)
            {
                var eventSystemObject = new GameObject("Sample EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                eventSystemObject.transform.SetParent(ownedRoot.transform, false);
            }

            var canvasObject = new GameObject("Sample Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(ownedRoot.transform, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<UIRuntimeInspector>();

            var panelObject = new GameObject("Sample Panel", typeof(RectTransform), typeof(Image), typeof(RuntimeDebuggerSamplePanel));
            panelObject.transform.SetParent(canvasObject.transform, false);
            RectTransform panel = panelObject.GetComponent<RectTransform>();
            panel.anchorMin = panel.anchorMax = new Vector2(1f, 0.5f);
            panel.pivot = new Vector2(1f, 0.5f);
            panel.anchoredPosition = new Vector2(-20f, 0f);
            panel.sizeDelta = new Vector2(420f, 500f);
            Image background = panelObject.GetComponent<Image>();
            background.color = new Color(0.08f, 0.12f, 0.18f, 0.96f);
            background.raycastTarget = false;

            Font font = sampleFont != null ? sampleFont : Resources.GetBuiltinResource<Font>("Arial.ttf");
            AddText(panel, font, "Runtime Debugger Sample", 20f, 36f, 22);
            AddText(panel, font, "Press F2, then expand the inspector.\nClick or use the listed shortcut keys.", 62f, 60f, 17);
            AddButton(panel, font, "Count Click", "Count a click", 140f).onClick.AddListener(() => clickCount++);
            AddButton(panel, font, "Reset Counters", "Reset counters", 195f).onClick.AddListener(ResetCounters);
            counterToggle = AddToggle(panel, font, 250f);
            statusText = AddText(panel, font, string.Empty, 310f, 160f, 17);

            // 尊重现有控制器；本示例只拥有并恢复自己创建的时间变速实例。
            if (FindObjectsOfType<TimeScaleHotkey>(true).Length == 0)
            {
                previousTimeScale = Time.timeScale;
                ownsTimeScale = true;
                ownedHotkeyObject = new GameObject("Sample TimeScaleHotkey");
                ownedHotkeyObject.SetActive(false);
                TimeScaleHotkey hotkey = ownedHotkeyObject.AddComponent<TimeScaleHotkey>();
                hotkey.originTimeScale = previousTimeScale;
                hotkey.changeTimeScale = targetTimeScale;
                hotkey.triggerKey = timeScaleKey;
                hotkey.triggerType = TimeScaleHotkeyMode.Press;
                hotkey.buildEnable = true;
                // TimeScaleHotkey 会对自己的根对象执行 DontDestroyOnLoad；保持独立并在退出时回收。
                ownedHotkeyObject.SetActive(true);
                timeScaleStatus = "Press " + timeScaleKey + " to toggle time scale.";
            }
            else
            {
                timeScaleStatus = "Existing TimeScaleHotkey found; its settings are preserved.";
            }
        }

        private static void SetTopRect(RectTransform rect, float top, float height)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -top);
            rect.sizeDelta = new Vector2(380f, height);
        }

        private static Text AddText(Transform parent, Font font, string value, float top, float height, int size)
        {
            var owner = new GameObject("Label", typeof(RectTransform), typeof(Text));
            owner.transform.SetParent(parent, false);
            Text text = owner.GetComponent<Text>();
            SetTopRect(text.rectTransform, top, height);
            text.font = font;
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
            text.text = value;
            return text;
        }

        private static Button AddButton(Transform parent, Font font, string objectName, string caption, float top)
        {
            var owner = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
            owner.transform.SetParent(parent, false);
            SetTopRect(owner.GetComponent<RectTransform>(), top, 42f);
            Image image = owner.GetComponent<Image>();
            image.color = new Color(0.18f, 0.42f, 0.66f);
            Button button = owner.GetComponent<Button>();
            button.targetGraphic = image;
            AddText(owner.transform, font, caption, 0f, 42f, 18);
            return button;
        }

        private static Toggle AddToggle(Transform parent, Font font, float top)
        {
            var owner = new GameObject("Advance Counters", typeof(RectTransform), typeof(Image), typeof(Toggle));
            owner.transform.SetParent(parent, false);
            SetTopRect(owner.GetComponent<RectTransform>(), top, 42f);
            Image image = owner.GetComponent<Image>();
            image.color = new Color(0.18f, 0.25f, 0.34f);
            Toggle toggle = owner.GetComponent<Toggle>();
            toggle.targetGraphic = image;
            var checkObject = new GameObject("Check", typeof(RectTransform), typeof(Image));
            checkObject.transform.SetParent(owner.transform, false);
            RectTransform checkRect = checkObject.GetComponent<RectTransform>();
            checkRect.anchorMin = checkRect.anchorMax = new Vector2(0f, 0.5f);
            checkRect.anchoredPosition = new Vector2(25f, 0f);
            checkRect.sizeDelta = new Vector2(20f, 20f);
            Image check = checkObject.GetComponent<Image>();
            check.color = Color.green;
            check.raycastTarget = false;
            toggle.graphic = check;
            toggle.isOn = true;
            AddText(owner.transform, font, "Advance counters", 0f, 42f, 18);
            return toggle;
        }

        private void ResetCounters()
        {
            clickCount = 0;
            scaledSeconds = 0f;
            realSeconds = 0f;
        }

        private void Update()
        {
            if (statusText == null) return;
            if (counterToggle.isOn)
            {
                scaledSeconds += Time.deltaTime;
                realSeconds += Time.unscaledDeltaTime;
            }
            statusText.text = "Clicks: " + clickCount + "\nScaled seconds: " + scaledSeconds.ToString("F1")
                + "\nUnscaled seconds: " + realSeconds.ToString("F1")
                + "\nTime scale: " + Time.timeScale.ToString("F2") + "\n" + timeScaleStatus;
        }

        private void OnDisable()
        {
            if (ownedHotkeyObject != null)
            {
                ownedHotkeyObject.SetActive(false);
                Destroy(ownedHotkeyObject);
                ownedHotkeyObject = null;
            }
            if (ownsTimeScale)
            {
                Time.timeScale = previousTimeScale;
                ownsTimeScale = false;
            }
            if (ownedRoot != null)
            {
                ownedRoot.SetActive(false);
                Destroy(ownedRoot);
                ownedRoot = null;
            }
            statusText = null;
            counterToggle = null;
        }
    }
}
