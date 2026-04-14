using UnityEngine;

/// <summary>
/// Implemented by any moving object that can transfer its velocity to a riding character.
/// The character motor checks for this interface on the surface it stands on.
/// </summary>
public interface IPlatformVelocityProvider
{
    Vector3 PlatformVelocity { get; }
}
