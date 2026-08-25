# ТЗ: screen_adaptation — Адаптация экранов

## 0. Паспорт документа
- Название фичи: Адаптация экранов
- ID / кодовое имя: `screen_adaptation`
- Проект / версия: **90 минут** / P2
- Фичеовнер: автор (соло)
- Статус: draft
- Дата создания: 2026-08-12
- Дата обновления: 2026-08-12
- Связанные документы:
  - `docs/project-concept.md`
  - `docs/feature-index.md`
  - `docs/2026-08-12_match_presentation_gd-spec.md`
  - `docs/2026-08-12_match_frame_ui_gd-spec.md`
  - `docs/2026-08-12_characters_dialogue_gd-spec.md`
  - `docs/2026-08-12_world_exploration_gd-spec.md`
  - `app_shell_menu`, `performance_lowspec` — specs TBD
- История изменений:
  - 2026-08-12 — первый draft

## 1. Саммари фичи
- Что это: единый UI/layout с якорями под **PC** (16:9, ультраширокие, 4:3) и **телефон в landscape**, с учётом safe areas (нотчи/вырезы). На мобилках: **виртуальный стик + tap interact**. Не отдельные «две игры», а одна схема, которая тянется и не ломает комикс-биты, ходьбу и диалоговые выборы.
- Для кого: игрок на разных дисплеях; соло-порт без двойной поддержки двух дизайнов.
- Проблема: комикс + Disco-like мир + табло легко «плывут» на нестандартных аспектах.
- Эффект: читаемо везде; на телефоне можно пройти кампанию горизонтально без portrait-режима в P2.

## 2. Бизнес-контекст
- Почему P2: сначала PC vertical slice; мобилки — «по возможности».
- Альтернативы: только 16:9 letterbox; отдельные Mobile UI; portrait phone.
- Почему один layout + landscape phone + stick/tap: меньше стоимости, привычный горизонтальный комикс/матч, достаточный контроль.

## 3. Цели
- Главная: корректный показ и ввод на PC-аспектах и phone landscape.
- Вторичные: safe areas; единые якоря; не жертвовать комиксом / walk / choices.
- Не цели: portrait phone в P2; уникальный mobile art pipeline; отдельный второй UI kit.
- Почему: соло-бюджет и единый визуальный язык.

## 4. Метрики успеха
- Основная: на целевых разрешениях нет клиппинга критичного UI; тексты выборов читаемы; стик не перекрывает главные CTA.
- Guardrail: комикс-панели не обрезают лица/бабблы; табло не прыгает в unsafe; диалоги ≥3 вариантов умещаются.
- Провал: «на ультрашироком пустая полоса с мелким UI»; «на телефоне нельзя нажать третий вариант»; «стик закрывает interact».

## 5. Позиционирование
- Зависит от `app_shell_menu` (resolution/window).
- Применяется ко всем presentation слоям: world, dialogue, match comic, journal/state, pause.
- Рядом с `performance_lowspec` (мобилки часто слабее — не смешивать ответственность: эта фича про layout/input, не про FPS).

## 6. Scope

### In (P2)
- PC targets:
  - 16:9 (baseline)
  - ultrawide (≈21:9 / 32:9 behavior: pillar/letter strategy — **решение:** keep gameplay camera content centered; side padding may show extended backdrop or soft bars; critical UI stays in center safe rect)
  - 4:3 / taller PC: letterbox or vertical fit with side/UI reflow via anchors
- Phone: **landscape only** in P2 (portrait → soft prompt rotate / lock landscape if OS allows).
- **One layout** with anchors/safe-area padding (not separate Desktop/Mobile schemes).
- Mobile controls: virtual **left stick** + **tap** on interactables/UI; no separate tap-to-move path as primary.
- Safe areas: notch/home indicator insets applied to all HUD/dialogue/stick.
- Reflow rules for: dialogue choice list, journal/state, match comic stage, match frame widget, pause menu.
- Priority: comic readability + walk usability + choice hit-targets — all must pass QA (no single sacrifice).

### Out
- Portrait gameplay layout.
- Fully separate Mobile UI redesign.
- Gyro controls.
- Split-screen.
- Per-device unique comic panel art crops (beyond safe generic framing).

### Future
- Portrait mode; more gamepad-specific layouts; cloud save cross-device.

### Зависимости
- `app_shell_menu`, all UI features, `world_exploration` input, `match_presentation` stage

## 7. Use Cases

### 7.1 PC 16:9
- Baseline reference layout.

### 7.2 PC ultrawide
- World camera keeps subject readable; UI anchored within central safe rect; no stretched text.

### 7.3 PC 4:3
- Scale to fit height/width with bars as needed; choices remain tappable/clickable with spacing.

### 7.4 Phone landscape
- Stick bottom-left in safe area; interact via tap on NPC/door/prompt; dialogue choices full-width bottom stack with min touch size.
- Comic beats: stage letterboxed inside safe rect; bubbles reflow within panel safe text areas.

### 7.5 Notch device
- All controls/text inset by safe area; no critical CTA under notch/home bar.

## 8. Сущности

### `display_profile`
- `platform` (pc|phone), `aspect`, `safe_insets`, `dpi/scale`

### `layout_anchor_set`
- shared ids: `hud_top`, `hud_corner`, `dialogue_bottom`, `stick_bl`, `journal_panel`, `comic_stage`

### `touch_controls_state`
- stick active only phone (or touch PC optional off by default)

### `orientation_policy`
- phone landscape required

## 9. Логика
1. On boot/resize: compute aspect + safe insets → `display_profile`.
2. Apply canvas scaler (reference resolution TBD, e.g. 1920×1080) with match-width/height hybrid.
3. Position anchored elements; inflate touch targets on phone.
4. Enable virtual stick if touch platform.
5. Comic stage: fit inside safe rect preserving panel aspect; never stretch non-uniformly.
6. If phone portrait detected: block/pause with rotate message.
7. Ultrawide: clamp UI to max readable width centered (`max_ui_width`).

Input mapping phone:
- stick → move
- tap NPC/door/prompt → interact
- tap UI buttons → UI
- no tap-empty-ground-to-move (avoids conflict with interact taps)

## 10. UI/UX
- One visual language; sizes scale by dpi.
- Min touch target phone: ~48–56 dp equivalent.
- Dialogue: stacked choices; grey options still visible with hint.
- Stick translucency high enough to see world; hide stick during dialogue/match presentation/menus.
- Match frame comic widget uses same anchors as presentation moments.
- Accessibility: text never below readable min font on phone landscape reference devices.

## 11. Параметры

| Параметр | Дефолт | Диапазон | Смысл |
|---|---|---|---|
| `reference_resolution` | 1920×1080 | | |
| `phone_orientation` | landscape | fixed P2 | |
| `separate_mobile_layout` | false | fixed | один layout |
| `virtual_stick_enabled_phone` | true | | |
| `tap_to_move_enabled` | false | fixed | |
| `safe_area_enabled` | true | | |
| `max_ui_width_ultrawide` | 1920 | 1600–2200 | |
| `min_choice_touch_h` | 48dp | | |
| `stick_hide_in_dialogue` | true | | |
| `stick_hide_in_match_pres` | true | | |
| `portrait_block` | true | phone | |

## 12. Формулы / баланс / локализация

### 12.1 Rules (not combat formulas)
#### `fit_comic_stage(rect, safe)`
- uniform scale to max size inside safe; center; preserve aspect

#### `ui_safe_rect(screen, insets, max_width)`
- screen minus insets; width clamped to max_width centered on ultrawide

#### `touch_target_pad(control)`
- enforce min size on phone

### 12.2 Config vars
| name | default | why |
|---|---|---|
| `max_ui_width_ultrawide` | 1920 | не расползаться |
| `min_font_phone_px` | TBD calibrated | читаемость |
| `stick_opacity` | 0.45 | видеть мир |
| `choice_spacing_phone` | larger | miss-tap |

### 12.3 Localization
Namespace: `screen`

| key | RU draft | where |
|---|---|---|
| `screen_rotate_to_landscape` | Поверните телефон горизонтально | portrait block |
| `screen_ultrawide_ok` | (no text needed) | — |

## 13. Контекстные системы
- All UI features consume anchors.
- `world_exploration`: stick movement on phone.
- `match_presentation`: comic fit + hide stick.
- `characters_dialogue`: choice layout.
- `soft_stats` journal tab reflow.
- `performance_lowspec`: may lower res scale on phone (separate feature).

## 14. Аналитика
| event | params |
|---|---|
| `screen_profile` | `aspect`, `platform`, `safe_insets` |
| `screen_orientation_block` | |
| `screen_ui_overflow_assert` | `element_id` (dev) |

## 15. Edge Cases
- Foldables / weird aspects: treat as ultrawide/tall with same clamp rules.
- Keyboard+touch hybrid PC: stick off unless enabled.
- Resize mid-dialogue: reflow without losing selection index.
- Match presentation during resize: pause-safe reflow.
- One-handed phone: stick left; critical choices remain reachable (bottom stack).

## 16. Риски
- One layout too compromised → strong anchor QA matrix.
- Stick vs dialogue overlap → hide stick in dlg/pres.
- Ultrawide empty feel → backdrop extend, UI centered.
- Solo testing device matrix large → prioritize: 16:9 PC, 21:9 PC, 4:3 PC, one mid phone landscape, one notched phone.

## 17. Acceptance Criteria
1. PC 16:9 / ultrawide / 4:3: no critical clip; readable choices; comic not stretched.
2. Phone landscape playable with virtual stick + tap interact.
3. Portrait phone blocked with rotate message.
4. Safe areas respected (notch/home).
5. Single layout/anchors (no second UI scheme).
6. Stick hidden in dialogue & match presentation.
7. Comic, walk, and choices all pass device QA checklist (none marked “acceptable loss”).
8. Ultrawide UI clamped to max width center.

Smoke: resize 16:9↔21:9↔4:3; phone landscape walk+talk+match beat; notch insets; portrait block; choice hit targets.

## 18. Релиз
- After PC MVP stable.
- P2 milestone: phone landscape soft launch / side build.
- Feature flag `phone_build_enabled`.

## 19. Пострелиз
- Успех: проходят на телефоне горизонтально без «сломанного UI».
- Провал: miss-taps; обрезанный комикс; просьбы portrait.
- v2: portrait; finer device profiles.

---

## Контекстные блоки
- Tech constraints / multi-aspect
- Touch input
- Premium portability without separate product
- Safe area compliance

## Rationale gate
| Решение | Почему |
|---|---|
| PC аспекты + phone landscape | Запрос + комикс/матч горизонтальны |
| Стик + tap interact | Прямой контроль без tap-to-move конфликтов |
| Один layout | Дешевле и цельный стиль |
| Safe areas сразу | Иначе ломается на реальных телефонах |
| Всё критично | Нельзя выбрать одно «пожертвовать» |
| Portrait block P2 | Scope control |
