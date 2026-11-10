using UnityEngine;

// A very simple walk animation made in code (no animation clip needed): while the character moves,
// the legs swing back and forth like pendulums, the knees bend, and the arms swing the opposite way.
// Stiff, but it suits the PS1 look. If an Animator Controller with a real walk animation is added
// later, this does nothing.
// Put this on the character's root object; the model must be a Humanoid rig.
public class SimpleWalkCycle : MonoBehaviour
{
    [Tooltip("The character's Animator (found automatically in the children if left empty).")]
    public Animator animator;

    [Tooltip("Set by CustomerWander each frame: how fast the character is moving (0 = standing).")]
    public float speed;

    [Tooltip("Steps per second at walking speed.")]
    public float stepsPerSecond = 1.8f;

    [Tooltip("How far the legs swing forward/back, in degrees.")]
    public float legSwing = 25f;

    [Tooltip("How much the knee bends as the leg swings through, in degrees.")]
    public float kneeBend = 35f;

    [Tooltip("How far the arms swing, in degrees.")]
    public float armSwing = 18f;

    // The bones we move, and their rotations in the model's own standing pose.
    private readonly HumanBodyBones[] bones =
    {
        HumanBodyBones.LeftUpperLeg, HumanBodyBones.RightUpperLeg,
        HumanBodyBones.LeftLowerLeg, HumanBodyBones.RightLowerLeg,
        HumanBodyBones.LeftUpperArm, HumanBodyBones.RightUpperArm,
    };
    private Transform[] boneTransforms;
    private Quaternion[] restRotations;

    private float phase;    // where we are in the step cycle (radians)
    private float weight;   // 0 = standing pose, 1 = full walk swing (blends smoothly)

    void Start()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        boneTransforms = new Transform[bones.Length];
        restRotations = new Quaternion[bones.Length];
        for (int i = 0; i < bones.Length; i++)
        {
            boneTransforms[i] = animator.GetBoneTransform(bones[i]);
            restRotations[i] = boneTransforms[i].localRotation;
        }
    }

    // LateUpdate runs after Update (where CustomerWander moves us), so we pose the bones for this frame.
    void LateUpdate()
    {
        if (animator.runtimeAnimatorController != null) return;   // a real animation is in charge

        // Blend the swing in when walking and out when standing, so the legs don't snap.
        weight = Mathf.MoveTowards(weight, speed > 0.01f ? 1f : 0f, 4f * Time.deltaTime);
        if (weight > 0f) phase += Time.deltaTime * stepsPerSecond * Mathf.PI;   // half a cycle per step

        // Start from the standing pose every frame (nothing else resets these bones).
        for (int i = 0; i < bones.Length; i++)
            boneTransforms[i].localRotation = restRotations[i];

        float swing = Mathf.Sin(phase);   // -1..1: +1 = left leg forward
        Vector3 side = transform.right;   // legs and arms swing around the body's left-right axis

        // Negative angle around 'side' swings a limb forward, positive swings it back.
        Rotate(0, side, -swing * legSwing * weight);   // left thigh
        Rotate(1, side, swing * legSwing * weight);    // right thigh

        // Each knee bends while its leg swings forward (lifting the foot), and straightens on the way back.
        float leftLift = Mathf.Max(0f, Mathf.Cos(phase));
        float rightLift = Mathf.Max(0f, -Mathf.Cos(phase));
        Rotate(2, side, leftLift * kneeBend * weight);
        Rotate(3, side, rightLift * kneeBend * weight);

        // Arms swing opposite to the legs.
        Rotate(4, side, swing * armSwing * weight);    // left arm goes back as left leg goes forward
        Rotate(5, side, -swing * armSwing * weight);
    }

    void Rotate(int i, Vector3 axis, float degrees)
    {
        boneTransforms[i].rotation = Quaternion.AngleAxis(degrees, axis) * boneTransforms[i].rotation;
    }
}
