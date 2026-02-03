using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
public class CharacterCamera
{
    protected Transform cameraPivot;
    protected IMovementBrain movementBrain;

    protected float sensitivity;
    protected float minPitch;
    protected float maxPitch;

    protected float smoothTime; // lower = snappier

    private float targetYaw;
    private float targetPitch;

    private float yaw;
    private float pitch;

    private float yawVelocity;
    private float pitchVelocity;

    public CharacterCamera(IMovementBrain movementBrain, Transform cameraPivot, float sensitivity, float minPitch, float maxPitch, float smoothTime) {
        this.movementBrain = movementBrain;
        this.cameraPivot = cameraPivot;
        this.sensitivity = sensitivity;
        this.minPitch = minPitch;
        this.maxPitch = maxPitch;
        this.smoothTime = smoothTime;
    }

    public void Tick(float tickDelta)
    {
        ApplyLook();
    }

    private void ApplyLook()
    {
        // Mouse delta should NOT be multiplied by deltaTime
        targetYaw += movementBrain.movementInputProvider.InputPayload.LookInput.x * sensitivity;
        targetPitch -= movementBrain.movementInputProvider.InputPayload.LookInput.y * sensitivity;

        targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);

        // SmoothDamp = physically plausible smoothing
        yaw = Mathf.SmoothDampAngle(yaw, targetYaw, ref yawVelocity, smoothTime);
        pitch = Mathf.SmoothDampAngle(pitch, targetPitch, ref pitchVelocity, smoothTime);

        cameraPivot.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
