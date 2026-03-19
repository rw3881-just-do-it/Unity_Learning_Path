// Checkpoint.cs
// Place on any GameObject to make it an activatable checkpoint.
//
// UNITY:
//1. Create a GameObject for your checkpoint (e.g. a flag or bonfire shape)
//2. Add a Collider2D (set Is Trigger = true)
//3. Add this script
//4. Tag the Player GameObject as "Player"
//The player walks into it → a prompt appears → press Interact to save

using UnityEngine;

namespace Demo.Systems
{
    public class Checkpoint : MonoBehaviour
    {
        [Header("Settings")]
        public bool activateAutomatically = false; // True = activates on touch, no prompt

        public bool IsActive { get; private set; } = false;

        // Visual feedback — swap these for real sprites later
        private SpriteRenderer sr;
        private static Checkpoint currentActive; // Only one checkpoint active at a time

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            UpdateVisual();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            if (activateAutomatically)
                Activate();
            else
                // Show UI prompt (handled by CheckpointUI or GameManager)
                GameManager.Instance?.ShowCheckpointPrompt(this);
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            GameManager.Instance?.HideCheckpointPrompt();
        }

        public void Activate()
        {
            // Deactivate previously active checkpoint
            if (currentActive != null && currentActive != this)
                currentActive.Deactivate();

            IsActive = true;
            currentActive = this;
            UpdateVisual();

            GameManager.Instance?.SetCheckpoint(transform.position);
            Debug.Log($"Checkpoint set at {transform.position}");
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdateVisual();
        }

        void UpdateVisual()
        {
            if (sr) sr.color = IsActive ? Color.yellow : Color.gray;
        }
    }
}