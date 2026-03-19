// Enemy.cs
// Abstract enemy base. All enemy types extend this.
// GroundEnemy is the concrete demo enemy.
//
// UNITY SETUP (GroundEnemy):
//   1. Create a GameObject named "Enemy"
//   2. Add: Rigidbody2D, BoxCollider2D, Health, GroundEnemy script
//   3. Set Rigidbody2D → Freeze Rotation Z = true, Gravity Scale = 3
//   4. Tag the Player GameObject as "Player"
//   5. Set enemyLayer on the Player's Inspector to match this enemy's Layer
//   6. Set playerLayer on this enemy to match the Player's Layer

using UnityEngine;

namespace Demo.Enemy
{
    using Core;

    public abstract class Enemy : LivingEntity
    {
        [Header("Detection")]
        public float detectionRange = 5f;
        public float attackRange = 1f;
        public LayerMask playerLayer;

        [Header("Combat")]
        public float attackDamage = 10f;
        public float attackCooldown = 1.5f;
        public float AttackCooldownTimer { get; set; }

        protected Transform player;
        public StateMachine stateMachine;

        protected override void Awake()
        {
            base.Awake();
            stateMachine = new StateMachine();

            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj) player = playerObj.transform;
            else Debug.LogWarning("Enemy: No GameObject tagged 'Player' found.");
        }

        void Update()
        {
            AttackCooldownTimer -= Time.deltaTime;
            stateMachine.Update();
        }

        void FixedUpdate() => stateMachine.FixedUpdate();

        public bool PlayerInDetectionRange()
            => player && Vector2.Distance(transform.position, player.position) <= detectionRange;

        public bool PlayerInAttackRange()
            => player && Vector2.Distance(transform.position, player.position) <= attackRange;

        // Called from attack state to deal damage to player
        public void PerformAttack()
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, attackRange, playerLayer);
            if (hit && hit.TryGetComponent<IDamageable>(out var target))
                target.TakeDamage(attackDamage);
        }

        protected override void OnHurt()
        {
            Debug.Log($"{name} hurt! HP: {health.CurrentHealth}");
        }

        protected override void OnDeath()
        {
            col.enabled = false;
            rb.linearVelocity = Vector2.zero;
            animator?.SetTrigger("Die");
            Debug.Log($"{name} defeated!");
            Destroy(gameObject, 1f);
        }
    }
}


// GroundEnemy.cs — a concrete, patrolling enemy
namespace Demo.Enemy
{
    using Core;

    public class GroundEnemy : Enemy
    {
        [Header("Patrol")]
        public float patrolSpeed = 2f;
        public Transform[] patrolPoints; // Assign 2 empty GameObjects as waypoints in Inspector

        public EnemyPatrolState patrolState;
        public EnemyChaseState chaseState;
        public EnemyAttackState attackState;
        public EnemyDeadState deadState;

        protected override void Awake()
        {
            base.Awake();
            patrolState = new EnemyPatrolState(this, stateMachine);
            chaseState = new EnemyChaseState(this, stateMachine);
            attackState = new EnemyAttackState(this, stateMachine);
            deadState = new EnemyDeadState(this, stateMachine);
        }

        void Start() => stateMachine.Initialize(patrolState);

        public override void HandleMovement()
        {
            rb.linearVelocity = new Vector2(facingDirection * patrolSpeed, rb.linearVelocity.y);
            Flip(facingDirection);
        }

        // Called when chasing — move toward player at chase speed
        public void ChasePlayer()
        {
            if (!player) return;
            float dir = Mathf.Sign(player.position.x - transform.position.x);
            rb.linearVelocity = new Vector2(dir * patrolSpeed * 1.8f, rb.linearVelocity.y);
            Flip(dir);
        }
    }
}