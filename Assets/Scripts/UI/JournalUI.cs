using NinetyMinutes.Stats;
using NinetyMinutes.UI;
using UnityEngine;
using UnityEngine.UI;

namespace NinetyMinutes.UI
{
    public sealed class JournalUI : MonoBehaviour
    {
        public static JournalUI Instance { get; private set; }

        GameObject _root;
        Text _body;
        bool _open;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            Build();
            SetOpen(false);
        }

        void Update()
        {
            if (Core.GameSession.Instance == null) return;
            if (Core.GameSession.Instance.Phase == Core.SessionPhase.MainMenu) return;
            if (Dialogue.DialogueRunner.Instance != null && Dialogue.DialogueRunner.Instance.IsOpen) return;
            if (Match.MatchPresentation.Instance != null && Match.MatchPresentation.Instance.IsPlaying) return;
            if (Input.GetKeyDown(KeyCode.Tab))
                SetOpen(!_open);
        }

        void Build()
        {
            var canvas = UiFactory.CreateCanvas("JournalCanvas", 240);
            DontDestroyOnLoad(canvas.gameObject);
            _root = UiFactory.Panel(canvas.transform, "JournalRoot", new Color(0f, 0f, 0f, 0.65f)).gameObject;
            var box = UiFactory.Box(_root.transform, "Box", Vector2.zero, new Vector2(900, 640),
                new Color(0.09f, 0.11f, 0.14f, 0.98f));
            box.gameObject.AddComponent<Outline>().effectColor = new Color(0.9f, 0.92f, 0.4f, 0.9f);

            UiFactory.Title(box, "Title", "ЖУРНАЛ · СОСТОЯНИЕ", 36, new Vector2(0, -28), new Color(0.95f, 0.92f, 0.35f));

            var bodyGo = new GameObject("Body", typeof(RectTransform), typeof(Text));
            bodyGo.transform.SetParent(box, false);
            var rt = bodyGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = new Vector2(40, 80);
            rt.offsetMax = new Vector2(-40, -90);
            _body = bodyGo.GetComponent<Text>();
            _body.font = UiFactory.DefaultFont;
            _body.fontSize = 26;
            _body.color = Color.white;
            _body.alignment = TextAnchor.UpperLeft;
            _body.horizontalOverflow = HorizontalWrapMode.Wrap;
            _body.verticalOverflow = VerticalWrapMode.Overflow;
            _body.raycastTarget = false;

            UiFactory.Button(box, "Close", "Закрыть (Tab)", new Vector2(0, -270), new Vector2(320, 52), () => SetOpen(false));
        }

        public void SetOpen(bool open)
        {
            _open = open;
            if (_root != null) _root.SetActive(open);
            if (open) Refresh();
        }

        public void Refresh()
        {
            if (SoftStatsService.Instance == null)
            {
                _body.text = "Состояние недоступно.";
                return;
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Внутри тоже есть счёт.\n");
            foreach (var row in SoftStatsService.Instance.Snapshot())
                sb.AppendLine($"{row.name}: {row.value:0.#}  ({row.band})");
            sb.AppendLine("\nЦели:");
            sb.AppendLine("• Поговорить с тренером");
            sb.AppendLine("• Осмотреться в раздевалке и на улице");
            _body.text = sb.ToString();
        }
    }
}
