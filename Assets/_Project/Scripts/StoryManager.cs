using System.Collections;
using UnityEngine;

// Runs the story of Chapter 1 in order: which subtitle to show, which objective is active,
// and what happens next. Put ONE of these in the scene (on the "StoryManager" object).
//
// It's a simple "state machine": the current Stage says where the player is in the story.
// Other scripts (triggers, pickups, the cashier...) report events to it, and it only reacts
// if the event makes sense for the current stage. That way beats can't happen out of order.
public class StoryManager : MonoBehaviour
{
    public static StoryManager Instance { get; private set; }

    // The story beats, in order. More are added as each feature is built.
    public enum Stage
    {
        Arrive,      // just stepped out of the car, walking to the store
        UseToilet,   // inside the store, objective "Use the toilet"
    }

    [Tooltip("Where the player is in the story right now (shown for debugging; changes while playing).")]
    public Stage stage = Stage.Arrive;

    [Header("Opening")]
    [Tooltip("Seconds after the game starts before the first subtitle appears.")]
    public float openingDelay = 1.5f;

    [TextArea] public string openingLine = "Ugh. Why am I always the one doing the snack run? They better pay me back this time.";

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(Opening());
    }

    // A "coroutine" is a function that can pause (yield) and carry on later,
    // which is handy for timed sequences like "wait 1.5 seconds, then speak".
    IEnumerator Opening()
    {
        yield return new WaitForSeconds(openingDelay);
        GameUI.Instance.Say("You", openingLine);
        GameUI.Instance.SetObjective("Head into the store");
    }

    // Called by StoryTrigger (and later by other scripts) when something happens in the world.
    // "id" says what happened, e.g. "EnteredStore".
    public void OnEvent(string id)
    {
        if (stage == Stage.Arrive && id == "EnteredStore")
        {
            stage = Stage.UseToilet;
            GameUI.Instance.SetObjective("Use the toilet");
        }
    }
}
