# Sprint 0 — Unity shell

## Что сделано
- Runtime bootstrap при Play (сцена может оставаться `SampleScene`)
- TV-меню: Новая игра / Продолжить / Настройки / Выход
- Пауза (Esc): Сохранить / Загрузить / Настройки / В меню / Выход
- `SaveService`: 3 ручных + 3 авто слота (JSON в `persistentDataPath/saves`)
- Автосейв на New Game
- Confirm при New Game, если сейвы уже есть
- Settings: графика L/M/H, FPS 30/60, fullscreen, язык, громкости (+ Crowd)

## Как запустить
1. Открыть проект в Unity **2022.3.13f1**
2. Открыть `Assets/Scenes/SampleScene.unity`
3. Нажать **Play**
4. Меню появится само (скрипты в `Assets/Scripts/`)

## Проверка
- [ ] Новая игра → экран stub
- [ ] Esc → пауза → Сохранить в слот
- [ ] Выход в меню → Продолжить работает
- [ ] Новая игра с существующим сейвом → confirm
- [ ] Настройки кликаются и сохраняются после перезапуска Play

## Дальше
Sprint 1: walk + 2 локации + диалог (см. `docs/vertical-slice.md`)
