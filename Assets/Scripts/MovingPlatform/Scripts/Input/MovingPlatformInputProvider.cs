/// <summary>
/// Reads the trigger state of an EventZone each tick and exposes it as a MovingPlatformInputPayload.
///
/// Mirrors PlayerInputProvider's role: translates a world sensor into a flat payload struct.
/// The EventZone handles replay suppression (IsReplayActive) so this class stays simple.
/// </summary>
public class MovingPlatformInputProvider : IInputProvider<MovingPlatformInputPayload>
{
    public readonly EventZone Zone;

    public MovingPlatformInputPayload InputPayload { get; private set; }

    public MovingPlatformInputProvider(EventZone zone)
    {
        Zone = zone;
    }

    public void Tick(float tickDelta)
    {
        InputPayload = new MovingPlatformInputPayload { IsTriggered = Zone.IsTriggered };
    }
}
