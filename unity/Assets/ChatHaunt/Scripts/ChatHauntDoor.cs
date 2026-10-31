using UnityEngine;

namespace ChatHaunt
{
    public sealed class ChatHauntDoor : ChatHauntInteractable
    {
        public Transform movingPart;
        public float openAngle = -95f;
        public float turnSpeed = 140f;
        public ChatHauntAudio audioCue;
        public bool IsOpen { get; private set; }
        public override string Prompt => IsOpen ? "E  Close door" : "E  Open door";

        Quaternion closedRotation;

        void Awake() { closedRotation = movingPart.localRotation; }

        void Update()
        {
            Quaternion target = closedRotation * Quaternion.Euler(0f, IsOpen ? openAngle : 0f, 0f);
            movingPart.localRotation = Quaternion.RotateTowards(movingPart.localRotation, target, turnSpeed * Time.deltaTime);
        }

        public override void Interact(ChatHauntPlayer player) { SetOpen(!IsOpen); }

        public void SetOpen(bool value)
        {
            if (IsOpen == value) return;
            IsOpen = value;
            if (audioCue != null) audioCue.PlayCue();
        }
    }
}
