using UnityEngine;
using UnityEngine.InputSystem;

namespace ChatHaunt
{
    public sealed class ChatHauntFlashlight : MonoBehaviour
    {
        public Light beam;
        public bool IsOn => beam != null && beam.enabled;

        void Start() { if (beam != null) beam.enabled = false; }

        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame &&
                Cursor.lockState == CursorLockMode.Locked && beam != null)
                Toggle();
        }

        public void Toggle() { if (beam != null) beam.enabled = !beam.enabled; }
    }
}
