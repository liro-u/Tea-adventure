using System;
using UnityEngine;

[Serializable]
public class FallData
{
    [SerializeField] public float FallSpeedLimit;
    [SerializeField][Range(0f, 100f)] public float MinimumDistanceToBeConsideredHardFall = 3f;

}
