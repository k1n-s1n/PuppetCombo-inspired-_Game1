using UnityEngine;
using UnityEngine.InputSystem;

// Lets the player use things by looking at them and pressing E.
// Put this on the Player. Every frame it casts an invisible ray (a "raycast") forward from
// the centre of the camera; if it hits an object with an IInteractable script, it shows a prompt.
public class PlayerInteractor : MonoBehaviour
{
    [Tooltip("The player's camera. Found automatically if left empty.")]
    public Camera playerCamera;

    [Tooltip("How far away (in metres) the player can reach things.")]
    public float interactDistance = 2.5f;

    [Tooltip("Show a small dot in the middle of the screen so the player knows what they're aiming at.")]
    public bool showCrosshair = true;

    [Tooltip("How long (seconds) messages such as \"It's locked\" stay on screen.")]
    public float messageDuration = 2.5f;

    private IInteractable current;   // what we're looking at right now (or null)
    private string message;          // temporary message text
    private float messageUntil;      // time when the message disappears
    private GUIStyle promptStyle;

    void Start()
    {
        if (playerCamera == null) playerCamera = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        current = FindInteractable();

        Keyboard keyboard = Keyboard.current;
        if (current != null && keyboard != null && keyboard.eKey.wasPressedThisFrame)
            current.Interact(this);
    }

    IInteractable FindInteractable()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        // DefaultRaycastLayers = every layer except "Ignore Raycast". The Player is on that layer,
        // so the ray doesn't hit the player's own body when looking down.
        // QueryTriggerInteraction.Collide also lets the ray hit "trigger" colliders (used for pickups later).
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
        {
            // The script may be on the object we hit or on one of its parents.
            IInteractable found = hit.collider.GetComponentInParent<IInteractable>();

            // An empty prompt means "can't be used right now" (e.g. the toilet before the story gets there).
            if (found != null && !string.IsNullOrEmpty(found.GetPrompt()))
                return found;
        }
        return null;
    }

    // Other scripts call this to show a short message, e.g. "It's locked."
    public void ShowMessage(string text)
    {
        message = text;
        messageUntil = Time.time + messageDuration;
    }

    // OnGUI is Unity's simplest way to draw text on screen. It draws on top of the camera
    // image (after the VHS effects), so prompts stay readable.
    void OnGUI()
    {
        if (promptStyle == null)
        {
            promptStyle = new GUIStyle(GUI.skin.label);
            promptStyle.alignment = TextAnchor.MiddleCenter;
            promptStyle.fontSize = Mathf.Max(16, Screen.height / 30);
            promptStyle.normal.textColor = Color.white;
        }

        float cx = Screen.width / 2f;
        float cy = Screen.height / 2f;

        if (showCrosshair)
        {
            GUI.color = current != null ? Color.white : new Color(1f, 1f, 1f, 0.4f);
            GUI.DrawTexture(new Rect(cx - 2f, cy - 2f, 4f, 4f), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        if (current != null)
            DrawShadowedLabel(new Rect(0f, cy + 30f, Screen.width, 40f), "[E] " + current.GetPrompt());

        if (Time.time < messageUntil)
            DrawShadowedLabel(new Rect(0f, Screen.height * 0.75f, Screen.width, 40f), message);
    }

    void DrawShadowedLabel(Rect rect, string text)
    {
        // Draw black text slightly offset first, then white on top, so it's readable on any background.
        promptStyle.normal.textColor = Color.black;
        GUI.Label(new Rect(rect.x + 2f, rect.y + 2f, rect.width, rect.height), text, promptStyle);
        promptStyle.normal.textColor = Color.white;
        GUI.Label(rect, text, promptStyle);
    }
}
