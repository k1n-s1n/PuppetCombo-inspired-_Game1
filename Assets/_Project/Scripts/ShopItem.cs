using UnityEngine;

// Something on a shelf the player has to collect (chips, beer, candy...). Press E to put it in the basket.
// The StoryManager counts every ShopItem in the scene, so adding another item here automatically
// makes it part of the shopping list.
// Needs a collider so the player's E ray can hit it.
public class ShopItem : MonoBehaviour, IInteractable
{
    [Tooltip("Name shown in the prompt, e.g. \"chips\" gives \"Take chips\".")]
    public string itemName = "chips";

    [Tooltip("Where the item sits inside the basket (relative to the basket: x = right, y = up, z = away from you).")]
    public Vector3 positionInBasket = Vector3.zero;

    [Tooltip("How the item is rotated once it's inside the basket, in degrees.")]
    public Vector3 rotationInBasket = Vector3.zero;

    private bool collected;

    public string GetPrompt()
    {
        if (collected) return "";

        // Show the prompt while looking for the basket too, so the player gets a hint if they try early.
        StoryManager.Stage stage = StoryManager.Instance.stage;
        if (stage == StoryManager.Stage.GrabBasket || stage == StoryManager.Stage.GrabSnacks)
            return "Take " + itemName;
        return "";
    }

    public void Interact(PlayerInteractor player)
    {
        if (ShoppingBasket.Held == null)
        {
            player.ShowMessage("I should grab a basket first.");
            return;
        }

        collected = true;
        ShoppingBasket.Held.AddItem(this);
        StoryManager.Instance.OnEvent("ItemCollected");
    }
}
