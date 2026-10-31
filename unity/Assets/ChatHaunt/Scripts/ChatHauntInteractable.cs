using UnityEngine;

namespace ChatHaunt
{
    public abstract class ChatHauntInteractable : MonoBehaviour
    {
        public abstract string Prompt { get; }
        public abstract void Interact(ChatHauntPlayer player);
    }
}
