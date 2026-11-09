// A "contract" for anything the player can use with the E key (doors, pickups, switches...).
// Any script that implements this interface can be found and triggered by PlayerInteractor.
public interface IInteractable
{
    // Text shown on screen when the player looks at it, e.g. "Open door".
    string GetPrompt();

    // Called when the player presses E while looking at it.
    void Interact(PlayerInteractor player);
}
