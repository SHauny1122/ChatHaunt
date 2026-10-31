using UnityEngine;

namespace ChatHaunt
{
    // A compact, independent version of the old house's sideways sliding door.
    public sealed class ChatHauntSlidingDoor : ChatHauntInteractable
    {
        public Transform movingPart;
        public Vector3 openOffset = new Vector3(0f, 0f, 1.65f);
        public float travelSeconds = 0.65f;
        public AudioSource audioSource;
        public AudioClip openingClip;
        public AudioClip closingClip;
        public bool IsOpen { get; private set; }
        public override string Prompt { get { return IsOpen ? "E  Close sliding door" : "E  Open sliding door"; } }

        Vector3 closedPosition;
        float panelHalfWidth;

        void Awake()
        {
            closedPosition = movingPart.localPosition;
            BoxCollider panel = movingPart.GetComponentInChildren<BoxCollider>();
            panelHalfWidth = panel != null ? panel.bounds.extents.z : 0.8f;
        }

        void Update()
        {
            // Keep a closing panel out of the player's doorway space.
            if (!IsOpen && DoorwayOccupied()) SetOpen(true);
            Vector3 target = closedPosition + (IsOpen ? openOffset : Vector3.zero);
            float speed = openOffset.magnitude / Mathf.Max(0.1f, travelSeconds);
            movingPart.localPosition = Vector3.MoveTowards(movingPart.localPosition, target, speed * Time.deltaTime);
        }

        public override void Interact(ChatHauntPlayer player) { SetOpen(!IsOpen); }

        public void SetOpen(bool value)
        {
            if (!value && DoorwayOccupied()) return;
            if (IsOpen == value) return;
            IsOpen = value;
            if (audioSource == null) return;
            AudioClip clip = value ? openingClip : closingClip;
            if (clip == null) return;
            audioSource.Stop();
            audioSource.PlayOneShot(clip);
        }

        bool DoorwayOccupied()
        {
            ChatHauntPlayer player = FindFirstObjectByType<ChatHauntPlayer>();
            if (player == null) return false;
            Vector3 p = transform.InverseTransformPoint(player.transform.position);
            return Mathf.Abs(p.x) < 0.34f && Mathf.Abs(p.z - closedPosition.z) < panelHalfWidth + 0.2f;
        }
    }
}
