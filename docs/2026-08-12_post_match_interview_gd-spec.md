# ТЗ: post_match_interview — Послематчевое интервью

## 0. Паспорт документа
- Название фичи: Послематчевое интервью
- ID / кодовое имя: `post_match_interview`
- Проект / версия: **90 минут** / P1
- Фичеовнер: автор (соло)
- Статус: draft
- Дата создания: 2026-08-12
- Дата обновления: 2026-08-12
- Связанные документы:
  - `docs/project-concept.md`
  - `docs/feature-index.md`
  - `docs/2026-08-12_narrative_core_gd-spec.md`
  - `docs/2026-08-12_soft_stats_gd-spec.md`
  - `docs/2026-08-12_characters_dialogue_gd-spec.md`
  - `docs/2026-08-12_trauma_system_gd-spec.md`
  - `docs/2026-08-12_choice_score_bridge_gd-spec.md`
  - `docs/2026-08-12_endings_system_gd-spec.md`
  - `docs/2026-08-18_story_content_gd-spec.md` — канон финала: эпилог (квартира / письмо / река), **не пресс-конференция**
- История изменений:
  - 2026-08-12 — первый draft
  - 2026-08-18 — контент интервью с журналистом снят; использовать эпилог библии

## 1. Саммари фичи
- Что это: после свистка — внутренний голос и эпилог (письмо, река), не диалог с журналистом. Выборы не меняют ending.
- Для кого: игрок на закрытии арки.
- Проблема: нужен ритуал осмысления после свистка, без переигрывания кампании в последней сцене.
- Эффект: облегчение/тяжесть финала; ending читается через слова, затем переход к ending card/credits.

## 2. Бизнес-контекст
- Почему P1: после endings + soft_stats summary rules.
- Альтернативы: сразу ending card; интерактивный финал, меняющий route; показ счёта крупно.
- Почему речь-only + locked ending: финал про смысл, не про last-second min-max; счёт уже прожит в матче.

## 3. Цели
- Главная: закрыть кампанию через прессу + монолог за ~5 минут.
- Вторичные: проявить итоги текстом; дать выборы тона без смены route.
- Не цели: скип; смена ending; UI табло/стат-sheet в кадре интервью.
- Почему: ending уже следствие матча/арок/травмы.

## 4. Метрики успеха
- Основная: интервью ощущается осмысленным финалом, не «меню результатов».
- Guardrail: ~5 мин; понятно, что выборы — про то, *как говоришь*, не про *чем закончится*.
- Провал: игроки думают, что последним выбором ломают концовку; или скучный пресс-клише без связи с run.

## 5. Позиционирование
- Акт после second_half (normal path).
- **Trauma path:** обычно **bypass** — trauma aftermath + trauma ending вместо этого акта (или урезанная special version только если author явно включит; default off).
- Читает: `ending_route_id` (уже выбран), score_signal, soft bands, key flags/arcs.
- Пишет: cosmetic tone flags optional; затем hands off to `endings_system` presentational finale.
- Не пишет: новый ending id.

## 6. Scope

### In (P1)
- Pipeline из нескольких коротких сегментов, минимум:
  1. пресс-диалог (журналист ↔ герой) с ≥3 вариантами на ключевых узлах;
  2. монолог / тишина / мысль героя (линейно или с cosmetic choices).
- Опционально третий микро-бит (уход из микс-зоны) — если укладывается в 5 мин.
- Ending **locked** before act start (`endings_system.resolve` на входе или в конце 2-го тайма).
- Выбор реплик влияет на wording / journalist reaction tone / optional epilogue flavor line — **не** на route.
- Только речь: бабблы/портреты/комикс-кадр студии или коридора; **без** match_frame табло в кадре.
- No skip.
- Soft summary lines вшиты в вопросы/ответы/монолог (из `soft_stats` interview rules).

### Out
- Skip.
- Last-chance ending rewrite.
- Scoreboard / stats spreadsheet on interview screen.
- Long walkable hub.
- QTE.

### Future
- NG+ journalist remembering prior runs (meta) — осторожно.

### Зависимости
- `endings_system` (route already set)
- `soft_stats` (summary line picks)
- `characters_dialogue` (press dialogue graph)
- `narrative_core` (act)
- `choice_score_bridge` / score (read-only for line variants, not for re-resolve ending here)
- `trauma_system` (bypass)

## 7. Use Cases

### 7.1 Normal clear
- Full time → resolve ending route silently → interview act.
- Press questions branch visually by win/draw/loss + arcs, but all lead to same route.
- Player picks tones (honest/evade/sharp…).
- Monologue reflects soft bands + route.
- → ending presentation.

### 7.2 Player expects to change ending
- UI/copy must not promise “decide your fate now”.
- Optional subtle continuity: journalist “понял вас” but route stable.

### 7.3 Trauma
- Skip interview act; trauma pipeline owns finale.

## 8. Сущности

### `interview_act_state`
- `ending_route_id` (locked), `segments_done[]`, `tone_flags[]`, `completed`

### `interview_segment`
- type `press_dialogue` | `monologue` | `bridge_beat`
- `graph_id` / `line_keys`
- `variant_requirements` (score/arcs/soft) for **line pools only**

### `cosmetic_choice`
- like dialogue choice but effects limited to `tone_flags` / local reactions

## 9. Логика
1. On act enter: assert `ending_route_id != null`; if null → emergency resolve from endings API.
2. Build segment playlist (~5 min budget).
3. Press graph: choices allowed; filter effects — strip any ending-changing effects at validation time.
4. Inject soft summary / arc-reflecting lines into pools.
5. Monologue segment plays.
6. Complete → call `endings_system.present(route)`.
7. No minute/score UI updates required; match already over.

Validation rule: content with `set_ending` inside interview fails CI/validator.

## 10. UI/UX
- Dialogue UI + monologue UI only.
- No scoreboard widget.
- No skip button.
- Journal may remain accessible but not necessary; if open, state tab ok.
- CTA: answer / continue.

## 11. Параметры

| Параметр | Дефолт | Диапазон | Смысл |
|---|---|---|---|
| `target_duration_min` | 5 | 4–6 | |
| `skip_enabled` | false | fixed | |
| `ending_locked` | true | fixed | |
| `show_scoreboard` | false | fixed | |
| `press_segments` | 1 | 1–2 | |
| `monologue_segments` | 1 | 1–2 | |
| `min_choices_on_press_nodes` | 3 | | where choices exist |
| `trauma_bypass` | true | | |

## 12. Формулы / баланс / локализация

### 12.1 Формулы
#### `interview_line_pool(route, score_signal, soft_bands, arcs)`
- pick narrator/journalist/hero lines from tagged pools
- no route mutation

#### `cosmetic_only(effects)`
- allow: tone_flags, local animator, journal optional note
- deny: ending, arc hard resolve, score deltas, trauma fire

### 12.2 Balance vars
| name | default | why |
|---|---|---|
| `target_duration_min` | 5 | короткий финал |
| `max_press_nodes` | 4 | уложить время |
| `max_monologue_beats` | 3 | |
| `soft_lines_injected_min` | 1 | итоги слышны |

### 12.3 Localization
Namespace: `interview`

| key | RU draft | where |
|---|---|---|
| `interview_act_title` | Послематчевое интервью | act |
| `interview_journalist_q_win_01` | Вы сегодня доказали характер. Что чувствуете? | press |
| `interview_journalist_q_loss_01` | Счёт жёсткий. Что скажете болельщикам? | press |
| `interview_journalist_q_draw_01` | Ничья. Это точка или пауза? | press |
| `interview_hero_tone_honest` | Скажу как есть. | choice |
| `interview_hero_tone_evade` | Не сейчас. | choice |
| `interview_hero_tone_sharp` | Вопросы не лечат. | choice |
| `interview_mono_open` | Когда камеры гаснут, остаёшься ты. | monologue |
| `interview_no_skip` | (no UI) | — |

Plus soft summary keys from `soft_*` injected into mono/press.

## 13. Контекстные системы
- `endings_system`: locked route + present after.
- `soft_stats`: summary lines.
- `characters_dialogue`: press graph engine.
- `narrative_core`: act scheduling.
- `trauma_system`: bypass.
- `match_frame_ui`: not shown.
- `choice_score_bridge`: read-only context for variants.

## 14. Аналитика
| event | params |
|---|---|
| `interview_start` | `ending_route_id`, `score_signal` |
| `interview_choice` | `choice_id`, `tone` |
| `interview_complete` | `duration_sec` |
| `interview_bypass_trauma` | bool |

## 15. Edge Cases
- Ending unresolved → force resolve, log error.
- Content tries to change ending → blocked + content error.
- Player opens journal — ok; cannot skip act.
- Extremely long VA — keep text-first; thoughts VA optional only.
- Draw/win/loss only changes pools, not duration targets.

## 16. Риски
- Ожидание last-second ending flip → ясный тон письма; cosmetic choices.
- Клише пресс-конференции → персональные вопросы от arcs/flags.
- Слишком длинно → жёсткий бюджет 5 мин / node caps.

## 17. Acceptance Criteria
1. Act включает press dialogue + monologue (оба).
2. Длительность ~5 мин на плейтесте.
3. Выборы не меняют `ending_route_id`.
4. Нет табло/score UI в кадре.
5. Нет скипа.
6. Итоги (soft/arcs/score flavor) слышны в речи.
7. Trauma path bypass’ит акт по умолчанию.
8. После complete стартует ending presentation.

Smoke: win route interview; loss route; cosmetic choice stability of ending id; trauma bypass; no scoreboard; no skip.

## 18. Релиз
- Stub: 1 press node + 1 mono beat + handoff ending.
- Full: variant pools per route/score/soft.

## 19. Пострелиз
- Успех: финал «садится»; выборы ощущаются характером, не рычагом концовки.
- Провал: путают с ending select; скучно; слишком долго.
- v2: больше персональных вопросов от NPC arcs.

---

## Контекстные блоки
- Narrative finale ritual
- Soft stats textual outcomes
- Premium closure without IAP
- Ending handoff

## Rationale gate
| Решение | Почему |
|---|---|
| Пресса + монолог | Запрос «и то и другое» |
| Ending locked | Финал не min-max |
| Только речь | Счёт уже прожит |
| Нет скипа | Ритуал закрытия |
| ~5 мин | Короткий сильный хвост |
| Trauma bypass | Свой отдельный финал |
