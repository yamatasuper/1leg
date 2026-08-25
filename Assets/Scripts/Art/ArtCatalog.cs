using System.Collections.Generic;
using UnityEngine;

namespace NinetyMinutes.Art
{
    /// <summary>
    /// Loads slice art from Resources/Art. Falls back to solid colors if missing.
    /// </summary>
    public static class ArtCatalog
    {
        static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        public static Sprite LocationStreet => Load("Art/Locations/bg_street");
        public static Sprite LocationLocker => Load("Art/Locations/bg_locker");
        public static Sprite LocationPitch => Load("Art/Locations/bg_pitch");
        public static Sprite PortraitBardin => Load("Art/Portraits/portrait_bardin");
        public static Sprite PortraitGlock => Load("Art/Portraits/portrait_glock");
        public static Sprite PortraitSokol => Load("Art/Portraits/portrait_sokol");
        public static Sprite PortraitCoach => Load("Art/Portraits/portrait_coach");
        public static Sprite PortraitPen => Load("Art/Portraits/portrait_pen");
        public static Sprite SpritePlayer => Load("Art/Characters/sprite_player");
        public static Sprite MatchAction => Load("Art/Match/match_panel_action");
        public static Sprite MatchGoal => Load("Art/Match/match_panel_goal");

        public static Sprite PortraitForSpeaker(string speaker)
        {
            if (string.IsNullOrEmpty(speaker)) return PortraitBardin;
            var s = speaker.Trim().ToLowerInvariant();
            if (s.Contains("глок") || s.Contains("серёг") || s.Contains("серег")) return PortraitGlock;
            if (s.Contains("пень") || s.Contains("олег")) return PortraitPen;
            if (s.Contains("сокол")) return PortraitSokol;
            if (s.Contains("виктор") || s.Contains("семён") || s.Contains("семен") || s.Contains("тренер"))
                return PortraitCoach;
            if (s.Contains("бардин") || s.Contains("алексей")) return PortraitBardin;
            return PortraitBardin;
        }

        public static Sprite Load(string resourcesPath)
        {
            if (string.IsNullOrEmpty(resourcesPath)) return null;
            if (Cache.TryGetValue(resourcesPath, out var cached) && cached != null)
                return cached;

            var sprite = Resources.Load<Sprite>(resourcesPath);
            if (sprite == null)
            {
                var tex = Resources.Load<Texture2D>(resourcesPath);
                if (tex != null)
                {
                    tex.filterMode = FilterMode.Bilinear;
                    sprite = Sprite.Create(
                        tex,
                        new Rect(0, 0, tex.width, tex.height),
                        new Vector2(0.5f, 0.5f),
                        100f);
                }
            }

            if (sprite != null)
                Cache[resourcesPath] = sprite;
            else
                Debug.LogWarning($"[90 минут] Art missing: Resources/{resourcesPath}");

            return sprite;
        }

        public static Texture2D Tex(Sprite sprite)
        {
            return sprite != null ? sprite.texture : null;
        }
    }
}
