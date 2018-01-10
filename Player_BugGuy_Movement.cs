using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Player_BugGuy_State))]

//-----------------------------------------------------------
// Handles player controlled movement for specific player type
//   Also controls facing direction
//-----------------------------------------------------------

public class Player_BugGuy_Movement : Player_Generic_Movement
{
    //VARIABLES----------------------------------------------
    public GameObject MovementDirectionFinder; //game object that holds the current movement direction
    public GameObject WheelBoneLeft, WheelBoneRight;//rotating wheel objects

    public float IfXZVelocityChangesMoreThanThisAmountCrash = 6; //rapid changes in velocity causes character to 'crash' or 'swap to ragdoll'
    public float IfYVelocityChangesMoreThanThisAmountCrash = 10; //

    public float ExtraGravity = 0; //add downward force to tweak fall speed

    private bool _playerOnGround = false;
    private Vector3 _lastVelocity;// used to calculate velocity change
    //-------------------------------------------------------
    
    
    // Use this for initialization---------------------------
    void Start()
    {
        //subscribe to state events
        GetComponent<Player_BugGuy_State>().stateOnGroundEvent += IsOnGround;
    }
    //-------------------------------------------------------


    //Called when player is on ground------------------------
    void IsOnGround(Collider colInfo)
    {
        _playerOnGround = true;
    }
    //-------------------------------------------------------


    // Update is called once per frame-----------------------
    void Update()
    {
        if (CheckForVelocityChange(rigidbody, IfXZVelocityChangesMoreThanThisAmountCrash,
                                   IfYVelocityChangesMoreThanThisAmountCrash))
        {
            if (transform.parent == null)
                return;
            transform.parent.gameObject.GetComponent<Player_Generic_BaseClass>().CrashSwapRagdollEvent();
        }
    }
    //-------------------------------------------------------


    // Called 60 times a second------------------------------
    void FixedUpdate()
    {
        // rotate direction finder based on this objects current velocity
        RotateBasedOnVelocity(MovementDirectionFinder);
        // rotate this object smoothly towards direction finder
        SmoothRotateTowards(MovementDirectionFinder, MinimumMoveSpeedBeforeTurning, SmoothTurnAmmount);

        // add extra gravity
        if (!_playerOnGround)
        {
            AddMoreGravity(ExtraGravity);
            return;
        }

        // All code below only gets called when player is on ground
        //_____________________________________________________
        
        if (GetComponent<Player_BugGuy_State>().CurrentControlState == Player_BugGuy_State.ControlState.Disabled)
            return;

        // All code below only gets called when controls aren't disabled
        //_____________________________________________________

        // add force on 'WASD'
        if (Input.GetAxis("Horizontal") != 0)
            MoveAlongAxis("Horizontal", Speed);
        if (Input.GetAxis("Vertical") != 0)
            MoveAlongAxis("Vertical", Speed);

        //Animate Chair Wheels
        // spin wheels based on speed
        RotateBasedOnSpeed(WheelBoneLeft, Vector3.left, true);
        RotateBasedOnSpeed(WheelBoneRight);

        _playerOnGround = false; //gets turned back to true by event call
    }
    //-------------------------------------------------------


    // Rotate Function/--------------------------------------
    /// <summary>
    /// Rotate object based on speed
    /// </summary>
    /// <param name="objectToRotate">object you want to rotate</param>
    void RotateBasedOnSpeed(GameObject objectToRotate)
    {
        RotateBasedOnSpeed(objectToRotate, Vector3.left);
    }
    /// <summary>
    /// Rotate object based on speed
    /// </summary>
    /// <param name="objectToRotate">object you want to rotate</param>
    /// <param name="axis">Axis to rotate along, default 'x'</param>
    void RotateBasedOnSpeed(GameObject objectToRotate, Vector3 axis)
    {
        RotateBasedOnSpeed(objectToRotate, axis, false);
    }
    /// <summary>
    /// Rotate object based on speed
    /// </summary>
    /// <param name="objectToRotate">object you want to rotate</param>
    /// <param name="axis">Axis to rotate along, default 'x'</param>
    /// <param name="inverse">Inverse rotation</param>
    void RotateBasedOnSpeed(GameObject objectToRotate, Vector3 axis, bool inverse)
    {
        Vector3 velocityMinusY = rigidbody.velocity; //total velocity
        velocityMinusY.y = 0;                        //velocity minus 'y'
        float speed = velocityMinusY.magnitude;      //speed is magnitude of velocity
        if (inverse)
            objectToRotate.transform.Rotate(axis, -speed, Space.Self);// rotate inverse around local axis
        else
            objectToRotate.transform.Rotate(axis, speed, Space.Self);  // rotate around local axis

    }

    //-----------------------------------------------------------
}
