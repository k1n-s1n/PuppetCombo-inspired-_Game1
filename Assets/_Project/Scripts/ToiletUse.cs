using UnityEngine;
using UnityEngine.InputSystem;

// The toilet aiming minigame. Press E at the toilet: the player steps up to it and looks down at the bowl.
// A cartoony stream (a LineRenderer drawn as an arc) runs from the player to the aim point.
// The mouse moves the aim, and the aim wobbles by itself, so the player has to keep correcting it.
// The progress bar only fills while the stream lands in the bowl. Missing leaves puddles on the floor.
// Put this on the Toilet object (it needs a collider so the player's E ray can hit it).
public class ToiletUse : MonoBehaviour, IInteractable
{
    [Header("Scene references")]
    [Tooltip("Empty object at the centre of the bowl opening, at rim height.")]
    public Transform bowlCentre;

    [Tooltip("Empty object in front of the toilet where the player stands during the minigame.")]
    public Transform standPoint;

    [Header("Gameplay")]
    [Tooltip("How close to the bowl centre the stream must land to count (metres).")]
    public float bowlRadius = 0.13f;

    [Tooltip("How far below the rim the water surface is (where a good shot lands).")]
    public float waterDepth = 0.15f;

    [Tooltip("Seconds of hitting the bowl needed to fill the bar.")]
    public float fillTime = 4f;

    [Tooltip("How far the aim moves per pixel of mouse movement (metres).")]
    public float aimSensitivity = 0.0012f;

    [Tooltip("How strongly the aim drifts by itself (metres per second). Higher = harder.")]
    public float wobbleStrength = 0.22f;

    [Tooltip("How quickly the wobble changes direction.")]
    public float wobbleSpeed = 0.7f;

    [Tooltip("The aim can't wander further than this from the bowl (metres).")]
    public float maxAimOffset = 0.5f;

    [Header("Stream look")]
    public Material streamMaterial;
    public float streamWidth = 0.018f;
    [Tooltip("How high the stream arcs above a straight line (metres).")]
    public float arcHeight = 0.12f;

    [Header("Puddles (when missing)")]
    public Material puddleMaterial;
    [Tooltip("Seconds between new puddles while missing.")]
    public float puddleInterval = 0.25f;
    public int maxPuddles = 30;

    [Header("Dialogue")]
    public string finishedLine = "What a mess... not my problem, heh.";

    private bool inUse;
    private float progress;        // 0 to 1
    private Vector2 aimOffset;     // where the aim is, relative to the bowl centre (x = right, y = forward)
    private float nextPuddleTime;
    private int puddleCount;
    private bool hitting;          // is the stream landing in the bowl this frame?

    private FirstPersonController playerMovement;
    private Transform cam;
    private LineRenderer stream;
    private GUIStyle labelStyle;

    const int StreamPoints = 16;   // how many points make up the arc

    void Start()
    {
        // The stream is a LineRenderer on its own child object, hidden until the minigame starts.
        GameObject streamObject = new GameObject("Stream");
        streamObject.transform.SetParent(transform, false);
        stream = streamObject.AddComponent<LineRenderer>();
        stream.positionCount = StreamPoints;
        stream.useWorldSpace = true;
        stream.startWidth = streamWidth;
        stream.endWidth = streamWidth * 0.7f;
        stream.material = streamMaterial;
        stream.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        stream.enabled = false;
    }

    public string GetPrompt()
    {
        // Only usable at the right point in the story, and not while already in use.
        if (inUse || StoryManager.Instance.stage != StoryManager.Stage.UseToilet) return "";
        return "Use toilet";
    }

    public void Interact(PlayerInteractor player)
    {
        inUse = true;
        progress = 0f;
        cam = player.playerCamera.transform;

        // Freeze normal movement/looking, step up to the toilet and look down at the bowl.
        playerMovement = player.GetComponent<FirstPersonController>();
        playerMovement.canControl = false;
        playerMovement.Teleport(standPoint.position);   // the stand point sits at the player's standing height on the floor
        playerMovement.LookAt(bowlCentre.position);

        // Start a little off to one side so the player has to aim straight away.
        aimOffset = Random.insideUnitCircle.normalized * 0.25f;
        stream.enabled = true;
    }

    void Update()
    {
        if (!inUse) return;

        // 1. Move the aim: the mouse pushes it, and Perlin noise (smooth randomness) makes it wobble.
        Vector2 mouse = Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;
        float t = Time.time * wobbleSpeed;
        Vector2 wobble = new Vector2(Mathf.PerlinNoise(t, 0f) - 0.5f, Mathf.PerlinNoise(0f, t + 10f) - 0.5f) * 2f;
        aimOffset += mouse * aimSensitivity + wobble * wobbleStrength * Time.deltaTime;
        aimOffset = Vector2.ClampMagnitude(aimOffset, maxAimOffset);

        // 2. Turn the 2D aim into a point in the world, on the flat plane at rim height.
        //    Mouse right = aim right, mouse up = aim further away, as seen from the camera.
        Vector3 right = new Vector3(cam.right.x, 0f, cam.right.z).normalized;
        Vector3 forward = new Vector3(cam.forward.x, 0f, cam.forward.z).normalized;
        Vector3 aimPoint = bowlCentre.position + right * aimOffset.x + forward * aimOffset.y;

        // 3. Did it land in the bowl? If not, find where it lands (floor, seat...) with a ray straight down.
        hitting = aimOffset.magnitude < bowlRadius;
        Vector3 landPoint;
        if (hitting)
        {
            landPoint = aimPoint + Vector3.down * waterDepth;   // down onto the water surface
            progress += Time.deltaTime / fillTime;
        }
        else if (Physics.Raycast(aimPoint + Vector3.up * 0.3f, Vector3.down, out RaycastHit hit, 2f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            landPoint = hit.point;
            if (Time.time >= nextPuddleTime) MakePuddle(hit.point, hit.normal);
        }
        else
        {
            landPoint = aimPoint;
        }

        DrawStream(landPoint);
        if (progress >= 1f) Finish();
    }

    // Draws the stream as an arc: a straight line from start to end, lifted in the middle by a parabola.
    void DrawStream(Vector3 end)
    {
        Vector3 flatForward = new Vector3(cam.forward.x, 0f, cam.forward.z).normalized;
        Vector3 start = cam.position + Vector3.down * 0.72f + flatForward * 0.18f;   // roughly waist height
        for (int i = 0; i < StreamPoints; i++)
        {
            float f = i / (float)(StreamPoints - 1);         // 0 at the start, 1 at the end
            Vector3 p = Vector3.Lerp(start, end, f);
            p += Vector3.up * arcHeight * 4f * f * (1f - f);   // 4f(1-f) is 0 at both ends and 1 in the middle
            stream.SetPosition(i, p);
        }
    }

    // Leaves a small flat yellow puddle where the stream missed. Cartoony, not realistic.
    void MakePuddle(Vector3 point, Vector3 normal)
    {
        nextPuddleTime = Time.time + puddleInterval;
        if (puddleCount >= maxPuddles) return;
        puddleCount++;

        GameObject puddle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        puddle.name = "Puddle";
        Destroy(puddle.GetComponent<Collider>());
        puddle.GetComponent<Renderer>().sharedMaterial = puddleMaterial;
        float size = Random.Range(0.06f, 0.13f);
        puddle.transform.localScale = new Vector3(size, 0.001f, size * Random.Range(0.7f, 1.3f));
        puddle.transform.position = point + normal * 0.003f;   // just above the surface so it doesn't flicker
        puddle.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal) * Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
    }

    void Finish()
    {
        inUse = false;
        stream.enabled = false;
        playerMovement.canControl = true;

        GameUI.Instance.Say("You", finishedLine);
        StoryManager.Instance.OnEvent("UsedToilet");
    }

    void OnGUI()
    {
        if (!inUse) return;

        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.alignment = TextAnchor.MiddleCenter;
            labelStyle.fontSize = Mathf.Max(14, Screen.height / 36);
            labelStyle.normal.textColor = Color.white;
        }

        // Bar near the bottom of the screen: dark background, fill is bright yellow while hitting, dull while missing.
        float width = Screen.width * 0.3f;
        float height = Screen.height * 0.03f;
        Rect back = new Rect((Screen.width - width) * 0.5f, Screen.height * 0.92f, width, height);

        GUI.color = new Color(0f, 0f, 0f, 0.7f);
        GUI.DrawTexture(back, Texture2D.whiteTexture);
        GUI.color = hitting ? new Color(1f, 0.85f, 0.2f) : new Color(0.55f, 0.45f, 0.15f);
        GUI.DrawTexture(new Rect(back.x + 2f, back.y + 2f, (back.width - 4f) * progress, back.height - 4f), Texture2D.whiteTexture);
        GUI.color = Color.white;

        string hint = hitting ? "Keep it in the bowl!" : "Move the mouse to aim at the bowl!";
        GUI.Label(new Rect(0f, back.y - 40f, Screen.width, 36f), hint, labelStyle);
    }
}
