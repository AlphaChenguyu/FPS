using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapping : MonoBehaviour
{
    [Header("References")]
    private PlayerMovement pm;
    public Transform cam;
    private Transform gunTrip;
    public LayerMask whatIsGrappleable;
    [Header("Grappling")]
    public float maxGrappleDistance;
    public float grappleDelayTime;

    private Vector3 grapplePoint;

    [Header("CoolDown")]
    public float grapplingCd;
    public float grapplingCdTimer;

    private void Start()
    {
        pm = GetComponent<PlayerMovement>();
 
    }
    private void StartGrapple()
    {

    }
    private void ExecuteGrapple()
    {
        
    }
    private void StopGrapple()
    {
        
    }
}
