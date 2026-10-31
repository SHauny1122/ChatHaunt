using UnityEngine;
using UnityEngine.InputSystem;

namespace ChatHaunt
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class ChatHauntPlayer : MonoBehaviour
    {
        public Camera view;
        public float walkSpeed = 3f;
        public float lookSensitivity = 0.09f;
        public float interactionRange = 2.5f;
        public ChatHauntInteractable Focused { get; private set; }

        CharacterController controller;
        readonly RaycastHit[] interactionHits = new RaycastHit[16];
        float verticalSpeed;
        float pitch;
        float lastFocusTime;

        void Awake() { controller = GetComponent<CharacterController>(); }

        void Start() { CaptureCursor(); }

        void OnDisable() { ReleaseCursor(); }

        public void CaptureCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void ReleaseCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        void Update()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) ReleaseCursor();
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                Focused = null;
                lastFocusTime = float.NegativeInfinity;
                if (mouse != null && mouse.leftButton.wasPressedThisFrame) CaptureCursor();
                return;
            }

            if (mouse != null)
            {
                Vector2 delta = mouse.delta.ReadValue() * lookSensitivity;
                transform.Rotate(0f, delta.x, 0f);
                pitch = Mathf.Clamp(pitch - delta.y, -80f, 80f);
                view.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            }

            Vector2 input = Vector2.zero;
            if (keyboard != null)
                input = new Vector2((keyboard.dKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed ? 1 : 0),
                                    (keyboard.wKey.isPressed ? 1 : 0) - (keyboard.sKey.isPressed ? 1 : 0));
            input = Vector2.ClampMagnitude(input, 1f);
            verticalSpeed = controller.isGrounded && verticalSpeed < 0f ? -2f : verticalSpeed - 18f * Time.deltaTime;
            Vector3 motion = (transform.right * input.x + transform.forward * input.y) * walkSpeed;
            controller.Move((motion + Vector3.up * verticalSpeed) * Time.deltaTime);

            RefreshFocus();
            if (keyboard != null && keyboard.eKey.wasPressedThisFrame && Focused != null)
                Focused.Interact(this);
        }

        public void RefreshFocus()
        {
            if (view == null) { Focused = null; return; }

            Vector3 origin = view.transform.position;
            Vector3 forward = view.transform.forward;
            ChatHauntInteractable candidate = null;
            float directDistance = interactionRange;
            if (Physics.Raycast(origin, forward, out RaycastHit directHit, interactionRange,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                directDistance = directHit.distance;
                candidate = directHit.collider.GetComponentInParent<ChatHauntInteractable>();
            }

            // A narrow doorway frame can make a single centre ray alternate between
            // the panel and the wall as the player moves by a few pixels.
            if (candidate == null)
            {
                int count = Physics.SphereCastNonAlloc(origin, 0.18f, forward, interactionHits,
                    interactionRange, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
                float nearest = float.MaxValue;
                for (int i = 0; i < count; i++)
                {
                    RaycastHit hit = interactionHits[i];
                    if (hit.collider == null || hit.distance > directDistance + 0.22f || hit.distance >= nearest) continue;
                    ChatHauntInteractable interactable = hit.collider.GetComponentInParent<ChatHauntInteractable>();
                    if (interactable == null) continue;
                    candidate = interactable;
                    nearest = hit.distance;
                }
            }

            if (candidate != null)
            {
                Focused = candidate;
                lastFocusTime = Time.unscaledTime;
            }
            else if (Focused == null || Time.unscaledTime - lastFocusTime > 0.25f || !StillFacing(Focused))
                Focused = null;
        }

        bool StillFacing(ChatHauntInteractable interactable)
        {
            if (interactable == null) return false;
            Vector3 toDoor = interactable.transform.position + Vector3.up * 1.45f - view.transform.position;
            return toDoor.sqrMagnitude <= (interactionRange + 0.25f) * (interactionRange + 0.25f) &&
                Vector3.Dot(view.transform.forward, toDoor.normalized) > 0.45f;
        }
    }
}
