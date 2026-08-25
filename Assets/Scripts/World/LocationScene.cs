using UnityEngine;

namespace NinetyMinutes.World
{
    public sealed class LocationScene : MonoBehaviour
    {
        public string LocationId;
        public Light LocalLamp;
        public DoorInteractable Door;
        public NpcInteractable[] Npcs;
    }
}
