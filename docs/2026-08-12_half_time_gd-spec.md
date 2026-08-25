# ТЗ: half_time — Перерыв

## 0. Паспорт документа
- Название фичи: Перерыв
- ID / кодовое имя: `half_time`
- Проект / версия: **90 минут** / P1
- Фичеовнер: автор (соло)
- Статус: draft
- Дата создания: 2026-08-12
- Дата обновления: 2026-08-12
- Связанные документы:
  - `docs/project-concept.md`
  - `docs/feature-index.md`
  - `docs/2026-08-12_narrative_core_gd-spec.md`
  - `docs/2026-08-12_pre_match_training_gd-spec.md`
  - `docs/2026-08-12_soft_stats_gd-spec.md`
  - `docs/2026-08-12_characters_dialogue_gd-spec.md`
  - `docs/2026-08-12_world_exploration_gd-spec.md`
  - `docs/2026-08-12_trauma_system_gd-spec.md`
  - `docs/2026-08-12_match_frame_ui_gd-spec.md`
- История изменений:
  - 2026-08-12 — первый draft

## 1. Саммари фичи
- Что это: обязательный акт между таймами — **несколько сцен** (раздевалка/коридор, walk + диалоги). Даёт **небольшое** восстановление soft stats и **ситуативное давление** (тренер/врач/партнёры) в зависимости от счёта и состояния. Скипа нет. QTE нет. Длительность сопоставима с тренировкой (~8–12 мин).
- Для кого: игрок на середине матч-ритуала.
- Проблема: нужен человеческий «вдох» между таймами без симулятора и без splash-экранов тайма.
- Эффект: минута скачет к второму тайму; герой чуть отходит / наоборот под давлением; сюжетные арки двигаются.

## 2. Бизнес-контекст
- Почему P1: после стабильного кора матча и soft_stats.
- Альтернативы: один катсцен-диалог; skip как на тренировке; QTE-разминка.
- Почему несколько сцен без скипа: перерыв — драматургическая опора; пропуск обесценил бы mid-campaign pressure; QTE здесь лишние.

## 3. Цели
- Главная: обязательный mid-act с несколькими сценами.
- Вторичные: лёгкий recover статов; situational pressure; движение арок/памяти NPC.
- Не цели: скип; QTE; полное хиление в mid; отдельный UI «конец 1-го тайма».
- Почему: минуты на табло достаточно; ценность — люди и состояние.

## 4. Метрики успеха
- Основная: перерыв запоминается репликами/давлением, не «загрузкой между таймами».
- Guardrail: ~8–12 мин; recover не отменяет цену плохих выборов; не stuck softlock.
- Провал: все скипнули бы если могли; пустая раздевалка; recover слишком жирный.

## 5. Позиционирование
- `narrative_core` act между first_half и second_half.
- Читает `match_score`, `soft_stats`, memory/arcs.
- Пишет soft_stats deltas, dialogue flags, journal.
- Frame: minute jump `45'`→`46'` (или контентный), без splash.
- Trauma: может warning’ить; fire windows могут включать half_time при collapse (если allowed).

## 6. Scope

### In (P1)
- 1 локация (раздевалка/смежные комнаты) + **несколько** диалоговых сцен (целевой коридор **3–5** узлов/встреч, не обязательно все обязательные — но акт нельзя скипнуть целиком).
- Обязательный completion gate (см. логику).
- Лёгкий recover package + situational pressure packages.
- Нет skip CTA.
- Нет QTE/drills.
- Доступ к журналу «Состояние».
- Переход во 2-й тайм после gate.

### Out
- Skip half_time.
- QTE.
- Полный сброс негативных статов.
- Broadcast splash half-time show.
- Новые большие локации сверх раздевалки.

### Future
- Больше вариативных сцен от score differential; NG+ lines.

### Зависимости
- `narrative_core`, `world_exploration`, `characters_dialogue`, `soft_stats`, `match_frame_ui`, `trauma_system` (warnings), `choice_score_bridge` (read score only)

## 7. Use Cases

### 7.1 Нормальный перерыв
- После last beat 1st half → page/world transition into locker location.
- Player walks, talks through available scenes.
- Soft recover ticks (small) on act enter and/or after key calm scene.
- Pressure scenes branch by score/stats (leading / drawing / losing; low morale; high pain…).
- Gate complete → minute to 2nd half → resume match loop.

### 7.2 Проигрыш / коллапс формы
- Stronger coach/doctor pressure; grey bold options possible.
- Warnings for trauma may appear here.

### 7.3 Победа по ходу матча
- Другой тон давления (не расслабляйся) + меньший recover или тот же лёгкий recover.

## 8. Сущности

### `half_time_act_state`
- `entered`, `scenes_done[]`, `pressure_profile`, `completed`

### `half_time_scene`
- `scene_id`, `dialogue_id` or exploration beat, `requirements`, `optional` bool, `on_complete_effects`

### `recover_package`
- small positive deltas (energy/focus/morale), optional small anxiety relief

### `pressure_profile`
- derived from score_diff + soft bands → selects scene set / line variants

## 9. Логика
1. Enter act → hide match presentation; show locker world; apply `recover_on_enter` (small).
2. Compute `pressure_profile` from score + stats.
3. Unlock scene pool (mandatory subset + optional).
4. Player must complete `mandatory_scenes` (default 2: e.g. coach OR captain + one personal beat) and `min_scenes_total` (default 3).
5. No skip; doors back to pitch locked until gate.
6. On complete: apply optional `recover_on_exit` tiny; set minute 46'; jump to second_half; clear half_time locks.

**Решение по recover:** суммарно лёгкий — не полный mid reset. Пример: enter +1 energy/+1 focus; calm scene +1 morale; pressure scene may −morale/+anxiety даже в перерыве.

## 10. UI/UX
- Как past exploration + dialogue.
- Нет кнопки «Пропустить перерыв».
- Табло скрыто в past-mode; при выходе на 2-й тайм снова через presentation/frame.
- Journal state available.

## 11. Параметры

| Параметр | Дефолт | Диапазон | Смысл |
|---|---|---|---|
| `target_duration_min` | 10 | 8–12 | как training |
| `mandatory_scenes` | 2 | 1–3 | |
| `min_scenes_total` | 3 | 2–5 | |
| `skip_enabled` | false | fixed | |
| `qte_enabled` | false | fixed | |
| `recover_enter_energy` | +1 | | |
| `recover_enter_focus` | +1 | | |
| `recover_calm_morale` | +1 | | |
| `pressure_on_losing` | true | | |
| `second_half_minute` | 46 | | |

## 12. Формулы / баланс / локализация

### 12.1 Формулы
#### `pressure_profile(score, stats)`
- if goals_for < goals_against → `losing`
- else if equal → `drawing`
- else → `leading`
- modify with low morale / high pain → `fragile_*` variants

#### `half_time_gate_ok(state)`
- mandatory done AND scenes_done >= min_scenes_total

### 12.2 Balance vars
| name | default | why |
|---|---|---|
| `recover_enter_energy` | +1 | лёгкий вдох |
| `recover_enter_focus` | +1 | |
| `recover_calm_morale` | +1 | |
| `pressure_losing_anxiety` | +1 | обстоятельства |
| `pressure_losing_morale` | -1 | |
| `min_scenes_total` | 3 | несколько сцен |
| `target_duration_min` | 10 | паритет с training |

### 12.3 Localization
Namespace: `ht`

| key | RU draft | where |
|---|---|---|
| `ht_act_title` | Перерыв | act |
| `ht_door_locked` | Ещё рано на поле. | door |
| `ht_coach_losing_01` | Это не счёт. Это ваш характер. | dlg |
| `ht_coach_leading_01` | Не расслабились. Соперник живой. | dlg |
| `ht_doctor_pain_01` | Ты бледный. Не геройствуй зря. | dlg |
| `ht_recover_feel` | Чуть легче. Только чуть. | optional thought |
| `ht_to_second_half` | Второй тайм. | transition |

## 13. Контекстные системы
- `soft_stats`: recover + pressure deltas; menu.
- `characters_dialogue`: multi scenes, mostly 1-on-1.
- `world_exploration`: locker location, locked pitch door.
- `match_frame_ui`: minute jump on exit.
- `trauma_system`: warnings / possible fire window.
- `narrative_core`: act sequencing.
- `endings_system`: indirect via arcs/stats.

## 14. Аналитика
| event | params |
|---|---|
| `ht_enter` | `score`, `pressure_profile` |
| `ht_scene_done` | `scene_id` |
| `ht_complete` | `duration_sec`, `scenes` |
| `ht_stat_delta` | `source`, `deltas` |

## 15. Edge Cases
- Trauma fires mid half_time → aftermath overrides remaining scenes.
- Too few optional scenes available due to flags → mandatory set must still be completable alone if min lowered by content fallback (validator: always completable).
- Save mid act: ok on world anchors.
- Leading by many: still no skip; pressure flavor changes, recover stays light.

## 16. Риски
- Ощущается как пауза без agency → несколько смысловых сцен + выборы.
- Recover слишком сильный → держать +1 уровень.
- Игроки хотят skip → сознательно нет; ценность давления.

## 17. Acceptance Criteria
1. Act обязателен, skip недоступен.
2. Есть несколько сцен (gate ≥3 или эквивалент mandatory set).
3. Лёгкий recover + situational pressure работают от счёта/статов.
4. Нет QTE.
5. Длительность в коридоре ~training.
6. Выход даёт minute jump во 2-й тайм, без splash half-time show.
7. Дверь на поле закрыта до gate.
8. Save/load mid-act корректен.

Smoke: enter recover; losing pressure scene; complete gate; minute 46; trauma warning optional; no skip button.

## 18. Релиз
- Stub: 2 dialogues + recover + jump.
- Full: 3–5 scenes, pressure profiles, locker art.

## 19. Пострелиз
- Успех: перерыв цитируют; чувствуют лёгкий вдох и давление.
- Провал: «хочу скип»; «меня полностью отхилили»; пусто.
- v2: больше score-based variants.

---

## Контекстные блоки
- Narrative ritual mid-point
- Soft stats light recover + pressure
- No FTUE-heavy (already trained)
- Premium pacing

## Rationale gate
| Решение | Почему |
|---|---|
| Несколько сцен | Запрос + mid-story weight |
| Лёгкий recover + pressure | Вдох без обнуления цены выборов |
| Нет скипа | Перерыв — опора, не loading screen |
| ~10 мин | Паритет с тренировкой |
| Без QTE | Фокус на людях/состоянии |
| Без splash | Уже решено в frame: минуты достаточно |
