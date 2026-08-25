using UnityEngine;
using UnityEngine.UI;
using NinetyMinutes.UI;

namespace NinetyMinutes.Match
{
    /// <summary>
    /// Comic scoreboard: opponent · score · minute. Hidden in past.
    /// </summary>
    public sealed class MatchFrameUI : MonoBehaviour
    {
        public static MatchFrameUI Instance { get; private set; }

        Canvas _canvas;
        GameObject _root;
        Text _opponent;
        Text _score;
        Text _minute;
        Image _frame;
        float _punchTimer;
        Vector3 _baseScale = Vector3.one;

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

        void OnEnable()
        {
            if (ChoiceScoreBridge.Instance != null)
            {
                ChoiceScoreBridge.Instance.ScoreChanged += Refresh;
                ChoiceScoreBridge.Instance.MinuteChanged += Refresh;
            }
        }

        void OnDisable()
        {
            if (ChoiceScoreBridge.Instance != null)
            {
                ChoiceScoreBridge.Instance.ScoreChanged -= Refresh;
                ChoiceScoreBridge.Instance.MinuteChanged -= Refresh;
            }
        }

        void Update()
        {
            if (_punchTimer <= 0f || _frame == null) return;
            _punchTimer -= Time.unscaledDeltaTime;
            var t = Mathf.Clamp01(_punchTimer / 0.45f);
            var punch = 1f + 0.12f * Mathf.Sin(t * Mathf.PI);
            _frame.transform.localScale = _baseScale * punch;
            if (_punchTimer <= 0f)
                _frame.transform.localScale = _baseScale;
        }

        void Build()
        {
            _canvas = UiFactory.CreateCanvas("MatchFrameCanvas", 280);
            DontDestroyOnLoad(_canvas.gameObject);

            _root = UiFactory.Box(_canvas.transform, "Frame", new Vector2(0, 420), new Vector2(560, 90),
                new Color(0.08f, 0.1f, 0.12f, 0.92f)).gameObject;
            _frame = _root.GetComponent<Image>();
            var outline = _root.AddComponent<Outline>();
            outline.effectColor = new Color(0.95f, 0.92f, 0.4f, 0.85f);
            outline.effectDistance = new Vector2(2, -2);
            _baseScale = _root.transform.localScale;

            _opponent = MakeSlot(_root.transform, "Opponent", new Vector2(-170, 0), new Vector2(180, 70), 26);
            _score = MakeSlot(_root.transform, "Score", new Vector2(0, 0), new Vector2(140, 70), 36);
            _minute = MakeSlot(_root.transform, "Minute", new Vector2(170, 0), new Vector2(140, 70), 28);
            Refresh();
        }

        static Text MakeSlot(Transform parent, string name, Vector2 pos, Vector2 size, int font)
        {
            var box = UiFactory.Box(parent, name, pos, size, new Color(0.12f, 0.14f, 0.16f, 0.95f));
            return UiFactory.Label(box, "T", "—", font, TextAnchor.MiddleCenter, new Color(1f, 0.98f, 0.9f));
        }

        public void Show()
        {
            if (_root != null) _root.SetActive(true);
            Refresh();
        }

        public void Hide()
        {
            if (_root != null) _root.SetActive(false);
        }

        public void PulseScore()
        {
            _punchTimer = 0.45f;
            Refresh();
        }

        public void Refresh()
        {
            var bridge = ChoiceScoreBridge.Instance;
            if (bridge == null || _score == null) return;
            _opponent.text = string.IsNullOrEmpty(bridge.OpponentName) ? "Прибой" : bridge.OpponentName;
            _score.text = $"{bridge.Score.GoalsFor}:{bridge.Score.GoalsAgainst}";
            _minute.text = FormatMinute(bridge.MatchMinute);
        }

        public static string FormatMinute(int minute, int stoppage = 0)
        {
            if (stoppage > 0 && minute >= 90) return $"90+{stoppage}'";
            return $"{minute}'";
        }
    }
}
