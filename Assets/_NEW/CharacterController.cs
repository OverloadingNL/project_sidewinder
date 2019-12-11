using System;
using UnityEngine;
using UnityEngine.AI;
[SelectionBase]
public class CharacterController : MonoBehaviour
{
    [Header("Animator")] [SerializeField] RuntimeAnimatorController animatorController;
    [SerializeField] AnimatorOverrideController animatorOverrideController;
    [SerializeField] Avatar characterAvatar;
    [SerializeField] [Range(.1f, 1f)] float animatorForwardCap = 1f;

    [Header("Audio")]
    [SerializeField]
    float audioSourceSpatialBlend = 0.5f;

    [Header("Capsule Collider")]
    [SerializeField]
    Vector3 colliderCenter = new Vector3(0, 1.03f, 0);
    [SerializeField] float colliderRadius = 0.2f;
    [SerializeField] float colliderHeight = 2.03f;
    [SerializeField] float groundCheckDistance = 0.1f;
    CapsuleCollider capsuleCollider;

    [Header("Movement")]
    [SerializeField]
    float moveSpeedMultiplier = .7f;
    [SerializeField] float animationSpeedMultiplier = 1.5f;
    [SerializeField] float movingTurnSpeed = 360;
    [SerializeField] float stationaryTurnSpeed = 180;
    [SerializeField] float jumpPower = 12f;
    [SerializeField] float moveThreshold = 1f;
    [SerializeField] float runCycleLegOffset = 0.2f; //specific to the character in sample assets, will need to be modified to work with others
    [Range(1f, 4f)] [SerializeField] float gravityMultiplier = 2f;

    [Header("Rigid Body")]
    [SerializeField]
    float rigidBodyMass;
    [SerializeField] bool isKinematic;

    public bool isMoving;
    const int noMovementFrames = 3;
    Vector3[] previousLocations = new Vector3[noMovementFrames];

    NavMeshAgent navMeshAgent;
    Animator animator;
    Rigidbody ridigBody;

    float turnAmount;
    float forwardAmount;
    bool isAlive = true;
    Vector3 groundNormal;
    bool isGrounded;
    bool isCrouching;
    float origGroundCheckDistance;

    const float k_Half = 0.5f;

    void Awake()
    {
        AddRequiredComponents();

        //For good measure, set the previous locations
        for (int i = 0; i < previousLocations.Length; i++)
        {
            previousLocations[i] = Vector3.zero;
        }
    }

    private void AddRequiredComponents()
    {
        capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
        capsuleCollider.center = colliderCenter;
        capsuleCollider.radius = colliderRadius;
        capsuleCollider.height = colliderHeight;

        ridigBody = gameObject.AddComponent<Rigidbody>();
        ridigBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionX;
        ridigBody.mass = rigidBodyMass;
        ridigBody.isKinematic = isKinematic;

        var audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = audioSourceSpatialBlend;

        animator = GetComponent<Animator>();
        if (!animator)
        {
            animator = gameObject.AddComponent<Animator>();
        }
        animator.runtimeAnimatorController = animatorController;
        animator.avatar = characterAvatar;
    }


    public float GetAnimSpeedMultiplier()
    {
        return animator.speed;
    }

    public void Kill()
    {
        isAlive = false;
    }

    public void SetDestination(Vector3 worldPos)
    {
        navMeshAgent.destination = worldPos;
    }

    public AnimatorOverrideController GetOverrideController()
    {
        return animatorOverrideController;
    }

    void SetForwardAndTurn(Vector3 movement)
    {
        // convert the world relative moveInput vector into a local-relative
        // turn amount and forward amount required to head in the desired direction
        if (movement.magnitude > moveThreshold)
        {
            movement.Normalize();
        }
        var localMove = transform.InverseTransformDirection(movement);
        turnAmount = Mathf.Atan2(localMove.x, localMove.z);
        forwardAmount = localMove.z;
    }

   void UpdateAnimator(Vector3 move)
    {
        // update the animator parameters
        animator.SetFloat("Forward", forwardAmount * animatorForwardCap, 0.1f, Time.deltaTime);
        animator.SetFloat("Turn", turnAmount, 0.1f, Time.deltaTime);
        animator.SetBool("Crouch", isCrouching);
        animator.SetBool("OnGround", isGrounded);
        if (!isGrounded)
        {
            animator.SetFloat("Jump", ridigBody.velocity.y);
        }

        // calculate which leg is behind, so as to leave that leg trailing in the jump animation
        // (This code is reliant on the specific run cycle offset in our animations,
        // and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
        float runCycle =
            Mathf.Repeat(
                animator.GetCurrentAnimatorStateInfo(0).normalizedTime + runCycleLegOffset, 1);
        float jumpLeg = (runCycle < k_Half ? 1 : -1) * forwardAmount;
        if (isGrounded)
        {
            animator.SetFloat("JumpLeg", jumpLeg);
        }

        // the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
        // which affects the movement speed because of the root motion.
        if (isGrounded && move.magnitude > 0)
        {
            animator.speed = animationSpeedMultiplier;
        }
        else
        {
            // don't use that while airborne
            animator.speed = 1;
        }
    }

    void ApplyExtraTurnRotation()
    {
        // help the character turn faster (this is in addition to root rotation in the animation)
        float turnSpeed = Mathf.Lerp(stationaryTurnSpeed, movingTurnSpeed, forwardAmount);
        transform.Rotate(0, turnAmount * turnSpeed * Time.deltaTime, 0);
    }

    void OnAnimatorMove()
    {
        // we implement this function to override the default root motion.
        // this allows us to modify the positional speed before it's applied.
        if (Time.deltaTime > 0)
        {
            Vector3 velocity = (animator.deltaPosition * moveSpeedMultiplier) / Time.deltaTime;

            // we preserve the existing y part of the current velocity.
            velocity.y = ridigBody.velocity.y;
            ridigBody.velocity = velocity;
        }

    }

    public void Move(Vector3 move, bool crouch, bool jump)
    {

        // convert the world relative moveInput vector into a local-relative
        // turn amount and forward amount required to head in the desired
        // direction.
        if (move.magnitude > 1f) move.Normalize();
        move = transform.InverseTransformDirection(move);
        CheckGroundStatus();
        move = Vector3.ProjectOnPlane(move, groundNormal);
        turnAmount = Mathf.Atan2(move.x, move.z);
        forwardAmount = move.z;

        ApplyExtraTurnRotation();

        // control and velocity handling is different when grounded and airborne:
        if (isGrounded)
        {
            HandleGroundedMovement(crouch, jump);
        }
        else
        {
            HandleAirborneMovement();
        }

        ScaleCapsuleForCrouching(crouch);
        PreventStandingInLowHeadroom();

        // send input and other state parameters to the animator
        UpdateAnimator(move);
    }

    void ScaleCapsuleForCrouching(bool crouch)
    {
        if (isGrounded && crouch)
        {
            if (isCrouching) return;
            capsuleCollider.height = capsuleCollider.height / 2f;
            capsuleCollider.center = capsuleCollider.center / 2f;
            isCrouching = true;
        }
        else
        {
            Ray crouchRay = new Ray(ridigBody.position + Vector3.up * capsuleCollider.radius * k_Half, Vector3.up);
            float crouchRayLength = colliderHeight - capsuleCollider.radius * k_Half;
            if (Physics.SphereCast(crouchRay, capsuleCollider.radius * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                isCrouching = true;
                return;
            }
            capsuleCollider.height = colliderHeight;
            capsuleCollider.center = colliderCenter;
            isCrouching = false;
        }
    }
    void CheckGroundStatus()
    {
        RaycastHit hitInfo;
#if UNITY_EDITOR
        // helper to visualise the ground check ray in the scene view
        Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * groundCheckDistance));
#endif
        // 0.1f is a small offset to start the ray from inside the character
        // it is also good to note that the transform position in the sample assets is at the base of the character
        if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, groundCheckDistance))
        {
            groundNormal = hitInfo.normal;
            isGrounded = true;
            animator.applyRootMotion = true;
        }
        else
        {
            isGrounded = false;
            groundNormal = Vector3.up;
            animator.applyRootMotion = false;
        }
    }
    void HandleGroundedMovement(bool crouch, bool jump)
    {
        // check whether conditions are right to allow a jump:
        if (jump && !crouch && animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded"))
        {
            // jump!
            ridigBody.velocity = new Vector3(ridigBody.velocity.x, jumpPower, ridigBody.velocity.z);
            isGrounded = false;
            animator.applyRootMotion = false;
            //groundCheckDistance = 0.1f;
        }
    }
    void HandleAirborneMovement()
    {
        // apply extra gravity from multiplier:
        Vector3 extraGravityForce = (Physics.gravity * gravityMultiplier) - Physics.gravity;
        ridigBody.AddForce(extraGravityForce);

        //groundCheckDistance = ridigBody.velocity.y < 0 ? origGroundCheckDistance : 0.01f;
    }

    void PreventStandingInLowHeadroom()
    {
        // prevent standing up in crouch-only zones
        if (!isCrouching)
        {
            Ray crouchRay = new Ray(ridigBody.position + Vector3.up * capsuleCollider.radius * k_Half, Vector3.up);
            float crouchRayLength = colliderHeight - capsuleCollider.radius * k_Half;
            if (Physics.SphereCast(crouchRay, capsuleCollider.radius * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                isCrouching = true;
            }
        }
    }
}