using UnityEngine;

namespace ChatHaunt
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class ChatHauntAudio : MonoBehaviour
    {
        public enum Tone { Ambience, Creak, Sting }
        public Tone tone;
        [Tooltip("Optional supplied clip. Ambient loops; event cues play once. Empty uses the existing placeholder tone.")]
        public AudioClip suppliedClip;
        [Range(0f, 1f)]
        public float volume = 0.22f;
        public int PlayCount { get; private set; }
        AudioSource source;
        AudioClip generated;

        void Awake()
        {
            source = GetComponent<AudioSource>();
            bool ambience = tone == Tone.Ambience;
            source.playOnAwake = ambience;
            source.loop = ambience;
            source.spatialBlend = ambience ? 0f : 1f;
            source.minDistance = 1f;
            source.maxDistance = 12f;
            generated = CreatePlaceholder();
            if (ambience)
            {
                source.clip = suppliedClip != null ? suppliedClip : generated;
                source.volume = volume;
            }
        }

        void Start()
        {
            if (tone == Tone.Ambience)
            {
                if (!source.isPlaying) source.Play();
            }
        }

        public void PlayCue()
        {
            AudioClip clip = suppliedClip != null ? suppliedClip : generated;
            if (source != null && clip != null)
            {
                source.PlayOneShot(clip, volume);
                PlayCount++;
            }
        }

        AudioClip CreatePlaceholder()
        {
            const int rate = 22050;
            float duration = tone == Tone.Ambience ? 4f : tone == Tone.Creak ? 0.55f : 1.3f;
            int length = Mathf.CeilToInt(rate * duration);
            float[] samples = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = i / (float)rate;
                if (tone == Tone.Ambience)
                    samples[i] = (Mathf.Sin(2f * Mathf.PI * 55f * t) * 0.32f +
                                  Mathf.Sin(2f * Mathf.PI * 82.5f * t) * 0.14f) *
                                 (0.8f + 0.2f * Mathf.Sin(2f * Mathf.PI * 0.25f * t));
                else if (tone == Tone.Creak)
                    samples[i] = Mathf.Sin(2f * Mathf.PI * (140f - 90f * t) * t) * Mathf.Exp(-4f * t) * 0.4f;
                else
                    samples[i] = (Mathf.Sin(2f * Mathf.PI * 63f * t) +
                                  0.35f * Mathf.Sin(2f * Mathf.PI * 71f * t)) * Mathf.Exp(-2.5f * t) * 0.4f;
            }
            AudioClip clip = AudioClip.Create("ChatHaunt placeholder " + tone, length, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        void OnDestroy() { if (generated != null) Destroy(generated); }
    }
}
