using UnityEngine;

// Simple shopper behaviour: walk around a loop of points in order. At "browse" points the customer
// turns to face the shelf (the way the point faces) and stands there for a while; at the others
// they just walk through. No pathfinding: the points are placed so the straight lines between them
// stay in the aisles. Put this on the customer's root object.
public class CustomerWander : MonoBehaviour
{
    // One point on the route.
    [System.Serializable]
    public class Stop
    {
        [Tooltip("Where to walk to. For browse points, rotate it so its blue (forward) arrow faces the shelf.")]
        public Transform point;

        [Tooltip("Seconds to stand and browse here (0 = just walk through).")]
        public float browseTime = 0f;
    }

    public Stop[] route;

    [Tooltip("Walking speed in metres per second (people stroll at about 1).")]
    public float walkSpeed = 0.9f;

    [Tooltip("How fast they turn, in degrees per second.")]
    public float turnSpeed = 180f;

    [Tooltip("Stop and wait if the player is closer than this in front of them (so they don't walk through you).")]
    public float personalSpace = 0.9f;

    [Tooltip("Optional: drives the leg/arm swing while walking.")]
    public SimpleWalkCycle walkCycle;

    // How fast they're moving right now (other scripts can read it).
    public float CurrentSpeed { get; private set; }

    private int index;           // which route point we're heading to
    private float browseUntil;   // time when browsing at the current point ends
    private bool browsing;
    private Transform player;

    void Start()
    {
        if (walkCycle == null) walkCycle = GetComponent<SimpleWalkCycle>();
        PlayerInteractor p = FindAnyObjectByType<PlayerInteractor>();
        if (p != null) player = p.transform;
    }

    void Update()
    {
        CurrentSpeed = 0f;
        if (route == null || route.Length == 0) return;
        Stop stop = route[index];

        if (browsing)
        {
            // Stand still and turn to face the shelf until browse time is up, then head for the next point.
            TurnTowards(stop.point.forward);
            if (Time.time >= browseUntil)
            {
                browsing = false;
                index = (index + 1) % route.Length;
            }
        }
        else
        {
            Vector3 toPoint = stop.point.position - transform.position;
            toPoint.y = 0f;

            if (toPoint.magnitude < 0.05f)
            {
                // Arrived. Browse here, or carry straight on to the next point.
                if (stop.browseTime > 0f)
                {
                    browsing = true;
                    browseUntil = Time.time + stop.browseTime * Random.Range(0.7f, 1.3f);   // vary it a bit
                }
                else
                {
                    index = (index + 1) % route.Length;
                }
            }
            else
            {
                TurnTowards(toPoint);
                if (!PlayerInTheWay())
                {
                    float step = Mathf.Min(walkSpeed * Time.deltaTime, toPoint.magnitude);
                    transform.position += toPoint.normalized * step;
                    CurrentSpeed = walkSpeed;
                }
            }
        }

        if (walkCycle != null) walkCycle.speed = CurrentSpeed;
    }

    // Smoothly rotate (around the vertical axis only) to face a direction.
    void TurnTowards(Vector3 direction)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f) return;
        Quaternion target = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeed * Time.deltaTime);
    }

    // True if the player is standing close in front of us.
    bool PlayerInTheWay()
    {
        if (player == null) return false;
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        return toPlayer.magnitude < personalSpace && Vector3.Dot(transform.forward, toPlayer.normalized) > 0.3f;
    }
}
