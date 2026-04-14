using UnityEngine;
using System;
using UnityEngine.Events;

public class AdvancedCharacterControllerMotor : CharacterControllerMotor, IAdvancedMotor
{
    protected readonly float groundToFallRayDistance;
    protected readonly float stickToGroundRayDistance;
    protected readonly LayerMask groundLayer;

    public Vector3 TargetDirection { get; protected set; }
    
    public event Action OnContactWithGround;
    public event Action OnContactWithGroundExited;

    public event Action OnFall;

    public AdvancedCharacterControllerMotor(
        CharacterController characterController,

        float groundToFallRayDistance = 1f,
        float stickToGroundRayDistance = 2f,
        LayerMask groundLayer = default,

        float mass = 1f,
        float gravity = -9.81f,
        float groundFriction = 20f,
        float airFriction = 1f
    )
        : base(characterController, mass, gravity, groundFriction, airFriction)
    {
        this.groundToFallRayDistance = groundToFallRayDistance;
        this.stickToGroundRayDistance = stickToGroundRayDistance;
        this.groundLayer = groundLayer;
    }

    #region Public API
    public override void ApplyForce(float tickDelta, bool? isGrounded = null)
    {
        bool wasGrounded = IsGrounded;

        base.ApplyForce(tickDelta, isGrounded);

        if (IsGrounded && !wasGrounded)
        {
            OnContactWithGround?.Invoke();
        }

        else if (!IsGrounded && wasGrounded)
        {
            OnContactWithGroundExited?.Invoke();
            onContactWithGroundExited();
        }
    }
    public void Move(Vector2 movementInput, float baseSpeed, float movementSpeedModifier, Quaternion cameraPivot)
    {

        if (movementInput == Vector2.zero || movementSpeedModifier == 0f)
        {
            return;
        }

        // Horizontal movement
        Vector3 movementInputDirection = GetMovementInputDirection(movementInput);

        TargetDirection = LocalToCameraDirection(movementInputDirection, cameraPivot);

        float movementSpeed = baseSpeed * movementSpeedModifier;

        AddForce(TargetDirection * movementSpeed - GetHorizontalVelocity(), ForceMode.VelocityChange);
    }

    public void StickToGround()
    {
        Vector3 capsuleColliderCenterInWorldSpace = characterController.transform.TransformPoint(Center);

        Ray downwardsRayFromCapsuleCenter = new Ray(capsuleColliderCenterInWorldSpace, Vector3.down);

        if (Physics.Raycast(downwardsRayFromCapsuleCenter, out RaycastHit hit, stickToGroundRayDistance, groundLayer, QueryTriggerInteraction.Ignore))
        {
            float centerToBottom = Height * 0.5f;
            float distanceToGround = hit.distance - centerToBottom;

            if (Mathf.Abs(distanceToGround) < 0.02f)
            {
                return;
            }

            float amountToPull = distanceToGround;


            Velocity = new Vector3(Velocity.x, -amountToPull, Velocity.z);
        }
    }

    public Vector3 InheritPlatformVelocity(float tickDelta)
    {
        Vector3 capsuleColliderCenterInWorldSpace = characterController.transform.TransformPoint(Center);

        Ray downRay = new Ray(capsuleColliderCenterInWorldSpace, Vector3.down);
        float rayDistance = stickToGroundRayDistance;

        if (!Physics.Raycast(downRay, out RaycastHit hit, rayDistance, groundLayer, QueryTriggerInteraction.Ignore))
        {
            return Vector3.zero;
        }

        IPlatformVelocityProvider provider = hit.collider.GetComponentInParent<IPlatformVelocityProvider>();
        if (provider == null)
        {
            return Vector3.zero;
        }

        Vector3 platformVelocity = provider.PlatformVelocity;
        // Apply platform displacement as a separate move, not by modifying Velocity.
        // This keeps platform movement independent of character physics.
        characterController.Move(platformVelocity * tickDelta);

        return platformVelocity;
    }
    #endregion

    protected void onContactWithGroundExited()
    {
        Vector3 capsuleColliderCenterInWorldSpace = characterController.transform.TransformPoint(Center);

        Vector3 centerToBottom = new Vector3(0f, Height * 0.5f, 0f);

        Ray downwardsRayFromCapsuleBottom = new Ray(capsuleColliderCenterInWorldSpace - centerToBottom, Vector3.down);

        if (!Physics.Raycast(downwardsRayFromCapsuleBottom, out _, groundToFallRayDistance, groundLayer, QueryTriggerInteraction.Ignore))
        {
            OnFall?.Invoke();
        }
    }

    protected Vector3 GetMovementInputDirection(Vector2 movementInput)
    {
        Vector3 movementInputDirection = new Vector3(movementInput.x, 0f, movementInput.y);
        movementInputDirection.Normalize();
        return movementInputDirection;
    }

    protected Vector3 LocalToCameraDirection(Vector3 movementInputDirection, Quaternion cameraPivot)
    {
        // Extract camera yaw
        Quaternion cameraYaw = Quaternion.Euler(0f, cameraPivot.eulerAngles.y, 0f);

        // Rotate movement by camera yaw
        Vector3 worldMovementDirection = cameraYaw * movementInputDirection;

        return worldMovementDirection;
    }
}
