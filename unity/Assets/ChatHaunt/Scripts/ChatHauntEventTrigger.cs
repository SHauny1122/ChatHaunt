using UnityEngine;

namespace ChatHaunt
{
    [RequireComponent(typeof(Collider))]
    public sealed class ChatHauntEventTrigger : MonoBehaviour
    {
        public ChatHauntHorrorEvent horrorEvent;

        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<ChatHauntPlayer>() != null)
                horrorEvent.TriggerEvent();
        }
    }
}
