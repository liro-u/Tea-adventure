public class AIInputProvider : IInputProvider<IMovementInputPayload>
{
    private MovementInputPayload inputPayload;
    public IMovementInputPayload InputPayload => inputPayload;
    public void Tick(float tickDelta) { }
}
