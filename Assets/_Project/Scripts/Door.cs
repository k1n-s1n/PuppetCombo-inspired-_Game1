using UnityEngine;

// A door that swings open/closed when the player presses E on it.
// The door object's pivot must be on its hinge edge (the 6twelve doors already are).
// It always swings AWAY from the player, so it never opens into their face.
public class Door : MonoBehaviour, IInteractable
{
    [Tooltip("How far the door swings open, in degrees.")]
    [Range(30f, 120f)]
    public float openAngle = 95f;

    [Tooltip("How fast the door swings, in degrees per second.")]
    public float swingSpeed = 180f;

    [Tooltip("Optional: the other half of a double door, opened and closed together with this one.")]
    public Door linkedDoor;

    [Header("Locking")]
    [Tooltip("A locked door won't open. (Unlocking with a key comes later.)")]
    public bool isLocked = false;

    [Tooltip("Message shown when the player tries a locked door.")]
    public string lockedMessage = "It's locked.";

    public bool IsOpen { get; private set; }

    private Quaternion closedRotation;  // rotation when shut (saved at start)
    private Vector3 hingeToMiddle;      // flat direction from the hinge to the middle of the closed door
    private float currentAngle;         // how far open we are right now
    private float targetAngle;          // where we're swinging to (0 = closed)

    void Awake()
    {
        closedRotation = transform.rotation;

        // Record where the middle of the door is while it's closed (used to pick the swing direction).
        Renderer rend = GetComponent<Renderer>();
        hingeToMiddle = rend != null ? rend.bounds.center - transform.position : transform.right * 0.5f;
        hingeToMiddle.y = 0f;
    }

    void Update()
    {
        // Smoothly move the current angle towards the target, then apply it around the world's up axis.
        currentAngle = Mathf.MoveTowards(currentAngle, targetAngle, swingSpeed * Time.deltaTime);
        transform.rotation = Quaternion.AngleAxis(currentAngle, Vector3.up) * closedRotation;
    }

    public string GetPrompt()
    {
        if (isLocked) return "Try door";
        return IsOpen ? "Close door" : "Open door";
    }

    public void Interact(PlayerInteractor player)
    {
        if (isLocked)
        {
            player.ShowMessage(lockedMessage);
            return;
        }

        if (IsOpen) Close();
        else Open(player.transform.position);

        // Keep the other half of a double door in step.
        if (linkedDoor != null)
        {
            if (IsOpen) linkedDoor.Open(player.transform.position);
            else linkedDoor.Close();
        }
    }

    public void Open(Vector3 playerPosition)
    {
        IsOpen = true;
        targetAngle = AngleAwayFrom(playerPosition);
    }

    public void Close()
    {
        IsOpen = false;
        targetAngle = 0f;
    }

    // Works out whether +openAngle or -openAngle swings the door away from the player.
    float AngleAwayFrom(Vector3 playerPosition)
    {
        // Where the middle of the door would end up if we swung by +openAngle.
        Vector3 swungMiddle = Quaternion.AngleAxis(openAngle, Vector3.up) * hingeToMiddle;

        Vector3 toPlayer = playerPosition - transform.position;
        toPlayer.y = 0f;

        // If that swing moves the door towards the player, swing the other way instead.
        return Vector3.Dot(swungMiddle, toPlayer) > 0f ? -openAngle : openAngle;
    }
}
