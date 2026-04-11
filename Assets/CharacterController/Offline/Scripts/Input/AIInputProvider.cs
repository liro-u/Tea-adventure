public class AIInputProvider : IInputProvider<PlayerInputPayload>
{
    public PlayerInputPayload InputPayload { get; private set; }

    public void Tick(float tickDelta) { }
}
