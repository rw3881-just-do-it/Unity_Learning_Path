// CheckpointPromptUI.cs
// Attach to the checkpoint prompt Canvas/Panel GameObject.
// Just shows "Press E to save checkpoint" when player is near one.
//
// UNITY SETUP:
//   1. Create: GameObject → UI → Canvas → name it "CheckpointPromptUI"
//   2. Add a Text child → set text to "Press E to save checkpoint"
//   3. Add this script to the Canvas GameObject
//   4. Drag this Canvas into GameManager's checkpointPromptUI field

using UnityEngine;

namespace Demo.UI
{
    public class CheckpointPromptUI : MonoBehaviour
    {
        void Start()
        {
            gameObject.SetActive(false); // Hidden by default
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);
    }
}