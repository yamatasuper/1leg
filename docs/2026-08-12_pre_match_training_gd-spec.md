# ТЗ: pre_match_training — Тренировка

## 0. Паспорт документа
- Название фичи: Тренировка
- ID / кодовое имя: `pre_match_training`
- Проект / версия: **90 минут** / P1
- Фичеовнер: автор (соло)
- Статус: draft
- Дата создания: 2026-08-12
- Дата обновления: 2026-08-12
- Связанные документы:
  - `docs/project-concept.md`
  - `docs/feature-index.md`
  - `docs/2026-08-12_narrative_core_gd-spec.md`
  - `docs/2026-08-12_world_exploration_gd-spec.md`
  - `docs/2026-08-12_characters_dialogue_gd-spec.md`
  - `docs/2026-08-12_soft_stats_gd-spec.md`
  - `docs/2026-08-12_match_presentation_gd-spec.md`
- История изменений:
  - 2026-08-12 — первый draft

## 1. Саммари фичи
- Что это: предстартовый акт кампании — короткая локация базы/поля в духе past-exploration, разговоры, лёгкие QTE-упражнения и возможность **скипнуть** тренировку ценой штрафа к soft stats (как неявка в жизни).
- Для кого: игрок на входе в матч-ритуал.
- Проблема: нужно сразу заложить тон, познакомить с людьми/рамкой «матч = жизнь» и дать первый тулзинг состояния без FIFA-симулятора.
- Эффект: игрок выходит на матч уже с настроем, NPC-якорями, пониманием рамки и (если не скипнул) небольшим плюсом формы; скип = осознанный минус.

## 2. Бизнес-контекст
- Почему P1: после кора ходьбы/диалогов/статов; в вертикальном срезе можно stub.
- Альтернативы: только катсцена; полноценный training mode; запрет скипа.
- Почему гибрид walk+dlg+light QTE + skip-with-penalty: логично стыкуется с Disco-like миром, учит всё сразу, даёт agency и жизненный штраф.

## 3. Цели
- Главная: стартовый ритуал, который делает **всё сразу**: soft stats, знакомство с NPC, туториал рамки матч↔жизнь.
- Вторичные: optional QTE drills; skip с минусом статов; открытие меню состояния (мысли/статы).
- Не цели: долгий вечерний акт; симулятор физподготовки; обязательный skill-check hell.
- Почему: темп кампании и соло-бюджет.

## 4. Метрики успеха
- Основная: после тренировки игрок понимает рамку и узнаёт 1–2 ключевых NPC.
- Guardrail: длительность ~8–12 мин; скип понятен по цене; QTE не бесят.
- Провал: «обязаловка без смысла» / «не понял зачем матч» / скип без видимой цены.

## 5. Позиционирование
- Первый `act` ритуала до 1-го тайма (`narrative_core`).
- Использует `world_exploration` (1 training location), `characters_dialogue`, `soft_stats`, optional pressure/QTE patterns близкие к `match_presentation` (упрощённо).
- FTUE: объясняет матч↔жизнь **до** или **сразу вокруг** первого настоящего match beat (согласовать с intro: если intro уже показал матч→прошлое, тренировка закрепляет и персонализирует).

## 6. Scope

### In (P1)
- Одна (максимум две связанные) локация «база/поле тренировки» со свободной ходьбой.
- 2–4 коротких диалоговых узла (тренер/врач/партнёр/rival light).
- Явный tutorial beat рамки: матч олицетворяет жизнь; выборы аукнутся на табло.
- Soft stats deltas за участие, разговоры, успех/провал drills.
- **Skip training** из меню акта/двери «уйти рано» → пакет штрафов в минус.
- Optional light QTE drills (1–2): timing bar / one-dodge style, без управления футболистом в матче.
- Меню/журнал показывает soft stats **в стиле мыслей Disco** (см. обновление `soft_stats`).
- Целевая длительность **8–12 минут** без скипа.
- Завершение акта → переход к kickoff / первому тайму (minute jump + 0:0 уже заданы frame).

### Out
- Полноценный симулятор упражнений.
- Большая карта базы.
- Обязательные сложные QTE.
- Начисление голов за тренировку.

### Future
- Доп. drills; вариативность реплик тренера по NG+.

### Зависимости
- `narrative_core`, `world_exploration`, `characters_dialogue`, `soft_stats`, `match_frame_ui` (после выхода), `save_anytime`

## 7. Use Cases

### 7.1 Полное прохождение тренировки
- Spawn на базе → walk → talk NPC → 1–2 drills → closing line тренера → tutorial confirm → act complete.
- Stats: небольшой плюс (morale/energy/focus и т.д. по таблице).

### 7.2 Skip
- Игрок выбирает «Не приходить / Уйти» до completion flag.
- Confirm: предупреждение о штрафе.
- Apply `skip_penalty_package` (минусы); act marked `skipped`.
- Сразу (или почти) kickoff flow без drills.

### 7.3 Drill QTE
- Успех: +energy/+focus/+strength light.
- Провал: малый минус или flat; **не** блокирует акт.
- Редко может чуть влиять на первый mid-band диалог, не на голы напрямую.

### 7.4 Меню состояния
- Из паузы/журнала открывается панель «состояние/мысли» со soft stats (иконки+короткие описания bands, можно числа в Disco-thoughts эстетике).

## 8. Сущности

### `training_act_state`
- `started`, `completed`, `skipped`, `drills_done[]`, `tutorial_seen`, `npcs_met[]`

### `training_drill`
- `drill_id`, `pattern` (`timing_bar`|`one_dodge`), `stat_rewards_success`, `stat_rewards_fail`, `optional` bool

### `skip_penalty_package`
- deltas map (negative bias to morale/energy/focus/anxiety…)

### `training_tutorial_beat`
- lines/keys explaining match=life + first journal goals

## 9. Логика
1. Enter act `training` → load training location; soft_stats already init for match lifetime (or init here if match clock starts after training — **решение:** init soft_stats at training start so skip penalties apply before kickoff).
2. Player explores until `completion_gate` (talk key NPC + tutorial flag + optional drills threshold `drills_required_min` default 0 or 1).
3. Skip path: confirm → apply penalties → set skipped → goto kickoff.
4. Complete path: apply participation rewards → kickoff.
5. Kickoff: narrative starts match acts; score 0:0; minute jump; first beat per core intro rules.

Completion gate рекомендуется: tutorial_seen == true AND (met_coach OR met_doctor); drills optional unless author sets required=1.

## 10. UI/UX
- Past HUD + interact.
- Skip CTA в паузе/у выхода: «Пропустить тренировку» + warning.
- Drill overlay как light pressure UI.
- Состояние: отдельная вкладка журнала «Состояние» (Disco-thoughts vibe).
- Не показывать good/bad на выборах тренировки сверх обычных dialogue rules.

## 11. Параметры

| Параметр | Дефолт | Диапазон | Смысл |
|---|---|---|---|
| `target_duration_min` | 10 | 8–12 | |
| `drills_available` | 2 | 1–3 | |
| `drills_required_min` | 0 | 0–1 | |
| `skip_enabled` | true | | |
| `skip_penalty_morale` | -2 | | |
| `skip_penalty_energy` | -2 | | |
| `skip_penalty_focus` | -1 | | |
| `skip_penalty_anxiety` | +2 | | |
| `complete_bonus_morale` | +1 | | |
| `complete_bonus_energy` | +1 | | |
| `qte_fail_blocks_act` | false | fixed | |
| `stats_menu_visible` | true | | |

## 12. Формулы / баланс / локализация

### 12.1 Формулы
#### `apply_skip_penalties()`
- add skip_penalty_* to soft_stats (clamp)

#### `apply_training_completion_bonuses()`
- small positives; cannot alone create absurd pulse (still soft)

#### `drill_resolve(success)`
- apply reward table; never award goals

### 12.2 Balance vars
| name | default | why |
|---|---|---|
| `skip_penalty_morale` | -2 | цена неявки |
| `skip_penalty_energy` | -2 | |
| `skip_penalty_focus` | -1 | |
| `skip_penalty_anxiety` | +2 | |
| `complete_bonus_morale` | +1 | |
| `complete_bonus_energy` | +1 | |
| `drill_success_strength` | +1 | |
| `drill_fail_pain` | +1 | лёгко |

### 12.3 Localization
Namespace: `train`

| key | RU draft | where |
|---|---|---|
| `train_act_title` | Тренировка | act |
| `train_skip_cta` | Пропустить тренировку | pause/door |
| `train_skip_warn` | Можно уйти. Тело и голова это заметят. | confirm |
| `train_skip_done` | Ты не пришёл. Это тоже выбор. | toast/narrative |
| `train_tutorial_match_life` | Этот матч — зеркало твоей жизни. | tutorial |
| `train_drill_timing_hint` | Поймай момент. | qte |
| `train_drill_dodge_hint` | Не сломай себя на разминке. | qte |
| `train_complete` | Хватит на сегодня. Пора выходить. | closing |
| `train_journal_state_tab` | Состояние | journal tab |

## 13. Контекстные системы
- `soft_stats`: init + bonuses/penalties; menu visibility.
- `world_exploration`: training location.
- `characters_dialogue`: coach/doctor/teammates; trauma warnings may foreshadow later.
- `narrative_core`: act order.
- `match_presentation` patterns reused lightly for drills.
- `choice_score_bridge`: no goals here.

## 14. Аналитика
| event | params |
|---|---|
| `train_start` | |
| `train_skip` | `penalties` |
| `train_drill` | `drill_id`, `success` |
| `train_npc_met` | `character_id` |
| `train_complete` | `duration_sec`, `drills_done` |
| `train_tutorial_seen` | |

## 15. Edge Cases
- Skip after partial drills: still full skip package (no partial refund) OR pro-rate — **решение:** full skip package only if `completed==false` and skip chosen; already gained drill bonuses **keep**, then apply skip package (net can still be negative). Альтернатива проще для автора: skip allowed only before first drill — **выбрать:** skip доступен всегда до `completed`, bonuses keep + penalties apply.
- Save mid-training: allowed on world anchors; not mid-dialogue/mid-drill.
- Trauma cannot fire in training (too early) unless author explicit — default `trauma_fire_windows` exclude training act.

## 16. Риски
- Игроки всегда скипят → сделать penalties ощутимыми + NPC/tutorial ценность.
- QTE бесят → optional, fail-soft.
- Дубль туториала с intro match → согласовать тексты, не повторять слово в слово.

## 17. Acceptance Criteria
1. Тренировка — walkable акт с диалогами и optional drills.
2. Даёт soft stats + NPC meet + tutorial рамки.
3. Skip доступен с confirm и минус-пакетом.
4. Длительность полного пути в целевом коридоре на плейтесте.
5. QTE не блокируют акт при fail.
6. Статы видны в меню/журнале «Состояние».
7. Нет голов/счёта за тренировку.
8. После complete/skip корректный переход к матчу 0:0.

Smoke: full clear; skip+penalties; drill success/fail; journal state tab; save/load mid-act.

## 18. Релиз
- Stub: one room + skip + one dialogue + tutorial line.
- Full P1: 1 location, 2 drills, key NPCs, penalties tuned.

## 19. Пострелиз
- Успех: тренировку часто проходят ради людей/тона; скип — осознанный roleplay.
- Провал: все скипят / все ненавидят QTE.
- v2: больше вариативности тренера.

---

## Контекстные блоки
- FTUE / обучение рамки
- Soft stats first touch + menu
- Narrative ritual act
- Premium onboarding without IAP

## Rationale gate
| Решение | Почему |
|---|---|
| Walk+dlg+light QTE | Логично с Disco-like кора |
| Всё сразу | Экономия акта |
| Skip с минусом | Реализм неявки + agency |
| ~10 мин | Не съесть вечер |
| Fail-soft QTE | Не платформер/не садомазо |
| Статы в меню как мысли | Запрос + читаемость без HUD-кассы на экране мира |
