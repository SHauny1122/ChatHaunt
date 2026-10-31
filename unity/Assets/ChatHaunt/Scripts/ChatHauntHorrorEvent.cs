using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ChatHaunt
{
    public sealed class ChatHauntHorrorEvent : MonoBehaviour
    {
        public ChatHauntLightRig lightRig;
        public ChatHauntLightRig[] houseLights;
        public GameObject silhouette;
        public ChatHauntAudio cue;
        public float blackoutSeconds = 2.6f;
        public float silhouetteDelay = 0.35f;
        public bool oneShot = true;
        public int TriggerCount { get; private set; }
        public bool Running { get; private set; }
        bool blackoutActive;

        void Start() { if (silhouette != null) silhouette.SetActive(false); }

        void Update()
        {
            if (Keyboard.current == null || !Keyboard.current.lKey.wasPressedThisFrame) return;
            ToggleLocalBlackout();
        }

        void ToggleLocalBlackout()
        {
            blackoutActive = !blackoutActive;
            ApplyBlackout();
        }

        // Called by the localhost bridge or an explicit local test.
        public void TriggerEvent()
        {
            if (Running || (oneShot && TriggerCount > 0)) return;
            TriggerCount++;
            blackoutActive = true;
            StartCoroutine(Play());
        }

        IEnumerator Play()
        {
            Running = true;
            ApplyBlackout();
            if (cue != null) cue.PlayCue();
            yield return new WaitForSeconds(silhouetteDelay);
            if (silhouette != null) silhouette.SetActive(true);
            yield return new WaitForSeconds(Mathf.Max(0f, blackoutSeconds - silhouetteDelay));
            if (silhouette != null) silhouette.SetActive(false);
            Running = false;
            ApplyBlackout();
        }

        void ApplyBlackout() { SetBlackout(blackoutActive); }

        void SetBlackout(bool value)
        {
            if (houseLights != null && houseLights.Length > 0)
            {
                foreach (ChatHauntLightRig rig in houseLights)
                    if (rig != null) rig.SetBlackout(value);
            }
            else if (lightRig != null) lightRig.SetBlackout(value);
        }
    }
}
