using System.Collections.Generic;
using NinetyMinutes.Match;

namespace NinetyMinutes.Dialogue
{
    public static class SliceDialogues
    {
        public static HashSet<string> Flags { get; } = new HashSet<string>();

        public static DialogueGraph IntroFlashback()
        {
            var g = new DialogueGraph { Id = "dlg_intro_flashback", StartNodeId = "n1" };
            g.Nodes["n1"] = new DialogueNode
            {
                Id = "n1",
                Speaker = "",
                Line =
                    "Прибрежье. Стадион «Торпедо». Западная трибуна закрыта три года.\nТы — Алексей Бардин. Тридцать четыре. Контракт кончается через месяц.\nЭто не симулятор. Это девяносто минут, в которых заканчивается всё, чем ты был.",
                EndsDialogue = true
            };
            return g;
        }

        public static DialogueGraph TrainingCoach()
        {
            var g = new DialogueGraph { Id = "dlg_train_coach", StartNodeId = "n1" };

            g.Nodes["n1"] = new DialogueNode
            {
                Id = "n1",
                Speaker = "Виктор Семёнович",
                Line = "Слушайте сюда. У некоторых из вас это последний матч. Я не буду говорить, у кого. Бардин, ты — капитан. Сделай что-нибудь.",
                Choices =
                {
                    new DialogueChoice
                    {
                        Id = "c1",
                        Text = "Встать: «Мы не проиграем».",
                        Tone = "honest",
                        NextNodeId = "n2_c1",
                        Tags = { "push_up" },
                        StatDeltas = { new StatDelta { Stat = "morale", Amount = 1 } }
                    },
                    new DialogueChoice
                    {
                        Id = "c2",
                        Text = "Молчать. Смотреть в пол.",
                        Tone = "evade",
                        NextNodeId = "n2_c2",
                        Tags = { "push_down" },
                        StatDeltas = { new StatDelta { Stat = "anxiety", Amount = 1 } }
                    },
                    new DialogueChoice
                    {
                        Id = "c3",
                        Text = "«Мы не проиграем. Я обещаю».",
                        Tone = "care",
                        NextNodeId = "n2_c3",
                        Tags = { "push_up" },
                        StatDeltas =
                        {
                            new StatDelta { Stat = "focus", Amount = 1 },
                            new StatDelta { Stat = "morale", Amount = 1 }
                        }
                    }
                }
            };

            g.Nodes["n2_c1"] = new DialogueNode
            {
                Id = "n2_c1",
                Speaker = "Виктор Семёнович",
                Line = "Хватит слов. Играйте так, чтобы запомнить.",
                NextNodeId = "n3"
            };
            g.Nodes["n2_c2"] = new DialogueNode
            {
                Id = "n2_c2",
                Speaker = "Виктор Семёнович",
                Line = "Молчишь. Ладно. Молчание тоже выбор.",
                NextNodeId = "n3"
            };
            g.Nodes["n2_c3"] = new DialogueNode
            {
                Id = "n2_c3",
                Speaker = "Виктор Семёнович",
                Line = "Обещания на этом стадионе дешевые. Докажи на траве.",
                NextNodeId = "n3"
            };

            g.Nodes["n3"] = new DialogueNode
            {
                Id = "n3",
                Speaker = "Виктор Семёнович",
                Line =
                    "Глок уже бинтуется. Сокол в наушниках. Пень белый как мел.\nВыйди на бровку. Поговори с ними. Потом — свисток.\n\n(Tab — журнал → Состояние)",
                OpensJournalHint = true,
                EndsDialogue = true
            };

            return g;
        }

        public static DialogueGraph TrainingSkip()
        {
            var g = new DialogueGraph { Id = "dlg_train_skip", StartNodeId = "n1" };
            g.Nodes["n1"] = new DialogueNode
            {
                Id = "n1",
                Speaker = "",
                Line = "Можно не слушать тренера. Колени это заметят. Ты не пришёл. Это тоже выбор.",
                Choices =
                {
                    new DialogueChoice
                    {
                        Id = "confirm",
                        Text = "Уйти из раздевалки молча",
                        NextNodeId = "n2",
                        Tags = { "push_down" },
                        SetFlags = { "training_skipped" },
                        StatDeltas =
                        {
                            new StatDelta { Stat = "morale", Amount = -2 },
                            new StatDelta { Stat = "energy", Amount = -2 },
                            new StatDelta { Stat = "focus", Amount = -1 },
                            new StatDelta { Stat = "anxiety", Amount = 2 }
                        }
                    },
                    new DialogueChoice
                    {
                        Id = "cancel",
                        Text = "Остаться",
                        NextNodeId = "n_cancel"
                    }
                }
            };
            g.Nodes["n2"] = new DialogueNode
            {
                Id = "n2",
                Speaker = "",
                Line = "Разминка не состоялась. Бровка всё равно ждёт.",
                EndsDialogue = true
            };
            g.Nodes["n_cancel"] = new DialogueNode
            {
                Id = "n_cancel",
                Speaker = "",
                Line = "Ещё не поздно подойти к Виктору Семёновичу.",
                EndsDialogue = true
            };
            return g;
        }

        public static DialogueGraph Segment1Glock()
        {
            var g = new DialogueGraph
            {
                Id = "dlg_seg1_glock",
                StartNodeId = "n1"
            };

            g.Nodes["n1"] = new DialogueNode
            {
                Id = "n1",
                Speaker = "Глок",
                Line = "Бардин. Если сегодня Соболев меня уроет... ты прикроешь? Или я сам?",
                Choices =
                {
                    new DialogueChoice
                    {
                        Id = "c1",
                        Text = "Прикрою, не парься.",
                        Tone = "care",
                        NextNodeId = "n2_c1",
                        Tags = { "push_up" },
                        StatDeltas = { new StatDelta { Stat = "morale", Amount = 1 } },
                        SetFlags = { "rel_glock_up", "promised_silence" }
                    },
                    new DialogueChoice
                    {
                        Id = "c2",
                        Text = "Скажи тренеру правду, Серёга.",
                        Tone = "honest",
                        NextNodeId = "n2_c2",
                        Tags = { "push_down" },
                        StatDeltas = { new StatDelta { Stat = "anxiety", Amount = 1 } },
                        SetFlags = { "rel_glock_down", "told_glock_truth" }
                    },
                    new DialogueChoice
                    {
                        Id = "c3",
                        Text = "Я тоже боюсь. Но я здесь.",
                        Tone = "honest",
                        NextNodeId = "n2_c3",
                        Tags = { "push_up" },
                        StatDeltas = { new StatDelta { Stat = "focus", Amount = 1 } },
                        SetFlags = { "rel_glock_up" }
                    },
                    new DialogueChoice
                    {
                        Id = "c4",
                        Text = "Молчать. Смотреть на бутсы.",
                        Tone = "evade",
                        NextNodeId = "n2_c4",
                        Tags = { "push_down" },
                        StatDeltas = { new StatDelta { Stat = "anxiety", Amount = 1 } },
                        SetFlags = { "rel_glock_down" }
                    }
                }
            };

            g.Nodes["n2_c1"] = new DialogueNode
            {
                Id = "n2_c1",
                Speaker = "Глок",
                Line = "Ты единственный, кто меня понимает. Капитан — не повязка. Это когда тебя слушают.",
                NextNodeId = "n3"
            };
            g.Nodes["n2_c2"] = new DialogueNode
            {
                Id = "n2_c2",
                Speaker = "Глок",
                Line = "Ты же обещал молчать. Я думал, ты друг.",
                NextNodeId = "n3"
            };
            g.Nodes["n2_c3"] = new DialogueNode
            {
                Id = "n2_c3",
                Speaker = "Глок",
                Line = "Он смотрит. Кивает. Тишина говорит больше, чем слова.",
                NextNodeId = "n3"
            };
            g.Nodes["n2_c4"] = new DialogueNode
            {
                Id = "n2_c4",
                Speaker = "Глок",
                Line = "Он замечает, как ты морщишься, затягивая бинт. Отворачивается.",
                NextNodeId = "n3"
            };

            g.Nodes["n3"] = new DialogueNode
            {
                Id = "n3",
                Speaker = "Глок",
                Line = "Если бы я не стал футболистом, я бы стал рыбаком. Просто сидеть у реки и ждать.",
                EndsDialogue = true
            };

            return g;
        }

        public static DialogueGraph Segment2Sokol()
        {
            var g = new DialogueGraph
            {
                Id = "dlg_seg2_sokol",
                StartNodeId = Flags.Contains("told_glock_truth") ? "n1b" : "n1",
                ForceIronyBeat = true
            };

            g.Nodes["n1"] = new DialogueNode
            {
                Id = "n1",
                Speaker = "Сокол",
                Line = "Что? Я хочу забить. Хочу, чтобы меня заметили.",
                NextNodeId = "n_choices"
            };
            g.Nodes["n1b"] = new DialogueNode
            {
                Id = "n1b",
                Speaker = "Сокол",
                Line = "Глок злой. Ты ему что-то сказал? Мне всё равно. Я хочу, чтобы меня заметили.",
                NextNodeId = "n_choices"
            };

            g.Nodes["n_choices"] = new DialogueNode
            {
                Id = "n_choices",
                Speaker = "Сокол",
                Line = "Ну?",
                Choices =
                {
                    new DialogueChoice
                    {
                        Id = "c1",
                        Text = "Уйдёшь в большой клуб. Не будь жадным. Отдавай пасы.",
                        Tone = "care",
                        NextNodeId = "n_irony",
                        Tags = { "push_up", "twist" }
                    },
                    new DialogueChoice
                    {
                        Id = "c2",
                        Text = "Я был таким же. Испортил несколько сезонов.",
                        Tone = "honest",
                        NextNodeId = "n_irony",
                        Tags = { "push_up" },
                        StatDeltas =
                        {
                            new StatDelta { Stat = "focus", Amount = 1 },
                            new StatDelta { Stat = "morale", Amount = -1 }
                        }
                    },
                    new DialogueChoice
                    {
                        Id = "c3",
                        Text = "Не дурачься. Отдай пас.",
                        Tone = "sharp",
                        NextNodeId = "n_irony",
                        Tags = { "push_down" }
                    }
                }
            };

            g.Nodes["n_irony"] = new DialogueNode
            {
                Id = "n_irony",
                Speaker = "Сокол",
                Line =
                    "Он надевает наушники. Потом снимает.\n«Ты просто старый».\nИли нет. Иногда он слышит. Мир всё равно не ведёт таблицу честности.",
                NextNodeId = "n3"
            };
            g.Nodes["n3"] = new DialogueNode
            {
                Id = "n3",
                Speaker = "Сокол",
                Line = "Цель выбери правильную. Я ещё не знаю, что это значит.",
                EndsDialogue = true
            };

            return g;
        }

        public static DialogueGraph HalfTimeCoach()
        {
            var leading = ChoiceScoreBridge.Instance != null &&
                          ChoiceScoreBridge.Instance.Score.GoalsFor > ChoiceScoreBridge.Instance.Score.GoalsAgainst;

            var g = new DialogueGraph { Id = "dlg_ht_coach", StartNodeId = "n1" };
            g.Nodes["n1"] = new DialogueNode
            {
                Id = "n1",
                Speaker = "Виктор Семёнович",
                Line = leading
                    ? "Не расслабились. Река не останавливается, и соперник тоже."
                    : "Это не счёт. Это ваш характер на табло. Бардин, ты — капитан. Сделай что-нибудь.",
                Choices =
                {
                    new DialogueChoice
                    {
                        Id = "c1",
                        Text = "«Мы не проиграем».",
                        Tone = "honest",
                        NextNodeId = "n2",
                        Tags = { "push_up" },
                        StatDeltas = { new StatDelta { Stat = "focus", Amount = 1 } }
                    },
                    new DialogueChoice
                    {
                        Id = "c2",
                        Text = "Молчать и смотреть в пол.",
                        Tone = "evade",
                        NextNodeId = "n2",
                        Tags = { "push_down" },
                        StatDeltas = { new StatDelta { Stat = "anxiety", Amount = 1 } }
                    },
                    new DialogueChoice
                    {
                        Id = "c3",
                        Text = "Скажите это Глоку и Пеню. Не только мне.",
                        Tone = "care",
                        NextNodeId = "n2",
                        Tags = { "push_up" }
                    }
                }
            };
            g.Nodes["n2"] = new DialogueNode
            {
                Id = "n2",
                Speaker = "Виктор Семёнович",
                Line = "Хватит слов. Десять минут — и снова свисток.",
                EndsDialogue = true
            };
            return g;
        }

        public static DialogueGraph HalfTimeGlock()
        {
            var g = new DialogueGraph { Id = "dlg_ht_glock", StartNodeId = "n1" };
            var choices = new List<DialogueChoice>
            {
                new DialogueChoice
                {
                    Id = "c1",
                    Text = "Я прикрою. Ты играешь, я пашу.",
                    Tone = "care",
                    NextNodeId = "n2_c1",
                    Tags = { "push_up" },
                    StatDeltas = { new StatDelta { Stat = "morale", Amount = 1 } },
                    SetFlags = { "rel_glock_up" }
                },
                new DialogueChoice
                {
                    Id = "c2",
                    Text = "Серёга, скажи тренеру. Я не хочу, чтобы ты ломался.",
                    Tone = "honest",
                    NextNodeId = "n2_c2",
                    Tags = { "push_down" }
                }
            };

            if (Flags.Contains("promised_silence") && !Flags.Contains("promised_silence_held"))
            {
                choices.Add(new DialogueChoice
                {
                    Id = "c3",
                    Text = "Я молчал. И буду молчать. Терпи — я прикрою.",
                    Tone = "care",
                    NextNodeId = "n2_c3",
                    Tags = { "push_up" },
                    SetFlags = { "promised_silence_held", "rel_glock_up" }
                });
            }

            g.Nodes["n1"] = new DialogueNode
            {
                Id = "n1",
                Speaker = "Глок",
                Line = "Колено. Я чувствую, что оно сейчас отвалится. Я не знаю, что делать.",
                Choices = choices
            };
            g.Nodes["n2_c1"] = new DialogueNode
            {
                Id = "n2_c1",
                Speaker = "Глок",
                Line = "Ты единственный, кто меня понимает.",
                NextNodeId = "n3"
            };
            g.Nodes["n2_c2"] = new DialogueNode
            {
                Id = "n2_c2",
                Speaker = "Глок",
                Line = "Ты обещал молчать.",
                NextNodeId = "n3"
            };
            g.Nodes["n2_c3"] = new DialogueNode
            {
                Id = "n2_c3",
                Speaker = "Глок",
                Line = "Он кивает. Больше ничего не надо.",
                NextNodeId = "n3"
            };
            g.Nodes["n3"] = new DialogueNode
            {
                Id = "n3",
                Speaker = "Глок",
                Line = "Капитан — не повязка. Это когда тебя слушают.",
                EndsDialogue = true
            };
            return g;
        }

        public static DialogueGraph Segment3Self()
        {
            var g = new DialogueGraph
            {
                Id = "dlg_seg3_self",
                StartNodeId = "n1"
            };

            g.Nodes["n1"] = new DialogueNode
            {
                Id = "n1",
                Speaker = "",
                Line = "Бутсы износились. Как и ты. Кто ты без футбола? Без денег? Без этой раздевалки?",
                Choices =
                {
                    new DialogueChoice
                    {
                        Id = "c1",
                        Text = "Запомнить запах. Запомнить реку. Не бежать.",
                        Tone = "honest",
                        NextNodeId = "n2_c1",
                        Tags = { "push_up" },
                        StatDeltas =
                        {
                            new StatDelta { Stat = "morale", Amount = 1 },
                            new StatDelta { Stat = "focus", Amount = 1 }
                        },
                        SetFlags = { "chose_self" }
                    },
                    new DialogueChoice
                    {
                        Id = "c2",
                        Text = "Затянуть шнурки и не думать.",
                        Tone = "evade",
                        NextNodeId = "n2_c2",
                        Tags = { "push_down" },
                        StatDeltas = { new StatDelta { Stat = "anxiety", Amount = 1 } },
                        SetFlags = { "chose_numb" }
                    },
                    new DialogueChoice
                    {
                        Id = "c3",
                        Text = "Я не обязан быть удобным. Ни клубу, ни табло.",
                        Tone = "sharp",
                        NextNodeId = "n2_c3",
                        Tags = { "push_up" },
                        StatDeltas =
                        {
                            new StatDelta { Stat = "strength", Amount = 1 },
                            new StatDelta { Stat = "energy", Amount = -1 }
                        },
                        SetFlags = { "chose_self" }
                    }
                }
            };

            g.Nodes["n2_c1"] = new DialogueNode
            {
                Id = "n2_c1",
                Speaker = "",
                Line = "Река не спрашивает, куда течь. Ты всегда спрашивал. Сейчас ты смотришь.",
                EndsDialogue = true
            };
            g.Nodes["n2_c2"] = new DialogueNode
            {
                Id = "n2_c2",
                Speaker = "",
                Line = "Ты бежишь, потому что не умеешь не бежать.",
                EndsDialogue = true
            };
            g.Nodes["n2_c3"] = new DialogueNode
            {
                Id = "n2_c3",
                Speaker = "",
                Line = "Жёсткость — не то же самое, что правда. Но хотя бы не ложь.",
                EndsDialogue = true
            };

            return g;
        }

        public static DialogueGraph InterviewPress()
        {
            var signal = ChoiceScoreBridge.Instance != null
                ? ChoiceScoreBridge.Instance.GetScoreSignal()
                : ScoreSignal.Draw;

            string q;
            switch (signal)
            {
                case ScoreSignal.Win:
                    q = "Свисток. 3:2 или нет — табло врёт так же, как деньги. Что ты скажешь себе?";
                    break;
                case ScoreSignal.Loss:
                    q = "Свисток. Счёт жёсткий. Ты уходишь. Куда?";
                    break;
                default:
                    q = "Свисток. Ничья. Сегодня было. Завтра этого не будет.";
                    break;
            }

            var g = new DialogueGraph { Id = "dlg_interview_press", StartNodeId = "n1" };
            g.Nodes["n1"] = new DialogueNode
            {
                Id = "n1",
                Speaker = "",
                Line = q,
                Choices =
                {
                    new DialogueChoice
                    {
                        Id = "c1",
                        Text = "Я был здесь. Этого достаточно.",
                        Tone = "honest",
                        NextNodeId = "n2_honest",
                        SetFlags = { "interview_tone_honest" }
                    },
                    new DialogueChoice
                    {
                        Id = "c2",
                        Text = "Не смотреть. Идти.",
                        Tone = "evade",
                        NextNodeId = "n2_evade",
                        SetFlags = { "interview_tone_evade" }
                    },
                    new DialogueChoice
                    {
                        Id = "c3",
                        Text = "Деньги не вернут этот свисток.",
                        Tone = "sharp",
                        NextNodeId = "n2_sharp",
                        SetFlags = { "interview_tone_sharp" }
                    }
                }
            };
            g.Nodes["n2_honest"] = new DialogueNode
            {
                Id = "n2_honest",
                Speaker = "",
                Line = "Ты смотришь на реку. Она течёт. Ты не знаешь, куда течёшь ты.",
                NextNodeId = "n3_honest"
            };
            g.Nodes["n3_honest"] = new DialogueNode
            {
                Id = "n3_honest",
                Speaker = "Бардин",
                Line = "Мяч останется. И я останусь. Без славы. Просто я.",
                EndsDialogue = true
            };
            g.Nodes["n2_evade"] = new DialogueNode
            {
                Id = "n2_evade",
                Speaker = "",
                Line = "Ты не оглядываешься. Ты не хочешь прощаться.",
                NextNodeId = "n3_evade"
            };
            g.Nodes["n3_evade"] = new DialogueNode
            {
                Id = "n3_evade",
                Speaker = "Бардин",
                Line = "Я умею молчать. Это я и уношу.",
                EndsDialogue = true
            };
            g.Nodes["n2_sharp"] = new DialogueNode
            {
                Id = "n2_sharp",
                Speaker = "",
                Line = "Ты смотришь на руки. Они ничего не держат.",
                NextNodeId = "n3_sharp"
            };
            g.Nodes["n3_sharp"] = new DialogueNode
            {
                Id = "n3_sharp",
                Speaker = "Бардин",
                Line = "Страшно было тогда — когда были деньги.",
                EndsDialogue = true
            };

            return g;
        }

        public static DialogueGraph InterviewMono()
        {
            var soft = "";
            if (Flags.Contains("chose_self"))
                soft = "\nТы хотя бы раз перестал бежать.";
            else if (Flags.Contains("chose_numb"))
                soft = "\nТы снова купил тишину в рассрочку.";
            else if (Flags.Contains("rel_glock_up"))
                soft = "\nТы был рядом. Без денег. Без славы. Просто два человека.";
            else if (NinetyMinutes.Stats.SoftStatsService.Instance != null &&
                     NinetyMinutes.Stats.SoftStatsService.Instance.State.Anxiety >= 3)
                soft = "\nСтрах будущего был рядом всё время.";

            var g = new DialogueGraph { Id = "dlg_interview_mono", StartNodeId = "n1" };
            g.Nodes["n1"] = new DialogueNode
            {
                Id = "n1",
                Speaker = "",
                Line =
                    "Новая квартира. Дешёвый линолеум. Обои в цветочек, как в общежитии.\nНа столе — старый мяч.\nТы пишешь письмо. Кому — не знаешь." +
                    soft,
                EndsDialogue = true
            };
            return g;
        }
    }
}
