using System;
using UnityEngine;

[Serializable]
public class RollData
{
    [SerializeField][Range(0f, 3f)] public float SpeedModifier = 1f;
}
