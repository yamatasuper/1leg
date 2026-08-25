# ТЗ: world_exploration — Мир вне поля

## 0. Паспорт документа
- Название фичи: Мир вне поля
- ID / кодовое имя: `world_exploration`
- Проект / версия: **90 минут** / полный MVP
- Фичеовнер: автор (соло)
- Статус: draft
- Дата создания: 2026-08-12
- Дата обновления: 2026-08-12
- Связанные документы:
  - `docs/project-concept.md`
  - `docs/feature-index.md`
  - `docs/2026-08-12_narrative_core_gd-spec.md`
  - `docs/2026-08-12_characters_dialogue_gd-spec.md`
  - `docs/2026-08-12_match_presentation_gd-spec.md`
- История изменений:
  - 2026-08-12 — первый draft

## 1. Саммари фичи
- Что это: свободное перемещение по локациям в духе Disco Elysium — ходьба, двери между местами, NPC в мире. Без лута и examine-геймплея: локации дают **атмосферу** и доступ к разговорам/сюжетным триггерам.
- Для кого: игрок в прошлых сегментах, где основа игры — жизнь вне поля.
- Проблема: нужен телесный «я хожу и встречаю людей» слой, а не только меню глав.
- Эффект: мир ощущается цельным и мрачно-живым; сюжет ограничивает двери; матч не ломает позицию героя.

## 2. Бизнес-контекст
- Почему сейчас: MVP-кора вместе с диалогами и narrative spine.
- Альтернативы: point-and-click меню локаций; одна комната; полный open world.
- Почему Disco-like walk + мало локаций: знакомый язык, сильно по атмосфере, подъёмно для 3–5 мест соло.

## 3. Цели
- Главная: дать свободную ходьбу по 3–5 локациям как основу past-геймплея.
- Вторичные: двери с сюжетными локами; NPC в мире; атмосфера без инвентарного шума.
- Не цели: платформинг, головоломки пространства, лут, стелс, крафт.
- Почему: вкусы проекта и границы производства.

## 4. Метрики успеха
- Основная: в past-сессиях игрок проводит время в ходьбе/встречах и не просит «просто меню локаций».
- Guardrail: не заблудиться; не упереться в непонятные двери без намёка; матч-биты не сбрасывают прогресс позиции хаотично.
- Провал: пустые одинаковые карты; двери-бесилки; exploration ощущается лишним перед диалогом.

## 5. Позиционирование
- Активен только в `time_mode = past` (и иных non-match presentation states).
- Стартует диалоги → `characters_dialogue`.
- Слушает флаги/`narrative_core` для unlock дверей и доступности NPC.
- На match beat: мир **suspend** (см. логику).

## 6. Scope

### In (MVP)
- Свободное движение героя (walk) в локации.
- Камера/ракурс **как в Disco** (top-down-ish / isometric-adjacent presentation).
- **3–5** уникальных локаций.
- Переходы **дверями** / проходами на границах сцены.
- Сюжетные локи дверей (`locked` до flag/arc).
- NPC стоят в мире, интерактивны (подойти + interact).
- Атмосфера: арт, звук, idle; **без** examine/pickup loop как фичи.
- Триггеры сегмента: enter zone / talk / door — для narrative.
- Сохранение позиции локации+координат на якорях сейва (вне диалога/presentation).

### Out
- Инвентарь, лут, examine-мини-игры.
- Головоломки/паркур.
- Карта-меню как основной travel (двери — основной способ).
- Большой open world >5 локаций в MVP.
- Управление в матч-слое.

### Future
- Больше локаций; лёгкие examine-флейворы если понадобятся сюжетно; карта-справка.

### Зависимости
- `narrative_core` — какие локации/двери открыты в сегменте
- `characters_dialogue` — interact NPC
- `match_presentation` / `match_frame_ui` — перехват экрана
- `save_anytime` — позиция
- `art_pipeline` / `audio_atmosphere` — атмосфера

## 7. Use Cases

### 7.1 Вход в сегмент прошлого
- Игрок появляется в стартовой локации сегмента (author spawn).
- Может ходить, говорить с доступными NPC, подходить к дверям.

### 7.2 Переход дверью
- Дверь unlocked → load/соседняя локация, spawn у paired door.
- Дверь locked → короткий отказ (текст/анимация), опционально hint журналу.

### 7.3 Диалог
- Interact NPC в радиусе → `characters_dialogue`; движение стопается.

### 7.4 Матч-бит (решение)
- На старте presentation: exploration **suspend**:
  - мир скрыт/выгружен визуально;
  - input walk отключён;
  - позиция и состояние локации **сохранены в runtime**.
- После бита: resume той же локации/позиции, если narrative не задал teleport.
- Почему так: комикс-матч читается чисто; нет рассинхрона «табло поверх улицы»; позиция не теряется.

### 7.5 Конец сегмента
- Narrative может запретить уход (door locks) пока не выполнены goals; затем match beat.

## 8. Сущности

### `location`
- `location_id`, `art_ref`, `nav_bounds`, `ambient_audio_id`, `spawn_points[]`

### `door`
- `door_id`, `from_location`, `to_location`, `locked`, `unlock_requirements[]`, `locked_hint_key`, `exit_anchor`, `enter_anchor`

### `world_npc_spawn`
- `spawn_id`, `character_id`, `location_id`, `position`, `available_requirements[]`, `dialogue_id`

### `player_world_state`
- `location_id`, `position`, `facing`, `suspended` (bool)

### `trigger_volume` (optional light)
- for scripted narrative beats without examine loot

## 9. Логика
1. Enter past segment → load location + spawns filtered by requirements.
2. Walk with collision vs nav bounds / blockers.
3. Interact prompt on NPC/door in range.
4. Door: check lock → transition or reject.
5. On dialogue/presentation/trauma cut: set `suspended=true`, disable walk.
6. On resume: `suspended=false`, restore transform unless override spawn.
7. Save anchors store `player_world_state` (not mid-dialogue/mid-presentation).

Lock priority: trauma/story hard lock > segment gate > arc flags > default open.

Movement: no jump/crouch puzzles; walk + collide only.

## 10. UI/UX
- Минимальный HUD в past: interact prompt; journal; без табло матча.
- Locked door feedback: короткий текст, без RPG-цветов обязательных.
- Fade/короткий cut при смене локации (не page-flip матча).
- Empty: нет NPC — атмосфера ок.
- Error: missing door target → don’t teleport; log content error.
- CTA: Interact, Open journal, Pause/Save (when allowed).

## 11. Параметры

| Параметр | Дефолт | Диапазон | Смысл |
|---|---|---|---|
| `location_count_mvp` | 4 | 3–5 | целевой объём |
| `walk_speed` | TBD feel | — | тюнинг |
| `interact_radius` | TBD | — | |
| `door_fade_sec` | 0.4 | 0.2–0.8 | |
| `suspend_on_match_beat` | true | fixed MVP | |
| `resume_keep_position` | true | bool | |
| `examine_system_enabled` | false | fixed MVP | |
| `minimap_enabled` | false | MVP false | |

## 12. Формулы / баланс / локализация

### 12.1 Формулы / правила
#### `door_is_open(door, state)`
- all `unlock_requirements` pass → open else locked

#### `npc_is_available(spawn, state)`
- requirements pass and not disabled by segment script

#### `suspend_world(reason)`
- reason ∈ {dialogue, match_presentation, trauma, menu}
- store state; disable input; hide world layer

Нет числовой экономики.

### 12.2 Balance vars
| name | default | notes |
|---|---|---|
| `walk_speed` | 1.0 | relative |
| `interact_radius` | 1.0 | relative |
| `door_fade_sec` | 0.4 | |
| `location_count_mvp` | 4 | 3–5 |
| `max_active_npc_per_location` | 3 | читаемость |
| `locked_door_repeat_hint_cd_sec` | 10 | анти-спам текста |

### 12.3 Localization
Namespace: `world`

| key | RU draft | where |
|---|---|---|
| `world_interact` | Говорить | prompt |
| `world_door_enter` | Войти | prompt |
| `world_door_locked` | Сейчас сюда нельзя. | locked |
| `world_door_locked_story` | Ещё не время. | locked story |
| `world_location_<id>_name` | (контент) | optional journal |
| `world_resume_from_match` | (обычно без текста; flip presentation) | — |

## 13. Контекстные системы
- `narrative_core`: segment spawn, door gates, when exploration allowed.
- `characters_dialogue`: NPC interact.
- `match_presentation`: suspend/hide world.
- `match_frame_ui`: not shown in past.
- `save_anytime`: store position on anchors.
- `art_pipeline`: location art Disco-like readable props.
- `audio_atmosphere`: ambients per location.
- `cliche_twist_content`: может диктовать когда локация «ломает ожидание».

## 14. Аналитика
| event | params |
|---|---|
| `world_enter_location` | `location_id` |
| `world_door_try` | `door_id`, `locked` |
| `world_npc_interact` | `character_id` |
| `world_suspend` | `reason` |
| `world_resume` | `kept_position` |
| `world_idle_time` | `location_id`, `sec` |

## 15. Edge Cases
- Save in location A, load → same spot.
- Match beat during near-door → resume same spot; don’t auto-traverse.
- NPC walks off after flag → despawn on next availability refresh.
- All doors locked and no NPC → must have narrative escape (validation).
- Trauma cut from world → suspend + route change; position may become irrelevant.
- Low-spec: simpler collision, fewer idle FX.

## 16. Риски
- Пустые локации → мало мест, сильный арт/звук, NPC осмысленно расставлены.
- Disco-сравнение завышает ожидания → честный scope 3–5, атмосфера важнее систем.
- Двери-фрустрация → clear hints + journal goals.
- Техсложность walk/nav для соло → простые bounds, без сложного pathfinding NPC (NPC idle static MVP).

## 17. Acceptance Criteria
1. Игрок свободно ходит минимум в 3 локациях MVP.
2. Переходы только дверями/проходами (не menu-travel как основа).
3. Запертая дверь не пускает + даёт понятный feedback.
4. NPC в мире интерактивны и стартуют диалог.
5. Нет examine/loot системы.
6. На match beat мир suspend+hide; после — resume позиции (если нет teleport).
7. В past нет матч-табло.
8. Save/load восстанавливает location+position на якорях.
9. Камера/presentation readable «как Disco» в рамках арта проекта.
10. Контент-валидатор: сегмент не может софтлочить без выхода.

Smoke: walk; door lock/unlock; NPC talk; suspend/resume after fake beat; save/load position.

## 18. Релиз
- Срез: 2 локации + 1 door lock demo + 1 NPC.
- Полный MVP: 3–5 локаций, сюжетные локи по кампании.
- Сначала blockout, потом арт.

## 19. Пострелиз
- Успех: «приятно побыть в мире»; двери понятны; матч не сбрасывает ориентацию.
- Провал: пусто/бессмысленно; хочется скипнуть ходьбу всегда.
- v2: +локации; optional flavor examines; лёгкая карта.

---

## Контекстные блоки
- Нарратив/арт: атмосфера локаций
- Прогрессия доступа: door locks
- Тех.ограничения: simple nav, suspend on beat
- Premium: мир в коробке, не live service map

## Rationale gate
| Решение | Почему |
|---|---|
| Свободный walk как Disco | Телесность + референс вкуса |
| 3–5 локаций | Реализм соло MVP |
| Двери | Естественные переходы |
| Только атмосфера | Без лишних систем |
| NPC в мире | Живость, меньше меню |
| Локи по сюжету | Контроль темпа/цельности |
| Suspend мира на матч | Чистый комикс-бит, позиция цела |
