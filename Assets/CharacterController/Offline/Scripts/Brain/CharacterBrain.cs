using UnityEngine;

public class CharacterBrain : MonoBehaviour, IMovementBrain
{
    [SerializeField] public CharacterController characterController;
    [SerializeField] private MovementSO Data;
    [SerializeField] private MovementAnimationEventTrigger movementAnimationEventTrigger;

    [SerializeField] public Animator animator;
    [SerializeField] Transform meshTransform;
    [SerializeField] float rotationSmoothTime = 0.1f;

    [SerializeField] public Transform cameraPivot;
    [SerializeField] public float sensitivity = 2.5f;
    [SerializeField] public float minPitch = -40f;
    [SerializeField] public float maxPitch = 80f;
    [SerializeField] public float smoothTime = 0.05f;

    public MovementSO movementData
    {
        get => Data;
        protected set => Data = value;
    }

    public IMovementBrainStatePayload movementBrainStatePayload { get; set; }
    public IInputProvider<IMovementInputPayload> movementInputProvider { get; protected set; }

    public IAdvancedMotor movementMotor { get; protected set; }
    public CharacterCamera characterCamera { get; protected set; }
    public MovementStateMachine movementStateMachine { get; protected set; }

    public MovementAnimationEventTrigger MovementAnimationEventTrigger { get => movementAnimationEventTrigger; }
    public CharacterAnimatorController characterAnimatorController { get; protected set; }

    public void Awake()
    {
        characterAnimatorController = new CharacterAnimatorController(animator, this, rotationSmoothTime, meshTransform);

        movementBrainStatePayload = new MovementBrainStatePayload();

        movementMotor = new AdvancedCharacterControllerMotor(
            characterController,
            Data.GroundedData.GroundToFallRayDistance,
            Data.GroundedData.StickToGroundRayDistance,
            Data.GroundedData.GroundLayer,
            1,
            Data.AirborneData.Gravity.y);

        movementInputProvider = new PlayerInputProvider(cameraPivot);

        movementStateMachine = new MovementStateMachine(this);

        characterCamera = new CharacterCamera(this, cameraPivot, sensitivity, minPitch, maxPitch, smoothTime);
    }

    public void Update()
    {
        characterAnimatorController.Tick(Time.deltaTime);
        characterCamera.Tick(Time.deltaTime);
        movementInputProvider.Tick(Time.deltaTime);
    }

    public void FixedUpdate()
    {
        movementStateMachine.Tick(Time.fixedDeltaTime);
        movementMotor.ApplyForce(Time.fixedDeltaTime, movementBrainStatePayload.IsGrounded);
    }

    private void LateUpdate()
    {
        Debuger.Instance.Clear();
        Debuger.Instance.Add($"State : {movementStateMachine.CurrentState}");
        Debuger.Instance.Add($"Position : {movementMotor.Position}");
        Debuger.Instance.Add($"Velocity : {movementMotor.Velocity}");
        Debuger.Instance.Add($"IsGrounded : {movementMotor.IsGrounded}");
    }
}
