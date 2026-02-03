using UnityEngine;

public interface IMovementBrain
{
    public IAdvancedMotor movementMotor { get; }

    public CharacterAnimatorController characterAnimatorController { get; }
    public MovementStateMachine movementStateMachine { get; }
    public IInputProvider<MovementInputPayload> movementInputProvider { get; }
    public MovementBrainStatePayload movementBrainStatePayload { get; set; }
    public MovementSO movementData { get; }
    public MovementAnimationEventTrigger MovementAnimationEventTrigger { get; }

}
