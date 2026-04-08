using UnityEngine;

[CreateAssetMenu(fileName = "MovementSO", menuName = "MovementSO")]
public class MovementSO : ScriptableObject
{
    [SerializeField] public GroundedData GroundedData;
    [SerializeField] public AirborneData AirborneData;
}
