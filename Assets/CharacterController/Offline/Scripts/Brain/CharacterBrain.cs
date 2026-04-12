using UnityEngine;

/// <summary>
/// MonoBehaviour wrapper around CharacterBrainCore for offline (non-networked) play.
/// Owns all serialised Unity references, constructs CharacterBrainCore in Awake,
/// then drives it from Unity's standard lifecycle callbacks.
///
/// Registration is performed on the core (Brain), not on this MonoBehaviour,
/// so WorldSimulation ticks the simulation object directly.
///
/// Fixed tick flow:
///   WorldSimulation → Brain (ISimulatableEntity) → Brain.OnSimulateTick(dt)
///
/// Per-frame flow:
///   Update → Brain.OnUpdate(dt)
/// </summary>
public class CharacterBrain : MonoBehaviour
{
    [SerializeField] public  CharacterController           characterController;
    [SerializeField] private MovementSO                    movementSO;
    [SerializeField] private MovementAnimationEventTrigger movementAnimationEventTrigger;
    [SerializeField] public  Animator                      animator;
    [SerializeField]         Transform                     meshTransform;
    [SerializeField]         float                         rotationSmoothTime = 0.1f;
    [SerializeField] public  Transform                     cameraPivot;
    [SerializeField] public  float                         sensitivity        = 2.5f;
    [SerializeField] public  float                         minPitch           = -40f;
    [SerializeField] public  float                         maxPitch           =  80f;
    [SerializeField] public  float                         smoothTime         = 0.05f;

    public CharacterBrainCore Brain { get; private set; }

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    protected virtual void Awake()
    {
        if (characterController == null) { Debug.LogError("CharacterController not assigned.", this); return; }
        if (movementSO          == null) { Debug.LogError("MovementSO not assigned.",          this); return; }
        if (animator            == null) { Debug.LogError("Animator not assigned.",            this); return; }
        if (cameraPivot         == null) { Debug.LogError("CameraPivot not assigned.",         this); return; }
        if (meshTransform       == null) { Debug.LogError("MeshTransform not assigned.",       this); return; }

        Brain = new CharacterBrainCore(
            characterController, movementSO, animator, meshTransform,
            movementAnimationEventTrigger, cameraPivot,
            sensitivity, minPitch, maxPitch, smoothTime, rotationSmoothTime);
    }

    protected virtual void Start()   { if (Brain != null) Brain.Register(WorldSimulation.Instance); }
    private          void Update()   { if (Brain != null) Brain.OnUpdate(Time.deltaTime); }
    private          void OnDestroy(){ if (Brain != null) Brain.Unregister(WorldSimulation.Instance); }

    private void LateUpdate()
    {
        if (Brain == null) return;
        Debuger.Instance.Clear();
        Debuger.Instance.Add($"State    : {Brain.movementStateMachine.CurrentState}");
        Debuger.Instance.Add($"Position : {Brain.movementMotor.Position}");
        Debuger.Instance.Add($"Velocity : {Brain.movementMotor.Velocity}");
        Debuger.Instance.Add($"Grounded : {Brain.movementMotor.IsGrounded}");
    }
}
