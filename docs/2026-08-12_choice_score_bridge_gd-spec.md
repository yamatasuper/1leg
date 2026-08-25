# ТЗ: choice_score_bridge — Выборы → счёт

## 0. Паспорт документа
- Название фичи: Выборы → счёт
- ID / кодовое имя: `choice_score_bridge`
- Проект / версия: **90 минут** / полный MVP
- Фичеовнер: автор проекта (соло)
- Стейкхолдеры: автор
- Статус: draft
- Дата создания: 2026-08-12
- Дата обновления: 2026-08-12
- Связанные документы:
  - `docs/project-concept.md`
  - `docs/feature-index.md`
  - `docs/2026-08-12_narrative_core_gd-spec.md`
- История изменений:
  - 2026-08-12 — первый draft по kickoff-интервью

## 1. Саммари фичи
- Что это: скрытый мост между выборами в прошлом и исходами матч-битов в настоящем. После сегмента (или по delayed-триггеру) система выдаёт пачку футбольных последствий: гол, пропуск, обводка, промах, аут, флаги буста.
- Для кого: игрок, которому нужна **наглядность последствий** без маркировки «правильный/неправильный» выбор.
- Какую проблему решает: сюжетные ветки сами по себе не ощущаются как игра; мост делает решения видимыми на табло и в событиях матча.
- Ожидаемый эффект: после удачного разрешения — **облегчение** (и другие эмоции от счёта); ощущение, что жизнь вне поля бьёт по матчу-зеркалу.

## 2. Бизнес-контекст
- Почему фича появилась: ключевой дифференциатор «это игра, не только VN».
- Почему сейчас: MVP-блокер вместе с `narrative_core` и рамкой матча.
- Альтернативы:
  1. Мгновенный исход на каждый выбор → слишком много прыжков прошлое↔матч.
  2. Только сюжетные флаги без счёта → слабее наглядность.
  3. Честный прозрачный «моральный meter» → ломает атмосферу и мешает твистам.
- Почему выбранный подход лучше: пачка после сегмента + delayed/накопление; скрытые теги; мораль и табло могут расходиться ради ломки клише.

## 3. Цели
- Главная цель: наглядно показать последствия решений через матч.
- Вторичные: поддержать вариативность и несколько концовок; дать автору инструмент твистов (в т.ч. «нечестных»).
- Не является целью: обучать «правильным» ответам; симулировать FIFA; делать победу в матче главной победой жизни героя.
- Почему так: в концепте понимание себя важнее счёта; счёт — зеркало и давление.

## 4. Метрики успеха
- Основная: на плейтесте игрок **сам связывает** свои решения с тем, что происходит на табло (без подсказок good/bad).
- Дополнительные: разнообразие итогов матча между прохождениями; наличие хотя бы одного «нечестного» твиста, который обсуждают.
- Guardrail: не чаще, чем «футбольный реализм» по плотности голов; не спамить прыжками; не превращать счёт в единственный критерий концовки.
- Горизонт: вертикальный срез друзьям → полное прохождение.
- Успех/провал: успех — «облегчение/напряжение от счёта» + понимание связи; провал — счёт кажется рандомом или, наоборот, моральной кассой.
- Почему эти метрики: для premium narrative важнее читаемость agency, чем сервисовые KPI.

## 5. Позиционирование
- Место: кора цикла segment → resolve → match_beat.
- Этап: вся кампания.
- Использует: теги выборов из `characters_dialogue` / контента; сегменты из `narrative_core`.
- Кормит: `match_frame_ui`, `match_presentation`, `endings_system`; позже модифицируется `soft_stats`.
- Может сломать: темп матча (слишком много голов), доверие к выборам (если всё random), цельность если счёт важнее арок.

## 6. Scope (In / Out / Future)

### In scope (MVP)
- Скрытая модель тегов выбора: `push_up` / `push_down` / `twist` / `delay` / `arc_only`.
- Скрытый **пульс формы** (`form_pulse`), который игрок **никогда** не видит (даже в journal).
- Резолв после сегмента → **пачка** исходов (`outcome_pack`).
- Delayed-триггеры: эффект может стрельнуть в более позднем матч-бите.
- Накопление: серия тегов копится в пульсе и/или в pending queue.
- Типы исходов моста: `goal_for`, `goal_against`, `dribble_win`, `dribble_lose`, `miss`, `ball_out`, плюс флаги буста для стыка с `soft_stats` (`boost_flag`).
- Старт матча: **0:0** с момента выхода на поле (intro тоже с 0:0).
- Потолки «футбольного реализма» на плотность событий и разумные счета.
- Стык с травмой: pending pack сбрасывается; мост не форсит исход.
- Авторский override на сегмент/бит (forced outcome / twist).

### Out of scope
- Показ good/bad в UI.
- Полная логика soft stat bars (`soft_stats`).
- Расчёт финальных ending routes (`endings_system`) — только поставка сигналов счёта.
- Визуал удара/комментарий (`match_presentation`).
- `half_end` как narrative act-gate — остаётся у `narrative_core` (мост может лишь не выдавать гол в этом бите).

### Future (v2)
- Более тонкие теги; взвешенные таблицы по актам; A/B плотности событий.

### Зависимости
- `narrative_core` (когда резолвить)
- контент выборов с тегами
- `match_frame_ui` (применить счёт/событие)
- `endings_system` (читать итог)
- `trauma_system` (перехват)
- `soft_stats` (P1 модификатор)

## 7. Use Cases

### 7.1 Первый вход
- Триггер: завершён первый past-сегмент после intro.
- Действия игрока: ничего специального — просто доиграл сегмент.
- Реакция системы: считает теги → обновляет `form_pulse` → собирает `outcome_pack` → запускает match_beat с исходом(ами).
- Результат: игрок видит следствие **только** в матче.

### 7.2 Повторный цикл
- Триггер: конец сегмента или срабатывание `delay` timer/flag.
- Действия: выборы без видимой оценки.
- Реакция: накопление и/или delayed fire; возможна пачка (гол + boost_flag и т.д.).
- Результат: счёт/событие меняются в пределах реализма.

### 7.3 Негативные / особые
- `twist` + морально «хороший» выбор → `goal_against` или `miss` (авторский/табличный твист).
- Delayed: тег не даёт бит сейчас, кладёт `pending_effect` на будущий `match_beat_id` / act.
- Trauma mid-segment: pack сегмента cancel; мост молчит.
- Пустой сегмент (`arc_only` only): pack может быть пустым или только soft narrative beat без изменения счёта — автор задаёт `allow_empty_pack`.

## 8. Сущности

### `choice_tag`
- Назначение: скрытая метка на варианте ответа/действии.
- Поля: `tag` (`push_up`|`push_down`|`twist`|`delay`|`arc_only`), `weight` (float), `delay_to` (nullable ref), `forced_outcome` (nullable).
- Жизненный цикл: назначается в контенте → читается при выборе → гасится в resolve.

### `form_pulse`
- Назначение: скрытый числовой пульс формы/состояния матча-зеркала.
- Поля: `value` (float), `act_id`, last_updated_segment.
- Состояния: living value; не сериализуется в UI.
- Связи: модифицируется тегами и позже `soft_stats`.

### `pending_effect`
- Назначение: отложенный эффект.
- Поля: `effect_id`, `source_segment_id`, `target_beat_id`|`target_act_id`, `outcome_hint`, `weight`, `status` (`queued`|`fired`|`cancelled`).
- Жизненный цикл: queue → fire|cancel (trauma/load).

### `outcome_pack`
- Назначение: пачка исходов на один resolve.
- Поля: `pack_id`, `segment_id`, `items[]` (`outcome_type`, `score_delta_for`, `score_delta_against`, `boost_flag`), `realism_clamped` (bool).
- Состояния: `built` → `applied` | `cancelled`.

### `match_score`
- Назначение: счёт матча.
- Поля: `goals_for`, `goals_against`, started `0:0`.
- Связи: UI табло; endings signal.

## 9. Логика работы

1. На выборе: собрать теги (кроме чистого UI), применить немедленный вклад в `form_pulse` для `push_up`/`push_down`; `delay` → создать `pending_effect`; `twist` → пометить resolve как twist-capable; `arc_only` → не трогает пульс (только арки narrative).
2. На конце сегмента: `build_outcome_pack(segment)`.
3. Clamp пачки правилами реализма.
4. Отдать pack в `match_frame_ui` / presentation на match_beat.
5. На delayed target: fire pending → отдельный pack или merge в ближайший beat.
6. На trauma: cancel queued pack/pending for cut segment; score freeze unless trauma route later writes its own beats.
7. На ending gate: отдать `score_signal` = win/draw/loss + raw score; endings решают мягко.

Приоритеты конфликта:
1. trauma cancel  
2. author `forced_outcome`  
3. twist table  
4. pulse thresholds  
5. empty/neutral pack  

## 10. UI/UX
- Точек входа нет: фича сервисная.
- Игрок **не видит**: теги, пульс, pending, good/bad.
- Игрок **видит только**: матч-бит и изменение счёта/события на поле (через соседние фичи).
- Никаких тостов «выбор повысил форму».
- Save/Load: пульс, score, pending queue сохраняются вместе с кампанией.

## 11. Параметризация / Конфиги

| Параметр | Тип | Дефолт | Диапазон | Кто |
|---|---|---|---|---|
| `form_pulse_start` | float | 0 | -100..100 | автор |
| `push_up_weight_default` | float | 1 | 0..5 | автор |
| `push_down_weight_default` | float | 1 | 0..5 | автор |
| `goal_for_threshold` | float | 3 | 1..10 | автор |
| `goal_against_threshold` | float | -3 | -10..-1 | автор |
| `max_goals_events_per_half` | int | 3 | 1..5 | автор |
| `max_goals_for_match` | int | 5 | 2..8 | автор |
| `max_goals_against_match` | int | 5 | 2..8 | автор |
| `allow_empty_pack_default` | bool | true | bool | автор |
| `twist_can_invert_moral` | bool | true | bool | автор |
| `soft_stats_bias_enabled` | bool | false (MVP) / true (P1) | bool | автор |

## 12. Формулы, баланс-переменные и локализация

### 12.1 Формулы

#### `apply_choice_to_pulse`
- `form_pulse += weight` для `push_up`
- `form_pulse -= weight` для `push_down`
- `twist` / `delay` / `arc_only` сами по себе пульс не двигают (кроме forced author extras)
- P1: `form_pulse += soft_stats_bias()` если включено

#### `build_outcome_pack`
Логика (design-level):
1. Если `forced_outcome` на сегменте → pack из него (+ optional extras).
2. Иначе если активен `twist` roll/table → исход по twist (может инвертировать ожидание).
3. Иначе:
   - если `form_pulse >= goal_for_threshold` → добавить `goal_for`, затем `form_pulse -= goal_for_consume` (дефолт = threshold)
   - если `form_pulse <= goal_against_threshold` → добавить `goal_against`, затем частично «сбросить» пульс вверх
   - иначе по mid-band таблице: `dribble_win` / `dribble_lose` / `miss` / `ball_out` / empty
4. Можно добавить второй item в pack (например `boost_flag`), если авторский `pack_extra` или сильный |pulse|.
5. `clamp_realism(pack)`.

#### `clamp_realism`
- Не превышать `max_goals_events_per_half` для `goal_for`+`goal_against` событий.
- Не превышать `max_goals_for_match` / `max_goals_against_match`.
- Лишние goal-исходы даунгрейдить в `miss` / `dribble_win` / `ball_out`.
- Запрет абсурдных счетов из одного сегмента: максимум **1** `goal_for` и **1** `goal_against` в одной пачке (если нужно два — только author force + warning в контент-валидации).

#### `score_signal_for_endings`
- `diff = goals_for - goals_against`
- `win` если diff > 0; `draw` если 0; `loss` если < 0
- Передаётся в endings как **слабый** сигнал (вес низкий относительно арок / self-understanding flags).

#### Почему так, а не проще
- Пороги пульса дают читаемый agency без UI meter.
- Twist/delay сохраняют антиклише и «эффект позже».
- Clamp бережёт футбольное правдоподобие и тон.

### 12.2 Переменные баланса
| Имя | Тип | Дефолт | Диапазон | Риск | Зачем |
|---|---|---|---|---|---|
| `form_pulse_start` | float | 0 | -100..100 | старт смещён | калибр intro |
| `push_up_weight_default` | float | 1 | 0..5 | слишком быстро голы | темп |
| `push_down_weight_default` | float | 1 | 0..5 | спам пропусков | темп |
| `goal_for_threshold` | float | 3 | 1..10 | редко/часто голы | ощущение награды |
| `goal_against_threshold` | float | -3 | -10..-1 | то же | давление |
| `goal_for_consume` | float | 3 | 1..10 | пульс не сбрасывается | анти-снежок |
| `goal_against_recover` | float | 2 | 0..10 | застревание в минусе | анти-снежок |
| `max_goals_events_per_half` | int | 3 | 1..5 | нереализм | правдоподобие |
| `max_goals_for_match` | int | 5 | 2..8 | то же | правдоподобие |
| `max_goals_against_match` | int | 5 | 2..8 | то же | правдоподобие |
| `twist_invert_chance` | float | 0.0–1.0 per content | 0..1 | недоверие | антиклише (часто per-scene, не global) |
| `endings_score_weight` | float | 0.25 | 0..1 | счёт перебивает смысл | приоритет арок |

### 12.3 Ключи локализации
Namespace: `score_bridge`  
(тексты в основном для presentation/debug author; игрок обычно слышит комментарий из `match_presentation` / `audio_atmosphere`)

| Ключ | Текст RU (черновик) | Где |
|---|---|---|
| `score_bridge_goal_for` | Гол! | match event caption (optional) |
| `score_bridge_goal_against` | Мяч в наших воротах… | optional |
| `score_bridge_dribble_win` | Обводка удалась. | optional |
| `score_bridge_dribble_lose` | Соперник убрал мяч. | optional |
| `score_bridge_miss` | Промах. | optional |
| `score_bridge_ball_out` | Мяч уходит за линию. | optional |
| `score_bridge_boost_felt` | Что-то щёлкнуло внутри. | optional, rare; лучше через narrative, не UI |

Игроку не показываем ключи про пульс/теги.

## 13. Контекстные системы
- `narrative_core`: вызывает resolve на границах сегмента; владеет half_end.
- `characters_dialogue`: источник тегов на выборах.
- `match_frame_ui`: применяет score deltas, показывает табло.
- `match_presentation`: играет beat под outcome type.
- `soft_stats` (P1): bias на пульс.
- `endings_system`: потребляет слабый score_signal.
- `trauma_system`: cancel pending/pack.
- `save_anytime`: persist pulse/score/pending.

## 14. Аналитика (лёгкая)
| event | params |
|---|---|
| `score_bridge_resolve` | `segment_id`, `pack_size`, `outcomes[]`, `pulse_before`, `pulse_after` |
| `score_bridge_delay_queue` | `effect_id`, `target` |
| `score_bridge_delay_fire` | `effect_id` |
| `score_bridge_twist` | `segment_id`, `outcome` |
| `score_bridge_clamp` | `dropped_or_downgraded` |
| `score_bridge_trauma_cancel` | `segment_id` |

Срезы: доля empty packs; средние goals/half; частота twist; корреляция «игрок ожидал гол / получил пропуск».

## 15. Edge Cases
- Нет тегов на сегменте → empty pack или author default miss/ball_out.
- Load mid-campaign → restore pulse/score/pending exactly.
- Два delayed на один beat → merge по приоритету author > goal > other; realism clamp after merge.
- Soft stats выключены → bias = 0.
- Параллельный arc_only выбор → не меняет счёт.
- Intro на поле: score 0:0 до первого resolve.

## 16. Риски и митигации
- Счёт кажется рандомом → митигация: устойчивые пороги; плейтест на «я понимаю связь»; не злоупотреблять twist.
- Счёт кажется моральной кассой → митигация: нет good/bad UI; разрешены инверсии.
- Слишком много голов → clamp реализма.
- Игрок игнорирует матч → короткие beats, сильные presentation hooks (соседняя фича).
- Счёт перебивает тему самопознания → низкий `endings_score_weight`.

## 17. Acceptance Criteria / QA
1. Старт на поле всегда 0:0.
2. После сегмента с `push_up`×N при пороге возникает `goal_for` (без UI-подсказки на выборе).
3. Сегмент может выдать пачку ≥2 items (контент-фикстура).
4. `delay` не меняет текущий бит, стреляет позже.
5. `twist` может выдать исход против «ожидаемой морали».
6. Игрок нигде не видит form_pulse / теги / good-bad.
7. Исходы видны только в матч-слое.
8. Clamp не даёт превысить per-half/per-match потолки.
9. Trauma cancel сбрасывает pending pack сегмента.
10. В endings уходит win/draw/loss + raw score как слабый сигнал.

Smoke: resolve goal_for; goal_against; empty; pack; delay fire; twist invert; clamp; save/load pulse; trauma cancel.

## 18. План релиза
- Сначала в вертикальном срезе друзьям вместе с 1–2 сегментами и табло.
- Затем полная кампания в premium-коробке.
- Тогглы: `twist_can_invert_moral`, `soft_stats_bias_enabled`.
- Почему: мост надо калибровать на живых людях до объёма текста.

## 19. Пострелизный анализ
- Успех: связывают решения и матч; есть облегчение/напряжение; счета разные; твисты обсуждают, но не злят массово.
- Провал: «рандом» / «моральный meter» / «матч бесит».
- v2: тонкие теги, таблицы по актам, лучший bias soft_stats.

---

## Контекстные блоки
- **Прогрессия/глубина:** скрытый пульс + delayed/накопление.
- **Контентная структура:** теги на выборах, pack на сегмент.
- **Нарратив:** антиклише через twist; feedback только через матч.
- **Premium:** ценность — читаемые последствия в коробке, без pay shortcuts.
- **F2P-монетизация:** не применяется.

## Rationale gate
| Решение | Почему |
|---|---|
| Резолв после сегмента / delayed | Меньше прыжков; эффект может созреть |
| Скрытый пульс | Agency без моральной кассы в UI |
| Пачка исходов | Богаче, чем только гол/пропуск |
| Нечестные твисты ок | Ломаем клише |
| Счёт слаб для endings | Главное — понимание себя |
| Feedback только в матче | Чище атмосфера |
| 0:0 со старта выхода | Честная рамка матча |
| Реализм-клоэмпы | Не убить правдоподобие футбола |
