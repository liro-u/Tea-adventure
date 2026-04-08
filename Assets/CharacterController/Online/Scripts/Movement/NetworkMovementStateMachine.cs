using System;
using UnityEngine;

public class NetworkMovementStateMachine : MovementStateMachine
{
    public NetworkMovementStateMachine(IMovementBrain movementBrain) : base(movementBrain)
    {
    }

    public MovementState getStateById(MovementStateId id)
    {
        return id switch
        {
            MovementStateId.Idling => IdlingState,

            MovementStateId.Walking => WalkingState,
            MovementStateId.Running => RunningState,
            MovementStateId.Sprinting => SprintingState,

            MovementStateId.LightStopping => LightStoppingState,
            MovementStateId.MediumStopping => MediumStoppingState,
            MovementStateId.HardStopping => HardStoppingState,

            MovementStateId.LightLanding => LightLandingState,
            MovementStateId.HardLanding => HardLandingState,
            MovementStateId.Rolling => RollingState,

            MovementStateId.Jumping => JumpingState,
            MovementStateId.Falling => FallingState,

            _ => throw new ArgumentOutOfRangeException(
                nameof(id),
                id,
                "Unknown MovementStateId. Network state is invalid or desynced."
            )
        };
    }

    public MovementStateId getIdByState(MovementState state)
    {
        return state.GetType() switch
        {
            var t when t == typeof(IdlingState) => MovementStateId.Idling,
            var t when t == typeof(WalkingState) => MovementStateId.Walking,
            var t when t == typeof(RunningState) => MovementStateId.Running,
            var t when t == typeof(SprintingState) => MovementStateId.Sprinting,

            var t when t == typeof(LightStoppingState) => MovementStateId.LightStopping,
            var t when t == typeof(MediumStoppingState) => MovementStateId.MediumStopping,
            var t when t == typeof(HardStoppingState) => MovementStateId.HardStopping,

            var t when t == typeof(LightLandingState) => MovementStateId.LightLanding,
            var t when t == typeof(HardLandingState) => MovementStateId.HardLanding,
            var t when t == typeof(RollingState) => MovementStateId.Rolling,

            var t when t == typeof(JumpingState) => MovementStateId.Jumping,
            var t when t == typeof(FallingState) => MovementStateId.Falling,

            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "Unknown MovementState. Network state is invalid or desynced."
            )
        };
    }
}
