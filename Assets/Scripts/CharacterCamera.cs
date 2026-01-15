using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class CharacterCamera : NetworkBehaviour
{
    [Header("Look")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private GameObject TPSCamera;

    [SerializeField] private float lookSensitivity = 120f;
    [SerializeField] private float minPitch = -40f;
    [SerializeField] private float maxPitch = 80f;

    private Vector2 lookInput;

    private float yaw;
    private float pitch;

    public override void OnNetworkSpawn()
    {
        if (IsOwner) return;
        TPSCamera.SetActive(false);
    }

    public void SetLookInput(Vector2 input)
    {
        lookInput = input;
    }

    private void Update()
    {
        ApplyLook();
    }

    private void ApplyLook()
    {
        if (lookInput.sqrMagnitude < 0.0001f)
            return;

        yaw += lookInput.x * lookSensitivity * Time.deltaTime;
        pitch -= lookInput.y * lookSensitivity * Time.deltaTime;

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Camera (pitch + yaw)
        cameraPivot.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }

}
