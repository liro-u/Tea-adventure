/// <summary>
/// Input snapshot for a moving platform on a single simulation tick.
///
/// Offline: produced by MovingPlatformInputProvider reading an EventZone.
/// Online (future): promote to INetworkSerializable so ClientPrediction can buffer
/// and replay it, and the server can receive it via ServerRpc.
/// </summary>
public struct MovingPlatformInputPayload
{
    /// <summary>True when the linked EventZone has at least one qualifying collider inside it.</summary>
    public bool IsTriggered;
}
