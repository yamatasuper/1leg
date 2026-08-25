# 90 минут — индекс фич

Источник концепта: `docs/project-concept.md`  
Модель дистрибуции: `premium`

## Канон контента (приоритет)

Новые документы — канон сюжета, арта, музыки, каста и промптов:

| файл | зачем |
|---|---|
| `docs/ai-content-index.md` | маршрутизатор |
| `docs/2026-08-18_story_content_gd-spec.md` | сюжет |
| `docs/2026-08-18_music_direction_gd-spec.md` | музыка |
| `docs/2026-08-18_art_direction_gd-spec.md` | арт |

Срез (`docs/vertical-slice/content/`) — тот же канон, укороченный проход.

## Feature index

| feature_id | feature_name | brief | player_value | priority | depends_on | distribution | status | spec_doc |
|---|---|---|---|---|---|---|---|---|
| `app_shell_menu` | Меню и оболочка | TV-стиль; New/Continue/Settings/Quit; пауза Save/Load | Вход в игру без трения | MVP | — | premium | planned | `docs/2026-08-12_app_shell_menu_gd-spec.md` |
| `save_anytime` | Сохранения | Ручные + авто на якорях; список; блок в диалоге/матч-бите | Безопасность экспериментов с выборами | MVP | `app_shell_menu` | premium | planned | `docs/2026-08-12_save_anytime_gd-spec.md` |
| `narrative_core` | Ядро сюжета и сегментов | Сегменты прошлого → возврат к матчу; ~неделя вечеров | Главный смысл и удержание | MVP | `save_anytime` | premium | slice | `docs/2026-08-12_narrative_core_gd-spec.md` |
| `world_exploration` | Мир вне поля | Ходьба/переходы по локациям (основа геймплея) | Исследование себя через места и людей | MVP | `narrative_core` | premium | planned | `docs/2026-08-12_world_exploration_gd-spec.md` |
| `characters_dialogue` | Персонажи и диалоги | Разговоры, отношения, трудные решения | Эмоциональное ядро MVP | MVP | `narrative_core`, `world_exploration` | premium | planned | `docs/2026-08-12_characters_dialogue_gd-spec.md` |
| `cliche_twist_content` | Ломание клише | Контент-стандарт: 1–2 жёстких твиста + banned list | Удивление, «не угадал заранее» | MVP | `characters_dialogue` | premium | planned | `docs/2026-08-12_cliche_twist_content_gd-spec.md` |
| `choice_score_bridge` | Выборы → счёт | После сегмента: гол / пропущенный | Вариативность не только «как в новелле» | MVP | `narrative_core` | premium | slice | `docs/2026-08-12_choice_score_bridge_gd-spec.md` |
| `soft_stats` | Временные бусты | Мотивация, энергия, сила… влияют на итоговый счёт | Ощущение «+мотивация как в жизни» | P1 | `choice_score_bridge` | premium | planned | `docs/2026-08-12_soft_stats_gd-spec.md` |
| `match_frame_ui` | Рамка матча | Табло, время, 2 тайма, ритуал матча | Понятный пульс кампании | MVP | `choice_score_bridge` | premium | slice | `docs/2026-08-12_match_frame_ui_gd-spec.md` |
| `match_presentation` | Показ матча | Иллюстрации + простые анимации + звук/комментарий | Атмосфера матча без FIFA-симуляции | MVP | `match_frame_ui` | premium | slice | `docs/2026-08-12_match_presentation_gd-spec.md` |
| `pre_match_training` | Тренировка | Сцена/акт до матча | Настройка тона и лёгкий вход | P1 | `narrative_core`, `match_frame_ui` | premium | slice | `docs/2026-08-12_pre_match_training_gd-spec.md` |
| `half_time` | Перерыв | Акт между таймами | Передышка и смена фокуса | P1 | `match_frame_ui`, `narrative_core` | premium | slice | `docs/2026-08-12_half_time_gd-spec.md` |
| `post_match_interview` | Послематчевое интервью | Финальный акт: пресса + монолог (ending уже зафиксирован) | Осмысление и закрытие арки | P1 | `endings_system`, `characters_dialogue` | premium | slice | `docs/2026-08-12_post_match_interview_gd-spec.md` |
| `endings_system` | Концовки | Ровно 4: good / mid / bad / trauma; резолв на финальном свистке | Реиграбельность и вес выборов | MVP | `choice_score_bridge`, `narrative_core` | premium | slice | `docs/2026-08-12_endings_system_gd-spec.md` |
| `trauma_system` | Травмы | Останавливают матч, не жизнь; доп. концовка; skip остатка сюжета | Резкий поворот судьбы + agency через load | P1 | `endings_system`, `save_anytime`, `narrative_core` | premium | planned | `docs/2026-08-12_trauma_system_gd-spec.md` |
| `audio_atmosphere` | Звук и музыка | Тишина+эмбиент; crowd bus; VA только key/редкие мысли | Погружение | P1 | `match_presentation` | premium | planned | `docs/2026-08-12_audio_atmosphere_gd-spec.md` |
| `art_pipeline` | Арт-пайплайн | Единый стиль; ИИ+ручной pass; flip/Ken Burns | Визуальная цельность | P1 | `match_presentation`, `world_exploration` | premium | planned | `docs/2026-08-12_art_pipeline_gd-spec.md` |
| `performance_lowspec` | Стабильность и low-spec | ~30 FPS, пресеты L/M/H, меньше нагрев | Доступность на слабом железе | P1 | `app_shell_menu` | premium | planned | `docs/2026-08-12_performance_lowspec_gd-spec.md` |
| `screen_adaptation` | Адаптация экранов | PC аспекты + телефон landscape; один layout; стик+tap | Комфорт на разных дисплеях | P2 | `app_shell_menu` | premium | planned | `docs/2026-08-12_screen_adaptation_gd-spec.md` |

## Dependency map (кратко)

- `app_shell_menu` → `save_anytime` → `narrative_core`
- `narrative_core` → `world_exploration` → `characters_dialogue` → `cliche_twist_content`
- `narrative_core` → `choice_score_bridge` → `match_frame_ui` → `match_presentation`
- `choice_score_bridge` → `endings_system`
- `endings_system` + `save_anytime` → `trauma_system`
- `endings_system` + `characters_dialogue` → `post_match_interview`
- `match_frame_ui` → `pre_match_training`, `half_time`
- `choice_score_bridge` → `soft_stats` (усиление, не блокер вертикального среза)
- `match_presentation` → `audio_atmosphere`, `art_pipeline`
- `app_shell_menu` → `performance_lowspec`, `screen_adaptation`

## Top-3 риска

1. **`match_presentation`** — анимация матча дорогая; нужен пайплайн «иллюстрации / простой motion / UI», иначе сорвёт сроки.
2. **Баланс `match_presentation` ↔ (`world_exploration` + `characters_dialogue`)** — если матч перетянет экранное время, сломается обещание «основа вне поля».
3. **`trauma_system` + `endings_system`** — доп. концовка со skip сюжета легко ощущается наказанием; критичны ясный foreshadowing и `save_anytime`.

## Предложенный порядок разработки

1. `app_shell_menu` + `save_anytime` (на заглушках)
2. `narrative_core` + `world_exploration` + `characters_dialogue` (первый сегмент)
3. `choice_score_bridge` + `match_frame_ui` + `match_presentation` (минимальный показ)
4. Вертикальный срез: тренировка → сегменты → матч-биты на 2 тайма → хотя бы 1 путь к концовке (`endings_system`, заглушки для `pre_match_training` / `half_time` / `post_match_interview`)
5. `cliche_twist_content` вшивать в контент по мере написания, не отдельным «после»
6. `trauma_system` + полный набор концовок
7. `soft_stats`, `audio_atmosphere`, `art_pipeline`, `performance_lowspec`
8. `screen_adaptation` (мобилки)

Принцип: **сначала база и сюжет на заглушках, потом полировка.**

Актуальный план среза: `docs/vertical-slice.md`  
Sprint 0 (shell): `docs/vertical-slice/SPRINT0.md`
