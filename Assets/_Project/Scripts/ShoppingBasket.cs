using UnityEngine;

// A shopping basket the player picks up with E. Once held, it sits in the lower right of the view
// (it becomes a child of the camera, so it moves with it), and collected ShopItems are placed inside it.
// Put this on the top basket of the stack by the entrance.
public class ShoppingBasket : MonoBehaviour, IInteractable
{
    // The basket the player is holding (null until one is picked up). ShopItem uses this.
    public static ShoppingBasket Held { get; private set; }

    [Header("Holding")]
    [Tooltip("Where the basket sits relative to the camera when held (x = right, y = up, z = forward).")]
    public Vector3 holdPosition = new Vector3(0.32f, -0.38f, 0.62f);

    [Tooltip("Rotation of the held basket relative to the camera, in degrees. Negative X tilts the top towards you so you can see inside.")]
    public Vector3 holdRotation = new Vector3(-22f, -20f, 0f);

    [Header("Items")]
    [Tooltip("Spots inside the basket where collected items go (relative to the basket), filled in order.")]
    public Vector3[] itemSlots =
    {
        new Vector3(-0.1f, 0.03f, 0.04f),
        new Vector3(0.1f, 0.03f, -0.04f),
        new Vector3(0.0f, 0.03f, 0.0f),
        new Vector3(-0.1f, 0.12f, -0.05f),
        new Vector3(0.1f, 0.12f, 0.05f),
    };

    private int itemCount;

    public string GetPrompt()
    {
        // Only usable when the story asks for a basket, and only if we're not already holding one.
        if (Held != null || StoryManager.Instance.stage != StoryManager.Stage.GrabBasket) return "";
        return "Pick up basket";
    }

    public void Interact(PlayerInteractor player)
    {
        Held = this;

        // Parent the basket to the camera so it follows the view, then put it in the hold position.
        transform.SetParent(player.playerCamera.transform, false);
        transform.localPosition = holdPosition;
        transform.localRotation = Quaternion.Euler(holdRotation);

        // Remove its colliders: a held object shouldn't bump into things or block the E ray.
        RemoveColliders(gameObject);

        StoryManager.Instance.OnEvent("PickedUpBasket");
    }

    // Called by ShopItem when the player takes it: move the item into the next free slot.
    public void AddItem(ShopItem item)
    {
        Vector3 slot = itemSlots[Mathf.Min(itemCount, itemSlots.Length - 1)];
        itemCount++;

        RemoveColliders(item.gameObject);
        item.transform.SetParent(transform, true);   // true = keep its size in the world
        item.transform.localPosition = slot;
        item.transform.localRotation = Quaternion.Euler(item.rotationInBasket);
    }

    // Destroys every collider on an object and its children.
    static void RemoveColliders(GameObject obj)
    {
        foreach (Collider c in obj.GetComponentsInChildren<Collider>())
            Destroy(c);
    }
}
