// Player.cs
// The concrete player class. Reads input, owns the state machine,
// exposes data that states need (moveInput, isGrounded, etc.)
//
// UNITY:
//1. Create a GameObject named "Player"
//2. Add: Rigidbody2D, BoxCollider2D, Health, this script
//3. Set Rigidbody2D → Gravity Scale = 3, Freeze Rotation Z = true
//4. Create an empty child GameObject named "GroundCheck" at the feet
//5. Assign groundCheck in the Inspector
// 6. Create a Layer named "Ground" and set it on your platform objects
//7. Install "Input System" package via Package Manager
//8. Go to Edit → Project Settings → Player → Active Input Handling → Both
//(or New Input System only)

using UnityEngine;
using UnityEngine.InputSystem;    // New Input System

namespace Demo.Player
{
    using Core;

    public class Player : LivingEntity
    {
        // ── Movement Settings ──────────────────────────────────────────────
        [Header("Movement")]
        public float moveSpeed = 8f;
        public float jumpForce = 16f;
        public float fallMultiplier = 2.5f;   // Makes falling feel snappier

        [Header("Dash")]
        public bool dashUnlocked = false;   // Toggle in Inspector to test
        public float dashSpeed = 20f;
        public float dashDuration = 0.15f;
        public float dashCooldown = 0.5f;

        [Header("Dive")]
        public float diveForce = 25f;         // Downward slam

        [Header("Ground Check")]
        public Transform groundCheck;
        public float groundCheckRadius = 0.2f;
        //public LayerMask groundLayer;

        [Header("Combat")]
        public float attackDamage = 20f;
        public float attackRange = 1.2f;    // Radius of placeholder attack hitbox
        public LayerMask enemyLayer;

        // ── Shared State (read by state classes) ──────────────────────────
        public Vector2 MoveInput { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool JumpHeld { get; private set; }
        public bool DashPressed { get; private set; }
        public bool DivePressed { get; private set; }
        public bool AttackPressed { get; private set; }
        public float DashCooldownTimer { get; set; }

        // ── State Machine ─────────────────────────────────────────────────
        public StateMachine stateMachine;
        public PlayerIdleState idleState;
        public PlayerRunState runState;
        public PlayerJumpState jumpState;
        public PlayerFallState fallState;
        public PlayerDiveState diveState;
        public PlayerDashState dashState;
        public PlayerAttackState attackState;
        public PlayerDeadState deadState;

        // ── Internal ──────────────────────────────────────────────────────
        private PlayerInputActions inputActions;   // Generated input asset
        //private PlayerInputActions inputActions = new PlayerInputActions();

        protected override void Awake()
        {
            base.Awake();
            inputActions = new PlayerInputActions(); // ← add this line here

            stateMachine = new StateMachine();
            idleState = new PlayerIdleState(this, stateMachine);
            runState = new PlayerRunState(this, stateMachine);
            jumpState = new PlayerJumpState(this, stateMachine);
            fallState = new PlayerFallState(this, stateMachine);
            diveState = new PlayerDiveState(this, stateMachine);
            dashState = new PlayerDashState(this, stateMachine);
            attackState = new PlayerAttackState(this, stateMachine);
            deadState = new PlayerDeadState(this, stateMachine);
        }

        void Start()
        {
            stateMachine.Initialize(idleState);
        }

        void OnEnable()
        {
            // New Input System setup — reads from auto-generated PlayerInputActions
            // (see README on how to create the Input Action Asset)
            inputActions = new PlayerInputActions();
            inputActions.Enable();
        }

        void OnDisable()
        {
            inputActions.Disable();
        }

        void Update()
        {
            ReadInput();
            DashCooldownTimer -= Time.deltaTime;
            stateMachine.Update();
        }

        void FixedUpdate()
        {
            ApplyBetterGravity();
            stateMachine.FixedUpdate();
        }

        void ReadInput()
        {
            var gameplay = inputActions.Gameplay;
            MoveInput = gameplay.Move.ReadValue<Vector2>();
            JumpPressed = gameplay.Jump.WasPressedThisFrame();
            JumpHeld = gameplay.Jump.IsPressed();
            DashPressed = gameplay.Dash.WasPressedThisFrame();
            //DivePressed = gameplay.Dive.WasPressedThisFrame();
            AttackPressed = gameplay.Attack.WasPressedThisFrame();
        }

        // Better-feeling jump: faster fall, hold to go higher
        void ApplyBetterGravity()
        {
            if (rb.linearVelocity.y < 0)
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }

        public override void HandleMovement()
        {
            rb.linearVelocity = new Vector2(MoveInput.x * moveSpeed, rb.linearVelocity.y);
            Flip(MoveInput.x);
        }

        /*public bool IsGrounded()
        {
            //return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius,
    LayerMask.GetMask("Ground"));
        }*/
        public bool IsGrounded()
        {
            bool grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius,
                LayerMask.GetMask("Ground"));
            Debug.Log($"IsGrounded: {grounded} | GroundCheck pos: {groundCheck.position} | Layer mask: {LayerMask.GetMask("Ground")}");
            return grounded;
        }

        // Called from AttackState — placeholder circle cast attack
        public void PerformAttack()
        {
            // Cast a circle in front of the player
            Vector2 attackPos = (Vector2)transform.position + Vector2.right * facingDirection * attackRange;
            Collider2D[] hits = Physics2D.OverlapCircleAll(attackPos, attackRange * 0.5f, enemyLayer);

            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<IDamageable>(out var target))
                    target.TakeDamage(attackDamage);
            }

            // Draw debug circle in Scene view so you can SEE the hitbox
            Debug.DrawRay(attackPos, Vector2.up * 0.3f, Color.red, 0.2f);
        }

        protected override void OnHurt()
        {
            Debug.Log($"Player hurt! HP: {health.CurrentHealth}");
        }

        protected override void OnDeath()
        {
            stateMachine.ChangeState(deadState);
        }
    }
}