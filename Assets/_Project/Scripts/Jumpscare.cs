using System.Collections;
using UnityEngine;

// The jumpscare when the player leaves the toilet: a desperate customer rushes into view,
// covered in shadow, the screen shakes with a loud sting, then the shadow clears and he complains.
// Afterwards he pushes past into the toilet and the door shuts behind him.
// Put this on the jumpscare character's root object. The StoryManager calls Play().
public class Jumpscare : MonoBehaviour
{
    [Header("Scene references")]
    [Tooltip("The character model (child object). Hidden until the jumpscare starts.")]
    public GameObject body;

    [Tooltip("Where he rushes in from (out of view, e.g. on the shop floor).")]
    public Transform startPoint;

    [Tooltip("Points he walks through afterwards to get into the toilet, in order.")]
    public Transform[] walkPath;

    [Tooltip("The toilet door, shut once he's inside.")]
    public Door toiletDoor;

    [Header("Timing")]
    [Tooltip("How long the rush into view takes (seconds). Short = scarier.")]
    public float rushTime = 0.3f;

    [Tooltip("How far in front of the camera he stops (metres).")]
    public float stopDistance = 0.75f;

    [Tooltip("How long it takes the shadow to clear once he's stopped.")]
    public float revealTime = 0.5f;

    public float walkSpeed = 2.5f;

    [Header("Screen shake")]
    public float shakeTime = 0.5f;
    public float shakeStrength = 0.06f;

    [Header("Sound")]
    [Tooltip("Loud sting played as he appears (optional).")]
    public AudioClip sting;
    [Range(0f, 1f)] public float stingVolume = 1f;

    [Header("Dialogue")]
    [TextArea] public string complaint1 = "FINALLY! Jeez, kid, what were you doin' in there? I been waitin' forever!";
    [TextArea] public string complaint2 = "Outta my way, I'm 'bout to bust!";
    [TextArea] public string playerReply = "...Good luck in there, heh.";

    private Renderer[] renderers;
    private MaterialPropertyBlock block;

    void Start()
    {
        renderers = body.GetComponentsInChildren<Renderer>(true);
        block = new MaterialPropertyBlock();
        body.SetActive(false);
    }

    public void Play(PlayerInteractor player)
    {
        StartCoroutine(Sequence(player));
    }

    IEnumerator Sequence(PlayerInteractor player)
    {
        FirstPersonController movement = player.GetComponent<FirstPersonController>();
        Transform cam = player.playerCamera.transform;
        movement.canControl = false;

        // Work out the stopping spot: straight in front of the camera, standing on the floor.
        Vector3 flatForward = new Vector3(cam.forward.x, 0f, cam.forward.z).normalized;
        Vector3 stopSpot = cam.position + flatForward * stopDistance;
        stopSpot.y = startPoint.position.y;

        // 1. Appear in shadow at the start point and rush to the spot, always facing the player.
        SetShade(0f);   // 0 = black silhouette
        body.SetActive(true);
        if (sting != null) AudioSource.PlayClipAtPoint(sting, cam.position, stingVolume);
        StartCoroutine(Shake(cam));

        Vector3 from = startPoint.position;
        for (float t = 0f; t < 1f; t += Time.deltaTime / rushTime)
        {
            transform.position = Vector3.Lerp(from, stopSpot, t);
            FaceTowards(cam.position);
            yield return null;
        }
        transform.position = stopSpot;
        FaceTowards(cam.position);

        // 2. The shadow clears.
        for (float t = 0f; t < 1f; t += Time.deltaTime / revealTime)
        {
            SetShade(t);
            yield return null;
        }
        SetShade(1f);

        // 3. He complains.
        yield return Say("Redneck", complaint1);
        yield return Say("Redneck", complaint2);

        // 4. He pushes past into the toilet. The player can move again.
        movement.canControl = true;
        GameUI.Instance.Say("You", playerReply);
        // If the player shut the toilet door behind them, he opens it (swinging away from him, into the toilet).
        if (toiletDoor != null && !toiletDoor.IsOpen) toiletDoor.Open(transform.position);
        foreach (Transform point in walkPath)
        {
            while (Vector3.Distance(transform.position, point.position) > 0.05f)
            {
                FaceTowards(point.position);
                transform.position = Vector3.MoveTowards(transform.position, point.position, walkSpeed * Time.deltaTime);
                yield return null;
            }
        }
        body.SetActive(false);
        if (toiletDoor != null) toiletDoor.Close();

        while (GameUI.Instance.IsSpeaking) yield return null;
        StoryManager.Instance.OnEvent("JumpscareFinished");
    }

    // Turn the whole body (only around the vertical axis) to face a point.
    void FaceTowards(Vector3 point)
    {
        Vector3 dir = point - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(dir);
    }

    // Tints every material from black (0) to its normal colour (1).
    // A MaterialPropertyBlock changes the colour for this character only, without editing the material assets.
    void SetShade(float amount)
    {
        Color c = Color.Lerp(Color.black, Color.white, amount);
        foreach (Renderer r in renderers)
        {
            r.GetPropertyBlock(block);
            block.SetColor("_Color", c);
            r.SetPropertyBlock(block);
        }
    }

    // Jiggles the camera for a moment, getting weaker over time, then puts it back.
    IEnumerator Shake(Transform cam)
    {
        Vector3 original = cam.localPosition;
        for (float t = 0f; t < shakeTime; t += Time.deltaTime)
        {
            float strength = shakeStrength * (1f - t / shakeTime);
            cam.localPosition = original + Random.insideUnitSphere * strength;
            yield return null;
        }
        cam.localPosition = original;
    }

    IEnumerator Say(string speaker, string text)
    {
        GameUI.Instance.Say(speaker, text);
        while (GameUI.Instance.IsSpeaking) yield return null;
    }
}
