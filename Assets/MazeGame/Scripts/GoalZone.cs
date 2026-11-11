using UnityEngine;

/// <summary>Trigger at the end of the maze. Tells the manager when the player arrives.</summary>
[RequireComponent(typeof(Collider2D))]
public class GoalZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() == null) return;

        MazeGameManager manager = FindAnyObjectByType<MazeGameManager>();
        if (manager != null) manager.ReportGoalReached();
    }
}
