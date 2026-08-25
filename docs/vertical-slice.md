# Вертикальный срез — «90 минут»

Канон: Алексей Бардин, Прибрежье, «Торпедо».  
Сюжет и каст: `docs/2026-08-18_story_content_gd-spec.md`.  
Арт: `docs/2026-08-18_art_direction_gd-spec.md`.

Цель среза: **один непрерывный играбельный проход** (меню → раздевалка → бровка → матч → концовка).

Не цель: все 7 сегментов прошлого, все концовки A–Ж, полировка VA.

Рекомендуемая длина прохода: **15–25 минут**.

---

## 1. Что игрок должен прожить

1. Main menu → Новая игра  
2. Intro на «Торпедо» (Бардин, река, закрытый запад)  
3. Раздевалка: Виктор Семёнович («ты — капитан») или skip  
4. Бровка: Глок, Сокол, разговор с собой  
5. Один матч-бит → финальный свисток  
6. Ending card → титры → меню  

Локации: **2** (`loc_locker`, бровка как `loc_street`).  
NPC: Виктор Семёнович, Глок, Сокол.

---

## 2. Контент среза

### Локации
| id | имя | зачем |
|---|---|---|
| `loc_locker` | Раздевалка «Торпедо» | капитан, бинты |
| `loc_street` | Бровка / край поля | Глок, Сокол, ты |

### NPC
| id | роль |
|---|---|
| `npc_coach` | Виктор Семёнович |
| `npc_glock` | Глок, колено |
| `npc_sokol` | Сокол, жадность |

### Акты
- A0 Intro матча  
- A1 Раздевалка (`dlg_train_coach`)  
- A2 Глок (`dlg_seg1_glock`)  
- A3 Сокол (`dlg_seg2_sokol`, irony)  
- A5 С собой (`dlg_seg3_self`)  
- A7 Ending из канона (три карты)  

Пресса в срезе нет: после свистка — внутренний голос и письмо.

---

## 3. Системы: In / Stub / Out

### Must work (In)
| feature | уровень в срезе |
|---|---|
| `app_shell_menu` | New/Continue/Settings/Quit; пауза Save/Load; TV-заглушка |
| `save_anytime` | 1–3 manual + 1–3 auto на якорях; блок в диалоге и матч-бите |
| `narrative_core` | акты/сегменты/биты по каркасу выше |
| `world_exploration` | 2 локации, двери, walk, NPC interact |
| `characters_dialogue` | ≥3 варианта; grey stub optional; память 1 callback |
| `choice_score_bridge` | теги → пульс → pack; 0:0 старт |
| `match_frame_ui` | соперник + счёт + минута (комикс-заглушка) |
| `match_presentation` | 2 шаблона комикса + flip; **без скипа**; pressure optional |
| `endings_system` | resolve на свистке; card good/mid/bad (trauma out) |
| `post_match_interview` | укороченный stub |
| `pre_match_training` | укороченный stub |
| `half_time` | укороченный stub |
| `cliche_twist_content` | 1 irony-beat в сегменте 2 (не полный major set) |

### Stub OK
| feature | stub |
|---|---|
| `soft_stats` | 6 статов есть; меню «Состояние»; слабый bias; без тонкой калибровки |
| `audio_atmosphere` | 1 ambient + 1 crowd bed + 2 stingers; без VA |
| `art_pipeline` | placeholders + единый tint/palette; 1 portrait sheet temp |
| `performance_lowspec` | preset Low/Med/High хотя бы переключает scale/PP flag |

### Out of slice
- `trauma_system` (полный route)
- `screen_adaptation` phone
- полный cast / 6–10 сегментов
- commentator/thought VA
- endings gallery
- major twists ×2 (достаточно 1 irony)

---

## 4. Критерии приёмки среза

Срез принят, если друзья/ты сам можете пройти и сказать «да» на все пункты:

1. Понятна рамка **матч = жизнь** без отдельного туториал-эссе.  
2. Есть ходьба + разговор, не «только новелла».  
3. После сегмента видно последствие на матче (счёт/событие).  
4. Нельзя скипнуть матч-бит; нельзя сейвиться mid-dialogue.  
5. Save/Load из паузы работает; Continue после перезапуска работает.  
6. Два тайма + перерыв реально прожиты (даже укороченно).  
7. Доходим до ending card.  
8. Два разных стиля выбора дают **разный счёт и/или разный ending**.  
9. Арт может быть заглушкой, но UI читаемый.  
10. Нет софтлока дверей/диалогов.

---

## 5. Порядок сборки (практический)

### Sprint 0 — оболочка (2–4 дня)
- Empty scene bootstrap
- `app_shell_menu` stub
- `save_anytime` stub (хотя бы 1 manual + 1 auto)

### Sprint 1 — past loop
- Player walk + 2 locations + door
- 1 dialogue graph with 3 choices
- Journal state tab (soft stub)

### Sprint 2 — match loop
- Bridge resolve + score/minute
- Comic beat 2 templates + flip in/out
- Hook: segment end → beat → back

### Sprint 3 — full slice spine
- Intro + training stub + 3 segments + HT + interview + ending
- Autosave anchors
- Playtest pass #1

### Sprint 4 — tighten
- Second playstyle → different ending
- 1 irony beat
- Audio stubs + preset graphics
- Playtest pass #2 (друзья)

---

## 6. Контент

Файлы: `docs/vertical-slice/content/` — тот же канон, что библия 18.08 (Бардин / Глок / Сокол / Виктор Семёнович).

---

## 7. Риски среза

| риск | митигация |
|---|---|
| Слишком длинно писать | жёсткий лимит 3 сегмента |
| Матч перетягивает | биты 10–14 с, мало панелей |
| Застряли на арте | placeholders законны |
| Нет ощущения «игра» | не вырезать walk |
| Одна концовка всегда | тест двух стилей прохождения |

---

## 8. Definition of Done

Есть PC build (или Play Mode в Unity), который проходит путь §1 без читов и без дыр в логике §4.

После DoD: расширять к полному MVP (больше сегментов → trauma → полировка audio/art → phone).
