# ТЗ: audio_atmosphere — Звук и музыка

## 0. Паспорт документа
- Название фичи: Звук и музыка
- ID / кодовое имя: `audio_atmosphere`
- Проект / версия: **90 минут** / P1
- Фичеовнер: автор (соло)
- Статус: draft
- Дата создания: 2026-08-12
- Дата обновления: 2026-08-12
- Связанные документы:
  - `docs/project-concept.md`
  - `docs/feature-index.md`
  - `docs/2026-08-12_app_shell_menu_gd-spec.md`
  - `docs/2026-08-12_match_presentation_gd-spec.md`
  - `docs/2026-08-12_world_exploration_gd-spec.md`
  - `docs/2026-08-12_characters_dialogue_gd-spec.md`
  - `docs/2026-08-12_performance_lowspec_gd-spec.md`
  - `docs/2026-08-12_pre_match_training_gd-spec.md`
  - `art_pipeline` — TBD (music/SFX sourcing)
  - `docs/2026-08-18_music_direction_gd-spec.md` — **канон треков и промптов; важнее списка кью в этом ТЗ**
- История изменений:
  - 2026-08-12 — первый draft
  - 2026-08-18 — контент-кью живут в music bible

## 1. Саммари фичи
- Что это: звуковая атмосфера с упором на **тишину + эмбиент**, а не непрерывный OST. Отдельные шины: Master / Music / SFX / Voice / **Crowd**. Комментатор озвучивает **только ключевые** матч-биты (текст баббла всегда). VA мыслей героя — **редко**. Приоритет качества звука — **матч**. ИИ-музыка допустима, если не палится.
- Для кого: игрок, для которого атмосфера Disco-уровня важнее «радиохитов» non-stop.
- Проблема: лишняя музыка убивает тревожную/медитативную тишину; плохой матч-звук ломает последствия выборов.
- Эффект: past дышит тишиной; матч бьёт звуком в нужных ударах.

## 2. Бизнес-контекст
- Почему P1: атмосфера — ядро обещания; можно stub в vertical slice.
- Альтернативы: continuous soundtrack; VA everywhere; один SFX bus.
- Почему silence-first + key VA + crowd bus + match priority: вкус пользователя + соло-бюджет озвучки.

## 3. Цели
- Главная: атмосферный mix с тишиной/эмбиентом и сильным матч-звуком на ключевых битах.
- Вторичные: crowd bus; rare thought VA; key commentator VA; preset-aware layer limits.
- Не цели: полный VA всех NPC; музыка нон-стоп; дешёвый stadium pack на каждом бите одинаково.
- Почему: точечность = качество.

## 4. Метрики успеха
- Основная: плейтест «звук матча цепляет»; past не раздражает лупами.
- Guardrail: low preset не убивает ключевые матч-стингеры; Voice/Crowd крутятся отдельно.
- Провал: «дешёвый стадион»; «музыка орёт всегда»; комментатор на каждом чихе бесит.

## 5. Позиционирование
- Driven by location/act/`match_presentation` events.
- Settings volumes from `app_shell_menu`.
- Layer budgets from `performance_lowspec`.
- Dialogue thought VA hooks from `characters_dialogue` (`is_thought`).
- Commentator lines from match caption keys when flagged `va_key`.

## 6. Scope

### In (P1)
- Bus layout:
  - `master`
  - `music` (редкие треки + лёгкий underscore)
  - `sfx`
  - `voice` (thoughts + commentator)
  - `crowd` (толпа/стадион bed + reactions) — **отдельно от SFX**
- Default posture: **silence / ambient beds** in past locations; music sparse.
- Dynamic but restrained crossfade past ↔ match (dip ambient, raise crowd on match enter).
- Match audio priority:
  - key beat stingers (goal/concede/miss/etc.)
  - crowd reactions
  - commentator VA only on **key beats** (author flag `commentator_va=true`)
  - always keep comic text even if VA missing
- Thought VA: rare, author-flagged peak scenes only.
- NPC spoken VA: not in MVP/P1 (text only).
- Training/half-time: quiet ambiences; avoid pump-up montage clichés unless twist.
- Menu TV: light broadcast bed optional, keep modest.
- Low preset: fewer ambient layers; keep critical match stingers; may drop non-key crowd layers.
- AI-sourced music/ambience allowed if not obviously AI; human VA for commentator/thoughts preferred when present.

### Out
- Full continuous playlist OST
- Full cast VA
- Adaptive music middleware complexity (Wwise-sized) as requirement — keep simple state machine
- Spatial 3D audio extravaganza (stereo/simple attenuation enough)

### Future
- More commentator coverage; richer adaptive stems; headphone HRTF optional

### Зависимости
- `match_presentation`, `world_exploration`, `characters_dialogue`, `app_shell_menu`, `performance_lowspec`, `art_pipeline`/audio sourcing

## 7. Use Cases

### 7.1 Walking past street
- Low ambient bed (wind/city/fridge hum); long gaps ok; no constant melody.

### 7.2 Enter match beat (key goal)
- Page flip; crowd swell; stinger; optional commentator VA; then settle.

### 7.3 Enter match beat (non-key)
- Short crowd/SFX; text bubble only; no VA.

### 7.4 Rare thought
- Soft duck ambience; play thought VA; resume.

### 7.5 Low preset
- Max 2 ambients; crowd simplified; stingers remain.

## 8. Сущности

### `audio_bus_id`
- master|music|sfx|voice|crowd

### `ambient_bed`
- `bed_id`, `location_or_act`, `layers[]`, `priority`

### `music_cue`
- `cue_id`, rare use, duck rules

### `stinger`
- `stinger_id`, mapped to outcome types

### `va_clip`
- `va_id`, `kind` (commentator|thought), `key`, optional

### `audio_state`
- current beds, match_mode bool, duck_stack

## 9. Логика
1. On location/act enter → start ambient set (silence-friendly).
2. On match presentation start → transition: reduce past beds, enable crowd bed, arm stingers.
3. On outcome → play stinger; if key+va available → voice.
4. On thought node with VA → duck + play.
5. On presentation end → reverse transition to past beds.
6. Settings change volumes live.
7. Preset changes layer counts.

Key beat selection: author content flag, not every bridge resolve.

## 10. UI/UX
- No in-world music player.
- Settings sliders: Master, Music, SFX, Voice, **Crowd** (если 4 слота уже в меню — **решение:** либо 5-й слайдер Crowd, либо Crowd под SFX с internal ratio; **выбрать:** добавить **Crowd** отдельным слайдером — запрос «нужны» отдельный bus).
- Mute safe; don’t break cues on unmute mid-beat (restart bed politely).

> Обновить `app_shell_menu` settings: Voice + Crowd раздельно.

## 11. Параметры

| Параметр | Дефолт | Смысл |
|---|---|---|
| `default_posture` | silence_ambient | |
| `continuous_ost` | false | |
| `commentator_va_key_beats_only` | true | |
| `thought_va_rarity` | rare | |
| `crowd_bus_separate` | true | |
| `audio_priority_domain` | match | |
| `ambient_layers_low` | 2 | |
| `ambient_layers_med` | 4 | |
| `ambient_layers_high` | 6 | |
| `music_duck_on_va_db` | -6..-10 | |
| `ai_audio_allowed_if_natural` | true | |

## 12. Формулы / баланс / локализация

### 12.1 Mix rules
#### `enter_match_audio()`
- fade_out past_beds (partial)
- fade_in crowd_bed
- set match_mode

#### `play_outcome_audio(outcome, key_flag)`
- sfx/stinger always (budget permitting)
- crowd reaction one-shot
- if key_flag and va exists → voice on `voice` bus

#### `layer_budget(preset)`
- clamp active ambients to table

### 12.2 Balance
| name | default | why |
|---|---|---|
| `match_stinger_gain` | higher relative | priority match |
| `past_music_chance` | low | silence-first |
| `non_key_va` | false | |
| `crowd_bed_level_match` | moderate | not harsh |

### 12.3 Localization
Namespace: `audio` (mostly settings; VA tied to content keys)

| key | RU draft | where |
|---|---|---|
| `audio_bus_crowd` | Толпа | settings |
| `audio_bus_voice` | Голос | settings |
| `audio_bus_music` | Музыка | settings |
| `audio_bus_sfx` | Эффекты | settings |

Commentator/thought texts remain in `match_pres_*` / `dlg_*`; VA is optional audio twin.

## 13. Контекстные системы
- `match_presentation`: primary consumer; quality bar highest.
- `world_exploration`: ambient beds per location.
- `characters_dialogue`: rare thought VA.
- `pre_match_training` / `half_time`: quiet rooms, crowd distant optional.
- `performance_lowspec`: layer caps.
- `app_shell_menu`: volumes (+ Crowd slider).
- `endings` / interview: sparse, intimate; avoid stadium bed.

## 14. Аналитика
| event | params |
|---|---|
| `audio_va_played` | `kind`, `id` |
| `audio_stinger` | `outcome` |
| `audio_preset_layers` | `count` |

## 15. Edge Cases
- Missing VA file → text only, no stall.
- User sets Music=0 → keep ambience if under SFX/Crowd as designed; don’t force silence of crowd.
- Rapid beat pack → don’t stack 3 commentator VA; queue/skip non-key.
- Trauma cut → hard stop crowd beds; intimate stinger/silence.
- Skip training → no special fanfare needed.

## 16. Риски
- AI music obvious → review pass.
- Crowd sample cheap → invest match pack first.
- Too much silence feels empty → location-specific subtle beds.
- Voice ducking harsh → tune.

## 17. Acceptance Criteria
1. Past default = ambient/silence, not continuous OST.
2. Crowd bus exists and is separately controllable.
3. Commentator VA only on key beats; text always works without VA.
4. Thought VA rare only.
5. Match stingers/crowd quality prioritized over menu/past candy.
6. Low preset reduces layers but keeps key match feedback.
7. Settings volumes apply live (Master/Music/SFX/Voice/Crowd).
8. No NPC full VA required.

Smoke: street ambient; key goal with VA; non-key miss without VA; rare thought; low preset layer clamp; trauma silence; settings sliders.

## 18. Релиз
- Slice: 1 ambient + 1 crowd bed + 2 stingers + optional 1 VA.
- P1: per-location beds, key VA set, settings Crowd slider wired.

## 19. Пострелиз
- Успех: матч звучит дорого относительно остального; past медитативен.
- Провал: дешёвый stadium; музыка-доставучесть; VA spam.
- v2: more key VA; richer adaptive beds.

---

## Контекстные блоки
- Silence-first narrative audio
- Match-priority production
- Sparse VA strategy
- Lowspec layer budgets
- Settings buses

## Rationale gate
| Решение | Почему |
|---|---|
| Тишина + эмбиент | Запрос вкуса / Disco atmosphere |
| VA комментатора только key | Бюджет + не бесить |
| Мысли редко | Интимность |
| Crowd отдельным bus | Запрос + mix control |
| Приоритет матч | Последствия выборов слышны |
| ИИ ок если незаметно | Уже политика арта/аудио проекта |
