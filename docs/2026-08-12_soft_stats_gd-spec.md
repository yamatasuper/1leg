# ТЗ: soft_stats — Временные бусты / скрытое состояние

## 0. Паспорт документа
- Название фичи: Временные бусты (скрытое состояние)
- ID / кодовое имя: `soft_stats`
- Проект / версия: **90 минут** / P1 (контракты гейтов желательно учитывать в MVP-диалогах на stub-значениях)
- Фичеовнер: автор (соло)
- Статус: draft
- Дата создания: 2026-08-12
- Дата обновления: 2026-08-12
- Связанные документы:
  - `docs/project-concept.md`
  - `docs/feature-index.md`
  - `docs/2026-08-12_choice_score_bridge_gd-spec.md`
  - `docs/2026-08-12_characters_dialogue_gd-spec.md`
  - `docs/2026-08-12_narrative_core_gd-spec.md`
  - `docs/2026-08-18_story_content_gd-spec.md` — канон статов полной игры; этот ТЗ не использовать для новых текстов
- История изменений:
  - 2026-08-12 — первый draft
  - 2026-08-18 — superseded для контента (ФВ/ХЛ/СК/ИН + скрытые)

## 1. Саммари фичи
- Что это: скрытый набор состояний героя на время матча-кампании: `morale`, `energy`, `strength`, `focus`, `pain`, `anxiety`. Они слегка смещают пульс формы для счёта, гейтят диалоговые пулы и помогают собрать **нарративные итоги** в послематчевом интервью.
- Для кого: система для автора/моста/диалогов; игрок видит состояние **только в меню/журнале** (в духе мыслей Disco), не как постоянный HUD-кассу на мире/выборах.
- Проблема: нужна «+мотивация как в жизни» без прокачки на каждом экране и без good/bad на репликах.
- Эффект: речь и матч чуть дышат состоянием; всегда остаётся минимальный диалоговый путь; интервью подводит итог; в журнале можно осознать свой «внутренний лист».

## 2. Бизнес-контекст
- Почему P1: вертикальный срез живёт на stub; полная калибровка после кора.
- Альтернативы: явные бары; только флаги без чисел; сильное влияние на голы.
- Почему скрытые + лёгкий bias: сохраняет атмосферу Disco/VN и антиклише; счёт не становится прокачкой.

## 3. Цели
- Главная: невидимое состояние, которое мягко связывает жизнь, речь и матч.
- Вторичные: источники из выборов/тренировки/перерыва/битов; итоги в интервью.
- Не цели: держать статы вечным overlay на мире/в диалогах; давать soft stats в одиночку форсить голы; отрезать все диалоговые пути в минусе.
- Почему: понимание себя > победа; mid-band agency обязателен.

## 4. Метрики успеха
- Основная: плейтестеры чувствуют «мне легче/тяжелее говорить и играть», не называя «статы».
- Guardrail: статы не торчат поверх мира/реплик; голы не объясняются «я накачал strength»; grey-опции редки, но понятны.
- Провал: min-max через журнал ломает атмосферу; или статы вообще незаметны даже в меню.

## 5. Позиционирование
- Питает: `choice_score_bridge` (слабый bias), `characters_dialogue` (requirements), `post_match_interview` / `endings_system` (summary lines).
- Питается: dialogue effects, training/half-time scripts, match beat outcomes, rare narrative events.
- На травме: матч-ветка soft_stats больше не ведёт основной сюжет — управление у `trauma_system` (состояние можно заморозить для логов/итогов травма-ветки).

## 6. Scope

### In (P1; контракт частично с MVP)
- Статы (float):
  - `morale` — мораль / смелость речи
  - `energy` — ресурс продолжать
  - `strength` — телесная «жёсткость»/напор
  - `focus` — собранность
  - `pain` — боль (обычно растёт во вред)
  - `anxiety` — тревога (обычно растёт во вред)
- Lifetime: **весь матч** (от тренировки/старта матч-lifetime до конца интервью).
- Источники изменения: выборы, тренировка, перерыв, матч-биты (и авторские скрипты).
- Влияние на bridge: **только слегка** (`soft_stats_bias`), не самостоятельный push через порог гола.
- Диалоги: гейты bold/low/focus и т.д.; **всегда** ≥1 mid-band доступный путь; на выборе **нет** numeric overlay.
- Видимость: вкладка журнала/меню **«Состояние»** в эстетике мыслей Disco (название, короткий текст band, допустимы числа в этом меню).
- Интервью: итоги **текстом** (не spreadsheet); может опираться на bands.
- Save/Load: persist на весь run.

### Out
- Видимые бары/цифры/иконки статов.
- Билдперки, дерево прокачки, экипировка.
- Сильный auto-goal от одного стата.
- Полная симуляция усталости каждую секунду realtime.

### Future
- Больше нюансов итогов; опциональные rare thoughts VA, завязанные на extreme stats.

### Зависимости
- `choice_score_bridge` — bias API
- `characters_dialogue` — requirements
- `narrative_core` — act lifetime, training/half-time hooks
- `post_match_interview` / `endings_system` — summary
- `trauma_system` — branch cut
- `match_presentation` — optional flavor only (без цифр)

## 7. Use Cases

### 7.1 Выбор повышает morale
- Effect `add_stat(morale, +X)` скрыто.
- Позже bold options открываются; bridge bias чуть вверх.

### 7.2 Тяжёлый бит / пропуск
- `anxiety++`, `morale--` и т.п. по таблице outcome→stats.
- Диалоги смещаются к low/heavy pool; mid-band остаётся.

### 7.3 Тренировка / перерыв
- Авторские пакеты дельт (дыхание, конфликт, тишина).

### 7.4 Травма
- Срабатывает trauma route; soft_stats матча **не продолжают** обычный цикл сегмент→гол как главный драйвер.
- Значения freeze в snapshot для возможных травма-итогов.

### 7.5 Финальное интервью
- Система читает score_signal + arcs + soft_stats bands.
- Показывает **итоги игры текстом** (и счётом матча через frame/narrative), без stat sheet.

## 8. Сущности

### `soft_stat_id`
- enum: `morale|energy|strength|focus|pain|anxiety`

### `soft_stats_state`
- map id→value; `match_id`; `frozen` bool; `snapshot_at_trauma` nullable

### `stat_delta_event`
- source (`dialogue|training|half_time|match_beat|script`), deltas{}, reason_key (author only)

### `stat_band`
- `very_low|low|mid|high|very_high` derived from thresholds

### `interview_summary_rule`
- conditions on bands + score + arcs → `line_keys[]`

## 9. Логика
1. Init on match start: defaults (обычно mid ~0).
2. Apply deltas clamped to `[stat_min, stat_max]`.
3. On bridge resolve: `bias = soft_stats_bias(state)` added to form_pulse **lightly**.
4. On dialogue req: evaluate bands/thresholds; never lock all mid options of a node.
5. On trauma: `frozen=true`, store snapshot, stop normal match feed.
6. On interview: evaluate summary rules → lines; still no numeric HUD.
7. On match end after interview: state can archive with save.

Bias principle:
- soft stats alone should not cross a goal threshold from a neutral pulse in one tick without prior choice tags; they only nudge.

## 10. UI/UX
- **Нет постоянного HUD** статов на мире/в диалогах.
- **Есть** меню/журнал «Состояние» (Disco-thoughts vibe): список статов с короткими описаниями band; числа допустимы здесь.
- Косвенно: grey hints в диалогах; тон реплик; rare presentation flavor; interview prose.
- Debug/author overlay — только editor/dev builds.
- CTA: открыть журнал/состояние.

## 11. Параметры

| Параметр | Дефолт | Диапазон | Смысл |
|---|---|---|---|
| `stat_min` | -10 | | |
| `stat_max` | 10 | | |
| `stat_default` | 0 | | |
| `bias_scale` | 0.25 | 0.1–0.4 | лёгкий nudge в pulse |
| `bias_cap_abs` | 1.0 | 0.5–2.0 | макс |bias| за resolve |
| `band_low` | -3 | | |
| `band_high` | 3 | | |
| `ensure_mid_dialogue_path` | true | fixed | |
| `show_stats_to_player` | true (menu only) | fixed | |
| `freeze_on_trauma` | true | | |
| `use_in_interview_summary` | true | | |

## 12. Формулы / баланс / локализация

### 12.1 Формулы

#### `clamp_stat(v)`
- `min(stat_max, max(stat_min, v))`

#### `band(v)`
- very_low < -6; low < band_low; high > band_high; very_high > 6; else mid  
  (пороги тюнятся)

#### `soft_stats_bias(state)`
```
raw = bias_scale * (
  w_morale*morale + w_energy*energy + w_strength*strength + w_focus*focus
  - w_pain*pain - w_anxiety*anxiety
)
return clamp(raw, -bias_cap_abs, +bias_cap_abs)
```
Дефолт весов: morale 0.35, energy 0.2, strength 0.15, focus 0.15, pain 0.25, anxiety 0.3 (сумма не обязана 1; это weighted terms).

#### `dialogue_min_path_guard(node)`
- after evaluating locks, if zero available choices → force-enable authored `fallback_mid` choice (content error if missing)

#### `interview_pick_lines(state, score_signal, arcs)`
- match rules by priority; pick 2–5 line keys as «итоги»

Почему лёгкий bias: гол остаётся за выбором/тегами; статы — дыхание формы.

### 12.2 Balance vars
| name | default | risk | why |
|---|---|---|---|
| `bias_scale` | 0.25 | слишком сильный счёт | лёгкость |
| `bias_cap_abs` | 1.0 | снежный ком | потолок |
| `w_morale` | 0.35 | | речь+форма |
| `w_energy` | 0.2 | | |
| `w_strength` | 0.15 | | |
| `w_focus` | 0.15 | | |
| `w_pain` | 0.25 | | вред |
| `w_anxiety` | 0.3 | | вред |
| `delta_choice_small` | 1 | шум | |
| `delta_choice_large` | 2 | резкость | |
| `delta_goal_for` | +1 morale / -1 anxiety | optional table | |
| `delta_goal_against` | -1 morale / +1 anxiety / +1 pain | | |

### 12.3 Localization
Namespace: `soft`

Игрок почти не видит ключи статов. Нужны для итогов/hints:

| key | RU draft | where |
|---|---|---|
| `soft_hint_low_morale` | Сейчас не до смелости. | dlg grey (shared) |
| `soft_hint_high_anxiety` | Тревога не отпускает. | dlg grey |
| `soft_hint_need_focus` | Мысли рассеяны. | dlg grey |
| `soft_sum_morale_high` | Ты вышел говорить и бить без оглядки. | interview |
| `soft_sum_morale_low` | Ты дотянул матч на остатках воли. | interview |
| `soft_sum_pain_high` | Тело помнит каждый удар. | interview |
| `soft_sum_anxiety_high` | Страх будущего был рядом всё время. | interview |
| `soft_sum_focus_high` | Ты видел момент и не отвёл взгляд. | interview |
| `soft_sum_energy_low` | К финалу сил почти не осталось. | interview |

Числа статов допустимы **только** во вкладке «Состояние»; на мире/в репликах — нет.

## 13. Контекстные системы
- `choice_score_bridge`: `soft_stats_bias_enabled` (P1 on).
- `characters_dialogue`: requirements + mid-path guard.
- `pre_match_training` / `half_time`: delta packages.
- `match_presentation`: optional non-numeric flavor only.
- `post_match_interview`: summary lines.
- `endings_system`: may read bands as weak signals alongside arcs/score.
- `trauma_system`: freeze + branch.

## 14. Аналитика
| event | params |
|---|---|
| `soft_delta` | `source`, `deltas`, `values_after` |
| `soft_bias_applied` | `bias`, `pulse_before`, `pulse_after` |
| `soft_freeze_trauma` | `snapshot` |
| `soft_interview_summary` | `line_keys[]` |

Не логировать в player-facing UI.

## 15. Edge Cases
- Stub MVP: all zeros → mid band; bold/low special lines carefully authored or inactive.
- Cap spam deltas in one segment via author budgets (optional `max_abs_delta_per_segment`).
- Trauma then load pre-trauma → unfreeze restored state.
- Interview without soft feature build → summary uses score+arcs only.
- Negative extremes: grey some lines, never softlock node.

## 16. Риски
- Игрок всё равно min-max’ит через серые подсказки → hints человеческие, не «нужно +2 strength».
- Bias слишком сильный → cap + scale.
- Незаметность → калибровать на плейтесте dialogue tone + interview lines.
- Рассинхрон с P1 сроком → stub defaults early.

## 17. Acceptance Criteria
1. Шесть статов существуют; видны в меню «Состояние», не как world HUD.
2. Живут весь матч до интервью включительно (для summary).
3. Дельты приходят из choices/training/half-time/match beats.
4. `|bias| ≤ bias_cap_abs`; один bias-тик не форсит гол с нулевого пульса без тегов.
5. Диалоговый узел всегда имеет ≥1 доступный mid path.
6. Trauma freeze + передача ветки; обычный match-loop soft feed останавливается.
7. Интервью показывает итоги текстом, без numeric sheet.
8. Save/load сохраняет values/frozen.

Smoke: delta→bias nudge; bold gate open/close; mid path guard; trauma freeze; interview lines pick; save/load.

## 18. Релиз
- MVP stub: zeros + API.
- P1: полные веса, таблицы outcome→delta, interview rules.
- Калибровка после среза с диалогами/мостом.

## 19. Пострелиз
- Успех: чувствуют состояние без sheet; итоги интервью резонируют.
- Провал: «где статы?» / «это прокачка голов» / softlock речи.
- v2: более тонкие summary; rare thought VA on extremes.

---

## Контекстные блоки
- Прогрессия без билдов
- Связь матч↔диалог
- Premium: глубина состояния в коробке
- Нарратив итогов в интервью
- F2P: N/A

## Rationale gate
| Решение | Почему |
|---|---|
| Статы только в меню-мыслях | Атмосфера на мире + осознанность в журнале |
| Расширенный набор | Богаче речь и итоги |
| Весь матч | Одна дуга формы |
| Все источники | Жизнь+ритуал матча |
| Лёгкий bias | Счёт за выборы |
| Mid-path всегда | Agency |
| Trauma → своя ветка | Уже зафиксировано в кора |
| Итоги в интервью текстом | Показать результат без sheet |
