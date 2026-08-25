# ТЗ: characters_dialogue — Персонажи и диалоги

## 0. Паспорт документа
- Название фичи: Персонажи и диалоги
- ID / кодовое имя: `characters_dialogue`
- Проект / версия: **90 минут** / полный MVP
- Фичеовнер: автор (соло)
- Статус: draft
- Дата создания: 2026-08-12
- Дата обновления: 2026-08-12
- Связанные документы:
  - `docs/project-concept.md`
  - `docs/feature-index.md`
  - `docs/2026-08-12_narrative_core_gd-spec.md`
  - `docs/2026-08-12_choice_score_bridge_gd-spec.md`
  - `docs/2026-08-12_match_presentation_gd-spec.md`
- История изменений:
  - 2026-08-12 — первый draft

## 1. Саммари фичи
- Что это: классическая система диалогов (реплика → варианты ответа) с памятью персонажей, серыми закрытыми опциями и связкой доступных реплик с матч-бустами/состоянием (мораль и др.).
- Для кого: игрок, который через разговоры исследует себя, людей и последствия.
- Проблема: нужен живой social layer без Disco-«хора навыков» и без тяжёлой DnD-прокачки.
- Эффект: выборы ощущаются серьёзными; NPC помнят; состояние после/вокруг матча меняет *как* ты можешь говорить.

## 2. Бизнес-контекст
- Почему сейчас: эмоциональное ядро MVP; источник тегов для `choice_score_bridge` и топлива для арок `narrative_core`.
- Альтернативы: чистая VN без памяти; полный Disco skill-check UX; free text.
- Почему классика + буст-гейты: понятно, подъёмно соло, стыкуется с футбольной рамкой («форма» влияет на смелость речи).

## 3. Цели
- Главная: дать сильные 1-on-1 разговоры с трудными решениями и памятью.
- Вторичные: редкие мультиперсонажные сцены; ложь/юление; влияние soft state на пул реплик.
- Не цели: Disco-внутренний хор; билды/скилл-меню; полная озвучка всех NPC в MVP.
- Почему: атмосфера и сюжет важнее системной сложности.

## 4. Метрики успеха
- Основная: плейтестеры помнят NPC и цитируют конкретные выборы/последствия.
- Доп.: заметна разница прохождений из‑за памяти; понятны серые опции.
- Guardrail: не читать wall of text без выбора; не превращать бусты в «угадай правильный стат».
- Провал: NPC «резиновые»; серые опции бесят без намёка; диалоги не влияют на матч/арки.

## 5. Позиционирование
- Вход из `world_exploration` / скриптов `narrative_core`.
- Пишет: dialogue flags, relationship scores, choice tags → bridge, arc flags → core, journal goals updates.
- Читает: soft stats / match boosts (мораль и др.), trauma flags, prior memories.
- Soft stats фича P1, но **контракт гейтов реплик** закладываем в MVP (даже если значения пока упрощены).

## 6. Scope

### In (MVP)
- Классический диалог: speaker line → **минимум 3** ответа на выборах (где выбор есть).
- В основном 1-on-1; редкие сцены 2+ NPC.
- Память: **flags** + **relationship score** на персонажа.
- Закрытые варианты: **серые**, с коротким намёком почему нельзя.
- Типы реплик (контентные): честность / ложь / юление / агрессия / забота / уход от темы — всё допустимо; различимы тегами, не обязательным UI-иконом.
- Теги для моста: `push_up` / `push_down` / `twist` / `delay` / `arc_only` (+ optional tone tags).
- Гейты от состояния/бустов: напр. `morale_high` открывает смелые; `morale_low` держит пул в «тяжёлых/грустных»; смешанные правила per-option.
- Нет Disco-skill thought layer.
- Озвучка MVP: **только мысли** героя (если есть); реплики NPC/вслух — текст.
- Запрет сейва mid-dialogue (якоря до/после) — контракт с `save_anytime`.
- Портрет/имя/комикс-баббл в стиле проекта.

### Out
- Free-text input.
- Постоянные skill checks с процентами как основной UX.
- Полный VA всех персонажей.
- Диалоговый крафт/торговля.

### Future / P1
- Полная калибровка `soft_stats` ↔ option gates.
- Больше multi-NPC сцен.
- Точечный VA ключевых NPC.

### Зависимости
- `narrative_core` — сегменты/арки
- `world_exploration` — старт разговора
- `choice_score_bridge` — теги
- `soft_stats` — гейты (P1 values)
- `save_anytime` — якоря
- journal (в core) — цели/мысли-текст

## 7. Use Cases

### 7.1 Обычный 1-on-1
- Триггер: подход/скрипт.
- Игрок читает реплику, выбирает из ≥3 вариантов (часть может быть grey).
- Система применяет эффекты: memory, relationship delta, tags queue, arc flags, journal.
- Конец узла → продолжение или выход в exploration.

### 7.2 Буст/мораль меняет пул
- При `morale > threshold`: доступны `bold_*` опции.
- При `morale < -threshold`: `bold_*` серые с намёком; доступны `low_*` тяжёлые варианты.
- Игрок видит серое + hint, не скрытый «пустой слот» без объяснения.

### 7.3 Память между сценами
- NPC ссылается на flag (`you_lied_yesterday`) и/или на уровень relationship (холоднее/мягче тон).

### 7.4 Редкий multi-NPC
- Очередь спикеров; выбор адресует target_npc; память/relationship обновляются точечно.

### 7.5 Mid-dialogue exit
- Save disabled; выход в меню без частичного сохранения узла; Continue возвращает к якорю до диалога (или после, если был завершён).

## 8. Сущности

### `character`
- `character_id`, `display_name_key`, `portrait_ref`, `default_tone`

### `relationship`
- `character_id`, `score` (float), `tier` (derived)

### `memory_flag`
- `flag_id`, `value` (bool/enum), `source_dialogue_id`, `created_segment_id`

### `dialogue_graph`
- `dialogue_id`, `participants[]`, `start_node_id`, `save_anchor_before`, `save_anchor_after`

### `dialogue_node`
- `node_id`, `speaker_id` (`hero`|npc|`narrator`), `line_key`, `is_thought` (bool), `next` / `choices[]`

### `dialogue_choice`
- `choice_id`, `text_key`, `requirements[]`, `effects[]`, `bridge_tags[]`, `tone` (`lie`|`evade`|`honest`|`care`|`push`|…), `locked_hint_key`

### `requirement`
- types: `flag`, `relationship_min/max`, `stat_min/max` (morale/energy/…), `arc_state`, `invert`

### `effect`
- types: `set_flag`, `add_relationship`, `queue_bridge_tag`, `set_arc`, `journal_add`, `grant_boost_flag` (редко)

## 9. Логика работы
1. Start dialogue → lock save → open UI.
2. Show node line (thought VA optional if `is_thought`).
3. If choices: evaluate requirements → available / grey(locked).
4. Ensure at least one selectable choice (author validation); if all locked → fallback pass choice (content error + safe continue).
5. On select: apply effects; push bridge tags to pending segment buffer (resolve later by bridge).
6. Advance graph until end.
7. Unlock save anchor after; return control.

Приоритет гейтов:
1. story hard locks (trauma/arc)
2. memory flags
3. soft stats / boosts
4. relationship tiers

Ложь/юление: обычные choices с tone+tags+memory; нет отдельного мини-геймa «детекции» обязательно — реакция NPC через flags/score.

## 10. UI/UX
- Комикс/новелла-бабблы + список вариантов.
- Минимум 3 слота визуально стабильны; grey options остаются видимыми.
- Hint на grey: одна короткая строка (`Нужно больше смелости` / `Слишком тяжёлое состояние` — тон проекта, без RPG-жаргана где можно).
- Нет skill icons chorus.
- Multi-NPC: индикатор кто говорит; optional «кому отвечаешь» если нужно.
- Empty/Error: missing line key → debug placeholder; never softlock.
- CTA: выбор реплики; Continue на linear nodes.

## 11. Параметры

| Параметр | Дефолт | Диапазон | Смысл |
|---|---|---|---|
| `min_choices_per_branch` | 3 | 3–6 | валидация контента |
| `relationship_default` | 0 | -100..100 | |
| `relationship_tier_step` | 25 | 10–50 | |
| `morale_bold_threshold` | 2 | 0–10 | открыть смелые |
| `morale_low_threshold` | -2 | -10..0 | low pool / grey bold |
| `grey_hint_enabled` | true | bool | |
| `thought_va_enabled` | true | bool | только мысли |
| `npc_va_enabled` | false | MVP false | |
| `save_during_dialogue` | false | fixed | |

## 12. Формулы / баланс / локализация

### 12.1 Формулы

#### `choice_lock_state(choice, state)`
- if all requirements pass → `available`
- else → `grey` + first failing requirement → `locked_hint_key`

#### `relationship_tier(score)`
- tiers by `relationship_tier_step` (e.g. hostile/cold/neutral/warm/close)
- used for tone variants / requirements

#### `apply_choice_effects(choice)`
- relationship.score += delta (clamp -100..100)
- set flags
- queue bridge tags with weights
- arc updates via narrative API

#### `morale_gate_example`
- bold requirement: `stat_min(morale, morale_bold_threshold)`
- low-only lines: `stat_max(morale, morale_low_threshold)`
- mid band: default lines always available unless story lock

Почему так: бусты матча ощущаются в разговоре, но не превращают UI в sheet статов.

### 12.2 Balance vars
| name | default | risk | why |
|---|---|---|---|
| `min_choices_per_branch` | 3 | мало agency | вариативность |
| `relationship_delta_small` | 5 | шум | тонкая память |
| `relationship_delta_large` | 15 | резкие скачки | ключевые сцены |
| `morale_bold_threshold` | 2 | слишком редкие bold | связь с матчем |
| `morale_low_threshold` | -2 | слишком часто grey | давление |
| `lie_detect_memory_weight` | 1 | NPC «всеведущие» | калибр последствий лжи |
| `bridge_tag_weight_default` | 1 | разгон счёта | стык с bridge |

### 12.3 Localization
Namespace: `dlg`

| key pattern | example | where |
|---|---|---|
| `dlg_<id>_n_<node>` | текст реплики | node |
| `dlg_<id>_c_<choice>` | текст выбора | choice |
| `dlg_hint_morale_low` | Сейчас не до смелости. | grey hint |
| `dlg_hint_morale_need_bold` | Нужен подъём внутри. | grey hint |
| `dlg_hint_flag_blocked` | Ты уже всё сказал об этом. | grey hint |
| `dlg_hint_rel_low` | Он тебя не слышит. | grey hint |
| `dlg_thought_va_n_<node>` | (optional) | thoughts |

Имена персонажей: `char_<id>_name`.

## 13. Контекстные системы
- `narrative_core`: arcs, segment buffer for tags.
- `choice_score_bridge`: consumes tags after segment (not per-line jump).
- `soft_stats`: morale/energy/strength gates (P1 full).
- `world_exploration`: interact → dialogue.
- `match_presentation`: не показывает good/bad; только поздние последствия.
- `trauma_system`: may hard-lock or abort dialogue into cut.
- `cliche_twist_content`: требования к содержанию реплик/поворотов.
- `audio_atmosphere`: thought VA only in MVP.

## 14. Аналитика
| event | params |
|---|---|
| `dlg_start` | `dialogue_id`, `participants` |
| `dlg_choice` | `choice_id`, `tones[]`, `tags[]`, `was_grey_visible` |
| `dlg_lock_seen` | `choice_id`, `fail_req` |
| `dlg_end` | `dialogue_id`, `duration` |
| `dlg_memory_set` | `flag_id` |
| `dlg_rel_delta` | `character_id`, `delta`, `score_after` |

## 15. Edge Cases
- Все опции grey → fallback continue + content error log.
- Multi-NPC target missing → default primary participant.
- Soft stats missing (до P1) → treat morale=0 mid-band; bold/low special lines content-gated off or authored carefully.
- Load mid-node forbidden; resume at pre-dialogue anchor.
- Trauma during dialogue → abort to trauma route; dialogue state discarded or marked interrupted.
- Repeat talk same NPC: graph may short-circuit via flags (`already_talked_topic`).

## 16. Риски
- Затянутость текста → лимит на длину node, обязательные выборы, editor pass.
- Серые опции бесят → хорошие hints; mid-band всегда даёт путь.
- Память слишком жёсткая → комбинация soft score + flags.
- Связь со статами превращается в min-max → не показывать цифры; намёки человеческим языком.
- Редкие multi-NPC дорогие → мало, но обязательны для арок (как в narrative_core).

## 17. Acceptance Criteria
1. Выборные узлы имеют ≥3 варианта (валидатор контента).
2. Память: flag и relationship влияют на позднюю сцену (smoke fixture).
3. Grey option виден + hint; не выбирается.
4. Morale/boost gate открывает/закрывает bold vs low pool (даже на stub stats).
5. Ложь/юление проходят как choices с последствиями памяти.
6. Нет Disco-skill chorus UI.
7. VA только на thoughts (если включено); NPC lines text-only MVP.
8. Save mid-dialogue blocked; anchors before/after work.
9. Bridge tags с choices доезжают до segment resolve (интеграционный smoke).
10. Есть ≥1 редкая multi-NPC сцена в контент-плане MVP.

Smoke: 1-on-1 branch; grey morale; memory callback; lie flag; multi-NPC rare; save blocked; tag to bridge.

## 18. Релиз
- Вертикальный срез: 1–2 ключевых NPC, память на 1 callback, 1 grey-gate demo.
- Полный MVP: основной cast, flags+scores, rare multi scenes.
- Сначала заглушки портретов/текста, потом полировка.

## 19. Пострелиз
- Успех: помнят персонажей; спорят о выборах; понимают серые намёки; чувствуют связь формы и речи.
- Провал: «все NPC одинаковые»; «непонятно почему серое»; «текст без agency».
- v2: шире soft_stats gates; точечный NPC VA; больше multi-scenes.

---

## Контекстные блоки
- FTUE: первый диалог учит grey hints без туториал-плашки.
- Нарратив: memory + tone variety + anti-cliché content requirements.
- Прогрессия: relationship/flags вместо билдов.
- Premium: глубина cast в коробке.
- Связь с матчем: бусты ↔ доступные реплики.

## Rationale gate
| Решение | Почему |
|---|---|
| Классика ≥3 | Понятно, вариативно |
| Flags + relationship | И точные крючки, и мягкий тон |
| Grey + hint | Честнее скрытых опций |
| Бусты гейтят речь | Матч↔жизнь без skill-chorus |
| Ложь/юление ок | Живые выборы, твисты |
| VA только мысли | Дёшево и интимно |
| 1-on-1 + rare multi | Фокус и иногда ансамбль |
| Нет сейва mid-dlg | Цельность разговора |
