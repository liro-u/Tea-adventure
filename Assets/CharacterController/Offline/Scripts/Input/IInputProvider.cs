using UnityEngine;


public interface IInputProvider<TInputPayload>
{
    public TInputPayload InputPayload { get; }

    public void Tick(float tickDelta);
}
