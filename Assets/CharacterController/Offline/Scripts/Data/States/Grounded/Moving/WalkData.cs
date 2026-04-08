using System;
using UnityEngine;

[Serializable]
public class WalkData
{
    [SerializeField][Range(0f, 1f)] public float SpeedModifier = 0.225f;
}
