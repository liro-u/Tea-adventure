public class RemotePlayerInputProvider : IInputProvider<IMovementInputPayload>
{
    private NetworkMovementInputPayload inputPayload;
    public IMovementInputPayload InputPayload => inputPayload;

    public void SetPayload(NetworkMovementInputPayload payload) => inputPayload = payload;

    // Payload is pushed externally; nothing to poll.
    public void Tick(float tickDelta) { }
}
