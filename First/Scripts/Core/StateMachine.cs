// StateMachine.cs
// Both Player and Enemy will use this.
// a plain C# class managed by whoever owns it.

namespace Demo.Core
{
    public class StateMachine
    {
        public State CurrentState { get; private set; }

        public void Initialize(State startState)
        {
            CurrentState = startState;
            CurrentState.Enter();
        }

        public void ChangeState(State newState)
        {
            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState.Enter();
        }

        // Called by the owner's Update() and FixedUpdate()
        public void Update() => CurrentState?.Update();
        public void FixedUpdate() => CurrentState?.FixedUpdate();
    }
}


// State.cs
// Abstract base for every state (PlayerIdle, EnemyPatrol, etc.)
// Each concrete state gets a reference to its owner and the state machine
// so it can read data and trigger transitions.

namespace Demo.Core
{
    public abstract class State
    {
        protected StateMachine stateMachine;

        protected State(StateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }

        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void Exit() { }
    }
}