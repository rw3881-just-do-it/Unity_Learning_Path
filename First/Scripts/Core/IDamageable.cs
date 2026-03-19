// IDamageable.cs
// Any object that can receive damage implements this interface.
// Lets the hitbox system stay generic — it just calls TakeDamage() without
// needing to know if the target is a Player, Enemy, or destructible object.

namespace Demo.Core
{
    public interface IDamageable
    {
        void TakeDamage(float amount);
        void Die();
    }
}