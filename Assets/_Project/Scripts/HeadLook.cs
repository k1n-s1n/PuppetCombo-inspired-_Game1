using UnityEngine;

// Makes a character's head turn to stare at the player's camera while the player is in front of them.
// The creepy indie-horror stare. Put this on the character's root object (the one that faces forward);
// the character model must be a Humanoid rig, so we can ask the Animator for its head bone.
//
// It runs in LateUpdate, which happens AFTER animations have posed the skeleton each frame,
// so our head rotation is applied on top of whatever animation is playing.
public class HeadLook : MonoBehaviour
{
    [Tooltip("The character's Animator (found automatically in the children if left empty).")]
    public Animator animator;

    [Tooltip("How far the head may turn left/right, in degrees.")]
    public float maxYaw = 65f;

    [Tooltip("How far the head may tilt up/down, in degrees.")]
    public float maxPitch = 30f;

    [Tooltip("The player is only tracked inside this angle either side of where the body faces (their field of view).")]
    public float fieldOfView = 110f;

    [Tooltip("The player is only tracked when closer than this (metres).")]
    public float maxDistance = 9f;

    [Tooltip("How quickly the head turns towards / away from the player (0 to 1 per second).")]
    public float turnSpeed = 2.5f;

    private Transform head;
    private Transform target;            // the player's camera
    private Quaternion restLocalRotation; // the head's own rotation when nothing animates it
    private float weight;                 // 0 = not looking, 1 = fully looking at the player

    void Start()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        head = animator.GetBoneTransform(HumanBodyBones.Head);
        restLocalRotation = head.localRotation;
        if (Camera.main != null) target = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (head == null || target == null) return;

        // With an animation playing, the Animator resets the head every frame, so we start from its pose.
        // Without one, nothing resets it, so we start from the saved rest rotation (otherwise our turns would add up).
        if (animator.runtimeAnimatorController == null)
            head.localRotation = restLocalRotation;

        // Direction from the head to the camera, split into left/right (yaw) and up/down (pitch).
        Vector3 toTarget = target.position - head.position;
        Vector3 flat = new Vector3(toTarget.x, 0f, toTarget.z);
        Vector3 bodyForward = new Vector3(transform.forward.x, 0f, transform.forward.z);
        float yaw = Vector3.SignedAngle(bodyForward, flat, Vector3.up);
        float pitch = -Mathf.Atan2(toTarget.y, flat.magnitude) * Mathf.Rad2Deg;   // negative = look up

        // Only stare while the player is in front of the character and close enough.
        bool canSee = Mathf.Abs(yaw) < fieldOfView && toTarget.magnitude < maxDistance;
        weight = Mathf.MoveTowards(weight, canSee ? 1f : 0f, turnSpeed * Time.deltaTime);

        yaw = Mathf.Clamp(yaw, -maxYaw, maxYaw) * weight;
        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch) * weight;

        // Tilt around the body's right axis, then turn around the world's up axis, on top of the current pose.
        Quaternion turn = Quaternion.AngleAxis(yaw, Vector3.up) * Quaternion.AngleAxis(pitch, transform.right);
        head.rotation = turn * head.rotation;
    }
}
