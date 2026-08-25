# ТЗ: match_frame_ui — Рамка матча

## 0. Паспорт документа
- Название фичи: Рамка матча
- ID / кодовое имя: `match_frame_ui`
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
- Что это: комикс-виджет матчевого состояния — **имя соперника, счёт, минута (со скачками)**. Без карточек, замен, турнирных сеток и отдельных экранов начала/конца тайма.
- Для кого: игрок в матч-слое, которому нужен читаемый «пульс» матча.
- Проблема: показать футбольную рамку просто и атмосферно, не превращая UI в симулятор.
- Эффект: в матч-битах всегда понятно, против кого играем, какой счёт и «когда» по матчу; в прошлом виджет **скрыт**.

## 2. Бизнес-контекст
- Почему фича нужна: счёт/минута — якорь последствий `choice_score_bridge` и ритма `narrative_core`.
- Альтернативы: полный broadcast UI; постоянный HUD в прошлом; отдельные splash «1st half / FT».
- Почему выбранный минимум: меньше шума, ближе к комиксу, дешевле в соло; минуты достаточно вместо splash-экранов.

## 3. Цели
- Главная: отображать соперника + счёт + минуту в матч-слое.
- Вторичные: стилистически быть частью комикса; поддерживаить таймлайн presentation (появление в разные моменты).
- Не цели: статистика матча; симуляция clock tick каждую секунду; UI в прошлых сегментах.
- Почему: прошлое — основа геймплея; матч — зеркало.

## 4. Метрики успеха
- Основная: игрок считывает счёт/минуту/соперника без пояснений.
- Guardrail: виджет не перекрывает сюжет комикса агрессивно; не торчит в прошлом.
- Провал: «не заметил счёт» / «HUD как в FIFA» / «табло мешает читать панели».

## 5. Позиционирование
- Данные: `match_score` и события из `choice_score_bridge`.
- Время/тайм: задаёт `narrative_core` авторскими скачками на битах/актах.
- Показ: вызывается `match_presentation` в моменты `before` / `between_panels` / `after` (иногда `hidden`).
- Вне матч-слоя: forcibly hidden.

## 6. Scope

### In (MVP)
- Виджет в комикс-стиле:
  - `opponent_name`
  - `goals_for` : `goals_against` (старт **0:0**)
  - `match_minute` (целое, со скачками; формат `67'`)
  - лёгкий индикатор тайма через минуту/контекст (1–45 / 46–90+), **без** отдельных экранов kickoff/FT
- API для presentation: show / hide / pulse_on_score_change / set_minute / set_opponent
- Анимация смены счёта (короткий comic punch)
- Скрытие во всех `time_mode = past` сегментах
- Поддержка доп. времени как `90+1'` (опционально, если акт задаёт)

### Out
- Имена своих/чужих в составе, карточки, замены, xG, владение
- Реалтайм-тикер секунд
- Splash «начало тайма / конец тайма / финальный свисток» как отдельные экраны
- Постоянный мини-HUD в мире прошлого
- Турнирная сетка / таблица

### Future
- Варианты скинa табло под разные матчи/сны
- Более богатый broadcast только если появится контентная нужда

### Зависимости
- `choice_score_bridge` — score deltas
- `match_presentation` — когда показать
- `narrative_core` — minute jumps, act boundaries, opponent identity for campaign match

## 7. Use Cases

### 7.1 Старт выхода на поле
- Счёт 0:0, минута `1'` (или `0'` → сразу скачок к первому биту по контенту), соперник задан.
- Виджет может мелькнуть в intro-бите.

### 7.2 Обычный матч-бит
- Presentation просит show в выбранный момент.
- При goal outcome — pulse/update счёта в момент голевой панели (не раньше, если author sync сказан).
- Минута скачет на значение бита (`set_minute(67)`), без плавного realtime.

### 7.3 Возврат в прошлое
- Hide виджета на transition_out / входе в past.
- В past ни счёт, ни минута не видны.

### 7.4 Перерыв / второй тайм
- Без splash: достаточно скачка минуты (`45'` → `46'`) и продолжения счёта.
- Соперник тот же (один матч кампании), если контент не меняет.

## 8. Сущности

### `match_frame_state`
- `opponent_name` (string, loc key or raw)
- `goals_for` (int ≥ 0)
- `goals_against` (int ≥ 0)
- `match_minute` (int ≥ 0)
- `stoppage` (int ≥ 0, for `90+N'`)
- `half_hint` (derived: `first` if minute≤45 and stoppage==0 on first half act; `second` otherwise)
- `visible` (bool)

### `match_frame_view`
- comic artframe + text slots
- anchors: `top`, `corner`, `panel_inset` (выбирает presentation)

## 9. Логика
1. Init on match act enter: score 0:0 (если новый матч), opponent from campaign config, minute from act start.
2. On bridge apply: update internal score; if visible, play score punch.
3. On presentation directive: set visibility + anchor + optional highlight.
4. On narrative beat: `set_minute` jump (может идти назад только в exceptional author debug — по умолчанию monotonic non-decreasing within match).
5. On enter past: `visible=false`.
6. On trauma cut: freeze numbers; hide unless trauma presentation explicitly shows torn-score motif (optional, default hide).

Правило минуты:
- Только авторские скачки (`minute_after_beat` в контенте бита/сегмента).
- Не тикает сама по себе.

## 10. UI/UX
- Выглядит как **нарисованное табло комикса**, не системный ImGui-бар.
- Состав: `[Opponent]` · `[GF:GA]` · `[M']`
- Не обязательно всегда top-overlay: может быть врезкой в панели.
- Empty: opponent TBD → placeholder `???` только если контент ошибся (QA fail).
- Error: если score desync — показать фактический state из frame authoritative store (frame владеет display state после apply).
- CTA: нет.
- В прошлом: полностью скрыто (и не занимаёт layout).

## 11. Параметры

| Параметр | Дефолт | Диапазон | Смысл |
|---|---|---|---|
| `score_punch_sec` | 0.45 | 0.2–0.8 | анимация гола |
| `default_anchor` | between_panel | before/between/after | если presentation не задал |
| `minute_format_stoppage` | true | bool | `90+1'` |
| `hide_in_past` | true | fixed | |
| `show_half_label` | false | bool | MVP без «1T/2T» текста |
| `max_display_minute` | 90 | 90–120 | clamp отображения |

## 12. Формулы / баланс / локализация

### 12.1 Формулы

#### `format_minute(m, stoppage)`
- if stoppage > 0 and m >= 90: return `90+{stoppage}'`
- else return `{m}'`

#### `derive_half_hint(act_id, minute)`
- mapping from narrative act: training→pre; first_half→first; second_half→second; interview→post
- UI may ignore label visually in MVP (minute enough)

#### `apply_score_delta(gf, ga)`
- goals_for += gf; goals_against += ga
- clamp ≥ 0
- if visible → punch

Нет экономичных формул; реализм голов — у bridge.

### 12.2 Balance / config vars
| name | default | notes |
|---|---|---|
| `score_punch_sec` | 0.45 | |
| `frame_opacity` | 1.0 | |
| `frame_scale_comic` | 1.0 | |
| `opponent_name_key` | `match_frame_opponent_default` | per campaign |
| `kickoff_minute` | 1 | or 0 per taste |
| `halftime_minute_mark` | 45 | jump reference |
| `fulltime_minute_mark` | 90 | |

### 12.3 Localization
Namespace: `match_frame`

| key | RU draft | where |
|---|---|---|
| `match_frame_opponent_default` | Соперник | fallback |
| `match_frame_score_a11y` | Счёт {goals_for}:{goals_against} | a11y |
| `match_frame_minute_a11y` | Минута {minute} | a11y |
| `match_frame_opponent_a11y` | Против {opponent} | a11y |

Имена конкретного соперника — контентные ключи кампании (`match_frame_opponent_<id>`).

> Визуальные цифры счёта/минуты обычно не через loc, а number draw; a11y — через ключи выше.

## 13. Контекстные системы
- `match_presentation` — владелец когда/где показать.
- `choice_score_bridge` — source of score changes (+ rare override).
- `narrative_core` — minute jumps, act, opponent identity.
- `art_pipeline` — comic frame asset.
- Не зависит от soft_stats напрямую (опциональные visual states позже).

## 14. Аналитика
| event | params |
|---|---|
| `match_frame_show` | `anchor`, `minute`, `score` |
| `match_frame_hide` | `reason` |
| `match_frame_score_punch` | `gf`, `ga` |
| `match_frame_minute_jump` | `from`, `to` |

## 15. Edge Cases
- Presentation `hidden`: state обновляется, UI не показывают до следующего show.
- Два гола в pack: два punch подряд или один punch на финальный score (флаг presentation `punch_mode=final_only` default).
- Save/Load: restore full `match_frame_state`; visibility = false если load в past.
- Mid-presentation save disabled (как в presentation spec) — frame просто заморожен в timeline.
- Trauma: hide + freeze.

## 16. Риски
- Табло выбивается из комикс-стиля → жёсткий art direction (нарисованное, не системный UI kit).
- Игрок не замечает счёт → score punch + появление between panels чаще.
- Путаница без splash тайма → явные скачки 45'/46'/90' в контенте битов.

## 17. Acceptance Criteria
1. В past виджет никогда не виден.
2. В матч-слое доступны opponent + score + minute.
3. Старт матча 0:0.
4. Минута меняется скачками, не realtime.
5. Нет экранов начала/конца тайма как отдельного режима — достаточно минут.
6. Нет карточек/замен/турнирной статистики.
7. Presentation может показать frame before/between/after.
8. Score punch играет при видимом изменении счёта.
9. Save/load восстанавливает числа; visibility по time_mode.

Smoke: intro 0:0; jump minute; goal punch; hide on past; show between panels; load mid-campaign.

## 18. Релиз
- Вместе с первым матч-битом в вертикальном срезе.
- Арт табло можно на заглушке → заменить comic asset.

## 19. Пострелиз
- Успех: счёт/минута считываются; стиль «как комикс».
- Провал: HUD мешает / не заметен / выглядит «как меню движка».
- v2: скины табло; аккуратный half label если плейтесты просят.

---

## Контекстные блоки
- UI/UX минимализм
- Нарратив/арт: комикс-табло
- Тех: simple widget, low-spec
- Premium: часть матч-рамки в коробке
- F2P: N/A

## Rationale gate
| Решение | Почему |
|---|---|
| Только соперник+счёт+минута | Минимум шума, всё нужное |
| Скачки минут | Сюжетный контроль, не симулятор |
| Скрыто в прошлом | Основа — жизнь вне поля |
| Комикс-стиль | Единый язык с presentation |
| Без splash таймов | Минуты достаточно, дешевле |
| Show по запросу presentation | Живое появление, не вечный overlay |
