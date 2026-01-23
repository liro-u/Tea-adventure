using System;
using UnityEngine;

[Serializable]
public class RunData
{
    [SerializeField][Range(1f, 2f)] public float SpeedModifier = 1f;
}
