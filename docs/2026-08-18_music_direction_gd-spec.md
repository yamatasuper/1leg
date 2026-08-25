# ТЗ: music_direction — Музыкальная библия для ИИ

## 0. Паспорт документа
- Название: Музыкальная библия (контент + промпты)
- ID: `music_direction`
- Проект: **90 минут** / полный канон кампании
- Фичеовнер: автор (соло)
- Статус: draft (content bible)
- Дата создания: 2026-08-18
- Дата обновления: 2026-08-18
- Связанные документы:
  - `docs/ai-content-index.md`
  - `docs/2026-08-18_story_content_gd-spec.md`
  - `docs/2026-08-12_audio_atmosphere_gd-spec.md`
  - `docs/project-concept.md`
- История изменений:
  - 2026-08-18 — адаптация исходного музыкального документа под работу с ИИ

## 1. Саммари
Это не плейлист «под радио». Это набор **редких кью**, которые говорят голосом Бардина. По умолчанию в прошлом — тишина и эмбиент (`audio_atmosphere`). Сюжетный трек включается только на указанных `scene_id` / `segment_id`.

## 2. AI contract (читать перед генерацией)

Ты — композитор атмосферного пост-рока / эмбиента для игры «90 минут».

**Сначала:** сюжетная библия (хотя бы паспорт героя + тон) и этот файл.  
**Потом:** генерируй один `cue_id` за раз.

### Обязательный выход
Для каждого трека верни:
1. `cue_id`
2. короткий brief (3–5 предложений: что чувствует Бардин)
3. English prompt для генератора (Suno / Udio / аналог) — 1 блок
4. negative prompt
5. arrangement (инструменты по секциям, секунды)
6. где в игре играет / где **не** играет
7. какие мотивы (`motif_river` / `motif_pitch` / `motif_farewell` / `motif_hope`) обязательны

### Запрещено
- вокал со словами, рэп, гимн, stadium rock, EDM drop, эпик-трейлер, «We Are The Champions»
- непрерывный OST на всю игру
- сладкие мажорные разрешения в треках 1–11
- очевидный ИИ-пластмассовый суперсоу
- менять длительность больше чем на ±20% от слота без пометки `TBD`

### Тон
Меланхолия, ностальгия, внутренняя борьба, одиночество, редкая надежда. Минимум нот. Паузы важнее заполнения. Каждый трек — портрет человека, который теряет карьеру и ищет себя.

## 3. Приоритет над `audio_atmosphere`

Эта библия **главнее** ТЗ 12 августа. 12 сюжетных треков + 5 эмбиентов — канон. Шины Master/Music/SFX/Voice/Crowd и human listen pass из старого ТЗ оставляем как технику, не как запрет альбома.

- Треки играются как кью на `scene_id` / `segment_id` (полный файл на ключевых сценах; в ходьбе можно укоротить 20–90 сек).
- Не превращать в radio-OST нон-стоп — но и не выкидывать сюжетные треки ради «тишины ради тишины».
- Ударные только в `mus_run` и `mus_pain`. Crowd не подменяет мелодию.

## 4. Стиль-лок

**Жанр:** атмосферный пост-рок / эмбиент + советский синтезатор 80-х + современный минимализм.

**Инструменты (можно):**
- электрическое пианино «старое», лёгкий tape wow
- синтезаторы в духе Juno-60 / DX7, delay + long reverb
- виолончель, скрипка — длинные ноты, мало вибрато
- акустическая гитара — только общежитие и эпилог
- слайд-гитара — только завод
- ударные — только ключевые моменты (`mus_run`, `mus_pain`)
- полевые шумы: дождь, ветер, гул стадиона, дыхание, металл, вода

**Эталоны настроения (не копировать мелодии):**
Vangelis / Blade Runner; Gustavo Santaolalla / TLOU; Cliff Martinez / Drive; Hans Zimmer / Interstellar (орган, нарастание); советское кино 70–80-х; Sigur Rós; Hammock.

**Banned looks (звук):** trailer brass, trap hats, stadium PA music, happy acoustic montage, glitch-core, lo-fi beats «для учёбы».

## 5. Повторяющиеся мотивы

| id | описание | где обязателен |
|---|---|---|
| `motif_river` | простая фраза пианино, 4 ноты, медленная | `mus_river`, `mus_wife`, `mus_whistle`, `mus_new_life` |
| `motif_pitch` | быстрая фортепианная ячейка | `mus_run`; в `mus_pain` — искажённая, диссонанс |
| `motif_farewell` | короткая струнная фраза | `mus_money`, `mus_factory`, `mus_dorm` |
| `motif_hope` | светлая синтезаторная фраза | **только** `mus_new_life` |

Мотивы должны узнаваться при повторном прослушивании. Не оркеструй их каждый раз по-новому до неузнаваемости.

## 6. Карта кью → сцены

| cue_id | RU имя | слот | когда играет | scene / segment ids |
|---|---|---|---|---|
| `mus_exit` | Выход на поле | 2:00–2:30 | пролог → выход | `pro_01`…`pro_05`, `trg_01` |
| `mus_locker` | Раздевалка | 3:00–3:30 | пролог и перерыв | `pro_*`, `ht_*` |
| `mus_run` | Бег | 2:30–3:00 | динамичные биты 1-го тайма | `trg_04`, `trg_07`, `trg_10`, `trg_15` |
| `mus_river` | Воспоминание о реке | 3:00–4:00 | река / жёлтое пальто / микропаузы реки | `trg_08`, `trg_21`, `mp_river`, `trg_24` |
| `mus_money` | Воспоминание о деньгах | 3:00–3:30 | сегмент квартиры | `seg_apartment_2017` |
| `mus_factory` | Воспоминание о заводе | 3:00–4:00 | сегмент завода | `seg_factory_2002` |
| `mus_dorm` | Воспоминание об общежитии | 2:30–3:00 | сегмент общежития | `seg_dorm_2008` |
| `mus_wife` | Воспоминание о жене | 3:30–4:30 | ресторан / пальто | `seg_restaurant_2015`, `trg_08` (слой) |
| `mus_halftime` | Перерыв | 2:30–3:00 | тишина раздевалки 2-й тайм ещё не начался | `ht_*` underscore |
| `mus_pain` | Боль | 2:30–3:30 | колено, падение, БК | `trg_27`, `mp_pain`, ending A |
| `mus_whistle` | Тишина после свистка | 3:00–4:00 | финальный свисток / концовки на поле | `trg_29`, `mp_whistle`, endings B–G field |
| `mus_new_life` | Новая жизнь | 3:00–4:00 | эпилог | `ep_01`…`ep_03` |
| `amb_crowd` | Гул стадиона | ~5:00 loop | матч bed (шина Crowd, не Music) | все `trg_*` на поле |
| `amb_river` | Шум реки | ~5:00 loop | вид на реку | `mp_river`, `ep_03` |
| `amb_locker` | Тишина раздевалки | ~3:00 loop | раздевалка | `pro_*`, `ht_*` |
| `amb_heartbeat` | Сердцебиение | ~2:00 | редкий слой тревоги | `mus_exit`, `trg_07`, `trg_27` |
| `amb_factory` | Заводской гул | ~4:00 loop | завод | `seg_factory_2002` |

Общая «альбомная» длительность полных файлов: 33–45 мин. В игре режутся под кью.

## 7. Карточки треков и промпты

Общий English prefix (не менять без нужды):

```
Sparse melancholic post-rock ambient, analog 1980s Soviet synth warmth, old electric piano, long reverb, delay, cinematic but quiet, no vocals, no trailer drums, no EDM, film-score intimacy, imperfect tape, wide stereo, dynamic range from whisper to swell
```

Negative prefix:

```
vocals, lyrics, rap, EDM, trap, stadium anthem, epic trailer, brass hits, glitch, lo-fi hip hop, happy major resolution, stock royalty-free sports music, cheesy guitar solo
```

### `mus_exit` — Выход на поле
- Настроение: ожидание, тревога, последний раз.
- Инструменты: синтезаторная педаль; сердцебиение (низкий бас); гул стадиона далеко; одна повторяющаяся нота пианино.
- Картина: музыка до выхода. Прыжок в неизвестность.
- Prompt: `{prefix}, slow rising synth pad, distant stadium murmur, single repeating piano note, quiet heartbeat bass pulse, 120-150 seconds, waiting before a last match, anxious but still`
- Arrangement: 0:00 pad+crowd air → 0:40 piano note → 1:20 heartbeat slightly closer → no climax.

### `mus_locker` — Раздевалка
- Настроение: теснота, пот, дерево, память.
- Инструменты: низкий Juno-эмбиент; скрип дерева; шёпот/дыхание; пианино из другой комнаты.
- Prompt: `{prefix}, closed locker room, wooden creaks, muffled voices unintelligible, distant piano through a wall, Juno-60 low drone, 180-210 seconds, intimate, no beat`

### `mus_run` — Бег
- Настроение: адреналин, борьба, молодость, которая уже не возвращается.
- Инструменты: быстрое минималистичное пианино; ровная электронная перкуссия; синтез-бас; короткие струнные.
- Prompt: `{prefix}, driving but not chaotic piano ostinato, steady electronic pulse like running breath, short string stabs, 150-180 seconds, athletic melancholy, motif_pitch`
- Не превращать в sports rock.

### `mus_river` — Река
- Настроение: ностальгия, спокойствие, принятие.
- Инструменты: простое грустное пианино; виолончель; вода; тихий синтезатор с середины.
- Prompt: `{prefix}, simple 4-note piano motif_river, long cello notes, river water ambience, quiet synth space in the middle, 180-240 seconds, flowing time, no drums`

### `mus_money` — Квартира / деньги
- Настроение: пустое богатство, холод, одиночество.
- Инструменты: холодное e-piano с вибрато; механическое арпеджио; тиканье часов; бутылка; низкий бас; диссонанс в середине.
- Prompt: `{prefix}, cold electric piano vibrato, mechanical synth arpeggio looping like empty luxury, clock ticks, low tension bass, mid-track dissonance, 180-210 seconds, motif_farewell, hollow apartment`

### `mus_factory` — Завод
- Настроение: отец, металл, тяжесть.
- Инструменты: индустриальные шумы; гул синтеза; одинокий слайд гитары; дыхание.
- Prompt: `{prefix}, abandoned factory, metal resonance, machine hum, lonely slide guitar like a father's voice, breath samples, 180-240 seconds, motif_farewell, no clean pop guitar`

### `mus_dorm` — Общежитие
- Настроение: молодость, бедность, ещё живое.
- Инструменты: простая акустическая гитара; светлый ностальгический синтез; смех / мяч далеко.
- Prompt: `{prefix}, simple acoustic guitar like a roommate song, bright but already sad synth, distant laughter and a ball bounce, 150-180 seconds, youth without money, motif_farewell, no campfire singalong`

### `mus_wife` — Жена / ресторан
- Настроение: любовь, потеря, непролитые слёзы.
- Инструменты: пианино с delay (фраза чуть меняется каждый цикл); виолончель/скрипка; дождь; пустая комната.
- Prompt: `{prefix}, delayed piano melody that almost repeats but changes, aching cello, rain ambience, empty restaurant after goodbye, 210-270 seconds, motif_river, no romantic pop ballad`

### `mus_halftime` — Перерыв
- Настроение: усталость, сомнение, тишина тяжелее стадиона.
- Инструменты: низкий эмбиент; далёкое пианино; тяжёлое дыхание; длинные паузы.
- Prompt: `{prefix}, almost silence, low frequency room tone, distant piano, tired breath, long rests, 150-180 seconds, decision whether to keep fighting`

### `mus_pain` — Боль
- Настроение: колено, сопротивление, надлом.
- Инструменты: диссонантное пианино; гудящий синтез; медленные тяжёлые удары; скрежет.
- Prompt: `{prefix}, dissonant piano, distorted motif_pitch, buzzing low synth, slow heavy drums, wood creak and crack, 150-210 seconds, physical collapse, not horror stingers`

### `mus_whistle` — После свистка
- Настроение: опустошение, принятие, тонкий свет в конце.
- Инструменты: простое пианино; длинные струнные; дождь или ветер; более светлый синтез к концу.
- Prompt: `{prefix}, after the final whistle, simple repeating piano seeking rest, long strings, wind or rain, faint hopeful synth space, 180-240 seconds, motif_river, empty stadium`

### `mus_new_life` — Эпилог
- Настроение: принятие, начало, тепло которого не было раньше.
- Инструменты: светлое простое пианино; тёплая акустическая гитара; лёгкие утренние струнные; открытый синтез.
- Prompt: `{prefix}, first appearance of motif_hope, simple bright piano without syrup, warm acoustic guitar, light morning strings, open synth, 180-240 seconds, new cheap apartment, river still flowing, no Hollywood fanfare`

## 8. Техтребования

| параметр | значение |
|---|---|
| `audio_format` | wav или flac |
| `bit_depth` | 24 |
| `sample_rate_hz` | 48000 |
| `dynamic_range` | широкий; не давить в loudness war |
| `stereo_field` | широкое, immersive, без хаотичного ping-pong |
| `prefer_sources` | аналог / живые струны / реальное пианино или честные библиотеки |

Стиль письма трека: минимализм, повторяющийся мотив, развитие без перегруза, свой «голос» у каждого кью, общая фраза между треками.

## 9. Сущности

### `music_cue`
- `cue_id`, `ru_title`, `duration_min_sec`, `duration_max_sec`
- `mood_tags[]`
- `instruments[]`
- `motifs[]`
- `scene_ids[]`
- `bus` = `music` (сюжетные) или связанные `ambient_bed` на `sfx`/`crowd`

### `music_motif`
- `motif_id`, `description`, `allowed_cue_ids[]`

### `generation_job`
- `cue_id`, `prompt`, `negative_prompt`, `seed` (TBD), `human_pass` bool

## 10. Формулы / баланс / локализация

### 10.1 Когда играть сюжетный трек
```
play_story_cue(cue_id) =
  current_scene in cue.scene_ids
  AND music_bus > 0
  AND not (continuous overlay)
  AND if past_exploration: only location-bound cues (money/factory/dorm/wife)
```

Почему так: иначе 12 треков убьют тишину, ради которой заведён `audio_atmosphere`.

### 10.2 Переменные баланса

| name | тип | дефолт | диапазон | зачем |
|---|---|---|---|---|
| `music_cue_fade_in_sec` | float | 2.5 | 0.5–6 | мягкий вход |
| `music_cue_fade_out_sec` | float | 3.5 | 1–8 | не резать хвост реверба |
| `past_story_cue_gain` | float | 0.55 | 0.3–0.8 | не забивать шаги/текст |
| `match_story_cue_gain` | float | 0.45 | 0.2–0.7 | толпа важнее мелодии |
| `pain_cue_gain` | float | 0.7 | 0.5–0.9 | физический удар |
| `epilogue_cue_gain` | float | 0.65 | 0.5–0.85 | тепло без гимна |
| `allow_underscore_under_dialogue` | bool | true | — | только locker/halftime, тихо |
| `ai_music_as_is_allowed` | bool | false | — | нужен listen pass |

### 10.3 Ключи локализации

Namespace: `music`

| key | RU | где |
|---|---|---|
| `music_cue_exit` | Выход на поле | кредиты / дев-меню |
| `music_cue_locker` | Раздевалка | |
| `music_cue_run` | Бег | |
| `music_cue_river` | Воспоминание о реке | |
| `music_cue_money` | Воспоминание о деньгах | |
| `music_cue_factory` | Воспоминание о заводе | |
| `music_cue_dorm` | Воспоминание об общежитии | |
| `music_cue_wife` | Воспоминание о жене | |
| `music_cue_halftime` | Перерыв | |
| `music_cue_pain` | Боль | |
| `music_cue_whistle` | Тишина после свистка | |
| `music_cue_new_life` | Новая жизнь | |
| `music_credits_line` | Музыка: атмосферный пост-рок и эмбиент | титры |

Игрок в MVP **не** видит названия кью. Ключи нужны для титров, дев-меню и заказов генерации.

## 11. Human listen pass (обязателен)

- [ ] Нет вокала и спортивного гимна
- [ ] Мотив на месте и узнаваем
- [ ] Не звучит как сток / очевидный ИИ-суперсоу
- [ ] Динамика живая, не одна громкость
- [ ] Стыкуется с соседним кью по тональности/настроению
- [ ] Под текст диалога не орёт
- [ ] Экспорт 24/48

## 12. Acceptance
1. 12 сюжетных кью описаны и промптятся по одному `cue_id`.
2. 4 мотива не размазаны по всем трекам.
3. `motif_hope` только в эпилоге.
4. Не ломает silence-first.
5. ИИ-файл без listen pass не считается готовым.

## 13. TBD
- точная тональность / BPM на кью
- стемы (piano / synth / noise) vs монофайл
- нужны ли отдельные кью для школы, больницы, офиса контракта (сейчас тишина + локационный эмбиент)
