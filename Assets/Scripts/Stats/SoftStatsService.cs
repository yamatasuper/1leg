using System;
using System.Collections.Generic;
using UnityEngine;

namespace NinetyMinutes.Stats
{
    public sealed class SoftStatsState
    {
        public float Morale;
        public float Energy;
        public float Strength;
        public float Focus;
        public float Pain;
        public float Anxiety;

        public void ClampAll()
        {
            Morale = Mathf.Clamp(Morale, -10, 10);
            Energy = Mathf.Clamp(Energy, -10, 10);
            Strength = Mathf.Clamp(Strength, -10, 10);
            Focus = Mathf.Clamp(Focus, -10, 10);
            Pain = Mathf.Clamp(Pain, -10, 10);
            Anxiety = Mathf.Clamp(Anxiety, -10, 10);
        }
    }

    public sealed class SoftStatsService : MonoBehaviour
    {
        public static SoftStatsService Instance { get; private set; }
        public SoftStatsState State { get; private set; } = new SoftStatsState();

        public event Action Changed;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        public void ResetForNewRun()
        {
            State = new SoftStatsState();
            Changed?.Invoke();
        }

        public void Apply(string key, float delta)
        {
            switch (key)
            {
                case "morale": State.Morale += delta; break;
                case "energy": State.Energy += delta; break;
                case "strength": State.Strength += delta; break;
                case "focus": State.Focus += delta; break;
                case "pain": State.Pain += delta; break;
                case "anxiety": State.Anxiety += delta; break;
            }

            State.ClampAll();
            Changed?.Invoke();
        }

        public string Band(float v)
        {
            if (v <= -6) return "очень низко";
            if (v < -3) return "низко";
            if (v > 6) return "очень высоко";
            if (v > 3) return "высоко";
            return "средне";
        }

        public List<(string name, float value, string band)> Snapshot()
        {
            return new List<(string, float, string)>
            {
                ("Мораль", State.Morale, Band(State.Morale)),
                ("Энергия", State.Energy, Band(State.Energy)),
                ("Сила", State.Strength, Band(State.Strength)),
                ("Фокус", State.Focus, Band(State.Focus)),
                ("Боль", State.Pain, Band(State.Pain)),
                ("Тревога", State.Anxiety, Band(State.Anxiety)),
            };
        }
    }
}
