using UnityEngine;
using System.Collections;

public class Player_BugGuy_State : Player_Generic_StateFinder
{
    public enum MotionState
    {
        Falling,
        Idle,
        Running,
        Walking,
        RecoverFront,
        RecoverBack,
        AssumePosition,
        JumpingOnChair,
        NotYetImplimented
    }

    public enum ControlState
    {
        Enabled,
        Disabled
    }

    public enum SideLanded
    {
        Front,
        Back
    }

    public MotionState CurrentMotionState; //displays current motion related state
    public ControlState CurrentControlState; //displays current control state; used to enable or disable controls
    public SideLanded CurrentSideLanded; //displays the current side resting on for ragdoll blending

    public float SpeedToReachBeforeRunning;

    private Vector3 _velocity;        //current object velocity
    private Vector3 _velocityMinusY;  //current object velocity minus 'y' axis

    // Use this for initialization
    void Start()
    {
        //Initialize
        CurrentControlState = ControlState.Enabled;
        stateOnGroundEvent += ObjectOnGround;
        stateStartRecoverEvent += StartRecover;
        stateEndRecoverEvent += EndRecover;
        stateStartChairJumpEvent += StartChairJump;

    }


    // Called When Object is on the ground
    void ObjectOnGround(Collider colInfo)
    {
        if(CurrentMotionState == MotionState.RecoverFront) //don't update state if recovering
            return;
        if(CurrentMotionState == MotionState.RecoverBack) //
            return;
        if(CurrentMotionState == MotionState.JumpingOnChair) //don't update state if jumping on chair
            return;
        if (CheckIfAbveSpeed(SpeedToReachBeforeRunning))
        {
            CurrentMotionState = MotionState.Running;
        }
        else if (CurrentMotionState == MotionState.Running)
        {
            CurrentMotionState = MotionState.Idle;
        }
    }

    // AssumePosition
    void AssumeAnimationPosition()
    {
        
    }

    // Called when Recover Start
    void StartRecover()
    {
        if (CurrentSideLanded == SideLanded.Back)
        {
            //print("Backddddd");
            CurrentMotionState = MotionState.RecoverBack;
        }
        else
        {
            CurrentMotionState = MotionState.RecoverFront;
        }

    }
    // Called when Recover End
    void EndRecover()
    {
        CurrentMotionState = MotionState.Idle;
    }

    void StartChairJump()
    {
        CurrentMotionState = MotionState.JumpingOnChair;
    }

    // Update is called once per frame
    void Update()
    {
        _velocity = rigidbody.velocity;
        _velocityMinusY = new Vector3(_velocity.x, 0, _velocity.z);
    }

    private bool CheckIfAbveSpeed(float speed)
    {
        if (_velocityMinusY.magnitude > speed)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
