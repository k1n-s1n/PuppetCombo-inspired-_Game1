using UnityEngine;

// Lowers a character's arms out of a T-pose (arms straight out) into a relaxed standing pose.
// Only needed for models that have no animation: with no animation playing, nothing else moves the bones,
// so rotating them once at the start keeps them there.
// If an Animator Controller (e.g. a Mixamo idle) is added later, this does nothing, because the animation poses the arms.
// Put this on the character's root object; the model must be a Humanoid rig.
public class ArmPose : MonoBehaviour
{
    [Tooltip("The character's Animator (found automatically in the children if left empty).")]
    public Animator animator;

    [Tooltip("How far to lower the upper arms from straight out, in degrees (90 = straight down by the sides).")]
    [Range(0f, 90f)] public float armDownAngle = 78f;

    [Tooltip("How far to bend the elbows forward, in degrees.")]
    [Range(0f, 60f)] public float elbowBend = 12f;

    void Start()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (animator.runtimeAnimatorController != null) return;   // an animation is in charge of the pose

        Vector3 forward = transform.forward;   // the way the character faces
        Vector3 right = transform.right;

        // Rotating around the forward axis swings an arm down in front of the body's side.
        // The left and right arms need opposite directions.
        Rotate(HumanBodyBones.LeftUpperArm, forward, armDownAngle);
        Rotate(HumanBodyBones.RightUpperArm, forward, -armDownAngle);

        // Bend the forearms slightly forward (around the body's right axis) so the arms don't look stiff.
        Rotate(HumanBodyBones.LeftLowerArm, right, -elbowBend);
        Rotate(HumanBodyBones.RightLowerArm, right, -elbowBend);
    }

    // Rotates one bone around a world-space axis.
    void Rotate(HumanBodyBones boneName, Vector3 axis, float degrees)
    {
        Transform bone = animator.GetBoneTransform(boneName);
        if (bone != null) bone.rotation = Quaternion.AngleAxis(degrees, axis) * bone.rotation;
    }
}
