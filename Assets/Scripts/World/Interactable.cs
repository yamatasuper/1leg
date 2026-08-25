using UnityEngine;

namespace NinetyMinutes.World
{
    public abstract class Interactable : MonoBehaviour
    {
        public string Prompt = "E — взаимодействие";
        public abstract void Interact(PlayerController player);
    }
}
