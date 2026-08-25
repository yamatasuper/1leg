# Art — канон Бардина

Папка: `Assets/Resources/Art/`  
Стиль и промпты: `docs/2026-08-18_art_direction_gd-spec.md`  
Human pass: `docs/2026-08-12_art_pipeline_gd-spec.md` (техника, не стиль)

**Эталоны стиля:** `Maps/map_priberezhe_approved.jpg`, `Maps/map_stadium_approved.jpg`.  
Цвет формы — горчичный жёлтый. Лица — взрослый реализм в манге, не shonen. Скромный клуб ≠ грязь.

| файл | роль |
|---|---|
| `Maps/map_priberezhe_approved.jpg` | карта Прибрежья (утверждённый стиль) |
| `Maps/map_stadium_approved.jpg` | схема «Торпедо» (утверждённый стиль) |
| `Locations/bg_locker.png` | раздевалка «Торпедо» (светлая, чистая) |
| `Locations/bg_street.png` | бровка |
| `Locations/bg_pitch.png` | поле |
| `Portraits/portrait_bardin.png` | Алексей Бардин |
| `Portraits/portrait_glock.png` | Глок |
| `Portraits/portrait_pen.png` | Пень |
| `Portraits/portrait_sokol.png` | Сокол |
| `Portraits/portrait_coach.png` | Виктор Семёнович |
| `Portraits/portrait_wife.png` | жена (жёлтое пальто) |
| `Characters/sprite_player.png` | аватар / фуллбоди |
| `Match/match_panel_action.png` | матч-панель (отбор) |
| `Match/match_panel_goal.png` | голевая панель |
| `Animation/anim_run_01–04.png` | кадры бега |
| `KeyArt/art_cover.png` | обложка |
| `KeyArt/art_whistle.png` | после свистка |
| `Refs/kit_yellow_color_only.jpg` | только цвет формы, не лицо |

Код: `Assets/Scripts/Art/ArtCatalog.cs`

Мир в игре — **3D**: текстуры локаций на стенах и поле, персонажи — билборды. Портреты в диалогах по-прежнему 2D UI.
