/// <summary>
/// Encodes the active movement state as a byte so it can be included in
/// PlayerStateSnapshot without holding an object reference.
/// The online layer compares these IDs to detect state-machine divergence
/// and decide whether reconciliation must call ChangeState.
/// </summary>
public enum MovementStateId : byte
{
    Idling          = 0,
    Walking         = 1,
    Running         = 2,
    Sprinting       = 3,
    LightStopping   = 4,
    MediumStopping  = 5,
    HardStopping    = 6,
    LightLanding    = 7,
    HardLanding     = 8,
    Rolling         = 9,
    Jumping         = 10,
    Falling         = 11,
}
