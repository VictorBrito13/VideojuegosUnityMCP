using UnityEngine;

/// <summary>
/// Keeps the whole maze on screen whatever aspect ratio the window has.
/// Without this the maze is cropped sideways in a narrow or portrait view.
/// </summary>
[RequireComponent(typeof(Camera))]
public class MazeCameraFit : MonoBehaviour
{
    [SerializeField] float margin = 0.75f;

    Camera view;
    float lastAspect;

    void Awake()
    {
        view = GetComponent<Camera>();
        Fit();
    }

    void Update()
    {
        if (!Mathf.Approximately(view.aspect, lastAspect)) Fit();
    }

    void Fit()
    {
        lastAspect = view.aspect;

        float halfHeight = MazeLevel.Rows * 0.5f + margin;
        float halfWidth = MazeLevel.Columns * 0.5f + margin;

        // Whichever constraint is tighter decides the zoom.
        view.orthographicSize = Mathf.Max(halfHeight, halfWidth / Mathf.Max(view.aspect, 0.01f));
    }
}
