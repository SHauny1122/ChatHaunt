using System;
using System.Collections;
using UnityEngine;

namespace ChatHaunt
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class GhostScareEvent : MonoBehaviour
    {
        [Serializable]
        public sealed class Placement
        {
            public string id;
            public ChatHauntRoomId roomId;
            public Transform anchor;
            public string animationState = "GW_FloorCrawl_Loop";
            public float visibleSeconds = 5f;
        }

        public GameObject ghostPrefab;
        public Placement[] placements;
        [Tooltip("Optional GhostWoman scream/shriek played when she appears.")]
        public AudioClip screamClip;
        [Range(0f, 1f)]
        public float screamVolume = 0.75f;
        public AudioSource screamSource;
        public bool Running { get; private set; }
        public int TriggerCount { get; private set; }
        public string ActivePlacementId { get; private set; }
        public Animator GhostAnimator => animator;
        public GameObject GhostInstance => instance;

        GameObject instance;
        Animator animator;
        GhostWomanLoopTravel loopTravel;
        Coroutine hideRoutine;

        void Awake()
        {
            if (screamSource == null) screamSource = GetComponent<AudioSource>();
            if (screamSource == null) screamSource = gameObject.AddComponent<AudioSource>();
            screamSource.playOnAwake = false;
            screamSource.loop = false;
            screamSource.spatialBlend = 1f;
            screamSource.minDistance = 1f;
            screamSource.maxDistance = 12f;
            if (ghostPrefab == null) return;
            instance = Instantiate(ghostPrefab, transform);
            instance.name = "GhostWoman Scare Instance";
            animator = instance.GetComponent<Animator>();
            loopTravel = instance.GetComponent<GhostWomanLoopTravel>();
            instance.SetActive(false);
        }

        public bool Trigger(string placementId)
        {
            if (Running || instance == null || animator == null || placements == null) return false;
            Placement placement = Array.Find(placements, item => item != null && item.id == placementId);
            if (placement == null || placement.anchor == null || string.IsNullOrEmpty(placement.animationState)) return false;

            transform.position = placement.anchor.position;
            instance.transform.SetPositionAndRotation(placement.anchor.position, placement.anchor.rotation);
            instance.SetActive(true);
            animator.speed = 1f;
            animator.Rebind();
            animator.Play(placement.animationState, 0, 0f);
            animator.Update(0f);
            if (loopTravel != null) loopTravel.ResetTracking();
            if (screamClip != null) screamSource.PlayOneShot(screamClip, screamVolume);
            ActivePlacementId = placement.id;
            Running = true;
            TriggerCount++;
            hideRoutine = StartCoroutine(HideAfter(Mathf.Max(.1f, placement.visibleSeconds)));
            Debug.Log("ChatHaunt ghost scare: " + placement.id + " / " + placement.animationState);
            return true;
        }

        IEnumerator HideAfter(float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);
            hideRoutine = null;
            Hide();
        }

        public void ResetScare()
        {
            if (hideRoutine != null) StopCoroutine(hideRoutine);
            hideRoutine = null;
            Hide();
        }

        void Hide()
        {
            if (screamSource != null) screamSource.Stop();
            if (instance != null)
            {
                if (animator != null && instance.activeInHierarchy)
                { animator.speed = 1f; animator.Rebind(); animator.Update(0f); }
                if (loopTravel != null) loopTravel.ResetTracking();
                instance.SetActive(false);
            }
            Running = false;
            ActivePlacementId = null;
        }

        void OnDisable() { ResetScare(); }
    }
}
