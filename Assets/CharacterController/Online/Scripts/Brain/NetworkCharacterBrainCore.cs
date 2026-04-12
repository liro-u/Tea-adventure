using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Online extension of CharacterBrainCore.
/// Creates a ClientPrediction instance wired to this brain's simulation and
/// exposes it for NetworkCharacterBrain to register and connect RPC callbacks.
///
/// All buffering, reconciliation, and role-aware tick logic lives in ClientPrediction.
/// Adding a new networked entity (vehicle, carried object, etc.) follows the same pattern:
/// extend the matching Core class, create a ClientPrediction in the constructor,
/// set CheckDivergence, and expose Prediction.
/// </summary>
public class NetworkCharacterBrainCore : CharacterBrainCore
{
    public readonly ClientPrediction<PlayerInputPayload, PlayerStateSnapshot> Prediction;

    public NetworkCharacterBrainCore(
        NetworkBehaviour              networkBehaviour,
        CharacterController           characterController,
        MovementSO                    movementSO,
        Animator                      animator,
        Transform                     meshTransform,
        MovementAnimationEventTrigger movementAnimationEventTrigger,
        Transform                     cameraPivot,
        float                         sensitivity,
        float                         minPitch,
        float                         maxPitch,
        float                         smoothTime,
        float                         rotationSmoothTime)
        : base(characterController, movementSO, animator, meshTransform,
               movementAnimationEventTrigger, cameraPivot,
               sensitivity, minPitch, maxPitch, smoothTime, rotationSmoothTime)
    {
        Prediction = new ClientPrediction<PlayerInputPayload, PlayerStateSnapshot>(
            this,
            () => CurrentInputPayload,
            networkBehaviour)
        {
            CheckDivergence = (server, local) =>
                Vector3.Distance(server.Position, local.Position) > 0.001f ||
                Vector3.Distance(server.Velocity, local.Velocity) > 0.001f
        };
    }
}
