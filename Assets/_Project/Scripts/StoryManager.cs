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
        GrabBasket,  // finished at the toilet (the jumpscare will go here, Tier 2), objective "Grab a basket"
    }

    [Tooltip("Where the player is in the story right now (shown for debugging; changes while playing).")]
    public Stage stage = Stage.Arrive;

    [Header("Opening")]
    [Tooltip("Seconds after the game starts before the first subtitle appears.")]
    public float openingDelay = 1.5f;

    [TextArea] public string openingLine = "Ugh. Why am I always the one doing the snack run? They better pay me back this time.";

    [Header("Entering the store")]
    [TextArea] public string enterStoreLine = "Man, I'm bursting... there should be a toilet in here somewhere.";

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

        // Skip the opening if the story has already moved on (e.g. the player rushed inside).
        if (stage != Stage.Arrive) yield break;

        GameUI.Instance.Say("You", openingLine);
        GameUI.Instance.SetObjective("Head into the store");
    }

    // Called by StoryTrigger (and later by other scripts) when something happens in the world.
    // "id" says what happened, e.g. "EnteredStore".
    public void OnEvent(string id)
    {
        if (stage == Stage.Arrive && id == "EnteredStore")
        {
            // Change stage straight away (so this can't fire twice), but show the new
            // objective only after the player has said their line.
            stage = Stage.UseToilet;
            GameUI.Instance.Say("You", enterStoreLine);
            StartCoroutine(SetObjectiveAfterSpeech("Use the toilet"));
        }
        else if (stage == Stage.UseToilet && id == "UsedToilet")
        {
            // ToiletUse has already queued the "What a mess..." line.
            stage = Stage.GrabBasket;
            StartCoroutine(SetObjectiveAfterSpeech("Grab a basket"));
        }
    }

    // Hide the objective, wait until all queued subtitles have finished, then show the new one.
    IEnumerator SetObjectiveAfterSpeech(string objective)
    {
        GameUI.Instance.SetObjective("");
        while (GameUI.Instance.IsSpeaking)
            yield return null;   // wait one frame, then check again
        GameUI.Instance.SetObjective(objective);
    }
}
