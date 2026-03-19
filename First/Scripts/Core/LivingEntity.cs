// LivingEntity.cs
using UnityEngine;

namespace Demo.Core
{
    [RequireComponent(typeof(Health))]
    public abstract class LivingEntity : Entity, IDamageable
    {
        protected Health health;

        protected override void Awake()
        {
            base.Awake();
            health = GetComponent<Health>();
            health.OnDeath += Die;
        }

        public virtual void TakeDamage(float amount)
        {
            if (health.IsDead()) return;
            health.TakeDamage(amount);
            OnHurt();
        }

        public virtual void Die()
        {
            OnDeath();
        }

        protected virtual void OnHurt() { }
        protected virtual void OnDeath() { }

        // ── Animator Helpers ──────────────────────────────────────────────
        // All animator calls go through here — null-safe, and easy to expand
        // later when real animations are added. States call these instead of
        // touching animator directly.

        public void SetAnimBool(string parameter, bool value)
        {
            if (animator == null) return;
            animator.SetBool(parameter, value);
        }

        public void SetAnimTrigger(string parameter)
        {
            if (animator == null) return;
            animator.SetTrigger(parameter);
        }

        public void SetAnimFloat(string parameter, float value)
        {
            if (animator == null) return;
            animator.SetFloat(parameter, value);
        }

        public void SetAnimInt(string parameter, int value)
        {
            if (animator == null) return;
            animator.SetInteger(parameter, value);
        }
    }
}