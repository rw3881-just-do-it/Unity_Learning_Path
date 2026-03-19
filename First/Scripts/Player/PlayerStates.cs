// PlayerStates.cs
// All player states in one file for demo clarity.
// In a full project, need to split each into its own file.
//
// States: Idle → Run → Jump → Fall → Dive → Dash → Attack → Dead

using UnityEngine;
using Demo.Systems;

namespace Demo.Player
{
    using Core;

    // ── IDLE ──────────────────────────────────────────────────────────────
    public class PlayerIdleState : State
    {
        private readonly Player p;
        public PlayerIdleState(Player player, StateMachine sm) : base(sm) => p = player;

        public override void Enter()
        {
            p.rb.linearVelocity = new Vector2(0, p.rb.linearVelocity.y); // stop horizontal drift
            //p.animator?.SetBool("isRunning", false);
            p.SetAnimBool("isRunning", false);
        }

        public override void Update()
        {
            if (p.AttackPressed) { stateMachine.ChangeState(p.attackState); return; }
            if (p.MoveInput.x != 0) { stateMachine.ChangeState(p.runState); return; }
            if (p.JumpPressed && p.IsGrounded()) { stateMachine.ChangeState(p.jumpState); return; }
            if (p.DivePressed && !p.IsGrounded()) { stateMachine.ChangeState(p.diveState); return; }
            if (p.DashPressed && p.dashUnlocked
                && p.DashCooldownTimer <= 0) { stateMachine.ChangeState(p.dashState); return; }
            if (p.rb.linearVelocity.y < -0.1f) { stateMachine.ChangeState(p.fallState); return; }
        }
    }

    // ── RUN ───────────────────────────────────────────────────────────────
    public class PlayerRunState : State
    {
        private readonly Player p;
        public PlayerRunState(Player player, StateMachine sm) : base(sm) => p = player;

        public override void Enter() => p.SetAnimBool("isRunning", true);//p.animator?.SetBool("isRunning", true);
        public override void Exit() => p.SetAnimBool("isRunning", false);//p.animator?.SetBool("isRunning", false);

        public override void Update()
        {
            if (p.AttackPressed) { stateMachine.ChangeState(p.attackState); return; }
            if (p.MoveInput.x == 0) { stateMachine.ChangeState(p.idleState); return; }
            if (p.JumpPressed && p.IsGrounded()) { stateMachine.ChangeState(p.jumpState); return; }
            if (p.DashPressed && p.dashUnlocked
                && p.DashCooldownTimer <= 0) { stateMachine.ChangeState(p.dashState); return; }
            if (p.rb.linearVelocity.y < -0.1f && !p.IsGrounded()) { stateMachine.ChangeState(p.fallState); return; }
        }

        public override void FixedUpdate() => p.HandleMovement();
    }

    // ── JUMP ──────────────────────────────────────────────────────────────
    public class PlayerJumpState : State
    {
        private readonly Player p;
        public PlayerJumpState(Player player, StateMachine sm) : base(sm) => p = player;

        public override void Enter()
        {
            p.rb.linearVelocity = new Vector2(p.rb.linearVelocity.x, p.jumpForce);
            //p.animator?.SetBool("isJumping", true);
            p.SetAnimBool("isJumping", true);
        }

        public override void Exit() => p.SetAnimBool("isJumping", false);//p.animator?.SetBool("isJumping", false);

        public override void Update()
        {
            if (p.DivePressed) { stateMachine.ChangeState(p.diveState); return; }
            if (p.DashPressed && p.dashUnlocked
                && p.DashCooldownTimer <= 0) { stateMachine.ChangeState(p.dashState); return; }
            if (p.AttackPressed) { stateMachine.ChangeState(p.attackState); return; }
            if (p.rb.linearVelocity.y < 0) { stateMachine.ChangeState(p.fallState); return; }
        }

        public override void FixedUpdate() => p.HandleMovement(); // allow air control
    }

    // ── FALL ──────────────────────────────────────────────────────────────
    public class PlayerFallState : State
    {
        private readonly Player p;
        public PlayerFallState(Player player, StateMachine sm) : base(sm) => p = player;

        public override void Update()
        {
            if (p.DivePressed) { stateMachine.ChangeState(p.diveState); return; }
            if (p.DashPressed && p.dashUnlocked
                && p.DashCooldownTimer <= 0) { stateMachine.ChangeState(p.dashState); return; }
            if (p.AttackPressed) { stateMachine.ChangeState(p.attackState); return; }
            if (p.IsGrounded()) { stateMachine.ChangeState(p.idleState); return; }
        }

        public override void FixedUpdate() => p.HandleMovement();
    }

    // ── DIVE ──────────────────────────────────────────────────────────────
    // Downward slam — locks horizontal movement, blasts downward
    public class PlayerDiveState : State
    {
        private readonly Player p;
        public PlayerDiveState(Player player, StateMachine sm) : base(sm) => p = player;

        public override void Enter()
        {
            // Slam straight down regardless of current velocity
            p.rb.linearVelocity = new Vector2(0, -p.diveForce);
        }

        public override void Update()
        {
            if (p.IsGrounded()) stateMachine.ChangeState(p.idleState);
        }
        // No FixedUpdate movement — we let the dive velocity carry through
    }

    // ── DASH ──────────────────────────────────────────────────────────────
    // Dashes in the direction of input (or facing direction if no input)
    // Unlockable — set Player.dashUnlocked = true in Inspector
    public class PlayerDashState : State
    {
        private readonly Player p;
        private float dashTimer;
        private Vector2 dashDirection;

        public PlayerDashState(Player player, StateMachine sm) : base(sm) => p = player;

        public override void Enter()
        {
            dashTimer = p.dashDuration;
            p.DashCooldownTimer = p.dashCooldown;

            // Dash toward input, or fall back to facing direction
            dashDirection = p.MoveInput.magnitude > 0.1f
                ? p.MoveInput.normalized
                : new Vector2(p.facingDirection, 0);

            p.rb.gravityScale = 0; // Ignore gravity during dash
            p.rb.linearVelocity = dashDirection * p.dashSpeed;
        }

        public override void Update()
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0) stateMachine.ChangeState(p.IsGrounded() ? p.idleState : p.fallState);
        }

        public override void Exit()
        {
            p.rb.gravityScale = 3f; // Restore gravity (match your Rigidbody2D setting)
            p.rb.linearVelocity = new Vector2(p.rb.linearVelocity.x * 0.5f, p.rb.linearVelocity.y);
        }
    }

    // ── ATTACK ────────────────────────────────────────────────────────────
    // Placeholder attack — fixed duration, deals damage via PerformAttack()
    public class PlayerAttackState : State
    {
        private readonly Player p;
        private float attackDuration = 0.3f;  // How long the attack "animation" lasts
        private float timer;
        private bool hasHit;

        public PlayerAttackState(Player player, StateMachine sm) : base(sm) => p = player;

        public override void Enter()
        {
            timer = attackDuration;
            hasHit = false;
            //p.animator?.SetTrigger("Attack");
            p.SetAnimTrigger("Attack");
        }

        public override void Update()
        {
            timer -= Time.deltaTime;

            // Deal damage halfway through the attack window
            if (!hasHit && timer < attackDuration * 0.5f)
            {
                p.PerformAttack();
                hasHit = true;
            }

            if (timer <= 0)
                stateMachine.ChangeState(p.IsGrounded() ? p.idleState : p.fallState);
        }
    }

    // ── DEAD ──────────────────────────────────────────────────────────────
    public class PlayerDeadState : State
    {
        private readonly Player p;
        public PlayerDeadState(Player player, StateMachine sm) : base(sm) => p = player;

        public override void Enter()
        {
            p.rb.linearVelocity = Vector2.zero;
            p.col.enabled = false;
            //p.animator?.SetTrigger("Die");
            p.SetAnimTrigger("Die");
            Debug.Log("Player died. Waiting for respawn...");

            // Notify the checkpoint system via GameManager
            GameManager.Instance?.OnPlayerDeath();
        }
    }
}