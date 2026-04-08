using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    public GameObject StartUI;

    // Stack of opened UI panels
    private Stack<GameObject> uiStack = new Stack<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate UIManager detected. Destroying this instance.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        OpenUI(StartUI);
    }

    public void OpenUI(GameObject uiPanel)
    {
        if (uiPanel == null) return;

        // Hide current top panel (if any)
        if (uiStack.Count > 0)
        {
            GameObject top = uiStack.Peek();
            top.SetActive(false);
        }

        // Show new panel
        uiPanel.SetActive(true);
        uiStack.Push(uiPanel);
    }


    public void GoBack()
    {
        if (uiStack.Count == 0) return;

        // Close top panel
        GameObject top = uiStack.Pop();
        top.SetActive(false);

        // Show previous panel if any
        if (uiStack.Count > 0)
        {
            GameObject previous = uiStack.Peek();
            previous.SetActive(true);
        }
    }

    public GameObject GetCurrentPanel()
    {
        if (uiStack.Count == 0) return null;
        return uiStack.Peek();
    }

    public void ClearAll()
    {
        while (uiStack.Count > 0)
        {
            GameObject panel = uiStack.Pop();
            panel.SetActive(false);
        }
    }
}
