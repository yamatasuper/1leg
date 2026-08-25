# ТЗ: match_presentation — Показ матча

## 0. Паспорт документа
- Название фичи: Показ матча
- ID / кодовое имя: `match_presentation`
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
- История изменений:
  - 2026-08-12 — первый draft

## 1. Саммари фичи
- Что это: комикс-показ матч-битов. Игрок не управляет футболистом на поле; смотрит/читает сцену исхода, редкие pressure-моменты в духе упрощённого Undertale.
- Для кого: игрок, которому нужны наглядные последствия выборов и атмосфера матча без FIFA.
- Проблема: матч нельзя дорого анимировать, но его нельзя и свести к сухой смене счёта.
- Эффект: каждый бит ощущается **новой комикс-страницей**; матч усиливает историю и не перетягивает кора (жизнь вне поля).

## 2. Бизнес-контекст
- Почему сейчас: MVP-блокер для ощущения «игра», стык с `choice_score_bridge`.
- Альтернативы: чистый UI-табло; видео; полноценный матч-геймплей; постоянные QTE.
- Почему комикс: реалистично для соло; сильно по атмосфере; хорошо стыкуется с narrative; дешевле анимации матча.
- Почему редкий Undertale-like: разнообразие и agency без превращения продукта в action/puzzle-game.

## 3. Цели
- Главная: атмосферно и читаемо показать исход матч-бита.
- Вторичные: набор шаблонов под типы исходов; ощущение новизны; редкий интерактивный pressure-бит.
- Не цели: управление игроком на поле; постоянные мини-игры; скип контента в MVP.
- Почему: основа игры вне поля; скип ломает наглядность последствий.

## 4. Метрики успеха
- Основная: на плейтесте матч-биты **запоминаются** и связываются с решениями.
- Доп.: биты не ощущаются копипастой; нет жалоб «матч слишком долгий/частый».
- Guardrail: экранное время матча << времени прошлого; QTE не бесят и почти никогда не ломают ожидаемый исход.
- Горизонт: срез друзьям → полный проход.
- Провал: «дешёвые одинаковые картинки», «хочу скип», «QTE сломали историю».

## 5. Позиционирование
- Слой presentation над `match_frame_ui` + данными `choice_score_bridge`.
- Вызывается `narrative_core` на `match_beat`.
- Не заменяет exploration/dialogue.

## 6. Scope

### In (MVP)
- Комикс-страницы для матч-битов (2–4 панели на обычный бит).
- Библиотека **6–8 базовых шаблонов** под типы исходов + вариации (порядок панелей, подписи, звук, альтернативные кадры).
- Типы покрытия: `goal_for`, `goal_against`, `dribble_win`, `dribble_lose`, `miss`, `ball_out`, плюс нейтральный/half vibe beat.
- Табло/время из `match_frame_ui` — **не всегда** поверх: может появиться до, между панелями, или после.
- Аудио ситуативно: толпа / свисток / удар / тишина / комментатор (текст в баббле всегда доступен; войс — когда есть запись).
- Переход в матч: **перелистывание комикс-страницы**.
- Переход обратно в прошлое: **обратное перелистывание** + лёгкий сдвиг тона (сепия/зерно), чтобы читалось «память/жизнь».
- **Skip запрещён** для битов и переходов в MVP.
- Редкие **pressure beats** (Undertale-inspired), 1–3 за прохождение.

### Out
- Управление футболистом / камерой на поле.
- Постоянные мини-игры на каждый бит.
- Тяжёлая 3D/скелетная анимация матча.
- Live2D как обязательство MVP.

### Future / P1
- Больше уникальных страниц на каждый гол.
- Расширенный Undertale-like набор паттернов.
- Полная озвучка комментатора на все биты.

### Зависимости
- `choice_score_bridge` → outcome pack
- `match_frame_ui` → score/clock widgets
- `narrative_core` → beat lifecycle
- `audio_atmosphere` / `art_pipeline` → поставка ассетов
- опционально `soft_stats` → визуальные намёки (пот, тяжесть ног) без раскрытия чисел

## 7. Use Cases

### 7.1 Обычный бит
- Триггер: resolve сегмента вернул outcome.
- Игрок: смотрит перелистывание → комикс → (табло в один из моментов) → обратный flip в прошлое.
- Система: выбирает шаблон + вариацию; играет таймлайн панелей; применяет подписи; **скипа нет**.
- Длительность (решение): обычный бит **10–14 с**; гол/тяжёлый пропуск **16–22 с**.

### 7.2 Пачка исходов
- Если pack > 1 item: либо одна страница с 3–4 панелями, закрывающая пачку; либо два коротких бита подряд без выхода в прошлое между ними (авторский флаг `pack_as_single_page`).

### 7.3 Pressure beat (Undertale-like)
- Триггер: редкий флаг `pressure_beat = true` на match_beat (скрипт/контент).
- Игрок: в одной панели входит в упрощённое «поле давления» (маленький хитбокс-«душа»/маркер):
  - MVP-паттерны (мало):
    1. **Timing bar** — нажать в зелёной зоне удара;
    2. **One-dodge** — уклониться от 1–2 телеграфированных объектов (не bullet hell).
- По умолчанию результат **подтверждает** исход моста (presentation juice).
- **Очень редко** (`override_chance` низкий + флаг `allow_presentation_override`) успех/провал может **переписать** outcome (например `miss` → `goal_for` или наоборот), затем clamp реализма через bridge API.
- После pressure: доиграть комикс-резолюцию под финальный outcome.

### 7.4 Негативные
- Нет ассета вариации → fallback на базовый шаблон + уникальная подпись комментатора.
- Нет войса → только текст баббла.
- Override запрещён на story-critical beats (author lock).

## 8. Сущности

### `comic_template`
- `template_id`, `outcome_types[]`, `panel_count`, `default_duration_sec`, `slot_camera` (tribune/tv/sideline)

### `comic_variation`
- `variation_id`, `template_id`, `panel_art_refs[]`, `caption_keys[]`, `audio_profile_id`, `novelty_weight`

### `presentation_timeline`
- sequence: `transition_in` → `panels[]` → optional `scoreboard_moment` → `transition_out`
- `scoreboard_moment_at`: `before` | `between_panels` | `after` | `hidden`

### `pressure_pattern`
- `pattern_id` (`timing_bar`|`one_dodge`), `duration`, `allow_override`, `override_success_outcome`, `override_fail_outcome`

### `match_beat_presentation_request`
- input from bridge/core: `beat_id`, `outcomes[]`, `score_before`, `score_after`, `flags`

## 9. Логика работы
1. Получить request.
2. Выбрать template по primary outcome.
3. Выбрать variation с учётом `novelty_weight` и истории «недавно использованных» (анти-копипаста).
4. Построить timeline; вставить scoreboard moment по правилу шаблона/рандому из whitelist.
5. Play transition_in (page flip).
6. Play panels + situational audio.
7. Если pressure — run pattern → maybe override via bridge hook → play resolve panels.
8. Play transition_out (reverse flip + past tone).
9. Вернуть управление `narrative_core`.

Правила новизны:
- Не повторять одну `variation_id` два бита подряд.
- Для `goal_for` стараться не повторять variation в одном тайме.
- Подписи комментатора ротировать даже на одном art.

Skip: **запрещён** (нет кнопки, нет input cancel timeline). Pause/menu допустимы через глобальное меню, но бит не «прокликивается».

## 10. UI/UX
- Полноэкранный комикс-режим.
- Бабблы комментатора; редко thought-bubble героя.
- Табло — виджет `match_frame_ui`, якорится в разные моменты.
- Pressure overlay: минимальный, читаемый, без перегруза RPG UI.
- Empty/Error: fallback template; error toast только если бит не стартовал.
- CTA: в обычном бите нет; в pressure — действие (click/key).

## 11. Параметры

| Параметр | Дефолт | Диапазон | Смысл |
|---|---|---|---|
| `beat_duration_normal_sec` | 12 | 10–14 | обычный бит |
| `beat_duration_goal_sec` | 18 | 16–22 | гол/тяжёлый |
| `panel_count_normal` | 3 | 2–4 | |
| `template_pool_size_mvp` | 8 | 6–8 | |
| `skip_enabled` | false | fixed MVP | |
| `pressure_beats_per_playthrough_max` | 3 | 1–3 | |
| `presentation_override_global_chance` | 0.05 | 0–0.1 | очень редко |
| `novelty_recent_block` | 2 | 1–5 | анти-повтор |
| `page_flip_sec` | 0.6 | 0.3–1.0 | |
| `past_tone_on_exit` | true | bool | сепия/зерно |

## 12. Формулы / баланс / локализация

### 12.1 Формулы

#### `pick_variation`
- candidates = variations(template) − recently_used
- weight = `novelty_weight` * rarity_boost
- pick weighted random (или author forced)

#### `maybe_override_outcome`
- if not `allow_presentation_override`: return bridge_outcome
- if random() > `presentation_override_global_chance`: return bridge_outcome
- if pressure success/fail maps to override table: call `bridge.apply_presentation_override(new_outcome)` + realism clamp
- else return bridge_outcome

#### `pack_presentation_mode`
- if `pack_as_single_page`: one timeline
- else: sequential beats without past transition between items; one transition_out at end

### 12.2 Balance vars
| name | default | notes |
|---|---|---|
| `beat_duration_normal_sec` | 12 | |
| `beat_duration_goal_sec` | 18 | |
| `presentation_override_global_chance` | 0.05 | редко |
| `pressure_beats_per_playthrough_max` | 3 | |
| `novelty_recent_block` | 2 | |
| `scoreboard_moment_weights` | before:0.25, between:0.5, after:0.2, hidden:0.05 | |
| `undertale_timing_window_sec` | 0.35 | difficulty light |
| `undertale_dodge_objects_max` | 2 | не bullet hell |

### 12.3 Localization keys
Namespace: `match_pres`

| key | RU draft | where |
|---|---|---|
| `match_pres_flip_to_match` | Страница матча | optional a11y |
| `match_pres_flip_to_past` | Страница памяти | optional a11y |
| `match_pres_comment_goal_for_01` | Есть! Смотрите на этот удар! | bubble |
| `match_pres_comment_goal_against_01` | И… вот это уже опасно. | bubble |
| `match_pres_comment_miss_01` | Мимо. Стадион замер. | bubble |
| `match_pres_comment_ball_out_01` | Мяч уходит. Передышка. | bubble |
| `match_pres_comment_dribble_win_01` | Обводка! Он проходит! | bubble |
| `match_pres_comment_dribble_lose_01` | Отбирают мяч. Жёстко. | bubble |
| `match_pres_pressure_hint_timing` | Почувствуй момент. | pressure |
| `match_pres_pressure_hint_dodge` | Не сломай себя. | pressure |
| `match_pres_hero_thought_rare_01` | Только не сейчас… | rare thought |

Комментаторские линии ротировать пачками `_01.._N` на каждый outcome.

## 13. Контекстные системы
- `match_frame_ui`: score/clock widgets on demand.
- `choice_score_bridge`: outcomes + rare override API.
- `narrative_core`: start/end beat, no skip contract.
- `art_pipeline`: comic panels, consistent ink/halftone look (ИИ ок, если не палится).
- `audio_atmosphere`: beds + rare VA.
- `soft_stats`: optional visual strain cues.

## 14. Аналитика
| event | params |
|---|---|
| `match_pres_beat_start` | `beat_id`, `template_id`, `variation_id` |
| `match_pres_beat_end` | `duration_sec` |
| `match_pres_pressure_start` | `pattern_id` |
| `match_pres_pressure_result` | `success`, `overrode` |
| `match_pres_fallback_template` | `beat_id` |

## 15. Edge Cases
- Pack с goal + boost: одна страница, последняя панель = реакция/буст.
- Trauma cut: presentation может проиграть короткий «обрыв страницы» (отдельный micro-template), дальше trauma route.
- Save mid-beat: нежелательно; якорь **до** бита или **после** (согласовать с save rules). Рекомендация: во время presentation save disabled (как mid-dialogue).
- Low-end: отключать particle; оставлять flip + panels.

## 16. Риски
- Одинаковость комиксов → novelty system + уникальные подписи + вариации кадров.
- Матч перетягивает внимание → лимиты длительности; мало pressure beats.
- QTE бесят / ломают сюжет → rare, light patterns, override almost never, story locks.
- Арт «палит ИИ» → art_pipeline checklist.
- Нет скипа бесит спидраннеров → сознательный MVP tradeoff ради immersion; P2 рассмотреть hold-to-skip только для NG+.

## 17. Acceptance Criteria
1. Бит всегда стартует page-flip in и заканчивается reverse-flip out (+ past tone).
2. Skip недоступен.
3. Есть ≥6 шаблонов, покрывающих все MVP outcomes.
4. Variation не повторяется два бита подряд.
5. Табло появляется не только «всегда сверху», а в разных моментах timeline.
6. Обычный/гол биты в целевых длительностях ±20%.
7. Pressure beats ≤3 за прохождение; patterns только timing/one-dodge.
8. Override срабатывает редко и проходит realism clamp; на locked beats невозможен.
9. Нет управления игроком на поле.
10. Save disabled during presentation timeline.

Smoke: goal_for page; goal_against; miss; pack single page; pressure no-override; pressure rare override; fallback variation; transition both ways.

## 18. Релиз
- В вертикальный срез: 2 шаблона + 1 pressure demo.
- Полный MVP: 6–8 шаблонов, ротация подписей, 1–3 pressure.
- Rollout: вместе с bridge+frame.

## 19. Пострелиз
- Успех: биты хвалят/помнят; не просят скип массово; матч не затмевает прошлое.
- Провал: копипаста; «хочу скип»; QTE злит.
- v2: больше уникальных страниц; аккуратный skip policy; шире Undertale-patterns.

---

## Контекстные блоки
- Нарратив/арт: комикс как главный язык матча.
- FTUE: первый бит обучает без скипа — игрок просто смотрит.
- Тех.ограничения: low-spec fallback.
- Premium: ценность атмосферы в коробке.
- Прогрессия: novelty anti-repeat.

## Rationale gate
| Решение | Почему |
|---|---|
| Комикс | Сильный стиль, подъёмно соло |
| Нет скипа | Последствия должны быть пережиты |
| Шаблоны + вариации | Баланс стоимости и новизны |
| Табло не всегда overlay | Живее, меньше UI-грязи |
| Аудио ситуативно | Разнообразие, реализм продакшена |
| Flip туда / reverse+tone обратно | Ясный язык «матч ↔ память» |
| Undertale-like редко | Избежать скуки без захвата кора |
| Override очень редко | Твист возможен, сюжет не ломаем |
| Save off mid-presentation | Как mid-dialogue, цельность бита |
