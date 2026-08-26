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
        public static Sprite SpriteCoach => Load("Art/Characters/sprite_coach");
        public static Sprite SpriteGlock => Load("Art/Characters/sprite_glock");
        public static Sprite SpriteSokol => Load("Art/Characters/sprite_sokol");
        public static Sprite MatchAction => Load("Art/Match/match_panel_action");
        public static Sprite MatchMid => Load("Art/Match/match_panel_mid");
        public static Sprite MatchGoal => Load("Art/Match/match_panel_goal");
        public static Sprite MenuKey => Load("Art/UI/ui_menu_key") ?? LocationStreet ?? LocationPitch;

        /// <summary>
        /// Frames of one strike drawn on a locked-off camera, so flipping them reads as motion.
        /// The standalone panels are only a fallback when the strip is missing.
        /// </summary>
        public static Sprite[] MatchSequence()
        {
            var strip = Filter(
                Load("Art/Match/match_frame_01"),
                Load("Art/Match/match_frame_02"),
                Load("Art/Match/match_frame_03"),
                Load("Art/Match/match_frame_04"));
            return strip.Length > 1 ? strip : Filter(MatchAction, MatchMid, MatchGoal);
        }

        public static Sprite[] RunFrames()
        {
            return Filter(
                Load("Art/Animation/anim_run_01"),
                Load("Art/Animation/anim_run_02"),
                Load("Art/Animation/anim_run_03"),
                Load("Art/Animation/anim_run_04"));
        }

        static Sprite[] Filter(params Sprite[] sprites)
        {
            var list = new List<Sprite>();
            if (sprites == null) return list.ToArray();
            foreach (var s in sprites)
                if (s != null) list.Add(s);
            return list.ToArray();
        }

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
