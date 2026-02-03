using UnityEngine;
using Unity.Netcode;

public class NetworkStartUI : MonoBehaviour
{
    public enum NetworkUIMode
    {
        Netcode,
        Relay
    }

    [SerializeField] private NetworkUIMode networkMode = NetworkUIMode.Relay;

    private string joinCode = "";
    private string currentJoinCode = "";

    private void OnEnable()
    {
        if (RelayManager.Singleton != null)
        {
            RelayManager.Singleton.onRelayCreated.AddListener(setCurrentJoinCode);
            RelayManager.Singleton.onRelayJoined.AddListener(setCurrentJoinCode);
        }
    }

    private void OnDisable()
    {
        if (RelayManager.Singleton != null)
        {
            RelayManager.Singleton.onRelayCreated.RemoveListener(setCurrentJoinCode);
            RelayManager.Singleton.onRelayJoined.RemoveListener(setCurrentJoinCode);
        }
    }

    private void setCurrentJoinCode(string joinCode)
    {
        currentJoinCode = joinCode;
    }

    void OnGUI()
    {
        float w = 200f, h = 40f;
        float x = 10f, y = 10f;

        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            switch (networkMode)
            {
                case NetworkUIMode.Netcode:
                    if (GUI.Button(new Rect(x, y, w, h), "Host")) NetworkManager.Singleton.StartHost();
                    if (GUI.Button(new Rect(x, y + h + 10, w, h), "Client")) NetworkManager.Singleton.StartClient();
                    if (GUI.Button(new Rect(x, y + 2 * (h + 10), w, h), "Server")) NetworkManager.Singleton.StartServer();
                    break;

                case NetworkUIMode.Relay:
                    if (GUI.Button(new Rect(x, y, w, h), "Host")) RelayManager.Singleton.CreateRelay();
                    GUI.Label(new Rect(x, y + h + 10, w, h / 2), "Join Code:");
                    joinCode = GUI.TextField(new Rect(x, y + h + 35, w, h), joinCode);
                    if (GUI.Button(new Rect(x, y + 25 + 2 * (h + 10), w, h), "Join") && !string.IsNullOrEmpty(joinCode)) RelayManager.Singleton.JoinRelay(joinCode);
                    break;
            }
        } else {
            if (networkMode == NetworkUIMode.Relay && !string.IsNullOrEmpty(currentJoinCode))
            {
                GUI.Label(new Rect(x, y, w, h), $"Current Join Code: {currentJoinCode}");
            }
        }
    }
}
