# ТЗ: performance_lowspec — Стабильность и low-spec

## 0. Паспорт документа
- Название фичи: Стабильность и low-spec
- ID / кодовое имя: `performance_lowspec`
- Проект / версия: **90 минут** / P1
- Фичеовнер: автор (соло)
- Статус: draft
- Дата создания: 2026-08-12
- Дата обновления: 2026-08-12
- Связанные документы:
  - `docs/project-concept.md`
  - `docs/feature-index.md`
  - `docs/2026-08-12_screen_adaptation_gd-spec.md`
  - `docs/2026-08-12_match_presentation_gd-spec.md`
  - `docs/2026-08-12_world_exploration_gd-spec.md`
  - `app_shell_menu` — settings presets TBD
- История изменений:
  - 2026-08-12 — первый draft

## 1. Саммари фичи
- Что это: целевая стабильность **~30 FPS** на слабом железе и телефоне, с упором на **не греть** устройство. Пресеты графики **Low / Medium / High** в настройках. Бюджет производительности одинаково важен для мира-ходьбы, диалогов и матч-комикса. В low режем то, что меньше бьёт по смыслу: постобработку, лишние VFX/частицы, лишний realtime, тяжелые тени/fullscreen эффекты, агрессивный preload — **не** читаемость текста, не логику сюжета, не обязательные комикс-панели.
- Для кого: игроки на слабых ПК/ноутбуках и будущих phone landscape-сборках.
- Проблема: narrative/comic игра тоже может греть CPU/GPU из‑за overdraw, высоких текстур, частиц и необузданного update.
- Эффект: стабильный проход кампании без «печки» и без сломанного UX.

## 2. Бизнес-контекст
- Почему P1: доступность и стабильность — явные цели качества проекта.
- Альтернативы: только авто-детект; только 60 FPS target; резать контент.
- Почему 30 + пресеты + равный приоритет сцен: реалистично для lowspec; игрок контролирует; нельзя «оптимизировать только меню».

## 3. Цели
- Главная: держать ~30 FPS стабильно на low preset / weak devices, без сильного нагрева.
- Вторичные: три пресета; единый perf budget на все gameplay modes; безопасные дефолты.
- Не цели: ультра-графика; 60 FPS как hard requirement на low; разные «качества истории» по пресетам.
- Почему: история важнее блеска.

## 4. Метрики успеха
- Основная: на reference low machine / mid phone landscape — medium-low нагрузка, ~30 FPS, температура/шум субъективно «не греет ужасно» за сессию ~60 мин.
- Guardrail: не падать в однозначный slideshow (<20) в комиксе/мире; не отключать сюжетный контент.
- Провал: low preset всё ещё печёт; пресеты не отличаются; на Low ломается читаемость.

## 5. Позиционирование
- Settings в `app_shell_menu`.
- Влияет на rendering/audio FX density across `world_exploration`, `match_presentation`, UI.
- Рядом с `screen_adaptation` (res scale может стыковаться).
- Не меняет design rules skip/dialogue/endings.

## 6. Scope

### In (P1)
- Target frame pacing: **30 FPS** cap option / target on Low (Medium/High may allow 30 or 60 if device can — **решение:** expose `FPS_limit` 30/60; Low defaults 30).
- Presets: **Low / Medium / High** (+ optional Custom later; MVP/P1 = 3 presets).
- Equal priority profiles for:
  - world walk
  - dialogue UI
  - match comic presentation
  - menus/journal
- Low cut order (less important first — решения):
  1. Post-processing (bloom, color grading heavy, film grain stacks)
  2. Particles / VFX spam / screen shake extras
  3. Realtime lights/shadows (if any) → baked/unlit
  4. Texture/aniso/mip bias; optional lower render scale (0.75–0.85)
  5. Reduce simultaneous ambient audio layers / stop distant ones
  6. Stricter unload of unused location textures after leave
  7. Disable non-essential idle animations extras
- Never cut on Low:
  - dialogue text/choices readability
  - required comic panels for beats
  - save/input correctness
  - core audio stingers needed for feedback (can lower quality, not mute critical)
- Thermal: prefer FPS lock 30 + lower res scale over chasing 60 on weak GPUs.
- Simple perf HUD for debug builds only.

### Out
- Per-scene “story quality” degradation.
- Ultra/RTX features.
- Complex auto ML upscaling pipelines as requirement.
- Guaranteeing 60 on iGPU.

### Future
- Auto recommend preset on first launch; Custom sliders; more platform profiles.

### Зависимости
- `app_shell_menu` settings UI
- `screen_adaptation` (render scale/safe)
- art/audio pipelines for streaming/compression
- all runtime presentation features

## 7. Use Cases

### 7.1 First launch
- Default **Medium** on PC; **Low** recommend on detected weak/phone (if detect weak — heuristic optional; if unsure Medium + hint).
- Player can switch preset anytime; apply without restart when possible (restart only if unavoidable).

### 7.2 Low preset session
- 30 FPS lock; reduced PP/VFX; possible 85% render scale; unlit/baked look still readable comic style.

### 7.3 High preset
- Full PP/VFX as authored; 60 unlock if monitor/device ok; no gameplay advantage.

### 7.4 Thermal pressure (phone)
- If OS thermal warnings available — soft suggest Low (optional). No forced story skip.

## 8. Сущности

### `graphics_preset`
- `low|medium|high`

### `perf_profile`
- maps preset → render_scale, pp_enabled, particles_budget, shadow_mode, fps_limit, audio_layers_max, texture_budget

### `perf_runtime_state`
- current preset, last_fps_avg, optional thermal_hint_shown

## 9. Логика
1. Load preset from settings.
2. Apply `perf_profile` to render pipeline/URP-like settings, particle budgets, audio buses density.
3. Cap FPS per profile.
4. On location transition: unload previous heavy assets (queue).
5. Match presentation uses same preset (no special “always high”).
6. Persist preset in save/settings.

Preset table (defaults):

| | Low | Medium | High |
|---|---|---|---|
| fps_limit | 30 | 30 | 60 (or 30 if user set) |
| render_scale | 0.8 | 1.0 | 1.0 |
| post_process | off/min | partial | full |
| particles | minimal | normal | full |
| shadows/lights | unlit/baked | simple | authored |
| audio layers | reduced | normal | full |
| texture bias | +1 mip | 0 | 0 |

## 10. UI/UX
- Settings → Graphics: preset dropdown Low/Medium/High.
- Short descriptions:
  - Low — меньше нагрузка, цель 30 FPS, меньше эффектов
  - Medium — баланс
  - High — максимум эффектов (если тянет)
- Optional FPS limit toggle 30/60 on Medium/High.
- No confusing “quality affects story” wording.
- Apply button or auto-apply with 3s revert confirm if screen blanks (nice-to-have).

## 11. Параметры

| Параметр | Дефолт | Смысл |
|---|---|---|
| `target_fps_low` | 30 | |
| `default_preset_pc` | medium | |
| `default_preset_phone` | low | |
| `render_scale_low` | 0.8 | |
| `pp_on_low` | false | |
| `equal_budget_all_modes` | true | |
| `allow_story_degrade` | false | |
| `fps_limit_options` | 30,60 | |

## 12. Формулы / баланс / локализация

### 12.1 Rules
#### `recommend_preset(device)`
- if phone or iGPU heuristic weak → low else medium

#### `frame_budget_ms(fps_limit)`
- `1000/fps_limit` soft budget; warn in dev if exceeded N frames

No player-facing formulas.

### 12.2 Balance / budgets
| name | low | med | high |
|---|---|---|---|
| `max_particles` | low | mid | high |
| `max_audio_ambients` | 2 | 4 | 6 |
| `location_texture_mb_hint` | tight | normal | normal |

### 12.3 Localization
Namespace: `perf`

| key | RU draft | where |
|---|---|---|
| `perf_preset_label` | Графика | settings |
| `perf_preset_low` | Низкая | |
| `perf_preset_medium` | Средняя | |
| `perf_preset_high` | Высокая | |
| `perf_preset_low_desc` | Меньше эффектов, цель 30 FPS, слабее нагрев | |
| `perf_preset_medium_desc` | Баланс качества и нагрузки | |
| `perf_preset_high_desc` | Максимум эффектов, если устройство тянет | |
| `perf_fps_limit` | Ограничение FPS | |
| `perf_thermal_hint` | Устройство греется. Можно включить «Низкая». | optional |

## 13. Контекстные системы
- `app_shell_menu`: hosts settings.
- `screen_adaptation`: render scale + resolution.
- `match_presentation` / world / UI: respect budgets equally.
- `audio_atmosphere`: layer limits.
- `art_pipeline`: compression/mip guidance for lowspec.

## 14. Аналитика
| event | params |
|---|---|
| `perf_preset_set` | `preset` |
| `perf_fps_sample` | `avg`, `scene_mode` |
| `perf_thermal_hint_shown` | |

## 15. Edge Cases
- User sets High on weak GPU → allow; may stutter; hint to lower.
- Changing preset mid match beat → apply end-of-beat if unsafe; else soft apply.
- VSync vs fps limit conflicts → document preference (limit ≤ refresh).
- Battery saver OS mode → recommend Low.

## 16. Риски
- Solo over-engineering graphics code → keep URP/quality levels simple.
- Low looks “broken” → cut FX not panels/text.
- Ignoring thermal → prefer 30 lock.
- Uneven optimization (only menu smooth) → equal mode policy + QA matrix.

## 17. Acceptance Criteria
1. Presets Low/Medium/High существуют и заметно отличаются нагрузкой/эффектами.
2. Low целится в ~30 FPS и меньше эффектов/scale.
3. World, dialogue, match comic — без привилегий «всегда high».
4. На Low не пропадают обязательные панели/текст/выборы.
5. Настройки сохраняются.
6. Default адекватен PC medium / phone low.
7. Нет требования 60 на lowspec.
8. Субъективно low меньше греет на reference weak device vs High.

Smoke: switch presets; play walk+dlg+match on Low @30; text readable; assets unload on location change; settings persist.

## 18. Релиз
- P1 with PC build; phone uses same presets when `screen_adaptation` lands.
- Profile on one weak laptop + one mid PC.

## 19. Пострелиз
- Успех: weak machines играют сессию без «печки»; пресеты понятны.
- Провал: Low бесполезен; High обязателен чтобы было играбельно.
- v2: auto recommend, custom sliders.

---

## Контекстные блоки
- Tech constraints / thermal
- Settings UX
- Equal mode budgets
- Premium accessibility of performance

## Rationale gate
| Решение | Почему |
|---|---|
| 30 FPS target | Запрос + lowspec realism |
| «Не греть» | Важнее пиковых FPS |
| Пресеты L/M/H | Контроль игрока |
| Режем PP/VFX/scale/shadows first | Меньше вреда смыслу |
| Не режем текст/панели/сюжет | История священна |
| Одинаковый приоритет режимов | Нет «красивого только меню» |
