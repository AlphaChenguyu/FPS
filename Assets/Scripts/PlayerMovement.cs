using System;
using System.Collections;

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private Vector2 moveInput;
    public float groundDrag;
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;

    public float dashSpeed;
    public float dashSpeedChangeFactor;

    public float maxYSpeed;

    bool isSprinting = false;
    private WallRunning wallrun;

    public float wallRunSpeed;
    
    [Header("Jump")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;
    public float wallJumpForce;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;
    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    bool exitingSlope;

    public Transform orientation;
    //private PlayerInputActions playerInputActions;
    public static PlayerInputActions playerInputActions { get; private set; }
    Vector3 moveDirection;

    Rigidbody rb;
    public MovementState state;//���浱ǰstate
    public enum MovementState
    {
        walking,
        sprinting,
        wallrunning,
        air,
        dashing
    }
    public bool wallrunning;
    public bool dashing;

    private void Awake()
    {
        playerInputActions = new PlayerInputActions();
    }
    private void OnEnable()
    {
        playerInputActions.Player.Enable();
        playerInputActions.Player.Move.performed += OnMove;  // ���� OnMove �¼�
        playerInputActions.Player.Move.canceled += OnMove;   // ���� OnMove ȡ���¼�
        playerInputActions.Player.Jump.performed += OnJump; // ������Ծ�¼�
        playerInputActions.Player.Sprint.performed += OnSprint;  
        playerInputActions.Player.Sprint.canceled += OnSprint;
    }
    private void OnDisable()
    {
        // �������붯����ȡ�������¼�
        playerInputActions.Player.Move.performed -= OnMove;
        playerInputActions.Player.Move.canceled -= OnMove;
        playerInputActions.Player.Jump.performed -= OnJump;
        playerInputActions.Player.Sprint.performed -= OnSprint;
        playerInputActions.Player.Sprint.canceled -= OnSprint;
        playerInputActions.Player.Disable();
    }
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        wallrun = GetComponent<WallRunning>();
        rb.freezeRotation = true;
        readyToJump = true;
    }
    private void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        SpeedControl();
        StateHandler();

        if (state ==MovementState.walking || state == MovementState.sprinting)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0;
        }

    }
    private void FixedUpdate()
    {
        MovePlayer();
    }
    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    private void OnJump(InputAction.CallbackContext context)
    {
        Jump();
    }
    private void OnSprint(InputAction.CallbackContext context)
    {
            if (context.phase == InputActionPhase.Performed)
        {
            isSprinting = true;  // ��ʼ���
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            isSprinting = false; // ֹͣ���
        }
    }

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    private MovementState lastState;
    private bool KeepMomentum;
    private float speedChangeFactor;
    private void StateHandler()
    {
        if (dashing)
        {
            state = MovementState.dashing;
            desiredMoveSpeed = dashSpeed;
            speedChangeFactor = dashSpeedChangeFactor;
        }
        else if (wallrunning)
        {
            state = MovementState.wallrunning;
            desiredMoveSpeed = wallRunSpeed;
        }
        else if (grounded&&isSprinting)
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }
        else
        {
            state = MovementState.air;
            if (desiredMoveSpeed < sprintSpeed)
                desiredMoveSpeed = walkSpeed;
            else 
                desiredMoveSpeed = sprintSpeed;
        }

        bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;
        if (lastState == MovementState.dashing) KeepMomentum = true;

        if (desiredMoveSpeedHasChanged)
        {
            if (KeepMomentum)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                StopAllCoroutines();
                moveSpeed = desiredMoveSpeed;
            }
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
        lastState = state;
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        float boostFactor = speedChangeFactor;
        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);
            time += Time.deltaTime * boostFactor;
            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
        speedChangeFactor = 1f;
        KeepMomentum = false;
    }

    private void MovePlayer()
    {
        if (state == MovementState.dashing) return;
        moveDirection = orientation.forward * moveInput.y + orientation.right * moveInput.x;
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);
            if (rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down * 40f, ForceMode.Force);
            }
        }
            
        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else if(!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * airMultiplier, ForceMode.Force);
        //if (!wallrunning)
        rb.useGravity = !OnSlope();
    }
    private void SpeedControl()
    {
        if (OnSlope()&&!exitingSlope)
        {
            if(rb.velocity.magnitude > moveSpeed)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        }
        else
        {
            Vector3 flatvel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            if (flatvel.magnitude > moveSpeed)
            {
                Vector3 limitedvel = flatvel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedvel.x, rb.velocity.y, limitedvel.z);
            }
        }
        if (maxYSpeed != 0 && rb.velocity.y > maxYSpeed)
            rb.velocity = new Vector3(rb.velocity.x, maxYSpeed, rb.velocity.z);
    }
    private void Jump()
    {
        exitingSlope = true;
        wallrun.exitingWall = true;
        wallrun.exitWallTimer = wallrun.exitWallTime;
        if (readyToJump && grounded)
        {
            readyToJump = false;
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

            Invoke(nameof(ResetJump), jumpCooldown);
        }
        else if (readyToJump && wallrunning)
        {
            readyToJump = false;

            // ����y���ٶ�
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            Vector3 wallNormal = wallrun.wallLeft ? wallrun.leftWallHit.normal : wallrun.rightWallHit.normal;
            Vector3 horizontalJumpDirection = wallNormal.normalized; // ȷ���ǵ�λ��������

            rb.AddForce(horizontalJumpDirection * wallJumpForce, ForceMode.Impulse);
            Debug.Log("��ǽ");
            Invoke(nameof(ApplyUpwardJumpForce), 0.1f);
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }
    private void ApplyUpwardJumpForce()
    {
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }
    private bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }
    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    } 
}
