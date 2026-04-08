using UnityEngine;
using UnityEngine.UI;

public class BackUI : MonoBehaviour
{
    [SerializeField] private Button button;  // Assign in inspector

    private void Awake()
    {
        if (button != null)
            button.onClick.AddListener(OnClickBack);
        else
            Debug.LogWarning("Button is not assigned in BackUI");
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(OnClickBack);
    }

    private void OnClickBack()
    {
        UIManager.Instance.GoBack();
    }
}
