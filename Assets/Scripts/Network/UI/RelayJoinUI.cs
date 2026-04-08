using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RelayJoinUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private Button joinButton;

    private void Start()
    {
        if (joinButton != null)
            joinButton.onClick.AddListener(OnJoinButtonClicked);
    }

    private void OnJoinButtonClicked()
    {
        if (string.IsNullOrEmpty(joinCodeInput.text))
        {
            Debug.LogWarning("Join code is empty!");
            return;
        }

        if (RelayManager.Singleton != null)
        {
            RelayManager.Singleton.JoinRelay(joinCodeInput.text);
        }
        else
        {
            Debug.LogError("RelayManager.Singleton is null!");
        }
    }
}
