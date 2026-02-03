using UnityEngine;
using System;

public class MovementBrainStatePayload
{
    public bool ShouldWalk;
    public bool ShouldSprint;

    public float MovementSpeedModifier;
    public float MovementDecelerationForce;
    public int RemainingJump;
    public Vector3 CurrentJumpForce;

    public bool IsGrounded;
    public bool IsMoving;
    public bool IsStopping;
    public bool IsLanding;
}