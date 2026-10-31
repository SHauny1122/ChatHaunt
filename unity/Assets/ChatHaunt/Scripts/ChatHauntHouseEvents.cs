using UnityEngine;

namespace ChatHaunt
{
    public sealed class ChatHauntHouseEvents : MonoBehaviour
    {
        public ChatHauntRoom[] rooms;
        public ChatHauntHorrorEvent hallwayBlackout;
        public GhostScareEvent ghostScare;
        public ChatHauntAudio doorSlam;

        public ChatHauntRoom FindRoom(ChatHauntRoomId id)
        {
            if (rooms == null) return null;
            foreach (ChatHauntRoom room in rooms)
                if (room != null && room.id == id) return room;
            return null;
        }

        public bool Trigger(string eventId)
        {
            if (eventId == "DOOR_SLAM" && doorSlam != null)
            {
                int beforeCue = doorSlam.PlayCount;
                doorSlam.PlayCue();
                return doorSlam.PlayCount > beforeCue;
            }
            if (eventId != "LIGHTS_HALLWAY_OFF" || hallwayBlackout == null) return false;
            int before = hallwayBlackout.TriggerCount;
            hallwayBlackout.TriggerEvent();
            return hallwayBlackout.TriggerCount > before;
        }

        public bool TriggerGhost(string placementId)
        {
            return ghostScare != null && ghostScare.Trigger(placementId);
        }
    }
}
