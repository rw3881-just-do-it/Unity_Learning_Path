// HealthBarUI.cs
// Displays a health bar that reacts to a Health component.
// Uses Unity UI (Canvas → Slider).
//
// UNITY:
//1. Create: GameObject → UI → Canvas
//2. Inside Canvas: GameObject → UI → Slider
//3. On the Slider: remove the Handle child (we don't need a draggable handle)
//4. Create an empty GameObject, add this script
//5. Assign the Slider and the target's Health component in Inspector

using UnityEngine;
using UnityEngine.UI;

namespace Demo.UI
{
    using Core;

    public class HealthBarUI : MonoBehaviour
    {
        [Header("References")]
        public Health targetHealth;  // Drag Player or Enemy here
        public Slider slider;
        public Image fillImage;     // The Fill of the Slider

        [Header("Colors")]
        public Color fullColor = Color.green;
        public Color lowColor = Color.red;
        public float lowThreshold = 0.3f;  // Below 30% = red

        void Start()
        {
            if (targetHealth == null) return;

            // Initialize slider
            slider.minValue = 0;
            slider.maxValue = targetHealth.maxHealth;
            slider.value = targetHealth.CurrentHealth;

            // Subscribe to health changes
            targetHealth.OnHealthChanged += UpdateBar;

            UpdateBar(targetHealth.CurrentHealth, targetHealth.maxHealth);
        }

        void OnDestroy()
        {
            if (targetHealth != null)
                targetHealth.OnHealthChanged -= UpdateBar;
        }

        void UpdateBar(float current, float max)
        {
            slider.value = current;

            // Color shift: green when healthy, red when low
            if (fillImage)
            {
                float ratio = current / max;
                fillImage.color = Color.Lerp(lowColor, fullColor, ratio / lowThreshold);
            }
        }
    }
}