using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WallRunning : MonoBehaviour
{
    [Header("Wallrunning")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce;
    public float maxWallRunTime;
    private float wallRunTimer;

    [Header("Input")]
    private Vector2 moveInput;

    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHieht;
    public RaycastHit leftWallHit;
    public RaycastHit rightWallHit;
    public bool wallLeft;
    public bool wallRight;

    [Header("Exiting")]
    public bool exitingWall;
    public float exitWallTime;
    public float exitWallTimer;

    [Header("Gravity")]
    public bool useGravity;
    public float gravityCounterForce;

    [Header("References")]
    public Transform orientation;
    private PlayerMovement pm;
    private Rigidbody rb;
    private PlayerInput playerInput;
    public PlayerCam cam;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
        playerInput = GetComponent<PlayerInput>();
        // ×¢²áMove actionµÄ»Øµ÷
        playerInput.actions["Move"].performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerInput.actions["Move"].canceled += ctx => moveInput = Vector2.zero;
    }
    private void Update()
    {
        CheckForWall();
        StateMachine();
    }
    private void FixedUpdate()
    {
        if (pm.wallrunning)
            WallRunningMovement();
    }
    public void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, whatIsWall);
    }
    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHieht, whatIsGround);
    }
    private void StateMachine()
    {
        float horizontalInput = moveInput.x;
        float verticalInput = moveInput.y;
        // wallrunning state
        if ((wallLeft || wallRight) && verticalInput>0 && AboveGround() && !exitingWall)
        {
            if (!pm.wallrunning)
                StartWallRun();
            if (wallRunTimer > 0)
                wallRunTimer -= Time.deltaTime;
            if (wallRunTimer <= 0 && pm.wallrunning)
            {
                exitingWall = true;
                exitWallTimer = exitWallTime;
            }
        }
        else if (exitingWall)
        {
            if (pm.wallrunning)
                StopWallRun();
            if (exitWallTimer > 0)
                exitWallTimer -= Time.deltaTime;
            if (exitWallTimer <= 0)
                exitingWall = false;
        }
        else
        {
            if (pm.wallrunning)
                StopWallRun();
        } 
    }
    private void StartWallRun()
    {
        pm.wallrunning = true;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        wallRunTimer = maxWallRunTime;
        if (wallLeft) cam.DoTilt(-5f);
        if (wallRight) cam.DoTilt(5f);
    }
    private void WallRunningMovement()
    {
        rb.useGravity = useGravity;

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);
        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        if(!(wallLeft&& moveInput.x>0) && !(wallRight && moveInput.x <0))
            rb.AddForce(-wallNormal * 10, ForceMode.Force);

        if (useGravity)
            rb.AddForce(transform.up * gravityCounterForce, ForceMode.Force);
    }
    private void StopWallRun()
    {
        pm.wallrunning = false;
        cam.DoTilt(0f);
    }
}
