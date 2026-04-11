using UnityEngine;

/// <summary>
/// Drives the camera pivot from look input.
/// Reads directly from the input provider — this is a visual/Update-rate concern,
/// not part of the fixed-tick simulation, so it bypasses SimulateTick entirely.
/// </summary>
public class CharacterCamera
{
    private readonly Transform cameraPivot;
    private readonly IInputProvider<PlayerInputPayload> inputProvider;

    private readonly float sensitivity;
    private readonly float minPitch;
    private readonly float maxPitch;
    private readonly float smoothTime;

    private float targetYaw;
    private float targetPitch;

    private float yaw;
    private float pitch;

    private float yawVelocity;
    private float pitchVelocity;

    public CharacterCamera(
        IInputProvider<PlayerInputPayload> inputProvider,
        Transform cameraPivot,
        float sensitivity,
        float minPitch,
        float maxPitch,
        float smoothTime)
    {
        this.inputProvider = inputProvider;
        this.cameraPivot = cameraPivot;
        this.sensitivity = sensitivity;
        this.minPitch = minPitch;
        this.maxPitch = maxPitch;
        this.smoothTime = smoothTime;
    }

    public void Tick(float deltaTime)
    {
        ApplyLook();
    }

    private void ApplyLook()
    {
        // Mouse delta must NOT be multiplied by deltaTime — it is already a frame delta
        targetYaw   += inputProvider.InputPayload.LookInput.x * sensitivity;
        targetPitch -= inputProvider.InputPayload.LookInput.y * sensitivity;
        targetPitch  = Mathf.Clamp(targetPitch, minPitch, maxPitch);

        yaw   = Mathf.SmoothDampAngle(yaw,   targetYaw,   ref yawVelocity,   smoothTime);
        pitch = Mathf.SmoothDampAngle(pitch, targetPitch, ref pitchVelocity, smoothTime);

        cameraPivot.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
