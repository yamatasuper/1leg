using NinetyMinutes.Art;
using NinetyMinutes.UI;
using UnityEngine;
using UnityEngine.UI;

namespace NinetyMinutes.Dialogue
{
    public sealed class DialogueUI : MonoBehaviour
    {
        public static DialogueUI Instance { get; private set; }

        GameObject _root;
        Text _speaker;
        Text _line;
        Image _portrait;
        Transform _choices;
        Button _continueBtn;
        bool _bound;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            Build();
            Hide();
        }

        void Start() => TryBind();

        void OnDestroy()
        {
            if (DialogueRunner.Instance == null || !_bound) return;
            DialogueRunner.Instance.Opened -= Show;
            DialogueRunner.Instance.Closed -= Hide;
            DialogueRunner.Instance.NodeChanged -= Refresh;
        }

        void TryBind()
        {
            if (_bound || DialogueRunner.Instance == null) return;
            DialogueRunner.Instance.Opened += Show;
            DialogueRunner.Instance.Closed += Hide;
            DialogueRunner.Instance.NodeChanged += Refresh;
            _bound = true;
        }

        void Build()
        {
            var canvas = UiFactory.CreateCanvas("DialogueCanvas", 250);
            DontDestroyOnLoad(canvas.gameObject);
            _root = UiFactory.Panel(canvas.transform, "DialogueRoot", new Color(0f, 0f, 0f, 0.45f)).gameObject;

            var portraitRt = UiFactory.Box(_root.transform, "Portrait", new Vector2(-620, -200), new Vector2(280, 280),
                new Color(0.05f, 0.06f, 0.07f, 0.95f));
            _portrait = portraitRt.GetComponent<Image>();
            _portrait.preserveAspect = true;
            var pOutline = portraitRt.gameObject.AddComponent<Outline>();
            pOutline.effectColor = new Color(0.7f, 0.75f, 0.55f, 0.7f);
            pOutline.effectDistance = new Vector2(2, -2);

            var box = UiFactory.Box(_root.transform, "Box", new Vector2(80, -280), new Vector2(1000, 360),
                new Color(0.08f, 0.1f, 0.12f, 0.96f));
            var outline = box.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.9f, 0.92f, 0.4f, 0.8f);
            outline.effectDistance = new Vector2(2, -2);

            _speaker = UiFactory.Title(box, "Speaker", "", 28, new Vector2(0, -16), new Color(0.95f, 0.92f, 0.35f));
            _speaker.alignment = TextAnchor.UpperLeft;
            _speaker.rectTransform.anchorMin = _speaker.rectTransform.anchorMax = new Vector2(0f, 1f);
            _speaker.rectTransform.pivot = new Vector2(0f, 1f);
            _speaker.rectTransform.anchoredPosition = new Vector2(28, -18);
            _speaker.rectTransform.sizeDelta = new Vector2(900, 40);

            var lineGo = new GameObject("Line", typeof(RectTransform), typeof(Text));
            lineGo.transform.SetParent(box, false);
            var lrt = lineGo.GetComponent<RectTransform>();
            lrt.anchorMin = new Vector2(0, 0.42f);
            lrt.anchorMax = new Vector2(1, 1);
            lrt.offsetMin = new Vector2(28, 0);
            lrt.offsetMax = new Vector2(-28, -60);
            _line = lineGo.GetComponent<Text>();
            _line.font = UiFactory.DefaultFont;
            _line.fontSize = 26;
            _line.color = Color.white;
            _line.alignment = TextAnchor.UpperLeft;
            _line.horizontalOverflow = HorizontalWrapMode.Wrap;
            _line.verticalOverflow = VerticalWrapMode.Overflow;
            _line.raycastTarget = false;

            var choicesGo = new GameObject("Choices", typeof(RectTransform));
            choicesGo.transform.SetParent(box, false);
            _choices = choicesGo.transform;
            var crt = choicesGo.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0.5f, 0f);
            crt.anchorMax = new Vector2(0.5f, 0f);
            crt.pivot = new Vector2(0.5f, 0f);
            crt.anchoredPosition = new Vector2(0, 16);
            crt.sizeDelta = new Vector2(940, 190);

            _continueBtn = UiFactory.Button(box, "Continue", "Далее", new Vector2(380, -140), new Vector2(220, 48),
                () => DialogueRunner.Instance?.ContinueLinear());
        }

        void Show()
        {
            TryBind();
            _root.SetActive(true);
            Refresh();
        }

        void Hide()
        {
            if (_root != null) _root.SetActive(false);
        }

        void Refresh()
        {
            var node = DialogueRunner.Instance?.ActiveNode;
            if (node == null || _root == null) return;

            _speaker.text = node.Speaker ?? "";
            _line.text = node.Line ?? "";

            var portrait = ArtCatalog.PortraitForSpeaker(node.Speaker);
            if (_portrait != null)
            {
                _portrait.sprite = portrait;
                _portrait.color = portrait != null ? Color.white : new Color(0.15f, 0.16f, 0.18f, 1f);
                _portrait.gameObject.SetActive(true);
            }

            for (var i = _choices.childCount - 1; i >= 0; i--)
                Destroy(_choices.GetChild(i).gameObject);

            var hasChoices = node.Choices != null && node.Choices.Count > 0;
            _continueBtn.gameObject.SetActive(!hasChoices);

            if (!hasChoices) return;

            float y = 70;
            foreach (var choice in node.Choices)
            {
                var captured = choice;
                UiFactory.Button(_choices, choice.Id, choice.Text, new Vector2(0, y), new Vector2(920, 50),
                    () => DialogueRunner.Instance.Choose(captured));
                y -= 58;
            }
        }
    }
}
