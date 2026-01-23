using System.Collections.Generic;
using UnityEngine;

public class Debuger : MonoBehaviour
{
    public static Debuger Instance { get; private set; }

    private readonly List<string> _lines = new(64);

    [Header("Layout")]
    public float X = 10f;
    public float Y = 30f;
    public float Width = 500f;
    public float LineHeight = 18f;
    public float Spacing = 2f;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Add(string text)
    {
        _lines.Add(text);
    }

    private void OnGUI()
    {
        float y = Y;

        for (int i = 0; i < _lines.Count; i++)
        {
            GUI.Label(
                new Rect(X, y, Width, LineHeight),
                _lines[i]
            );

            y += LineHeight + Spacing;
        }
    }

    private void LateUpdate()
    {
        _lines.Clear();
    }
}
