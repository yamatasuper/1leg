# ТЗ: app_shell_menu — Меню и оболочка

## 0. Паспорт документа
- Название фичи: Меню и оболочка
- ID / кодовое имя: `app_shell_menu`
- Проект / версия: **90 минут** / полный MVP
- Фичеовнер: автор (соло)
- Статус: draft
- Дата создания: 2026-08-12
- Дата обновления: 2026-08-12
- Связанные документы:
  - `docs/project-concept.md`
  - `docs/feature-index.md`
  - `docs/2026-08-12_performance_lowspec_gd-spec.md`
  - `docs/2026-08-12_screen_adaptation_gd-spec.md`
  - `docs/2026-08-12_narrative_core_gd-spec.md`
  - `save_anytime` — spec TBD
- История изменений:
  - 2026-08-12 — первый draft

## 1. Саммари фичи
- Что это: оболочка приложения в эстетике **ТВ/матч-бродкаста** — главное меню, пауза, настройки. Главное меню: **Новая игра / Продолжить / Настройки / Выход**. Язык — в настройках. В паузе обязательно **Сохранить / Загрузить**. Новая игра при существующем сейве — **только с подтверждением**. Кредиты/соцссылки в MVP не нужны.
- Для кого: любой вход в игру и выход в ОС.
- Проблема: нужен понятный, стильный shell без лишней мета-оболочки.
- Эффект: быстрый вход в кампанию; безопасные сейвы; единый TV-look с матч-рамкой.

## 2. Бизнес-контекст
- Почему MVP: без shell нет продукта.
- Альтернативы: комикс-журнал меню; огромный хаб; gallery endings.
- Почему TV-broadcast: стык с футбольной рамкой; отличается от past Disco-world; легко читается.

## 3. Цели
- Главная: вход/пауза/настройки/выход без трения.
- Вторичные: Save/Load в паузе; подтверждение New Game; пресеты графики и аудио; язык.
- Не цели: выбор глав; gallery; соцкнопки; кредиты в main menu (титры после ending отдельно).
- Почему: фокус на кампании.

## 4. Метрики успеха
- Основная: новый игрок за <30 с понимает как начать/продолжить.
- Guardrail: нет случайного вайпа сейва; пауза всегда доступна вне forbidden states (или корректно disabled).
- Провал: непонятный Continue; потеря прогресса; меню выбивается из стиля матча.

## 5. Позиционирование
- Корень приложения; поднимает `narrative_core` New/Continue.
- Хостит settings для `performance_lowspec` и audio; later `screen_adaptation`.
- Save/Load UI вызывает `save_anytime`.
- После ending credits → возврат сюда.

## 6. Scope

### In (MVP)
- **Main menu** (TV style):
  - Новая игра
  - Продолжить (disabled/grey если нет сейва)
  - Настройки
  - Выход
- **Pause menu** (TV style overlay):
  - Продолжить
  - Сохранить
  - Загрузить
  - Настройки
  - В главное меню (с confirm если unsaved dirty — **решение:** confirm always when leaving to title)
  - Выход в ОС (confirm)
- **Settings**:
  - Язык
  - Графика: preset Low/Medium/High (+ FPS limit 30/60 per perf spec)
  - Экран: полноэкран / окно (PC)
  - Аудио: Master, Music, SFX, Voice (thoughts VA), **Crowd** (толпа/стадион)
  - (Optional light) чувствительность стика — только если phone build active
- **New Game** flow: if save exists → modal confirm wipe/overwrite slot policy.
- **Continue**: load latest/autoslot per `save_anytime`.
- No chapter select.
- No credits/social in main menu.
- Respect pause locks from dialogue mid-line / match presentation / trauma cut (pause may open but Save disabled when forbidden — align with save rules).

### Out
- Endings gallery
- Mods
- Cloud login
- Profile/achievements UI
- Trailer/social links

### Future
- Credits from main menu; multiple manual slots UI polish; key rebind.

### Зависимости
- `save_anytime`, `performance_lowspec`, `narrative_core`, `screen_adaptation`, audio buses

## 7. Use Cases

### 7.1 First boot
- Continue grey; New Game starts training/intro flow.
- Language default OS or RU.

### 7.2 New Game with existing save
- Confirm: «Текущий прогресс будет перезаписан. Продолжить?»
- Cancel → back; OK → wipe/start fresh per save policy.

### 7.3 Pause during exploration
- Save/Load available (if not in forbidden state).

### 7.4 Pause mid-dialogue / mid-match beat
- Pause can open for settings/quit; **Save disabled** with short reason (per narrative/presentation contracts).
- Load still available (with confirm) — **решение:** Load allowed with confirm; Save blocked mid-dialogue/presentation.

### 7.5 After ending
- Credits → Main menu; Continue may point to finished clear-state or disabled until New Game — **решение:** Continue loads post-credits unfinished only if save kept as finished flag; prefer enable New Game prominently; Continue opens finished save at menu-only or last interview end — simpler: **finished run save remains loadable to ending card/credits only OR start New Game**. Practical MVP: finished save → Continue shows brief “История завершена” + offer New Game; Load still can open slot if multi-slot later. For single slot: Continue disabled after finish; New Game primary.

## 8. Сущности

### `menu_mode`
- `main|pause|settings|modal_confirm`

### `settings_state`
- language, graphics_preset, fps_limit, fullscreen, vol_master/music/sfx/voice

### `nav_item`
- id, label_key, enabled, action

## 9. Логика
1. Boot → main menu; probe saves.
2. New Game → confirm if save → start campaign.
3. Continue → load.
4. Pause toggle (Esc/Start/phone) when gameplay allows.
5. Settings apply live when possible.
6. Quit / title → confirms.
7. TV art frame chrome around panels (broadcast bezel, scanline light optional via preset).

Save enable matrix (MVP):
- exploration: Save+Load
- dialogue: Load yes / Save no
- match presentation: Load yes / Save no
- menus: n/a

## 10. UI/UX
- Visual: TV broadcast — bezel, “ON AIR” light optional, scoreboard-inspired typography (not gameplay score).
- Clear focus states for gamepad/keyboard/touch.
- Continue grey + hint if empty (“Нет сохранений”).
- Modals for destructive actions.
- Settings list/panels readable on phone landscape.
- No clutter badges/news.

## 11. Параметры

| Параметр | Дефолт | Смысл |
|---|---|---|
| `main_items` | new, continue, settings, quit | |
| `pause_has_save_load` | true | |
| `new_game_confirm_if_save` | true | |
| `credits_in_main_menu` | false | |
| `default_language` | ru | |
| `tv_chrome_enabled` | true | |
| `save_blocked_in_dialogue` | true | |
| `save_blocked_in_match_pres` | true | |

## 12. Формулы / баланс / локализация

### 12.1 Rules
#### `can_save(game_state)`
- false if dialogue active OR match_presentation active OR trauma_cut transition
- else true in gameplay

#### `continue_enabled()`
- has loadable non-corrupt save AND not marked `finished_only_block` policy

### 12.2 Config
| name | default |
|---|---|
| `audio_default_master` | 1.0 |
| `graphics_default_pc` | medium |
| `graphics_default_phone` | low |

### 12.3 Localization
Namespace: `menu`

| key | RU draft | where |
|---|---|---|
| `menu_new_game` | Новая игра | main |
| `menu_continue` | Продолжить | main/pause |
| `menu_settings` | Настройки | |
| `menu_quit` | Выход | |
| `menu_save` | Сохранить | pause |
| `menu_load` | Загрузить | pause |
| `menu_title` | В главное меню | pause |
| `menu_no_save` | Нет сохранений | hint |
| `menu_new_game_confirm` | Текущий прогресс будет перезаписан. Начать новую игру? | modal |
| `menu_save_blocked_dialogue` | Во время разговора сохранить нельзя. | hint |
| `menu_save_blocked_match` | Во время матч-сцены сохранить нельзя. | hint |
| `menu_settings_language` | Язык | settings |
| `menu_settings_graphics` | Графика | |
| `menu_settings_audio` | Звук | |
| `menu_settings_display` | Экран | |
| `menu_vol_master` | Общая громкость | |
| `menu_vol_music` | Музыка | |
| `menu_vol_sfx` | Эффекты | |
| `menu_vol_crowd` | Толпа | |
| `menu_fullscreen` | Полный экран | |
| `menu_finished_hint` | История завершена. Начните новую игру. | continue |

## 13. Контекстные системы
- `save_anytime`: slots/load/save API + forbidden states.
- `performance_lowspec`: graphics preset + fps.
- `screen_adaptation`: fullscreen/window; phone controls.
- `narrative_core`: start/continue campaign.
- `audio_atmosphere`: buses.
- Endings → credits → main.

## 14. Аналитика
| event | params |
|---|---|
| `menu_new_game` | `confirmed_overwrite` |
| `menu_continue` | |
| `menu_pause_open` | `can_save` |
| `menu_save_click` | `blocked` |
| `menu_settings_change` | `key`, `value` |
| `menu_quit` | |

## 15. Edge Cases
- Corrupt save → Continue error modal + offer New Game.
- Rapid New Game confirm cancel.
- Alt-tab during modal.
- Phone: touch-sized buttons; stick hidden in menus.
- Language change mid-run → reload UI strings live.

## 16. Риски
- TV style becomes gimmick → keep clean typography, light chrome.
- Save confusion → clear blocked reasons.
- Accidental overwrite → mandatory confirm.

## 17. Acceptance Criteria
1. Main: New / Continue / Settings / Quit only (language inside settings).
2. Pause includes Save + Load.
3. New Game with existing save requires confirm.
4. Settings include language, graphics presets, display mode, audio sliders Master/Music/SFX/Voice/**Crowd** (+ fps limit).
5. Save blocked mid-dialogue & mid-match presentation with hint.
6. No chapter select, no social, no credits entry in main (MVP).
7. TV-broadcast visual language applied.
8. After ending credits return to main menu safely.

Smoke: fresh New; Continue; overwrite confirm; pause save/load; settings apply; blocked save in dlg; quit confirm.

## 18. Релиз
- First vertical slice shell with stub art.
- Polish TV chrome with art_pipeline.

## 19. Пострелиз
- Успех: вход без вопросов; никто не теряет сейв случайно.
- Провал: вайпы; «где сохранить?»; меню как чужой продукт.
- v2: credits link; keybinds; multi-slot browser UI.

---

## Контекстные блоки
- FTUE entry
- Settings for perf/audio/language
- Save UX gates
- Premium simple shell
- TV aesthetic pillar

## Rationale gate
| Решение | Почему |
|---|---|
| Короткое main menu | Меньше шума |
| Язык в settings | Запрос |
| Save/Load в паузе | Обязательный UX |
| Confirm New Game | Анти-вайп |
| TV style | Футбольная рамка |
| Settings L/M/H + audio + fullscreen | Нужный минимум (решение) |
| Block save in dlg/pres | Уже контракты кора |
