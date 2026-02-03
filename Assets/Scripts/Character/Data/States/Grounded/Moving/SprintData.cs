using System;
using UnityEngine;

[Serializable]
public class SprintData
{
    [SerializeField][Range(0f, 1f)] public float SpeedModifier = 0.225f;
    [SerializeField][Range(0f, 5f)] public float SprintToRunTime = 1f;
    [SerializeField][Range(0f, 10f)] public float RunToWalkTime = 1.7f;
}
