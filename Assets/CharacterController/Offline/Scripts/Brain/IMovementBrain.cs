public interface IMovementBrain
{
    IAdvancedMotor movementMotor { get; }
    MovementStateMachine movementStateMachine { get; }
    IMovementBrainStatePayload movementBrainStatePayload { get; set; }
    MovementSO movementData { get; }
    MovementAnimationEventTrigger MovementAnimationEventTrigger { get; }
    CharacterAnimatorController characterAnimatorController { get; }
}
