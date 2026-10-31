using UnityEngine;

namespace ChatHaunt
{
    public enum ChatHauntRoomId { Bedroom, Hallway, LivingRoom, Kitchen, Genkan }

    public sealed class ChatHauntRoom : MonoBehaviour
    {
        public ChatHauntRoomId id;
        public ChatHauntLightRig lighting;
        public ChatHauntSlidingDoor[] doors;
        public Transform eventAnchor;
    }
}
