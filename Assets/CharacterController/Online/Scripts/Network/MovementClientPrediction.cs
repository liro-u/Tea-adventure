using UnityEngine;

public class MovementClientPrediction : ClientPrediction<NetworkMovementInputPayload, NetworkMovementBrainStatePayload>
{
    private readonly NetworkCharacterBrain brain;

    public MovementClientPrediction(NetworkCharacterBrain brain, float tickRate = 50) : base(brain, tickRate)
    {
        this.brain = brain;
    }

    protected override NetworkMovementInputPayload CreateInputPayload(int currentTick)
    {
        IMovementInputPayload raw = brain.movementInputProvider.InputPayload;
        return new NetworkMovementInputPayload
        {
            Tick = currentTick,
            MoveInput = raw.MoveInput,
            LookInput = raw.LookInput,
            CameraPivot = raw.CameraPivot,
            IsSprinting = raw.IsSprinting,
            IsWalkToggle = raw.IsWalkToggle,
            IsJumping = raw.IsJumping,
        };
    }

    protected override bool ReconciliationNeeded(NetworkMovementBrainStatePayload latestServerState, NetworkMovementBrainStatePayload matchingClientState)
    {
        return Vector3.Distance(latestServerState.Position, matchingClientState.Position) > 0.001f;
    }

    protected override NetworkMovementBrainStatePayload Simulate(NetworkMovementInputPayload input)
    {
        brain.Tick(tickDelta);
        return new NetworkMovementBrainStatePayload();
    }

    // ================= RPC BRIDGES =================

    protected override void SendInputToServer(NetworkMovementInputPayload input)
        => brain.SendMovementInputServerRpc(input);

    protected override void BroadcastStateToClients(NetworkMovementBrainStatePayload state)
        => brain.BroadcastMovementStateClientRpc(state);

    protected override void ForwardInputToClients(NetworkMovementInputPayload input)
        => brain.ForwardMovementInputClientRpc(input);
}
