// Entity.cs
// The most abstract base. Every "thing" in the game world inherits from this.
// Attach to any GameObject that needs physics, animation, and collision.

using UnityEngine;

namespace Demo.Core
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public abstract class Entity : MonoBehaviour
    {
        [Header("Core Components (auto-assigned)")]
        [HideInInspector] public Rigidbody2D rb;
        [HideInInspector] public Collider2D col;
        [HideInInspector] public Animator animator;   // Optional — placeholder sprites won't need this yet

        //protected float facingDirection = 1f; // 1 = right, -1 = left
        public float facingDirection = 1f;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            animator = GetComponent<Animator>(); // null is fine for placeholder
        }

        // Flip the sprite based on movement direction
        protected void Flip(float moveInput)
        {
            if (moveInput == 0) return;
            facingDirection = Mathf.Sign(moveInput);
            transform.localScale = new Vector3(facingDirection, 1f, 1f);
        }

        // All entities must define how they move
        public abstract void HandleMovement();
    }
}