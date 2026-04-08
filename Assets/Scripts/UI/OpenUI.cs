using UnityEngine;
using UnityEngine.UI;

public class OpenUI : MonoBehaviour
{
    [SerializeField] private Button button;         // Assign in inspector
    [SerializeField] private GameObject panelToOpen; // Assign panel to open

    private void Awake()
    {
        if (button != null)
            button.onClick.AddListener(OnClickOpen);
        else
            Debug.LogWarning("Button is not assigned in OpenUI");
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(OnClickOpen);
    }

    private void OnClickOpen()
    {
        if (panelToOpen != null)
            UIManager.Instance.OpenUI(panelToOpen);
        else
            Debug.LogWarning("PanelToOpen is not assigned in OpenUI");
    }
}
