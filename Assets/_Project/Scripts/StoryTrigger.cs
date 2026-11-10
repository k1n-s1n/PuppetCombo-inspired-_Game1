using UnityEngine;

// An invisible box that tells the StoryManager when the player walks into it.
// Needs a Collider with "Is Trigger" ticked: a trigger collider doesn't block movement,
// it just reports when something enters it (OnTriggerEnter).
[RequireComponent(typeof(Collider))]
public class StoryTrigger : MonoBehaviour
{
    [Tooltip("The event name sent to the StoryManager, e.g. \"EnteredStore\".")]
    public string eventId = "EnteredStore";

    void OnTriggerEnter(Collider other)
    {
        // Only react to the player (the Player object has the "Player" tag).
        if (other.CompareTag("Player"))
            StoryManager.Instance.OnEvent(eventId);
    }
}
