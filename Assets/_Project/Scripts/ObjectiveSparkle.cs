using UnityEngine;

// A small twinkling star that marks an item the player needs (a common trick in PS1-style horror games
// so objective items don't get lost among the shelves). Every few seconds it flashes: it grows, spins
// a little and fades out again. It always faces the camera (a "billboard").
// Put this on an empty child object of the item, where the twinkle should appear.
// It only shows during the story stages listed in "Active Stages"; the item hides it when picked up.
public class ObjectiveSparkle : MonoBehaviour
{
    [Tooltip("Additive material with the star texture (Sparkle.mat).")]
    public Material material;

    [Tooltip("The story stages during which this sparkle is shown.")]
    public StoryManager.Stage[] activeStages = { StoryManager.Stage.GrabSnacks };

    [Tooltip("Seconds between twinkles.")]
    public float interval = 1.6f;

    [Tooltip("How long one twinkle lasts (seconds).")]
    public float twinkleTime = 0.6f;

    [Tooltip("Size of the star at its biggest (metres).")]
    public float size = 0.22f;

    private Transform quad;
    private Renderer quadRenderer;
    private float offset;   // so several sparkles don't all flash at exactly the same moment

    void Start()
    {
        // A Quad is a flat square; the star texture is drawn on it.
        GameObject q = GameObject.CreatePrimitive(PrimitiveType.Quad);
        q.name = "SparkleQuad";
        Destroy(q.GetComponent<Collider>());   // it mustn't block the E ray
        q.transform.SetParent(transform, false);
        quad = q.transform;
        quadRenderer = q.GetComponent<Renderer>();
        quadRenderer.sharedMaterial = material;
        quadRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        quadRenderer.receiveShadows = false;

        offset = Random.Range(0f, interval);
    }

    void LateUpdate()
    {
        bool active = System.Array.IndexOf(activeStages, StoryManager.Instance.stage) >= 0;
        Camera cam = Camera.main;
        if (!active || cam == null)
        {
            quadRenderer.enabled = false;
            return;
        }

        // Where are we in the current cycle? 0..interval, the twinkle plays during the first twinkleTime seconds.
        float t = Mathf.Repeat(Time.time + offset, interval);
        float k = t < twinkleTime ? Mathf.Sin(t / twinkleTime * Mathf.PI) : 0f;   // 0 -> 1 -> 0 smoothly
        quadRenderer.enabled = k > 0.01f;
        if (!quadRenderer.enabled) return;

        // Face the camera, spin slightly while flashing, and grow/shrink with k.
        quad.rotation = Quaternion.LookRotation(quad.position - cam.transform.position) * Quaternion.Euler(0f, 0f, t * 90f);
        // Divide by the parent's scale so the star is the same size on every item.
        float worldScale = transform.lossyScale.x;
        quad.localScale = Vector3.one * (size * k / Mathf.Max(0.0001f, worldScale));
    }
}
