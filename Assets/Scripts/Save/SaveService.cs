using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace NinetyMinutes.Save
{
    public sealed class SaveService : MonoBehaviour
    {
        public const int ManualSlotCount = 3;
        public const int AutoSlotCount = 3;
        public const int SchemaVersion = 1;

        public static SaveService Instance { get; private set; }

        public SaveBlockReason BlockReason { get; private set; } = SaveBlockReason.None;

        string _root;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            _root = Path.Combine(Application.persistentDataPath, "saves");
            Directory.CreateDirectory(_root);
        }

        public void SetBlockReason(SaveBlockReason reason) => BlockReason = reason;

        public bool CanManualSave => BlockReason == SaveBlockReason.None;

        public string BlockHint
        {
            get
            {
                switch (BlockReason)
                {
                    case SaveBlockReason.Dialogue:
                        return "Во время разговора сохранить нельзя.";
                    case SaveBlockReason.MatchPresentation:
                        return "Во время матч-сцены сохранить нельзя.";
                    case SaveBlockReason.TraumaCut:
                        return "Сейчас сохранение недоступно.";
                    default:
                        return string.Empty;
                }
            }
        }

        public IReadOnlyList<SaveMeta> ListSlots()
        {
            var list = new List<SaveMeta>();
            for (var i = 0; i < ManualSlotCount; i++)
                list.Add(ReadMetaOrEmpty(SaveKind.Manual, i));
            for (var i = 0; i < AutoSlotCount; i++)
                list.Add(ReadMetaOrEmpty(SaveKind.Auto, i));
            return list.OrderByDescending(m => m.unixTimestamp).ToList();
        }

        public bool TryGetLatest(out SavePayload payload)
        {
            payload = null;
            SavePayload best = null;
            long bestTs = -1;
            foreach (var meta in ListSlots())
            {
                if (meta.unixTimestamp <= 0) continue;
                if (!TryLoad(meta.kind, meta.index, out var p) || p == null) continue;
                if (meta.unixTimestamp > bestTs)
                {
                    bestTs = meta.unixTimestamp;
                    best = p;
                }
            }

            payload = best;
            return payload != null;
        }

        public bool HasAnySave() => TryGetLatest(out _);

        public bool TrySaveManual(int index, SavePayload payload, out string error)
        {
            error = null;
            if (!CanManualSave)
            {
                error = BlockHint;
                return false;
            }

            if (index < 0 || index >= ManualSlotCount)
            {
                error = "Неверный слот.";
                return false;
            }

            return Write(SaveKind.Manual, index, payload, out error);
        }

        public bool TrySaveAuto(string anchorType, SavePayload payload, out string error)
        {
            error = null;
            var index = PickAutoSlotIndex();
            if (payload.meta == null) payload.meta = new SaveMeta();
            payload.meta.summaryLabel = string.IsNullOrEmpty(anchorType)
                ? "Авто"
                : $"Авто · {anchorType}";
            return Write(SaveKind.Auto, index, payload, out error);
        }

        public bool TryLoad(SaveKind kind, int index, out SavePayload payload)
        {
            payload = null;
            var path = SlotPath(kind, index);
            if (!File.Exists(path)) return false;
            try
            {
                var json = File.ReadAllText(path);
                var file = JsonUtility.FromJson<SaveSlotFile>(json);
                if (file?.payload == null) return false;
                if (file.payload.schemaVersion > SchemaVersion)
                {
                    Debug.LogWarning($"Save schema newer than game: {file.payload.schemaVersion}");
                }

                payload = file.payload;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Load failed: {e.Message}");
                return false;
            }
        }

        public void DeleteAllForNewGame()
        {
            if (!Directory.Exists(_root)) return;
            foreach (var file in Directory.GetFiles(_root, "*.json"))
            {
                try { File.Delete(file); }
                catch (Exception e) { Debug.LogWarning(e.Message); }
            }
        }

        bool Write(SaveKind kind, int index, SavePayload payload, out string error)
        {
            error = null;
            try
            {
                if (payload.meta == null) payload.meta = new SaveMeta();
                payload.schemaVersion = SchemaVersion;
                payload.meta.kind = kind;
                payload.meta.index = index;
                payload.meta.slotId = $"{kind.ToString().ToLowerInvariant()}_{index}";
                payload.meta.unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if (string.IsNullOrEmpty(payload.meta.summaryLabel))
                    payload.meta.summaryLabel = kind == SaveKind.Manual ? "Ручное" : "Авто";

                var file = new SaveSlotFile { payload = payload };
                var json = JsonUtility.ToJson(file, true);
                File.WriteAllText(SlotPath(kind, index), json);
                return true;
            }
            catch (Exception e)
            {
                error = "Не удалось сохранить.";
                Debug.LogError(e);
                return false;
            }
        }

        SaveMeta ReadMetaOrEmpty(SaveKind kind, int index)
        {
            if (TryLoad(kind, index, out var p) && p?.meta != null)
                return p.meta;

            return new SaveMeta
            {
                kind = kind,
                index = index,
                slotId = $"{kind.ToString().ToLowerInvariant()}_{index}",
                unixTimestamp = 0,
                summaryLabel = "Пустой слот"
            };
        }

        int PickAutoSlotIndex()
        {
            long oldestTs = long.MaxValue;
            var oldest = 0;
            for (var i = 0; i < AutoSlotCount; i++)
            {
                var meta = ReadMetaOrEmpty(SaveKind.Auto, i);
                if (meta.unixTimestamp == 0) return i;
                if (meta.unixTimestamp < oldestTs)
                {
                    oldestTs = meta.unixTimestamp;
                    oldest = i;
                }
            }

            return oldest;
        }

        string SlotPath(SaveKind kind, int index) =>
            Path.Combine(_root, $"{kind.ToString().ToLowerInvariant()}_{index}.json");
    }
}
