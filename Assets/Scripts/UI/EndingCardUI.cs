using System;
using NinetyMinutes.Narrative;
using NinetyMinutes.UI;
using UnityEngine;
using UnityEngine.UI;

namespace NinetyMinutes.UI
{
    public sealed class EndingCardUI : MonoBehaviour
    {
        public static EndingCardUI Instance { get; private set; }

        Canvas _canvas;
        GameObject _cardRoot;
        GameObject _creditsRoot;
        Text _title;
        Text _body;
        Text _credits;
        Action _onCardDone;
        Action _onCreditsDone;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            Build();
            HideAll();
        }

        void Build()
        {
            _canvas = UiFactory.CreateCanvas("EndingCanvas", 320);
            DontDestroyOnLoad(_canvas.gameObject);

            _cardRoot = UiFactory.Panel(_canvas.transform, "EndingCard", new Color(0.03f, 0.04f, 0.05f, 0.98f)).gameObject;
            var frame = UiFactory.Box(_cardRoot.transform, "Frame", Vector2.zero, new Vector2(980, 620),
                new Color(0.1f, 0.12f, 0.14f, 1f));
            _title = UiFactory.Title(frame, "Title", "", 44, new Vector2(0, -48), new Color(0.95f, 0.92f, 0.4f));
            _body = UiFactory.Label(frame, "Body", "", 26, TextAnchor.UpperCenter, new Color(1f, 0.97f, 0.9f));
            _body.rectTransform.offsetMin = new Vector2(60, 100);
            _body.rectTransform.offsetMax = new Vector2(-60, -110);
            UiFactory.Button(frame, "Next", "Титры", new Vector2(0, -250), new Vector2(280, 56), () =>
            {
                _cardRoot.SetActive(false);
                _onCardDone?.Invoke();
            });

            _creditsRoot = UiFactory.Panel(_canvas.transform, "Credits", new Color(0.02f, 0.02f, 0.03f, 0.98f)).gameObject;
            var cframe = UiFactory.Box(_creditsRoot.transform, "Frame", Vector2.zero, new Vector2(900, 560),
                new Color(0.08f, 0.09f, 0.11f, 1f));
            _credits = UiFactory.Label(cframe, "CreditsText", "", 28, TextAnchor.MiddleCenter, Color.white);
            _credits.rectTransform.offsetMin = new Vector2(40, 80);
            _credits.rectTransform.offsetMax = new Vector2(-40, -80);
            UiFactory.Button(cframe, "Menu", "В меню", new Vector2(0, -220), new Vector2(280, 56), () =>
            {
                HideAll();
                _onCreditsDone?.Invoke();
            });
        }

        public void ShowEnding(EndingRoute route, Action onContinue)
        {
            _onCardDone = onContinue;
            _title.text = EndingsService.Title(route);
            _body.text = EndingsService.Body(route);
            _creditsRoot.SetActive(false);
            _cardRoot.SetActive(true);
        }

        public void ShowCredits(Action onDone)
        {
            _onCreditsDone = onDone;
            _credits.text =
                "90 МИНУТ\n\nАлексей Бардин\nГлок · Пень · Сокол\nВиктор Семёнович\n\nПрибрежье · «Торпедо»";
            _cardRoot.SetActive(false);
            _creditsRoot.SetActive(true);
        }

        void HideAll()
        {
            if (_cardRoot != null) _cardRoot.SetActive(false);
            if (_creditsRoot != null) _creditsRoot.SetActive(false);
        }
    }
}
