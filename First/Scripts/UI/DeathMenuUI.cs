// DeathMenuUI.cs
// Attach to the death menu Canvas/Panel GameObject.
// Wires the Return and Restart buttons to GameManager.
//
// UNITY SETUP:
//   1. Create: GameObject → UI → Canvas → name it "DeathMenuUI"
//   2. Add a Text child → name it "MessageText"
//   3. Add two Button children → name them "ReturnButton" and "RestartButton"
//   4. Add this script to the Canvas GameObject
//   5. Assign MessageText, ReturnButton, RestartButton in Inspector
//   6. Drag this Canvas into GameManager's deathMenuUI field

using UnityEngine;
using UnityEngine.UI;

namespace Demo.UI
{
    using Systems;

    public class DeathMenuUI : MonoBehaviour
    {
        [Header("References")]
        public Text messageText;
        public Button returnButton;
        public Button restartButton;

        void Start()
        {
            // Wire buttons to GameManager methods
            returnButton.onClick.AddListener(() => GameManager.Instance?.ReturnToCheckpoint());
            restartButton.onClick.AddListener(() => GameManager.Instance?.Restart());

            gameObject.SetActive(false); // Hidden by default
        }

        public void Show(string message, bool showReturnButton)
        {
            gameObject.SetActive(true);

            if (messageText)
                messageText.text = message;

            // Hide Return button if no checkpoint data exists
            if (returnButton)
                returnButton.gameObject.SetActive(showReturnButton);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}