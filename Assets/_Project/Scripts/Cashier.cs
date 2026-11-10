using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// The cashier behind the counter. At the "Pay for your items" stage, pressing E on him starts the
// checkout conversation:
//   basket goes on the counter -> he asks your name -> you choose (tell him / refuse)
//   -> he asks for ID -> the ID card is shown -> you get your bag -> "Head back to your car".
// The whole conversation is one coroutine, so it reads top to bottom like a script.
public class Cashier : MonoBehaviour, IInteractable
{
    [Header("Scene references")]
    [Tooltip("Empty object on the counter where the basket is put down.")]
    public Transform basketSpot;

    [Tooltip("The paper bag the player is handed (starts hidden).")]
    public GameObject paperBag;

    [Tooltip("Where the bag sits relative to the camera once held.")]
    public Vector3 bagHoldPosition = new Vector3(0.32f, -0.4f, 0.6f);
    public Vector3 bagHoldRotation = new Vector3(-10f, -20f, 0f);

    [Header("Dialogue")]
    public string total = "$14.75";

    private bool talking;   // true while the conversation is running
    private FirstPersonController playerMovement;
    private Camera playerCamera;

    public string GetPrompt()
    {
        if (talking || StoryManager.Instance.stage != StoryManager.Stage.PayForItems) return "";
        return "Pay for items";
    }

    public void Interact(PlayerInteractor player)
    {
        talking = true;
        playerMovement = player.GetComponent<FirstPersonController>();
        playerCamera = player.playerCamera;
        StartCoroutine(Conversation());
    }

    IEnumerator Conversation()
    {
        StoryManager.Instance.OnEvent("CheckoutStarted");
        playerMovement.canControl = false;   // stand still during the conversation

        // Put the basket down on the counter.
        Transform basket = ShoppingBasket.Held.transform;
        basket.SetParent(basketSpot, false);
        basket.localPosition = Vector3.zero;
        basket.localRotation = Quaternion.identity;

        string playerName = IDCard.Instance.holderName.Split(' ')[0];   // first name only

        yield return Say("Cashier", "Evening. Didn't hear you come in.");
        yield return Say("Cashier", "Haven't seen you round here before. What's your name, kid?");

        // Dialogue choice: wait until the player presses 1 or 2.
        GameUI.Instance.ShowChoice("Tell him your name", "\"None of your business.\"");
        while (GameUI.Instance.IsChoosing)
            yield return null;

        bool toldName = GameUI.Instance.ChoiceResult == 0;
        if (toldName)
        {
            yield return Say("You", "Uh... it's " + playerName + ".");
            yield return Say("Cashier", playerName + ". Listen here, " + playerName + "... folks have been going missing round these parts.");
            yield return Say("Cashier", "Kids your age, mostly. Whatever you're up to tonight, you be careful.");
            yield return Say("You", "...Right. Sure thing, old man.");
            yield return Say("Cashier", "That'll be " + total + ". And I'll need to see some ID for the beer.");
        }
        else
        {
            yield return Say("You", "None of your business, old timer. Scan my stuff and let me get going.");
            yield return Say("Cashier", "...You'll get what's coming for ya.");
            // From here on he doesn't say a word.
            yield return Say("", "(He scans your items in silence, then holds out his hand.)");
        }

        // Show the ID card and wait for E.
        IDCard.Instance.Show();
        yield return null;   // skip a frame so an earlier E press can't count
        while (!Keyboard.current.eKey.wasPressedThisFrame)
            yield return null;
        IDCard.Instance.Hide();

        if (toldName)
            yield return Say("Cashier", "Mm-hm. Here's your bag. Get home safe, " + playerName + ".");
        else
            yield return Say("", "(He stares at the card for far too long, then shoves the bag across the counter.)");

        // Swap the basket for the paper bag in the player's hand.
        basket.gameObject.SetActive(false);
        paperBag.SetActive(true);
        paperBag.transform.SetParent(playerCamera.transform, false);
        paperBag.transform.localPosition = bagHoldPosition;
        paperBag.transform.localRotation = Quaternion.Euler(bagHoldRotation);
        foreach (Collider c in paperBag.GetComponentsInChildren<Collider>()) Destroy(c);

        playerMovement.canControl = true;
        talking = false;
        StoryManager.Instance.OnEvent("CheckoutFinished");
    }

    // Shows a subtitle and waits until it has finished. Use with "yield return".
    IEnumerator Say(string speaker, string text)
    {
        GameUI.Instance.Say(speaker, text);
        while (GameUI.Instance.IsSpeaking)
            yield return null;
    }
}
