using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Tracks the run timer and the win state, and draws a minimal on-screen HUD.
/// </summary>
public class MazeGameManager : MonoBehaviour
{
    public bool HasWon { get; private set; }

    float elapsedSeconds;
    PlayerController player;
    GUIStyle hudStyle;
    GUIStyle bannerStyle;

    void Awake()
    {
        player = FindAnyObjectByType<PlayerController>();
    }

    void Update()
    {
        if (!HasWon) elapsedSeconds += Time.deltaTime;

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.rKey.wasPressedThisFrame) Restart();
    }

    public void ReportGoalReached()
    {
        if (HasWon) return;

        HasWon = true;
        if (player != null) player.SetControlsEnabled(false);
    }

    void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnGUI()
    {
        EnsureStyles();

        GUI.Label(new Rect(16f, 12f, 520f, 26f), "Arrow keys to move  -  R to restart", hudStyle);
        GUI.Label(new Rect(16f, 38f, 520f, 26f), $"Time: {elapsedSeconds:0.0}s", hudStyle);

        if (!HasWon) return;

        var banner = new Rect(0f, Screen.height * 0.5f - 40f, Screen.width, 80f);
        GUI.Label(banner, $"GOAL REACHED IN {elapsedSeconds:0.0}s!\nPress R to play again", bannerStyle);
    }

    void EnsureStyles()
    {
        if (hudStyle != null) return;

        hudStyle = new GUIStyle(GUI.skin.label) { fontSize = 18 };
        hudStyle.normal.textColor = new Color(0.85f, 0.88f, 0.95f);

        bannerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 28,
            alignment = TextAnchor.MiddleCenter
        };
        bannerStyle.normal.textColor = new Color(0.45f, 0.95f, 0.55f);
    }
}
