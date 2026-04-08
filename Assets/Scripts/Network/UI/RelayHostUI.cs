using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RelayHostUI : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [Tooltip("Scene to load when hosting. Set to -1 to not load any scene.")]
    [SerializeField] private int sceneOnHost = -1;

    private void Start()
    {
        if (hostButton != null)
            hostButton.onClick.AddListener(OnHostButtonClicked);
    }

    private void OnHostButtonClicked()
    {
        if (RelayManager.Singleton != null)
        {
            if (sceneOnHost > -1)
            {
                SceneManager.LoadScene(sceneOnHost, LoadSceneMode.Single);
            }

            RelayManager.Singleton.CreateRelay();
        }
        else
        {
            Debug.LogError("RelayManager.Singleton is null!");
        }
    }
}
