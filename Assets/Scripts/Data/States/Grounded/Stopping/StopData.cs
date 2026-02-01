using System;
using UnityEngine;

[Serializable]
public class StopData
{
    [SerializeField][Range(0f, 15f)] public float LightDecelerationForce = 5f;
    [SerializeField][Range(0f, 15f)] public float MediumDecelerationForce = 6.5f;
    [SerializeField][Range(0f, 15f)] public float HardDecelerationForce = 5f;
}
