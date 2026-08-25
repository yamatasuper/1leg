# ТЗ: trauma_system — Травмы

## 0. Паспорт документа
- Название фичи: Травмы
- ID / кодовое имя: `trauma_system`
- Проект / версия: **90 минут** / P1
- Фичеовнер: автор (соло)
- Статус: draft
- Дата создания: 2026-08-12
- Дата обновления: 2026-08-12
- Связанные документы:
  - `docs/project-concept.md`
  - `docs/feature-index.md`
  - `docs/2026-08-12_narrative_core_gd-spec.md`
  - `docs/2026-08-12_choice_score_bridge_gd-spec.md`
  - `docs/2026-08-12_soft_stats_gd-spec.md`
  - `docs/2026-08-12_characters_dialogue_gd-spec.md`
  - `docs/2026-08-12_match_presentation_gd-spec.md`
- История изменений:
  - 2026-08-12 — первый draft

## 1. Саммари фичи
- Что это: система **максимум одной** травмы за прохождение. Травма может оборвать матч, но не «убивает» смысл жизни героя: уводит в **отдельный ending-route** с 1–2 сценами после обрыва. Перед этим — намёки (врач команды, люди). Триггер связан с уходом soft stats в минус; после точки невозврата травма почти неизбежна; остаётся **маленький шанс** всё же получить травма-прохождение даже вне «идеального» коллапса. Сейв до травмы = простой полный откат.
- Для кого: игрок, для которого травма — резкий поворот судьбы, а не game over.
- Проблема: нужен сильный риск без ощущения несправедливого наказания без agency.
- Эффект: foreshadowing → напряжение → cut → короткий хвост сцен → отдельная концовка; load даёт второй шанс.

## 2. Бизнес-контекст
- Почему P1: нужна после стабильных soft_stats + narrative routes.
- Альтернативы: травма только скриптовый QTE; травма = bad ending без ветки; много травм за run.
- Почему так: одна травма сохраняет вес; намёки честны; отдельный ending расширяет реиграбельность; откат сейва сохраняет agency.

## 3. Цели
- Главная: дать отдельный травма-route с обрезанием остатка обычного сюжета.
- Вторичные: warning pipeline; PoNR; интеграция с soft stats; безопасный load.
- Не цели: перманентный softlock; спам травм; скрытый обрыв без намёков; показ numeric «травмаметра» как обязательный HUD.
- Почему: «травма останавливает матч, не жизнь».

## 4. Метрики успеха
- Основная: плейтестеры понимают связь «я просел → мне намекали → случилось» и не считают это рандомом.
- Guardrail: не бесят; load до травмы работает; обычные концовки остаются достижимы без травмы.
- Провал: внезапный cut без foreshadow; ощущение «игра наказывает»; сломанный save.

## 5. Позиционирование
- Слушает `soft_stats` bands/values.
- Режет `narrative_core` segment flow (`skipped_by_trauma`).
- Отменяет pending у `choice_score_bridge`.
- Freeze soft_stats snapshot.
- Уводит в отдельный route `endings_system` (trauma tier).
- Presentation: короткий «обрыв страницы» optional.
- Save/Load: pre-trauma anchors must exist.

## 6. Scope

### In (P1)
- Hard cap: **`trauma_count <= 1`** за campaign run.
- Trigger model:
  - primary: soft stats уходят в минус (коллапс — см. формулы);
  - warnings от врача/NPC до PoNR;
  - **point of no return** после серии намёков + сохраняющегося минуса;
  - **residual small chance** на вход в травма-игру, если герой в danger-zone, но ещё не полный коллапс (чтобы «маленький шанс на игру с травмой» оставался).
- Warning content: dialogue/examine-free hints via characters (team doctor, close people).
- On fire:
  - cancel bridge pending pack;
  - mark active segment `skipped_by_trauma` if mid-segment;
  - suspend world; optional torn-page presentation;
  - freeze soft_stats;
  - jump to trauma aftermath (1–2 scenes) → **separate trauma ending**.
- Avoidability: до PoNR можно выправиться (поднять статы / правильные выборы) и уйти в обычные ending tiers.
- Save: load до травмы = **простой откат** всего состояния сейва (не частичный patch флага).

### Out
- Множественные травмы за run.
- Обязательная травма в каждом прохождении.
- Numeric trauma HUD для игрока.
- Trauma как мгновенный game over без сцен.

### Future
- Вариации травма-сцен по акту (ранняя/поздняя) с разным skip объёмом.

### Зависимости
- `soft_stats`, `narrative_core`, `choice_score_bridge`, `characters_dialogue`, `endings_system`, `save_anytime`, `match_presentation` (micro cut), `post_match_interview` обычно bypass в пользу trauma scenes.

## 7. Use Cases

### 7.1 Warning phase
- Статы проседают → unlock warning dialogues (doctor/people).
- Journal/goals могут добавить «провериться / поговорить».
- Игрок ещё может восстановиться.

### 7.2 Point of no return
- Условия PoNR достигнуты → `trauma_locked_in≈true` (кроме редкого author mercy, default off).
- Ближайший валидный fire window запускает травму (конец сегмента / mid scripted beat / после матч-бита — author table).

### 7.3 Residual small chance
- В danger-zone, но не full collapse: каждый check window roll `trauma_residual_chance` (низкий).
- Если proc и cap позволяет → входим в травма-route (с теми же aftermath сценами).
- Намёки к этому моменту уже должны были прозвучать хотя бы раз (если нет — сначала force warning beat, отложить proc).

### 7.4 Fire + aftermath
- Match stops as life-story continues.
- 1–2 сцены (больница/дом/разговор) → trauma ending.

### 7.5 Load до травмы
- Игрок грузит сейв → полный rollback: статы, сюжет, score, flags, позиция — как в сейве.
- `trauma_triggered` снова false, если сейв был до fire.

## 8. Сущности

### `trauma_state`
- `warnings_seen` (int / set ids)
- `ponr_reached` (bool)
- `trauma_triggered` (bool)
- `trauma_count` (0..1)
- `danger_zone` (bool)
- `fire_act_id` / `fire_segment_id`
- `residual_rolls_done`

### `trauma_warning`
- `warning_id`, `source_character_id`, `requirements`, `dialogue_id`, `sets_flag`

### `trauma_fire_window`
- `window_id`, `when` (`segment_end|mid_segment|after_beat|script`), `allowed_acts[]`

### `trauma_aftermath_pipeline`
- ordered `scene_ids` (1–2) → `ending_route_id = trauma`

## 9. Логика

### Danger / collapse
Let neg count = number of stats among {morale, energy, strength, focus} that are `< 0`, plus pain/anxiety that are `> 0` (вредные в плюсе).

- `danger_zone` if: morale < 0 AND at least 2 other «bad» conditions (neg utility or high pain/anxiety).
- `collapse` if: morale < 0 AND energy < 0 AND (strength < 0 OR focus < 0) AND (pain > 0 OR anxiety > 0)  
  (тюнинг допустим; смысл — «мораль и всё остальное в минус»).

### Pipeline
1. Each soft_stats delta / segment boundary → recompute danger/collapse.
2. If danger and warnings missing → prioritize offering warning dialogue soon (narrative hook).
3. If collapse sustained across `ponr_sustain_segments` (default 1–2) after ≥`warnings_required` → `ponr_reached=true`.
4. If `ponr_reached` and `trauma_count==0` → schedule fire at next allowed window.
5. Else if danger and not ponr and `trauma_count==0` → rare residual roll.
6. On fire: set triggered; cancel bridge pending; skip segment if needed; freeze stats; run aftermath; ending trauma.
7. Never fire twice.

Priority vs other systems:
1. trauma fire (once)
2. normal narrative next
3. residual chance only if not already scheduled

## 10. UI/UX
- Нет отдельного trauma meter.
- Намёки — через диалоги/текст мира.
- Cut: optional match_pres micro-template «страница обрывается».
- Save UI: без спецкнопки; игроку в FTUE/паузе можно мягко напомнить, что сохраняться стоит (не морализаторство).
- Grey options may reflect low state (already soft_stats/dialogue).

## 11. Параметры

| Параметр | Дефолт | Диапазон | Смысл |
|---|---|---|---|
| `max_trauma_per_run` | 1 | fixed | |
| `warnings_required` | 2 | 1–3 | до PoNR |
| `ponr_sustain_segments` | 1 | 1–2 | минус держится |
| `trauma_residual_chance` | 0.05 | 0.02–0.08 | малый шанс |
| `residual_requires_prior_warning` | true | | |
| `aftermath_scene_count` | 2 | 1–2 | |
| `cancel_bridge_on_fire` | true | | |
| `freeze_soft_stats_on_fire` | true | | |
| `load_is_full_rollback` | true | fixed | |

## 12. Формулы / баланс / локализация

### 12.1 Формулы

#### `eval_danger(state)` / `eval_collapse(state)`
- см. логику выше

#### `maybe_residual_trauma()`
- if trauma_count>0 or ponr scheduled: no
- if not danger_zone: no
- if residual_requires_prior_warning and warnings_seen<1: queue warning instead
- else if random() < trauma_residual_chance: schedule fire

#### `fire_trauma(window)`
- assert trauma_count==0
- trauma_triggered=true; trauma_count=1
- bridge.cancel_pending()
- soft_stats.freeze()
- narrative.cut_to(aftermath)

### 12.2 Balance vars
| name | default | risk | why |
|---|---|---|---|
| `trauma_residual_chance` | 0.05 | «рандомная жестокость» | держать низко + warnings |
| `warnings_required` | 2 | слишком внезапно / слишком долго | честность |
| `ponr_sustain_segments` | 1 | | |
| `collapse_morale_must_negative` | true | | по ТЗ |
| `mercy_disable_residual` | false | | author toggle |

### 12.3 Localization
Namespace: `trauma`

| key | RU draft | where |
|---|---|---|
| `trauma_warn_doctor_01` | Тебе бы не геройствовать. Тело уже орёт. | doctor dlg |
| `trauma_warn_people_01` | Мы видим, что ты себя ломаешь. | npc dlg |
| `trauma_cut_caption` | Свисток. Не тот. | presentation |
| `trauma_aftermath_beat_01` | Матч закончился раньше жизни. | scene |
| `trauma_ending_title` | Травма | ending card |
| `trauma_journal_hint` | Поговорить с врачом / с теми, кто рядом | journal |

## 13. Контекстные системы
- `soft_stats`: danger/collapse input; freeze on fire.
- `narrative_core`: skip remainder; aftermath pipeline; segment state.
- `choice_score_bridge`: cancel pending; no forced goal on cut.
- `characters_dialogue`: warning conversations.
- `endings_system`: separate `ending_trauma` tier (не просто bad).
- `save_anytime`: must allow pre-trauma saves; load = full rollback.
- `match_presentation`: optional rupture page.
- `match_frame_ui`: hide/freeze on cut.

## 14. Аналитика
| event | params |
|---|---|
| `trauma_warning_seen` | `warning_id` |
| `trauma_danger_enter` | `stats_snapshot` |
| `trauma_ponr` | `act_id` |
| `trauma_residual_roll` | `success` |
| `trauma_fire` | `window`, `segment_id`, `cause` (`collapse`|`residual`|`script`) |
| `trauma_ending` | `route_id` |

## 15. Edge Cases
- Warning ignored forever but collapse true → PoNR after sustain; still had chance to see warnings if available — if content missing, force one emergency warning scene before fire.
- Trauma mid-dialogue → abort dialogue like trauma mid-segment.
- Residual proc during presentation → delay fire until beat ends (don’t softlock comic), then cut.
- Second collapse after load → can trauma again only if previous fire not in this run timeline (load before fire allows again).
- Good ending stats after PoNR: default still fire (PoNR means locked); author mercy flag only if explicitly enabled.

## 16. Риски
- Кажется жестоким рандомом → residual низкий + обязательные warnings.
- Кажется неизбежным наказанием → явная возможность восстановления до PoNR.
- Skip большого сюжета злит → 1–2 качественные aftermath сцены + сильный ending; предупредить весом намёков.
- Save scum ожидаем → ok для premium singleplayer agency.

## 17. Acceptance Criteria
1. Не больше одной травмы за run.
2. Collapse path требует негативную мораль + остальные в минус/вред по правилам.
3. До fire показаны намёки (doctor/people) — минимум `warnings_required` на collapse path.
4. Существует PoNR после sustained collapse.
5. Residual chance низкий и не стреляет без prior warning (если флаг on).
6. Fire отменяет bridge pending, skip’ает сегмент при mid, freeze soft_stats.
7. Aftermath 1–2 сцены → отдельный trauma ending.
8. Load сейва до травмы полностью откатывает run state.
9. Обычные good/mid/bad достижимы без травмы при восстановлении до PoNR.

Smoke: warning→recover→no trauma; warning→collapse→PoNR→fire→aftermath→ending; residual rare proc; load rollback; bridge cancel.

## 18. Релиз
- После soft_stats P1 и базовых endings.
- Срез: 1 warning + forced debug fire + 1 aftermath.
- Полностью: 2+ warnings, PoNR, residual, ending.

## 19. Пострелиз
- Успех: травму обсуждают как сильный route; load используют осознанно; не ненавидят систему.
- Провал: «рандом», «не дали шанса», «обрезали сюжет зря».
- v2: ранняя/поздняя вариации aftermath.

---

## Контекстные блоки
- Нарратив: отдельный ending + foreshadow
- Прогрессия риска через soft_stats
- Save/agency
- Premium: один сильный route в коробке
- Антиклише: травма не обязательный sad-fail без ветки жизни

## Rationale gate
| Решение | Почему |
|---|---|
| Max 1 | Вес события |
| Триггер через минус статов | Связь с формой/жизнью |
| Намёки врача/людей | Честность, не удар из темноты |
| PoNR | Драматургия неизбежности после игнора |
| Малый residual шанс | «Игра с травмой» остаётся возможной |
| 1–2 сцены после | Матч кончился — жизнь нет |
| Отдельный ending | Не схлопывать в просто bad |
| Load = полный откат | Простой и честный agency |
