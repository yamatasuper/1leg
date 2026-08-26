using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NinetyMinutes.UI
{
    public static class UiFactory
    {
        static Font _font;

        public static Font DefaultFont
        {
            get
            {
                if (_font != null) return _font;

                // OS fonts with Cyrillic first (Unity 2022 builtin often lacks RU glyphs).
                _font = Font.CreateDynamicFontFromOSFont(
                    new[]
                    {
                        "Segoe UI",
                        "Arial",
                        "Tahoma",
                        "Microsoft YaHei",
                        "Roboto",
                        "Helvetica"
                    },
                    32);

                if (_font == null)
                    _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (_font == null)
                    _font = Resources.GetBuiltinResource<Font>("Arial.ttf");

                if (_font == null)
                    Debug.LogError("[90 минут] Не найден шрифт UI — текст будет пустым.");
                else
                    Debug.Log($"[90 минут] UI font: {_font.name}");

                return _font;
            }
        }

        public static Canvas CreateCanvas(string name, int sortOrder = 100)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        public static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Object.DontDestroyOnLoad(es);
        }

        public static Image Panel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            Stretch(go.GetComponent<RectTransform>());
            return img;
        }

        public static RectTransform Box(Transform parent, string name, Vector2 anchoredPos, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            go.GetComponent<Image>().color = color;
            return rt;
        }

        public static Text Label(Transform parent, string name, string text, int fontSize, TextAnchor anchor, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(12, 8);
            rt.offsetMax = new Vector2(-12, -8);
            return ConfigureText(go.GetComponent<Text>(), text, fontSize, anchor, color);
        }

        public static Text Title(Transform parent, string name, string text, int fontSize, Vector2 anchoredPos, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(860, 90);
            return ConfigureText(go.GetComponent<Text>(), text, fontSize, TextAnchor.UpperCenter, color);
        }

        public static Text Footer(Transform parent, string name, string text, int fontSize, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0, 24);
            rt.sizeDelta = new Vector2(820, 48);
            return ConfigureText(go.GetComponent<Text>(), text, fontSize, TextAnchor.MiddleCenter, color);
        }

        static Text ConfigureText(Text t, string text, int fontSize, TextAnchor anchor, Color color)
        {
            t.font = DefaultFont;
            t.text = text;
            t.fontSize = fontSize;
            t.alignment = anchor;
            t.color = color;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            t.supportRichText = false;
            return t;
        }

        public static Button Button(Transform parent, string name, string label, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction onClick)
        {
            // High contrast vs dark TV bezel so buttons are visible even before text loads.
            var rt = Box(parent, name, pos, size, new Color(0.22f, 0.28f, 0.22f, 1f));
            var outline = rt.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.9f, 0.95f, 0.4f, 1f);
            outline.effectDistance = new Vector2(3, -3);

            Label(rt, "Label", label, 30, TextAnchor.MiddleCenter, Color.white);
            var btn = rt.gameObject.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.15f, 1.15f, 1f, 1f);
            colors.pressedColor = new Color(0.75f, 0.75f, 0.7f, 1f);
            colors.disabledColor = new Color(0.35f, 0.35f, 0.35f, 0.85f);
            btn.colors = colors;
            btn.targetGraphic = rt.GetComponent<Image>();
            if (onClick != null) btn.onClick.AddListener(onClick);
            return btn;
        }

        public static Image FullImage(Transform parent, string name, Sprite sprite, Color color)
        {
            var img = Panel(parent, name, color);
            if (sprite != null)
            {
                img.sprite = sprite;
                img.color = Color.white;
                img.preserveAspect = false;
                img.type = Image.Type.Simple;
            }

            return img;
        }

        public static RectTransform LeftColumn(Transform parent, string name, float width, float padLeft)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchoredPosition = new Vector2(padLeft, 0f);
            rt.sizeDelta = new Vector2(width, 0f);
            return rt;
        }

        public static RectTransform PaperCard(Transform parent, string name, Vector2 pos, Vector2 size)
        {
            var rt = Box(parent, name, pos, size, new Color(0.10f, 0.09f, 0.07f, 0.94f));
            var outline = rt.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.78f, 0.58f, 0.22f, 0.55f);
            outline.effectDistance = new Vector2(1, -1);
            Hairline(rt, "Rule", new Vector2(0f, size.y * 0.5f - 8f), new Vector2(size.x - 48f, 3f),
                new Color(0.78f, 0.58f, 0.22f, 0.9f));
            return rt;
        }

        public static Image Hairline(Transform parent, string name, Vector2 pos, Vector2 size, Color color)
        {
            var rt = Box(parent, name, pos, size, color);
            return rt.GetComponent<Image>();
        }

        public static Text Headline(Transform parent, string name, string text, int fontSize, Vector2 pos, Vector2 size, Color color, TextAnchor anchor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return ConfigureText(go.GetComponent<Text>(), text, fontSize, anchor, color);
        }

        public static Button GhostButton(Transform parent, string name, string label, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction onClick)
        {
            var rt = Box(parent, name, pos, size, new Color(0.08f, 0.07f, 0.05f, 0.28f));
            var img = rt.GetComponent<Image>();
            img.raycastTarget = true;

            var bar = Box(rt, "Accent", new Vector2(-size.x * 0.5f + 4f, 0f), new Vector2(6f, size.y - 10f),
                new Color(0.78f, 0.58f, 0.22f, 1f));
            var accent = bar.GetComponent<Image>();
            accent.enabled = false;
            accent.raycastTarget = false;

            var text = Label(rt, "Label", label, 28, TextAnchor.MiddleLeft, new Color(0.96f, 0.93f, 0.86f));
            text.rectTransform.offsetMin = new Vector2(28, 4);
            text.rectTransform.offsetMax = new Vector2(-16, -4);

            var btn = rt.gameObject.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = new Color(1f, 1f, 1f, 0.15f);
            colors.highlightedColor = new Color(1f, 0.92f, 0.7f, 0.28f);
            colors.pressedColor = new Color(0.78f, 0.58f, 0.22f, 0.35f);
            colors.disabledColor = new Color(0.4f, 0.4f, 0.38f, 0.2f);
            colors.fadeDuration = 0.08f;
            btn.colors = colors;
            btn.targetGraphic = img;
            if (onClick != null) btn.onClick.AddListener(onClick);

            var style = rt.gameObject.AddComponent<MenuButtonStyle>();
            style.Label = text;
            style.Accent = accent;
            return btn;
        }

        public static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
