// Health.cs
// Reusable component. Attach to Player, Enemy, or any destructible object.
// Uses C# events so UI, audio, and effects can react WITHOUT tight coupling.
//
// Setup: Add this component to any GameObject that can take damage.

using UnityEngine;
using System;

namespace Demo.Core
{
    public class Health : MonoBehaviour
    {
        [Header("Stats")]
        public float maxHealth = 100f;

        // Public getter, private setter — other scripts can READ health but
        // cannot SET it directly (they must go through TakeDamage / Heal)
        public float CurrentHealth { get; private set; }

        // Events — subscribe to these from UI, audio, VFX scripts
        public event Action<float, float> OnHealthChanged; // (current, max)
        public event Action OnDeath;

        // Prevent Die() from firing more than once
        private bool isDead = false;

        void Awake()
        {
            CurrentHealth = maxHealth;
        }

        public void TakeDamage(float amount)
        {
            if (isDead) return;

            CurrentHealth = Mathf.Clamp(CurrentHealth - amount, 0, maxHealth);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

            if (CurrentHealth <= 0)
            {
                isDead = true;
                OnDeath?.Invoke();
            }
        }

        public void Heal(float amount)
        {
            if (isDead) return;
            CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, maxHealth);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        public void ResetHealth()
        {
            isDead = false;
            CurrentHealth = maxHealth;
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        public bool IsDead() => isDead;
    }
}