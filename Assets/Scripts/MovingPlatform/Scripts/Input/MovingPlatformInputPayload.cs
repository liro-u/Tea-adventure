using Unity.Netcode;

/// <summary>
/// Input snapshot for a moving platform on a single simulation tick.
///
/// Offline: produced by MovingPlatformInputProvider reading an EventZone.
/// Online: buffered per tick by NetworkMovingPlatformBrainCore so replays use
/// the same inputs that were live when the tick originally ran.
/// No ServerRpc is needed — the server generates its own input from its EventZone.
/// </summary>
public struct MovingPlatformInputPayload : INetworkSerializable
{
    /// <summary>True when the linked EventZone has at least one qualifying collider inside it.</summary>
    public bool IsTriggered;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        => serializer.SerializeValue(ref IsTriggered);
}
