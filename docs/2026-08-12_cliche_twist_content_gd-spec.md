# ТЗ: cliche_twist_content — Ломание клише (контент-стандарт)

## 0. Паспорт документа
- Название фичи: Ломание клише / неожиданные повороты
- ID / кодовое имя: `cliche_twist_content`
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
  - `docs/2026-08-12_choice_score_bridge_gd-spec.md`
  - `docs/2026-08-12_endings_system_gd-spec.md`
  - `docs/2026-08-12_trauma_system_gd-spec.md`
  - `docs/2026-08-12_post_match_interview_gd-spec.md`
- История изменений:
  - 2026-08-12 — первый draft

## 1. Саммари фичи
- Что это: **не отдельный gameplay-system**, а **контент-стандарт** для сюжета/диалогов/матч-последствий. В полном MVP обязательны **1–2 крупных жёстких твиста**, которые могут ломать доверие к NPC/жанровым ожиданиям. Есть чёрный список клише. Концовка `good` допускается, но **не как сказочный хэппи-энд**: она должна быть неожиданной по форме/цене; твисты могут уводить с «очевидного хорошего пути».
- Для кого: автор при написании; плейтест на предсказуемость.
- Проблема: без стандарта игра скатывается в штампы, которые пользователь уже отверг.
- Эффект: удивление и послевкусие «жизнь/матч не по шаблону».

## 2. Бизнес-контекст
- Почему MVP: это часть дифференциации vs обычной VN/спорт-драмы.
- Альтернативы: системный random twist engine; zero constraints; twist every scene.
- Почему стандарт + 1–2 hard twists: подъёмно соло; сохраняет вес; не превращает кампанию в сплошной байт.

## 3. Цели
- Главная: зафиксировать правила анти-клише и обязательный минимум твистов.
- Вторичные: чеклист сцен; стык с `twist` тегами моста; критерии приёмки контента.
- Не цели: отдельный UI «твистметр»; рандомные сюжетные генераторы.
- Почему: качество письма важнее механики твиста.

## 4. Метрики успеха
- Основная: плейтестеры не угадывают финал/ключевой поворот заранее; упоминают сюрприз.
- Guardrail: жёсткий твист не ощущается дешёвым обманом без foreshadow (намёки можно тонкие, но не zero).
- Провал: «всё как всегда»; или твист ради твиста без эмоционального смысла.

## 5. Позиционирование
- Применяется при написании сегментов, арок, interview lines, endings copy, match beat caption irony.
- Технически может использовать `choice_score_bridge` tag `twist` и narrative flags — но фича = стандарт контента.
- Trauma route сам по себе резкий, но **не засчитывается автоматически** как единственный обязательный story-twist (можно совместить, если травма ещё и ломает жанровое ожидание осмысленно).

## 6. Scope

### In (MVP)
- Content standard document (этот файл) + checklist для сцен.
- **Обязательно 1–2 major hard twists** per full campaign playthrough spine (scripted; not optional DLC).
- Hard twist definition: ломает доверие к персонажу/жанровому обещанию/моральной кассе; игрок должен почувствовать «так не должно было пойти в клише-версии».
- Banned cliché list (минимум):
  - хэппи-энд «по умолчанию» без цены
  - сюжетная броня ключевых лиц без последствий
  - полная предсказуемость следующего бита
  - «хороший поступок → сразу гол/награда» как железное правило
  - magically healed trauma without cost (если не trauma-route logic)
  - mentor who only exists to speech then vanish without dent
  - rival becomes best friend overnight without earned beat
- Soft rules:
  - allow ironic match outcomes vs moral intent (`twist` tags)
  - allow lose-match / win-self and reverse (already endings)
- **Good ending policy (решение):**  
  - `ending_good` **разрешён**, но должен быть **неожиданным по форме**: не «кубок+семья+все живы и счастливы».  
  - Good = честный взгляд на себя / принятая цена / странный мирный исход, который всё же не читается как рекламный хэппи-энд.  
  - Major twists **могут** закрыть путь к «очевидному good» на конкретном run; good остаётся достижимым другим маршрутом или в другой форме.
- Foreshadowing: для каждого major twist — минимум 2 тонких намёка ранее (не обязательно понятных).
- Content review gate before lock.

### Out
- Runtime twist randomizer.
- UI surfacing “you hit a twist”.
- More than 2 mandatory majors if it bloats (extra small ironies ok).

### Future
- Expanded anti-cliché bible; NG+ inverted expectations.

### Зависимости
- Writing pipeline for narrative/dialogue/endings/interview
- Optional bridge `twist` tags
- Playtest feedback loop

## 7. Use Cases

### 7.1 Authoring a segment
- Run checklist: banned clichés? agency? can player predict? twist slot?
- If major twist scene: place foreshadow flags earlier.

### 7.2 Bridge irony
- Mark choice with `twist` so moral “good” yields miss/concede sometimes.

### 7.3 Ending copy pass
- Ensure good/mid/bad/trauma texts don’t collapse into banned happy/armor patterns.

### 7.4 Playtest
- Ask: “What did you think would happen?” — if majority correct on major beat, revise.

## 8. Сущности (контентные)

### `major_twist_card`
- `twist_id`, `act_placement`, `what_breaks`, `foreshadow_ids[≥2]`, `affected_arcs[]`, `ending_impact_notes`

### `scene_checklist_result`
- scene_id → pass/fail notes vs banned list

### `foreshadow_beat`
- id, placement, subtlety note

## 9. Логика (процесс, не код)
1. Outline campaign spine.
2. Reserve **1–2** major_twist_card slots (recommend: one mid/late first half or half-time; one late second half — не оба в последние 5 минут).
3. Write foreshadow early.
4. Draft scenes; run banned-cliché checklist.
5. Wire optional bridge twist tags where irony belongs.
6. Playtest predictability; revise.
7. Lock content.

Acceptance of “hard”: if removing the twist restores a stock sports/redemption cliché, it’s hard enough.

## 10. UI/UX
- Нет специального игрового UI.
- Игрок встречает твист только через story/match/dialogue.
- Author may keep private checklist in docs/tooling.

## 11. Параметры

| Параметр | Дефолт | Смысл |
|---|---|---|
| `major_twists_required_min` | 1 | |
| `major_twists_required_max` | 2 | |
| `foreshadow_min_per_major` | 2 | |
| `allow_stock_happy_ending` | false | |
| `allow_plot_armor` | false | |
| `hard_twists_allowed` | true | |
| `good_ending_must_be_non_stock` | true | |
| `runtime_random_twists` | false | |

## 12. Формулы / баланс / локализация

### 12.1 Content “формула” твиста
```
expectation (genre/NPC/moral-reward)
- earned foreshadow (subtle)
+ hard break
= lasting emotional reframe
```
Не: `random betrayal with zero setup`.

### 12.2 Balance of surprise vs trust
| name | default | why |
|---|---|---|
| `major_twists_required_max` | 2 | не обесценить |
| `bridge_twist_irony_rate` | sparingly | иначе недоверие ко всем выборам |
| `foreshadow_min` | 2 | анти-дешёвка |

### 12.3 Localization
Namespace: `twist` (mostly author-facing; in-game via scene keys)

| key | RU draft | where |
|---|---|---|
| `twist_author_checklist_title` | Анти-клише чеклист | docs/tool |
| `twist_banned_happy_default` | Запрет: хэппи-энд без цены | docs |
| `twist_banned_plot_armor` | Запрет: сюжетная броня | docs |
| `twist_banned_predictable_beat` | Запрет: следующий бит угадывается всеми | docs |

In-game lines remain under `dlg_*` / `ending_*` / `interview_*`.

## 13. Контекстные системы
- `narrative_core`: placement of twists in segments/acts.
- `characters_dialogue`: hard character betrayals/reveals.
- `choice_score_bridge`: `twist` irony.
- `endings_system`: non-stock good; twists can divert obvious good on a run.
- `trauma_system`: may amplify hardness; not auto-count as the only twist unless designed so + second twist elsewhere or strong genre-break inside trauma.
- `post_match_interview`: must not rewrite twist into stock PR redemption without cost.
- `cliche_twist_content` reviews `half_time` / training speeches for pep-talk clichés.

## 14. Аналитика
Optional playtest form (not telemetry-heavy):
- predicted_ending
- most_surprising_beat
- felt_cheap (y/n)

If telemetry exists: `story_flag_twist_<id>_fired`.

## 15. Edge Cases
- Only one major twist shipped → still meets min=1; document explicitly.
- Players always expect trauma → ensure other twist isn’t only “injury”.
- Bridge irony too frequent → players ignore moral stakes; keep sparse.
- Good ending playtest reads as stock happy → rewrite form/cost.

## 16. Риски
- Edgelord twists without meaning → require emotional reframe test.
- Over-foresight → becomes new predictability; keep subtle.
- Author burnout → max 2 majors.

## 17. Acceptance Criteria
1. Стандарт зафиксирован и используется при контент-ревью.
2. В кампании 1–2 major hard twists с карточками и ≥2 foreshadow каждый.
3. Banned list соблюдён (нет stock happy / plot armor / железная moral→goal связка).
4. `ending_good` если есть — non-stock по форме/цене.
5. Hard twists реально ломают ожидание на плейтесте (качественно).
6. Нет отдельного twist UI.
7. Хотя бы несколько bridge `twist` ironic outcomes существуют, но не на каждом выборе.

Content smoke: outline cards present; foreshadow grep; playtest notes filed.

## 18. Релиз
- Parallel to writing MVP spine; gate before content lock.
- Vertical slice: at least 1 smaller irony; full game: 1–2 majors.

## 19. Пострелиз
- Успех: спорят о поворотах; не называют историю шаблонной.
- Провал: «как везде»; или «просто жестоко без смысла».
- v2: расширенный bible.

---

## Чеклист сцены (краткий)

- [ ] Есть ли здесь stock happy / armor / 100% predictable beat?
- [ ] Moral choice ≠ гарантированный гол?
- [ ] Если claim “twist” — что именно ломается?
- [ ] Есть ли тонкий foreshadow раньше?
- [ ] Останется ли у игрока новое понимание персонажа/себя после сцены?

## Rationale gate
| Решение | Почему |
|---|---|
| Контент-стандарт | Запрос; не раздувать systems |
| 1–2 major | Вес сюрприза |
| Жёсткие твисты ок | Запрос вкуса |
| Banned list | Явные анти-паттерны пользователя |
| Good = non-stock form | Неожиданнее, чем запрет good или дешёвый happy |
| Twists can block obvious good | Реиграбельность и цена выборов |
| Foreshadow ≥2 | Не дешёвый rug-pull |
