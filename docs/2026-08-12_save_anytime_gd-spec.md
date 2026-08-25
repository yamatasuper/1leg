# ТЗ: save_anytime — Сохранения

## 0. Паспорт документа
- Название фичи: Сохранения
- ID / кодовое имя: `save_anytime`
- Проект / версия: **90 минут** / полный MVP
- Фичеовнер: автор (соло)
- Статус: draft
- Дата создания: 2026-08-12
- Дата обновления: 2026-08-12
- Связанные документы:
  - `docs/project-concept.md`
  - `docs/feature-index.md`
  - `docs/2026-08-12_app_shell_menu_gd-spec.md`
  - `docs/2026-08-12_narrative_core_gd-spec.md`
  - `docs/2026-08-12_characters_dialogue_gd-spec.md`
  - `docs/2026-08-12_match_presentation_gd-spec.md`
  - `docs/2026-08-12_trauma_system_gd-spec.md`
  - `docs/2026-08-12_world_exploration_gd-spec.md`
- История изменений:
  - 2026-08-12 — первый draft

## 1. Саммари фичи
- Что это: система сохранений с **раздельными** ручными слотами и автосейвами. Ручной сейв доступен **везде в геймплее, кроме диалогов** (и кроме матч-presentation / травма-cut — целостность бита, уже в контрактах). Автосейв пишется **на якорях**. Загрузка — **список** сейвов с метаданными. Load до травмы = полный откат. Отдельных предупреждений «сохранись перед риском» нет.
- Для кого: игрок, которому нужен agency и безопасные эксперименты с выборами.
- Проблема: «сейв везде» ломает диалоги/комикс-биты, если не ограничить; без автосейвов легко потерять час.
- Эффект: удобный ручной контроль + страховка якорями; честный rollback.

## 2. Бизнес-контекст
- Почему MVP: без сейвов нет длинных вечерних сессий.
- Альтернативы: только авто; один слот; checkpoint-only.
- Почему manual+auto отдельно + list load: привычный UX; авто не затирает ручные решения игрока.

## 3. Цели
- Главная: надёжный Save/Load с ручным и авто контурами.
- Вторичные: якоря автосейва; list UI с метаданными; полный rollback; уважение запретов диалога/матч-бита.
- Не цели: cloud sync в MVP; предупреждения перед травмой; сейв mid-dialogue.
- Почему: простота и уже принятые narrative-контракты.

## 4. Метрики успеха
- Основная: игрок не теряет прогресс при краше после якоря; понимает разницу ручной/авто.
- Guardrail: нет partial dialogue saves; load полностью восстанавливает состояние.
- Провал: затёрли ручной слот автосейвом; corrupt без recovery; сейв в диалоге всё же проходит.

## 5. Позиционирование
- API для `app_shell_menu` Save/Load/Continue/New Game.
- Снапшотит: narrative, world position, score/frame, soft_stats, dialogue memories/arcs, bridge pending, trauma state, settings-independent gameplay.
- Запреты согласованы с dialogue / match_presentation / trauma.

## 6. Scope

### In (MVP)
- **Manual saves**: отдельные слоты (рекомендуемый минимум **3** ручных).
- **Autosaves**: отдельные слоты (рекомендуемый минимум **3** rotating OR 1–3 named autos).
- Manual save allowed in exploration **anywhere** (any position on map) except when blocked.
- Blocks:
  - mid-dialogue (hard)
  - mid-match presentation timeline (hard; already locked)
  - trauma cut transition (hard)
- Autosave on **anchors**:
  - before dialogue / after dialogue
  - before match beat action / after match beat resolved
  - segment start / segment end (recommended)
  - act enter (training/half-time/interview) recommended
  - NOT spam every few seconds
- Load UI: **list** of manual + auto entries with metadata:
  - timestamp
  - playtime (optional)
  - act/minute if in match lifetime
  - score if match started
  - location name (past) or “Матч” mode
  - type badge: Ручное / Авто
- Continue = load most recent valid save (manual or auto by timestamp).
- New Game overwrite: confirm; clears/rotates per policy (don’t delete all autos silently without confirm — confirm covers wipe).
- Load = **full rollback** of gameplay state (включая soft_stats, pending bridge, trauma flags as in that snapshot).
- No “please save” warnings before PoNR/trauma.

### Out
- Cloud saves MVP
- Cross-device sync
- Photo mode screenshots as required meta (optional nice-to-have later)
- Mid-dialogue checkpointing

### Future
- More slots; cloud; export; screenshot thumbnails

### Зависимости
- `app_shell_menu`, all stateful gameplay systems

## 7. Use Cases

### 7.1 Manual save in street
- Pause → Save → pick manual slot → write snapshot → toast success.

### 7.2 Try save in dialogue
- Save disabled + hint (`menu_save_blocked_dialogue`).

### 7.3 Autosave after dialogue
- On dialogue end anchor → write autosave slot (rotate).

### 7.4 Load pre-trauma
- Open list → pick older save → full state restore → trauma not fired.

### 7.5 Crash recovery
- Relaunch → Continue loads latest autosave/manual.

## 8. Сущности

### `save_slot`
- `slot_id`, `kind` (`manual|auto`), `index`, `timestamp`, `meta`, `payload_ref`, `version`, `corrupt` bool

### `save_meta`
- `act_id`, `time_mode`, `location_id?`, `match_minute?`, `goals_for?`, `goals_against?`, `playtime_sec`, `ending_locked?`, `trauma_triggered?`

### `save_payload`
- full gameplay snapshot (versioned schema)

### `save_forbidden_reason`
- `dialogue|match_presentation|trauma_cut|none`

## 9. Логика
1. `can_manual_save()` → check forbidden reasons.
2. `write_manual(slot)` → serialize payload+meta.
3. `write_auto(anchor_type)` → serialize into next auto slot (FIFO rotate).
4. `load(slot)` → deserialize full; rebuild systems; clear transient UI.
5. `list_saves()` → sort by timestamp desc for UI.
6. Schema `version`; migrations or reject with modal if incompatible.

Forbidden priority: trauma_cut > match_presentation > dialogue.

Autosave failures: log; don’t block gameplay.

Manual save failure: modal error, keep previous file.

## 10. UI/UX
- Pause Load/Save screens: tabs or sections Manual / Auto; list rows with meta.
- Empty slots shown for manual (“Пустой слот”).
- Overwrite manual slot → confirm.
- Loading auto → confirm soft (“Загрузить автосохранение?”) optional light confirm — **решение:** confirm on any load that discards unsaved current session dirty state; if just opened pause after autosave, still confirm load.
- No pre-trauma nag UI.

## 11. Параметры

| Параметр | Дефолт | Смысл |
|---|---|---|
| `manual_slot_count` | 3 | |
| `auto_slot_count` | 3 | rotate |
| `manual_allowed_in_exploration` | true | anywhere |
| `manual_allowed_in_dialogue` | false | |
| `manual_allowed_in_match_pres` | false | integrity |
| `autosave_on_anchors` | true | |
| `autosave_interval_sec` | off | no timed spam |
| `pre_risk_save_nudge` | false | |
| `continue_uses_latest` | true | |
| `load_is_full_rollback` | true | |

## 12. Формулы / баланс / локализация

### 12.1 Rules
#### `can_manual_save(state)`
- if in_dialogue or in_match_presentation or in_trauma_cut → false
- else if in_gameplay → true

#### `pick_auto_slot()`
- least recent auto / round-robin index

#### `continue_slot()`
- max(timestamp) among valid non-corrupt slots

### 12.2 Config
| name | default |
|---|---|
| `save_schema_version` | 1 |
| `auto_rotate` | true |

### 12.3 Localization
Namespace: `save`

| key | RU draft | where |
|---|---|---|
| `save_tab_manual` | Ручные | UI |
| `save_tab_auto` | Авто | UI |
| `save_empty_slot` | Пустой слот | |
| `save_success` | Сохранено | toast |
| `save_failed` | Не удалось сохранить | modal |
| `save_load_confirm` | Загрузить это сохранение? Текущий прогресс сессии будет потерян. | modal |
| `save_overwrite_confirm` | Перезаписать слот? | modal |
| `save_meta_score` | Счёт {gf}:{ga} | list |
| `save_meta_minute` | {minute}' | list |
| `save_meta_location` | {location} | list |
| `save_corrupt` | Файл повреждён | |
| `save_blocked_dialogue` | Во время разговора сохранить нельзя. | |
| `save_blocked_match` | Во время матч-сцены сохранить нельзя. | |

## 13. Контекстные системы
- All gameplay writers must be serializable.
- `trauma_system` relies on full rollback.
- `choice_score_bridge` pending must restore.
- `soft_stats` values restore.
- `world_exploration` position restore.
- Menu Continue/New Game flows.

## 14. Аналитика
| event | params |
|---|---|
| `save_manual` | `slot`, `success` |
| `save_auto` | `anchor`, `slot` |
| `save_load` | `slot`, `kind` |
| `save_blocked` | `reason` |
| `save_corrupt` | `slot` |

## 15. Edge Cases
- Load while “dirty” unsaved → confirm.
- Autosave during pause open → allowed if not forbidden state.
- Multiple rapid anchors → debounce autosave (e.g. 2s) to avoid IO spam.
- Finished ending save: still listable; Continue policy per menu spec.
- Version mismatch after patch → modal, don’t crash.

## 16. Риски
- IO hitch on weak disks → async write + “Сохранение…” brief.
- Player confusion manual vs auto → clear badges.
- Missing system in snapshot → load bugs; maintain schema checklist.
- Allowing save in match beat breaks comic → keep block.

## 17. Acceptance Criteria
1. Manual и Auto — раздельные слоты.
2. Autosave только на якорях (не таймерный спам).
3. Load UI — список с метаданными.
4. Manual save в exploration anywhere; blocked in dialogue.
5. Blocked in match presentation / trauma cut.
6. Load = полный откат состояния.
7. Нет nudge «сохранись» перед травмой.
8. Continue берёт latest valid.
9. Overwrite/load confirms where destructive.

Smoke: manual save on map; block in dlg; autosave after dlg; list meta; load older pre-flag; crash→continue; overwrite confirm.

## 18. Релиз
- With first playable slice.
- Test corrupt simulation once.

## 19. Пострелиз
- Успех: доверяют сейвам; пользуются load до травмы.
- Провал: потеря прогресса; путаница слотов.
- v2: cloud; thumbnails; more slots.

---

## Контекстные блоки
- Agency / anti-punishment via load
- Integrity of dialogue & match beats
- Premium singleplayer save UX
- No risk-nudge spam

## Rationale gate
| Решение | Почему |
|---|---|
| Manual + Auto отдельно | Не затирать ручное |
| Autosave на якорях | Страховка без спама |
| Список с мета | Понятный выбор |
| Ручной везде кроме диалогов | Запрос + свобода в мире |
| Блок match presentation | Целостность бита (контракт) |
| Полный rollback | Честный load до травмы |
| Без risk nudge | Запрос |
