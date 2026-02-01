using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
public class CharacterCamera : NetworkBehaviour
{
    [Header("Look")]
    [SerializeField] public Transform cameraPivot;
    [SerializeField] private GameObject TPSCamera;

    [SerializeField] private float sensitivity = 2.5f;
    [SerializeField] private float minPitch = -40f;
    [SerializeField] private float maxPitch = 80f;

    [Header("Smoothing")]
    [SerializeField] private float smoothTime = 0.05f; // lower = snappier

    private Vector2 lookInput;

    private float targetYaw;
    private float targetPitch;

    private float yaw;
    private float pitch;

    private float yawVelocity;
    private float pitchVelocity;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            TPSCamera.SetActive(false);
    }

    public void SetLookInput(Vector2 input)
    {
        lookInput = input;
    }

    private void LateUpdate()
    {
        if (!IsOwner) return;
        ApplyLook();
    }

    private void ApplyLook()
    {
        // Mouse delta should NOT be multiplied by deltaTime
        targetYaw += lookInput.x * sensitivity;
        targetPitch -= lookInput.y * sensitivity;

        targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);

        // SmoothDamp = physically plausible smoothing
        yaw = Mathf.SmoothDampAngle(yaw, targetYaw, ref yawVelocity, smoothTime);
        pitch = Mathf.SmoothDampAngle(pitch, targetPitch, ref pitchVelocity, smoothTime);

        cameraPivot.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
