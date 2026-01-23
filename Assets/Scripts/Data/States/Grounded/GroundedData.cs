using System;
using UnityEngine;

[Serializable]
public class GroundedData
{
    [SerializeField][Range(0f, 25f)] public float BaseSpeed = 5f;

    [SerializeField] public WalkData WalkData;
    [SerializeField] public RunData RunData;
    [SerializeField] public StopData StopData;
}
