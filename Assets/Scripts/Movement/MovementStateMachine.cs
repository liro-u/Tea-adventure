using UnityEngine;
using Unity.Netcode;

public struct MovementInputPayload : ITickPayload, INetworkSerializeByMemcpy
{
    public int Tick { get; set; }
    public Vector2 MoveInput;
}

public struct MovementStatePayload : ITickPayload, INetworkSerializeByMemcpy
{
    public int Tick { get; set; }
    public Vector3 Position;
}

[GenerateSerializationForType(typeof(MovementInputPayload))]
[GenerateSerializationForType(typeof(MovementStatePayload))]
public class MovementStateMachine : StateMachine<MovementInputPayload, MovementStatePayload>
{
    [SerializeField] public Transform cameraPivot;
    [SerializeField] public CharacterController characterController;

    public IdlingState IdlingState;

    public WalkingState WalkingState;
    public RunningState RunningState;

    public MovementStateMachine()
    {
        IdlingState = new IdlingState(this);

        WalkingState = new WalkingState(this);
        RunningState = new RunningState(this);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();


        ChangeState(IdlingState);
    }

    protected override bool ReconciliationNeeded(MovementStatePayload latestServerState, MovementStatePayload matchingClientState)
    {
        float error = Vector3.Distance(
            latestServerState.Position,
            matchingClientState.Position
        );
        return error > 0.001f;
    }

    protected override MovementInputPayload CreateInputPayload(int currentTick)
    {
        return new MovementInputPayload
        {
            Tick = currentTick,
            MoveInput = Vector2.left,
        };
    }

    private void LateUpdate()
    {
        characterController.enabled = false;
        transform.position = Vector3.Lerp(transform.position, GetCurrentState().Position, 10 * Time.deltaTime);
        characterController.enabled = true;
    }

}
