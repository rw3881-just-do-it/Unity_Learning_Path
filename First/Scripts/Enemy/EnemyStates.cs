// EnemyStates.cs
// States: Patrol → Chase → Attack → Dead

using UnityEngine;

namespace Demo.Enemy
{
    using Core;

    // ── PATROL ────────────────────────────────────────────────────────────
    // Walks back and forth between patrol points (or just flips at walls)
    public class EnemyPatrolState : State
    {
        private readonly GroundEnemy e;
        private int currentPatrolIndex = 0;
        private float patrolWaitTimer;
        private const float WAIT_AT_POINT = 1f;

        public EnemyPatrolState(GroundEnemy enemy, StateMachine sm) : base(sm) => e = enemy;

        public override void Enter()
        {
            patrolWaitTimer = 0;
            //e.animator?.SetBool("isWalking", true);
            e.SetAnimBool("isWalking", true);
        }

        public override void Update()
        {
            if (e.PlayerInDetectionRange())
            {
                stateMachine.ChangeState(e.chaseState);
                return;
            }
        }

        public override void FixedUpdate()
        {
            if (e.patrolPoints == null || e.patrolPoints.Length == 0)
            {
                // No waypoints set — just flip at a wall or edge
                SimplePatrol();
                return;
            }

            PatrolBetweenPoints();
        }

        void SimplePatrol()
        {
            e.HandleMovement();

            // Flip if hitting a wall (check via raycast)
            Vector2 origin = e.transform.position;
            Vector2 direction = Vector2.right * e.facingDirection;
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, 0.6f, ~LayerMask.GetMask("Enemy"));

            if (hit.collider != null)
                e.facingDirection *= -1;
        }

        void PatrolBetweenPoints()
        {
            if (patrolWaitTimer > 0) { patrolWaitTimer -= Time.fixedDeltaTime; return; }

            Transform target = e.patrolPoints[currentPatrolIndex];
            float distToTarget = Vector2.Distance(e.transform.position, target.position);

            if (distToTarget < 0.3f)
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % e.patrolPoints.Length;
                patrolWaitTimer = WAIT_AT_POINT;
                return;
            }

            float dir = Mathf.Sign(target.position.x - e.transform.position.x);
            e.facingDirection = dir;
            e.HandleMovement();
        }

        public override void Exit() => e.SetAnimBool("isWalking", false);//e.animator?.SetBool("isWalking", false);
    }

    // ── CHASE ─────────────────────────────────────────────────────────────
    public class EnemyChaseState : State
    {
        private readonly GroundEnemy e;
        public EnemyChaseState(GroundEnemy enemy, StateMachine sm) : base(sm) => e = enemy;

        public override void Enter() => e.SetAnimBool("isWalking", true);//e.animator?.SetBool("isWalking", true);

        public override void Update()
        {
            if (!e.PlayerInDetectionRange())
            {
                stateMachine.ChangeState(e.patrolState);
                return;
            }
            if (e.PlayerInAttackRange() && e.AttackCooldownTimer <= 0)
            {
                stateMachine.ChangeState(e.attackState);
                return;
            }
        }

        public override void FixedUpdate() => e.ChasePlayer();

        public override void Exit() => e.SetAnimBool("isWalking", false);//e.animator?.SetBool("isWalking", false);
    }

    // ── ATTACK ────────────────────────────────────────────────────────────
    public class EnemyAttackState : State
    {
        private readonly GroundEnemy e;
        private float attackDuration = 0.6f;
        private float timer;
        private bool hasHit;

        public EnemyAttackState(GroundEnemy enemy, StateMachine sm) : base(sm) => e = enemy;

        public override void Enter()
        {
            timer = attackDuration;
            hasHit = false;
            e.rb.linearVelocity = Vector2.zero; // Stop moving while attacking
            //e.animator?.SetTrigger("Attack");
            e.SetAnimTrigger("Attack");
            e.AttackCooldownTimer = e.attackCooldown;
        }

        public override void Update()
        {
            timer -= Time.deltaTime;

            if (!hasHit && timer < attackDuration * 0.5f)
            {
                e.PerformAttack();
                hasHit = true;
            }

            if (timer <= 0)
                stateMachine.ChangeState(e.PlayerInDetectionRange() ? e.chaseState : e.patrolState);
        }
    }

    // ── DEAD ──────────────────────────────────────────────────────────────
    public class EnemyDeadState : State
    {
        public EnemyDeadState(GroundEnemy enemy, StateMachine sm) : base(sm) { }
        // Nothing to do — Enemy.OnDeath() already handles cleanup
    }
}