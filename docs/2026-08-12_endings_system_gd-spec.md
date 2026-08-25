# ТЗ: endings_system — Концовки

## 0. Паспорт документа
- Название фичи: Концовки
- ID / кодовое имя: `endings_system`
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
  - `docs/2026-08-12_soft_stats_gd-spec.md`
  - `docs/2026-08-12_trauma_system_gd-spec.md`
  - `docs/2026-08-12_post_match_interview_gd-spec.md`
  - `docs/2026-08-12_characters_dialogue_gd-spec.md`
  - `docs/2026-08-18_story_content_gd-spec.md` — канон концовок A–Ж; этот ТЗ (4 карточки) не использовать для новых текстов
- История изменений:
  - 2026-08-12 — первый draft
  - 2026-08-18 — superseded для контента концовок

## 1. Саммари фичи
- Что это: резолв и показ **ровно четырёх** концовок — `good` / `mid` / `bad` / `trauma`. На **финальном свистке** 2-го тайма система считает итог по аркам, soft stats, счёту и ключевым flags: можно **проиграть матч и остаться в плюсе по морали/смыслу**, и наоборот. Trauma — отдельный route (не просто bad). После карточки — титры → меню. Галерею открытых ending в MVP не показываем.
- Для кого: игрок, для которого победа — понимание себя, а не только табло.
- Проблема: нужен честный расчёт «жизнь ↔ матч», а не одна ось счёта.
- Эффект: реиграбельность через разные балансы; интервью лишь проговаривает уже выбранный route.

## 2. Бизнес-контекст
- Почему MVP: без концовок нет завершённой коробки.
- Альтернативы: ending только от счёта; много вариативных эпилогов; gallery collectathon.
- Почему 4 карточки + mixed formula: ясно для соло; поддерживает тезис проекта; trauma отдельно уже зафиксирована.

## 3. Цели
- Главная: на финальном свистке выбрать один из 4 route id.
- Вторичные: смешанный расчёт (арки/статы/счёт/flags); handoff в interview или trauma pipeline; credits → menu.
- Не цели: менять ending в интервью; gallery в MVP; десятки микро-эпилогов.
- Почему: ясность и производство.

## 4. Метрики успеха
- Основная: плейтестеры получают разные ending при разных стилях игры; понимают, что проигрыш≠обязательный bad.
- Guardrail: trauma не путают с bad; формула не кажется чистым рандомом.
- Провал: все всегда good; или score единственный король; или «не понял почему такая концовка».

## 5. Позиционирование
- Trigger resolve: final whistle second half (`narrative_core` / match frame minute full-time).
- If `trauma_triggered`: route=`trauma` (уже могли уйти раньше; если fire mid-run — эта система только present, не пересчитывает good/mid/bad).
- Else: compute `life_score` → map to good/mid/bad.
- Lock `ending_route_id` → `post_match_interview` (normal) → `present` → credits → main menu.

## 6. Scope

### In (MVP)
- Routes: `ending_good`, `ending_mid`, `ending_bad`, `ending_trauma`.
- Resolve at final whistle (normal path).
- Formula inputs:
  - character arcs resolved/abandoned/active quality
  - soft_stats bands/values (esp. morale, but all matter)
  - match score_signal (win/draw/loss) as **weak-to-moderate** term — not dictator
  - key story flags (honesty breakthroughs, relationships, major lies, etc.)
- Explicit support for mixed outcomes: loss + high morale/self flags → can be mid/good; win + hollow/collapse inner state → mid/bad.
- Ending card (short text + art) → credits → menu.
- No endings gallery / unlock UI in MVP (internal telemetry may still log).
- Interview cannot change route.

### Out
- Ending gallery / achievement UI (MVP).
- Per-route branching interview that rewrites ending.
- More than 4 primary cards.
- Determinism break via RNG on final whistle (except already-fired trauma residual earlier).

### Future
- Gallery; micro-epilogue variants; NG+ hints.

### Зависимости
- `narrative_core`, `choice_score_bridge`, `soft_stats`, `characters_dialogue` (flags/arcs), `trauma_system`, `post_match_interview`, `match_frame_ui` (FT whistle)

## 7. Use Cases

### 7.1 Normal FT
- Whistle → compute → lock route → interview → card → credits → menu.

### 7.2 Win but empty inside
- High score term, low life/arcs/morale → likely `mid` or `bad`.

### 7.3 Loss but stood up for self
- Low score term, high morale/arcs/flags → likely `mid` or `good`.

### 7.4 Trauma already fired
- Skip normal resolve; present trauma ending after aftermath (no interview by default).

## 8. Сущности

### `ending_route_id`
- `ending_good` | `ending_mid` | `ending_bad` | `ending_trauma`

### `ending_resolution`
- `route_id`, `life_score`, `subscores{}`, `locked_at`, `inputs_snapshot`

### `ending_card`
- `route_id`, `title_key`, `body_key`, `art_ref`

### `life_subscore`
- `arcs`, `soft`, `score`, `flags` — weighted components

## 9. Логика

### Resolve order
1. If `trauma_triggered` or route already trauma: skip compute; go trauma present pipeline.
2. On final whistle:
   - `S_arcs = arc_score()`
   - `S_soft = soft_score()`
   - `S_match = match_score_component()`
   - `S_flags = flags_score()`
   - `life_score = w_arcs*S_arcs + w_soft*S_soft + w_match*S_match + w_flags*S_flags`
3. Map life_score → good/mid/bad via thresholds.
4. Lock route; snapshot inputs for interview line pools.
5. Start interview act.
6. After interview complete → show card → credits → menu.

### Component sketches (design-level, тюнинг)

#### `arc_score`
- each major arc: resolved_well +2, resolved_bittersweet +1, abandoned -1, broken -2
- normalize to ~[-1..+1] or points scale 0–10

#### `soft_score`
- morale heavy weight; energy/focus positive; pain/anxiety negative
- example:  
  `0.35*n(morale)+0.15*n(energy)+0.1*n(strength)+0.15*n(focus)-0.15*n(pain)-0.2*n(anxiety)`  
  where `n` normalizes [-10..10] → [-1..1]

#### `match_score_component`
- win +1, draw 0, loss -1; then multiply by `w_match` (keep smaller than arcs+soft combined)

#### `flags_score`
- author table of key flags with weights (breakthrough + , betrayal - , helped_x + …)
- clamp

### Thresholds (defaults, tune on playtests)
Assume `life_score` roughly in [-3..+3]` after weights:
- `>= good_threshold` (1.0) → good
- `<= bad_threshold` (-1.0) → bad
- else → mid

### Anti-dominance rule
- Even with win, if `S_soft` and `S_arcs` both very low, clamp away from good (cap at mid) unless flags extraordinary.
- Even with loss, if `S_soft` and `S_arcs` both very high, clamp away from bad (floor at mid) unless flags catastrophic.
- Это формализует «можно проиграть, но быть в плюсе — и наоборот».

## 10. UI/UX
- Ending card: title + short body + art; no checklist of stats.
- Credits scroll/list.
- Then main menu (Continue disabled or points to finished run per save policy — **решение:** finished run → New Game / Load other saves).
- No gallery screen.
- No scoreboard required on card (optional one line “матч закончился …” in text if author wants — not live widget).

## 11. Параметры

| Параметр | Дефолт | Диапазон | Смысл |
|---|---|---|---|
| `resolve_at` | `final_whistle` | fixed | |
| `w_arcs` | 0.35 | 0.2–0.45 | |
| `w_soft` | 0.30 | 0.2–0.4 | |
| `w_flags` | 0.20 | 0.1–0.3 | |
| `w_match` | 0.15 | 0.05–0.25 | слабее жизни |
| `good_threshold` | 1.0 | | |
| `bad_threshold` | -1.0 | | |
| `anti_dominance_enabled` | true | | |
| `gallery_enabled` | false | MVP | |
| `interview_can_change_ending` | false | fixed | |
| `credits_then_menu` | true | | |

## 12. Формулы / баланс / локализация

### 12.1 Core formula
```
life_score = w_arcs*S_arcs + w_soft*S_soft + w_flags*S_flags + w_match*S_match
route = trauma if trauma else band(life_score)
```
`band`: good / mid / bad with anti-dominance clamps.

### 12.2 Balance vars
| name | default | risk | why |
|---|---|---|---|
| `w_match` | 0.15 | score-king | жизнь важнее |
| `w_arcs` | 0.35 | | люди/арки |
| `w_soft` | 0.30 | | мораль и форма |
| `w_flags` | 0.20 | | ключевые решения |
| `good_threshold` | 1.0 | слишком редкий good | |
| `bad_threshold` | -1.0 | | |
| `morale_soft_weight_inside` | high | | win/loss vs +morale cases |

### 12.3 Localization
Namespace: `ending`

| key | RU draft | where |
|---|---|---|
| `ending_good_title` | Ты остался собой | card |
| `ending_good_body` | Счёт — только часть истории. Ты выдержал взгляд на себя. | card |
| `ending_mid_title` | Ничья с судьбой | card |
| `ending_mid_body` | Что-то спасено, что-то потеряно. Завтра не будет прежним. | card |
| `ending_bad_title` | Пустой свисток | card |
| `ending_bad_body` | Даже победа не закрыла дыру. Или поражение добило то, что дрожало. | card |
| `ending_trauma_title` | Травма | card |
| `ending_trauma_body` | Матч кончился раньше. Жизнь — нет. | card |
| `ending_credits_header` | 90 минут | credits |

Тексты — черновики; авторская правка обязательна.

## 13. Контекстные системы
- `match_frame_ui` / narrative: final whistle signal.
- `soft_stats`, arcs/flags from dialogue/core.
- `choice_score_bridge`: score_signal.
- `trauma_system`: exclusive route.
- `post_match_interview`: consumes locked route.
- Save: store locked route; finished flag.

## 14. Аналитика
| event | params |
|---|---|
| `ending_resolve` | `route`, `life_score`, `subscores`, `score_signal` |
| `ending_card_shown` | `route` |
| `ending_credits_done` | |
| `ending_anti_dominance_applied` | `from`, `to` |

## 15. Edge Cases
- Whistle without required systems → resolve with available subscores, missing=0, log warning.
- Trauma after whistle impossible if match already cut earlier; if somehow both, trauma wins.
- Manual debug force route — author only.
- Load mid-interview: route stays locked.
- Second playthrough: no gallery, but naturally different routes.

## 16. Риски
- Непрозрачность формулы → interview/card language must echo why (без цифр).
- Score still feels king → keep w_match low + anti-dominance.
- Bad/trauma confusion → separate art/copy.
- Tuning hell → expose weights in config.

## 17. Acceptance Criteria
1. Ровно 4 route cards в MVP.
2. Resolve на финальном свистке normal path.
3. Loss + strong morale/arcs/flags может дать mid/good.
4. Win + hollow inner state может дать mid/bad.
5. Trauma отдельный и приоритетный.
6. Interview не меняет route.
7. Card → credits → menu.
8. Нет gallery UI.
9. Snapshot/subscores логируются для тюнинга.

Smoke: win+high life→good; loss+high life→mid/good; win+low life→mid/bad; trauma present; interview lock stable; credits to menu.

## 18. Релиз
- Вместе с вертикальным срезом: 2 routes stub; full 4 before content lock.
- Tune weights after playtests.

## 19. Пострелиз
- Успех: обсуждают «проиграл матч, но не себя»; разные ending на разных run.
- Провал: все ending одинаковые; непонятный bad; score-only feeling.
- v2: gallery; finer epilogues.

---

## Контекстные блоки
- Premium closure
- Mixed life/match evaluation
- Trauma exclusivity
- Interview handoff
- No meta gallery yet

## Rationale gate
| Решение | Почему |
|---|---|
| Resolve на свистке | Якорь ритуала матча |
| Всё важно в формуле | Проигрыш≠плохая жизнь |
| Ровно 4 карточки | Ясный MVP scope |
| Credits → меню | Чистый выход |
| Без gallery | Меньше мета-оболочки сейчас |
| Trauma отдельно | Уже зафиксировано |
| Interview cosmetic only | Уже зафиксировано |
