// GameManager.cs
// Central singleton.
// Manages: game state, player death/respawn, checkpoint logic.
// Uses DontDestroyOnLoad so it persists across scenes.
//
// UNITY SETUP:
//1. Create an empty GameObject named "GameManager"
//2. Add this script
//3. Assign playerObject in Inspector (drag your Player GameObject)
//4. Assign respawnDelay if desired
//5. Assign deathMenuUI (a Canvas/Panel with two buttons: "Return" and "Restart")

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace Demo.Systems
{
    using Core;
    using Demo.UI;
    using Unity.Multiplayer.Center.Common.Analytics;

    public class GameManager : MonoBehaviour
    {
        // ── Singleton ─────────────────────────────────────────────────────
        public static GameManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ── References ────────────────────────────────────────────────────
        [Header("Player")]
        public GameObject playerObject;
        public float respawnDelay = 2f;

        [Header("UI")]
        public DeathMenuUI deathMenuUI;
        public CheckpointPromptUI checkpointPromptUI;
        //public GameObject checkpointPromptUI;  // "Press E to save checkpoint" panel
        //public GameObject deathMenuUI;         // Panel with Return / Restart buttons

        [Header("Input")]
        public InputActionReference interactAction;  // Assign in Inspector
        public InputActionReference returnAction;    // Assign in Inspector
        public InputActionReference restartAction;   // Assign in Inspector
        // ── State ─────────────────────────────────────────────────────────
        private CheckpointData checkpointData = new CheckpointData();
        private Checkpoint pendingCheckpoint;

        // ── Unity Lifecycle ───────────────────────────────────────────────
        void Start()
        {
        }

        void Update()
        {
            // Checkpoint interaction
            if (pendingCheckpoint != null && interactAction != null
                && interactAction.action.WasPressedThisFrame())
            {
                pendingCheckpoint.Activate();
                HideCheckpointPrompt();
            }

            // Death menu keyboard fallback
            if (deathMenuUI != null && deathMenuUI.gameObject.activeSelf)
            {
                if (returnAction != null && returnAction.action.WasPressedThisFrame()
                    && checkpointData.HasData)
                    ReturnToCheckpoint();

                if (restartAction != null && restartAction.action.WasPressedThisFrame())
                    Restart();
            }
        }

        // ── Checkpoint ────────────────────────────────────────────────────
        public void SetCheckpoint(Vector2 position)
        {
            float currentHealth = 0;
            if (playerObject.TryGetComponent<Health>(out var hp))
                currentHealth = hp.CurrentHealth;

            checkpointData.Save(currentHealth, position);
            Debug.Log($"Checkpoint saved — HP: {currentHealth} at {position}");
        }

        public void ShowCheckpointPrompt(Checkpoint checkpoint)
        {
            pendingCheckpoint = checkpoint;
            checkpointPromptUI?.Show();
        }

        public void HideCheckpointPrompt()
        {
            pendingCheckpoint = null;
            checkpointPromptUI?.Hide();
        }

        // ── Death Handling ────────────────────────────────────────────────
        public void OnPlayerDeath()
        {
            Debug.Log("Player has died.");
            ShowDeathMenu(isEnemyDeath: false);
        }

        public void OnEnemyDeath(string enemyName)
        {
            Debug.Log($"{enemyName} has been defeated! The path forward is clear.");
            ShowDeathMenu(isEnemyDeath: true);
        }

        // Shows the death/victory menu with appropriate message
        // isEnemyDeath = true means the player won (enemy died)
        // isEnemyDeath = false means the player lost (player died)
        private void ShowDeathMenu(bool isEnemyDeath)
        {
            Time.timeScale = 0f;

            string message = isEnemyDeath
                ? "Enemy defeated!\nReturn to checkpoint or restart?"
                : "You died.\nReturn to checkpoint or restart?";

            deathMenuUI?.Show(message, showReturnButton: checkpointData.HasData);

            // Console fallback
            Debug.Log(checkpointData.HasData
                ? "Options: [R] Return to Checkpoint | [N] Restart"
                : "No checkpoint saved. Options: [N] Restart");
        }

        // Called by the "Return to Checkpoint" button in the death menu UI
        // Also triggered by pressing R in the fallback keyboard check below
        public void ReturnToCheckpoint()
        {
            if (!checkpointData.HasData)
            {
                Debug.Log("No checkpoint data — restarting instead.");
                Restart();
                return;
            }

            Time.timeScale = 1f;
            //if (deathMenuUI) deathMenuUI.SetActive(false);
            deathMenuUI?.Hide();
            StartCoroutine(RespawnRoutine());
        }

        // Called by the "Restart" button in the death menu UI
        public void Restart()
        {
            Time.timeScale = 1f;
            checkpointData.Clear();
            //if (deathMenuUI) deathMenuUI.SetActive(false);
            deathMenuUI?.Hide();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            Debug.Log("Restarting scene.");
        }

        // Fallback keyboard input for before the UI buttons are wired up
        // Remove this once your UI buttons call ReturnToCheckpoint() and Restart()
        void OnGUI()
        {
            if (deathMenuUI != null && !deathMenuUI.gameObject.activeSelf) return;

            if (returnAction != null && returnAction.action.WasPressedThisFrame()
                && checkpointData.HasData)
                ReturnToCheckpoint();

            if (restartAction != null && restartAction.action.WasPressedThisFrame())
                Restart();
        }

        // ── Respawn Routine ───────────────────────────────────────────────
        IEnumerator RespawnRoutine()
        {
            yield return new WaitForSecondsRealtime(respawnDelay); // RealTime ignores timeScale

            if (playerObject == null) yield break;

            // Restore position and health to what was saved at checkpoint
            playerObject.transform.position = checkpointData.Position;

            if (playerObject.TryGetComponent<Health>(out var hp))
            {
                hp.ResetHealth();
                // Restore to the HP value saved at checkpoint, not full health
                float damageToApply = hp.maxHealth - checkpointData.Health;
                if (damageToApply > 0) hp.TakeDamage(damageToApply);
            }

            // Re-enable collider
            if (playerObject.TryGetComponent<Collider2D>(out var col))
                col.enabled = true;

            // Reset state machine to idle
            if (playerObject.TryGetComponent<Player.Player>(out var player))
                player.stateMachine.ChangeState(player.idleState);

            Debug.Log($"Respawned at checkpoint — HP: {checkpointData.Health}");
        }
    }
}