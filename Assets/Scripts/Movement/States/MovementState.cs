using UnityEngine;
using UnityEngine.Windows;
using Unity.Netcode;


public class MovementState : IState<MovementInputPayload, MovementStatePayload>
{
    private MovementStateMachine stateMachine;

    public MovementState(MovementStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public void Enter()
    {
    }

    public void Exit()
    {
    }

    public MovementStatePayload Simulate(MovementInputPayload input) 
    {
        MovementStatePayload prevState = stateMachine.GetPrevState(input.Tick);

        if (input.MoveInput == Vector2.zero)
        {
            return new MovementStatePayload
            {
                Tick = input.Tick,
                Position = prevState.Position,
            };
        }

        // Horizontal movement
        Vector3 movementInputDirection = GetMovementInputDirection(input);

        return new MovementStatePayload
        {
            Tick = input.Tick,
            Position = prevState.Position + movementInputDirection * stateMachine.tickDelta,
        };
    }

    protected Vector3 GetMovementInputDirection(MovementInputPayload input)
    {
        Vector3 movementInputDirection = new Vector3(input.MoveInput.x, 0f, input.MoveInput.y);
        if (movementInputDirection.sqrMagnitude > 1f)
            movementInputDirection.Normalize();
        return movementInputDirection;
    }

}