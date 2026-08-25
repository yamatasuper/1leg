# ТЗ: art_pipeline — Арт-пайплайн

## 0. Паспорт документа
- Название фичи: Арт-пайплайн
- ID / кодовое имя: `art_pipeline`
- Проект / версия: **90 минут** / P1
- Фичеовнер: автор (соло, мало опыта) 
- Статус: draft
- Дата создания: 2026-08-12
- Дата обновления: 2026-08-12
- Связанные документы:
  - `docs/project-concept.md`
  - `docs/feature-index.md`
  - `docs/2026-08-12_match_presentation_gd-spec.md`
  - `docs/2026-08-12_world_exploration_gd-spec.md`
  - `docs/2026-08-12_app_shell_menu_gd-spec.md`
  - `docs/2026-08-12_characters_dialogue_gd-spec.md`
  - `docs/2026-08-12_audio_atmosphere_gd-spec.md`
  - `docs/2026-08-12_performance_lowspec_gd-spec.md`
  - `docs/2026-08-18_art_direction_gd-spec.md` — **канон стиля, каста, локаций и промптов; важнее style prefix в этом ТЗ**
- История изменений:
  - 2026-08-12 — первый draft (beginner-friendly)
  - 2026-08-18 — визуальный канон в art bible

## 1. Саммари фичи
- Что это: **единый** визуальный язык на past / match-comic / TV-menu + простой соло-пайплайн производства арта. ИИ можно использовать, но **не as-is**: короткий обязательный ручной pass. Анимация в P1 — **только лёгкая** (page-flip, Ken Burns, простые UI/персонаж-трансформы), без сложных ригов. В спеке есть мини стиль-гайд. При нехватке времени первым режем **уникальные вариации матч-панелей**.
- Для кого: автор с малым опытом, которому нужен повторяемый процесс без «арт-дирекции на троих».
- Проблема: без пайплайна стиль плывёт, ИИ палится, объём убивает соло.
- Эффект: игра выглядит цельно; матч-комикс и мир читаются как одна вселенная.

## 2. Бизнес-контекст
- Почему P1: после blockout; сначала заглушки (уже в концепте).
- Альтернативы: три несвязанных стиля; ручной арт only; сложная анимация героя.
- Почему unified + AI-with-pass + minimal motion: максимум качества на единицу опыта/времени.

## 3. Цели
- Главная: единый стиль + повторяемый beginner pipeline.
- Вторичные: ИИ-safe checklist; минимальная анимация; приоритеты cut при нехватке времени.
- Не цели: AAA персонажная анимация; отдельный стиль на каждый режим; публикация сырого ИИ-арта.
- Почему: соло и мало опыта.

## 4. Метрики успеха
- Основная: на плейтесте «один мир», ИИ не бросается в глаза.
- Guardrail: lowspec не ломает обязательные кадры; текст на комиксе читаем.
- Провал: каша стилей; «это Stable Diffusion»; пустые локации без характера.

## 5. Позиционирование
- Поставляет ассеты в world / portraits / match comic templates / TV menu chrome.
- Согласован с `match_presentation` novelty (вариации — желательны, но режутся первыми).
- Texture budgets с `performance_lowspec`.

## 6. Scope

### In (P1)
- **Unified look** across:
  - past exploration
  - match comic pages
  - TV main/pause menu
- Shared: палитра, линия/зерно, лица, свет (мрак + редкие «светлое будущее» вспышки).
- Beginner pipeline (см. §9).
- AI generation allowed → **mandatory human pass** (не as-is).
- Motion only:
  - comic page-flip
  - Ken Burns (slow zoom/pan on stills)
  - simple sprite bob/flip for walk if using 2D character
  - UI fades
  - **No** complex bone/Live2D/3D match animation requirement
- Embedded style guide (§12.3 / §Style).
- Placeholder → polish order.
- Cut priority when short on time (§9).

### Out
- Separate art Bibles per mode that diverge visually
- Full animated cutscenes
- Photoreal FIFA look
- Shipping unedited AI dumps

### Future
- More match variations; richer walk cycles; hand-painted hero shots

### Зависимости
- All presentation features; audio optional sync for flip; lowspec compression

## 7. Use Cases

### 7.1 New location
- Blockout grey → AI draft under style prompt → human pass → import + compression.

### 7.2 Match template panel
- Reuse template slots; swap captions; add new art only if novelty budget allows.

### 7.3 Portrait
- High priority keep; consistent face sheet for cast.

### 7.4 Time-crash week before slice
- Freeze new match variations; keep portraits + 1 set panels per outcome; simplify location bg detail.

## 8. Сущности

### `art_asset`
- `id`, `type` (location|portrait|comic_panel|menu_chrome|ui_icon), `path`, `preset_mips`

### `comic_template_art_set`
- panels for outcome types (min set)

### `style_token`
- palette refs, line weight, grain, banned AI artifacts list

### `placeholder_flag`
- asset marked temp until polish

## 9. Логика / пайплайн (beginner)

### Pipeline steps (обязательные)
1. **Brief** (1–3 предложения: настроение, кто/где, что нельзя).
2. **Reference lock** (2–4 рефа из moodboard; не менять mid-scene).
3. **AI draft** (если используется) одним и тем же style prompt prefix.
4. **Human pass (обязателен, короткий):**
   - crop/composition
   - color grade to palette
   - убрать лишние пальцы/текст-артефакты/plastic skin/watermark
   - проверить лицо vs portrait sheet
   - добавить лёгкое зерно/комикс-overlay при необходимости
5. **Export** правильных размеров + mip/compress.
6. **In-engine check** next to text/bubbles + low preset.
7. Mark placeholder cleared.

### Почему не as-is
Мало опыта ⇒ нужен короткий чеклист, а не «стань иллюстратором». 10–20 минут на кадр better, чем сырой ИИ.

### Motion policy (мало опыта)
- Match: flip + Ken Burns only.
- World: static bg + simple character move/bob; NPCs static poses ok.
- Menu: static TV frame + light flicker optional (can skip).

### Cut order (time shortage)
1. **First cut:** extra unique match panel variations (reuse templates + caption/audio novelty).
2. Then: secondary location background detail / props density.
3. **Protect longer:** NPC portraits, key goal/concede panels (one solid set), readable UI/TV chrome basics.

## 10. UI/UX (production, not player)
- Naming: `art/<type>/<id>_v##.png`
- Placeholder tint or watermark in editor only (not shipping).
- No player-facing “AI generated” label.

## 11. Параметры

| Параметр | Дефолт | Смысл |
|---|---|---|
| `unified_style` | true | |
| `ai_allowed` | true | |
| `ai_as_is_allowed` | false | |
| `human_pass_required` | true | |
| `motion_complex_rigs` | false | |
| `ken_burns_enabled` | true | |
| `page_flip_enabled` | true | |
| `cut_first` | match_panel_variations | |
| `protect_portraits` | true | |
| `style_guide_embedded` | true | |

## 12. Формулы / баланс / локализация

### 12.1 Production “формула”
```
brief + locked refs + (AI draft?) + human pass + engine check = shippable
```

### 12.2 Budgets (soft)
| type | MVP/P1 target | notes |
|---|---|---|
| locations | 3–5 | world spec |
| portraits | main cast | protect |
| panels per outcome | 1 solid set | variations optional |
| menu chrome | 1 TV frame set | |

### 12.3 Style guide

Канон стиля, палитры и prompt prefix — **только** `docs/2026-08-18_art_direction_gd-spec.md`. Этот ТЗ задаёт пайплайн (brief → draft → human pass), не внешний вид.

Не использовать старый prefix «muted cinematic comic realism / Disco past / bright future cyan». Не рисовать Артемия, Лену, Веру.

## 13. Контекстные системы
- `match_presentation`: templates/panels/Ken Burns/flip.
- `world_exploration`: locations, character sprites.
- `characters_dialogue`: portraits.
- `app_shell_menu`: TV chrome.
- `performance_lowspec`: sizes/mips.
- `cliche_twist_content`: avoid stock visual clichés in “good” imagery.
- `audio_atmosphere`: flip timing sync optional.

## 14. Аналитика
Dev-only: count placeholders remaining; playtest “AI noticeable?” y/n.

## 15. Edge Cases
- AI face drift → freeze portrait sheet; regenerate with face ref always.
- Ultrawide pads → extend bg softly, don’t stretch characters.
- Low res scale → ensure portraits still readable.
- Missing panel variation → fallback template art (allowed).

## 16. Риски
- Scope creep animation → forbidden complex rigs in P1.
- AI detectability → human pass mandatory.
- Burnout → cut variations first.
- Style drift over months → locked prompt prefix + palette.

## 17. Acceptance Criteria
1. Unified style across past/match/menu.
2. No shipped AI asset without human pass checklist.
3. Motion limited to flip/Ken Burns/simple transforms.
4. Style rules documented in this spec and followed on new assets.
5. Time-crash cut order respected (variations first).
6. Portraits + key outcome panels present for MVP spine.
7. Placeholders replaced or explicitly listed as known temp for slice only.
8. Lowspec import sizes OK.

Smoke: one location + one portrait + one goal page + TV frame in-engine; AI-pass checklist signed; low preset readable.

## 18. Релиз
- Vertical slice: placeholders ok if marked; 1 polished chain (portrait↔panel↔menu).
- P1: full cast portraits, location set 3–5, outcome panel set, TV chrome.

## 19. Пострелиз
- Успех: «цельно», «не палит ИИ», матч-комикс узнаваем.
- Провал: каша стилей; AI artifacts; слишком много недоделанных вариаций.
- v2: more variations; slightly richer walk anim.

---

## Human pass checklist (print)
- [ ] Composition/crop ok for bubbles/UI
- [ ] Palette match
- [ ] Faces consistent
- [ ] No AI artifacts/watermark/text mush
- [ ] Grain/line unified
- [ ] Exported size/mips
- [ ] Looks ok on Low

## Rationale gate
| Решение | Почему |
|---|---|
| Единый стиль | Запрос + цельность |
| AI + короткий human pass | Мало опыта, но нельзя as-is |
| Только Ken Burns/flip | Реалистично по навыку |
| Стиль-гайд в спеке | Нужен якорь без отдельного отдела |
| Режем вариации панелей первыми | Novelty можно заменить подписями/звуком |
| Бережём портреты | Эмоциональное ядро |
