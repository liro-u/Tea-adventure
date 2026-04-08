using System;
using UnityEngine;

[Serializable]
public class JumpData
{
    [SerializeField][Range(0, 10)] public int MaxConsecutiveJump  = 2;
    [SerializeField][Range(0, 10)] public float DecelerationForce = 1.5f;
    [SerializeField] public Vector3 StationaryForce;
    [SerializeField] public Vector3 WeakForce;
    [SerializeField] public Vector3 MediumForce;
    [SerializeField] public Vector3 StrongForce;
}
