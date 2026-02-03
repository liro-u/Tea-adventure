using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.Events;

public class RelayManager : MonoBehaviour
{
    [SerializeField] private int maxPlayerCount = 3;

    [SerializeField] public UnityEvent<string> onRelayCreated;
    [SerializeField] public UnityEvent<string> onRelayJoined;

    public static RelayManager Singleton { get; private set; }

    private void Awake()
    {
        // Singleton pattern enforcement
        if (Singleton != null && Singleton != this)
        {
            Debug.LogWarning("Duplicate RelayManager detected. Destroying this instance.");
            Destroy(gameObject);
            return;
        }

        Singleton = this;
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayerCount);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log(joinCode);

            var relayServerData = AllocationUtils.ToRelayServerData(allocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();

            onRelayCreated.Invoke(joinCode);

        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void JoinRelay(string joinCode)
    {
        try
        {
            Debug.Log("Joining Relay with " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            var relayServerData = AllocationUtils.ToRelayServerData(joinAllocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();

            onRelayJoined.Invoke(joinCode);
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }
}
