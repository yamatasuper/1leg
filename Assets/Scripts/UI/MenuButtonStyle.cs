using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NinetyMinutes.UI
{
    public sealed class MenuButtonStyle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        public Text Label;
        public Image Accent;
        public Color Normal = new Color(0.96f, 0.93f, 0.86f);
        public Color Hover = new Color(0.86f, 0.68f, 0.28f);
        public Color Disabled = new Color(0.55f, 0.52f, 0.46f, 0.7f);
        Button _btn;

        void Awake()
        {
            _btn = GetComponent<Button>();
        }

        void OnEnable() => Apply(false);

        public void OnPointerEnter(PointerEventData eventData) => Apply(true);
        public void OnPointerExit(PointerEventData eventData) => Apply(false);
        public void OnSelect(BaseEventData eventData) => Apply(true);
        public void OnDeselect(BaseEventData eventData) => Apply(false);

        void Apply(bool hover)
        {
            var on = _btn == null || _btn.interactable;
            if (Label != null)
                Label.color = !on ? Disabled : hover ? Hover : Normal;
            if (Accent != null)
                Accent.enabled = on && hover;
        }
    }
}
