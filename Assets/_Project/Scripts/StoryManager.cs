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
        Arrive,       // just stepped out of the car, walking to the store
        UseToilet,    // inside the store, objective "Use the toilet"
        GrabBasket,   // finished at the toilet (the jumpscare will go here, Tier 2), objective "Grab a basket"
        GrabSnacks,   // holding the basket, collecting every ShopItem
        PayForItems,  // everything collected, objective "Pay for your items"
        Checkout,     // talking to the cashier (Cashier.cs runs this part)
        ReturnToCar,  // got the bag, objective "Head back to your car"
    }

    [Tooltip("Where the player is in the story right now (shown for debugging; changes while playing).")]
    public Stage stage = Stage.Arrive;

    [Header("Opening")]
    [Tooltip("Seconds after the game starts before the first subtitle appears.")]
    public float openingDelay = 1.5f;

    [TextArea] public string openingLine = "Ugh. Why am I always the one doing the snack run? They better pay me back this time.";

    [Header("Entering the store")]
    [TextArea] public string enterStoreLine = "Man, I'm bursting... there should be a toilet in here somewhere.";

    [Header("Shopping")]
    [TextArea] public string allItemsLine = "That should do it. Time to pay and get out of here.";

    [Header("Leaving")]
    [TextArea] public string leftStoreLine = "That guy gave me the creeps.";
    private bool saidLeftStoreLine;

    private int totalItems;      // how many ShopItems are in the scene
    private int collectedItems;  // how many are in the basket

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Every ShopItem in the scene is on the shopping list.
        totalItems = FindObjectsByType<ShopItem>(FindObjectsSortMode.None).Length;
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
        StartCoroutine(SetObjectiveAfterSpeech("Head into the store"));
    }

    // Called by StoryTrigger and the interactable scripts when something happens in the world.
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
        else if (stage == Stage.GrabBasket && id == "PickedUpBasket")
        {
            stage = Stage.GrabSnacks;
            GameUI.Instance.SetObjective(ShoppingObjective());
        }
        else if (stage == Stage.GrabSnacks && id == "ItemCollected")
        {
            collectedItems++;
            if (collectedItems < totalItems)
            {
                GameUI.Instance.SetObjective(ShoppingObjective());   // updates the count (and flashes)
            }
            else
            {
                stage = Stage.PayForItems;
                GameUI.Instance.Say("You", allItemsLine);
                StartCoroutine(SetObjectiveAfterSpeech("Pay for your items"));
            }
        }
        else if (stage == Stage.PayForItems && id == "CheckoutStarted")
        {
            stage = Stage.Checkout;
            GameUI.Instance.SetObjective("");   // no objective during the conversation
        }
        else if (stage == Stage.Checkout && id == "CheckoutFinished")
        {
            stage = Stage.ReturnToCar;
            GameUI.Instance.SetObjective("Head back to your car");
        }
        else if (stage == Stage.ReturnToCar && id == "LeftStore" && !saidLeftStoreLine)
        {
            saidLeftStoreLine = true;
            GameUI.Instance.Say("You", leftStoreLine);
        }
    }

    // e.g. "Grab snacks and beer (1/3)"
    string ShoppingObjective()
    {
        return "Grab snacks and beer (" + collectedItems + "/" + totalItems + ")";
    }

    // Hide the objective, wait until all queued subtitles have finished, then show the new one.
    // If the story moves on while we wait (e.g. the player grabs the basket mid-sentence),
    // this objective is out of date, so it's skipped.
    IEnumerator SetObjectiveAfterSpeech(string objective)
    {
        Stage startedIn = stage;
        GameUI.Instance.SetObjective("");
        while (GameUI.Instance.IsSpeaking)
            yield return null;   // wait one frame, then check again
        if (stage == startedIn)
            GameUI.Instance.SetObjective(objective);
    }
}
