using System;
using UnityEngine;

[Serializable]
public class GroundedData
{
    [SerializeField][Range(0f, 25f)] public float BaseSpeed = 5f;
    [SerializeField] public LayerMask GroundLayer;
    [SerializeField] public float GroundToFallRayDistance = 1f;
    [SerializeField] public float StickToGroundRayDistance = 2f;
    [SerializeField] public float StepReachForce = 25f;

    [SerializeField] public WalkData WalkData;
    [SerializeField] public RunData RunData;
    [SerializeField] public SprintData SprintData;


    [SerializeField] public StopData StopData;
    [SerializeField] public RollData RollData;
}
