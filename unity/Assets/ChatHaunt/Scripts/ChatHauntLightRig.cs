using UnityEngine;

namespace ChatHaunt
{
    public sealed class ChatHauntLightRig : MonoBehaviour
    {
        public Light[] roomLights;
        public bool IsBlackout { get; private set; }
        float[] originalIntensities;
        bool[] originalEnabled;

        void Awake()
        {
            originalIntensities = new float[roomLights.Length];
            originalEnabled = new bool[roomLights.Length];
            for (int i = 0; i < roomLights.Length; i++)
            {
                originalIntensities[i] = roomLights[i].intensity;
                originalEnabled[i] = roomLights[i].enabled;
            }
        }

        public void SetBlackout(bool blackout)
        {
            IsBlackout = blackout;
            for (int i = 0; i < roomLights.Length; i++)
            {
                roomLights[i].intensity = originalIntensities[i];
                roomLights[i].enabled = !blackout && originalEnabled[i];
            }
        }
    }
}
