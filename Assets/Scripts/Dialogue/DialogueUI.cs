using NinetyMinutes.Art;
using NinetyMinutes.UI;
using UnityEngine;
using UnityEngine.UI;

namespace NinetyMinutes.Dialogue
{
    public sealed class DialogueUI : MonoBehaviour
    {
        public static DialogueUI Instance { get; private set; }

        static readonly Color Cream = new Color(0.96f, 0.93f, 0.86f, 1f);
        static readonly Color Mustard = new Color(0.78f, 0.58f, 0.22f, 1f);

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

        void Update()
        {
            if (_root == null || !_root.activeSelf) return;
            if (DialogueRunner.Instance == null || !DialogueRunner.Instance.IsOpen) return;
            if (_continueBtn != null && _continueBtn.gameObject.activeSelf
                && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
                DialogueRunner.Instance.ContinueLinear();
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
            _root = UiFactory.Panel(canvas.transform, "DialogueRoot", new Color(0.05f, 0.04f, 0.03f, 0.28f)).gameObject;

            var card = UiFactory.PaperCard(_root.transform, "Card", new Vector2(0f, -268f), new Vector2(1520f, 420f));

            var portraitRt = UiFactory.Box(card, "Portrait", new Vector2(-600f, 16f), new Vector2(260f, 300f),
                new Color(0.08f, 0.07f, 0.06f, 1f));
            _portrait = portraitRt.GetComponent<Image>();
            _portrait.preserveAspect = true;
            _portrait.raycastTarget = false;

            _speaker = UiFactory.Headline(card, "Speaker", "", 26, new Vector2(80f, -22f), new Vector2(860f, 40f), Mustard,
                TextAnchor.UpperLeft);
            _speaker.rectTransform.anchorMin = _speaker.rectTransform.anchorMax = new Vector2(0.5f, 1f);

            var lineGo = new GameObject("Line", typeof(RectTransform), typeof(Text));
            lineGo.transform.SetParent(card, false);
            var lrt = lineGo.GetComponent<RectTransform>();
            lrt.anchorMin = new Vector2(0.22f, 0.38f);
            lrt.anchorMax = new Vector2(0.96f, 0.88f);
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;
            _line = lineGo.GetComponent<Text>();
            _line.font = UiFactory.DefaultFont;
            _line.fontSize = 26;
            _line.color = Cream;
            _line.alignment = TextAnchor.UpperLeft;
            _line.horizontalOverflow = HorizontalWrapMode.Wrap;
            _line.verticalOverflow = VerticalWrapMode.Overflow;
            _line.raycastTarget = false;

            var choicesGo = new GameObject("Choices", typeof(RectTransform));
            choicesGo.transform.SetParent(card, false);
            _choices = choicesGo.transform;
            var crt = choicesGo.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0.22f, 0f);
            crt.anchorMax = new Vector2(0.96f, 0.4f);
            crt.offsetMin = new Vector2(0f, 16f);
            crt.offsetMax = Vector2.zero;

            _continueBtn = UiFactory.GhostButton(card, "Continue", "Далее  ↵", new Vector2(560f, -156f), new Vector2(280f, 52f),
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

            _speaker.text = string.IsNullOrEmpty(node.Speaker) ? "Бардин" : node.Speaker;
            _line.text = node.Line ?? "";

            var portrait = ArtCatalog.PortraitForSpeaker(node.Speaker);
            if (_portrait != null)
            {
                _portrait.sprite = portrait;
                _portrait.color = portrait != null ? Color.white : new Color(0.12f, 0.11f, 0.1f, 1f);
                _portrait.gameObject.SetActive(true);
            }

            for (var i = _choices.childCount - 1; i >= 0; i--)
                Destroy(_choices.GetChild(i).gameObject);

            var hasChoices = node.Choices != null && node.Choices.Count > 0;
            _continueBtn.gameObject.SetActive(!hasChoices);

            if (!hasChoices) return;

            float y = 48f;
            foreach (var choice in node.Choices)
            {
                var captured = choice;
                UiFactory.GhostButton(_choices, choice.Id, choice.Text, new Vector2(0f, y), new Vector2(1040f, 48f),
                    () => DialogueRunner.Instance.Choose(captured));
                y -= 54f;
            }
        }
    }
}
