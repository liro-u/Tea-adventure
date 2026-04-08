using System;
using UnityEngine;

[Serializable]
public class AirborneData 
{
    [SerializeField] public Vector3 Gravity = new Vector3(0, -9.81f, 0);

    [SerializeField] public JumpData JumpData;
    [SerializeField] public FallData FallData;
}
