using UnityEngine;
using System.Collections;

public class Enemy_BugEnemy_State : EnemyGeneric_StateFinder
{
    public enum MotionState
    {
        Falling,
        Idle,
        Running,
        Walking,
        RecoverFront,
        RecoverBack,
        Shunting,
        NotYetImplimented
    }

    public enum ControlState
    {
        Stop,
        HuntPlayer,
        LookForPlayer,
        HuntPower,
        Patrol
    }

    public enum SideLanded
    {
        Front,
        Back
    }

    public MotionState CurrentMotionState; //displays current motion related state
    public SideLanded CurrentSideLanded; //displays the current side resting on for ragdoll blending
    public ControlState CurrentControlState;//displays the current enemy control state

    public float SpeedToReachBeforeRunning;

    private Vector3 _velocity;        //current object velocity
    private Vector3 _velocityMinusY;  //current object velocity minus 'y' axis

    // Use this for initialization
    void Start()
    {
        //Initialize
        stateOnGroundEvent += ObjectOnGround;
        stateStartRecoverEvent += StartRecover;
        stateEndRecoverEvent += EndRecover;

        stateLostSightEvent += LostSight;
        stateTrackPlayerEvent += TrackPlayer;
    }

    //------------------------------------------------------
    // Motion State
    //------------------------------------------------------
    // Called When Object is on the ground
    void ObjectOnGround(Collider colInfo)                   //states available when on ground
    {
        if (CurrentMotionState == MotionState.RecoverFront) //don't update state if recovering
            return;
        if (CurrentMotionState == MotionState.RecoverBack) //
            return;
        if (CheckIfAbveSpeed(SpeedToReachBeforeRunning))  //set to running if going fast enough
        {
            CurrentMotionState = MotionState.Running;
        }
        else if (CurrentMotionState == MotionState.Running)
        {
            CurrentMotionState = MotionState.Idle;      //set to idle if not
        }
    }

    // Called when Recover Start
    void StartRecover()
    {
        if (CurrentSideLanded == SideLanded.Back)
        {
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


    //------------------------------------------------------


    //------------------------------------------------------
    // Control State
    //------------------------------------------------------
    void LostSight() //called when enemy loses sight of player
    {
        StartCoroutine(LostSightDelay());
    }
    IEnumerator LostSightDelay()
    {
        yield return new WaitForSeconds(4); //adds a two second delay before an enemy realizes that he can no longer see the player
        CurrentControlState = ControlState.LookForPlayer;
    }

    void TrackPlayer() //called when enemy sees player
    {
        CurrentControlState = ControlState.HuntPlayer;
    }
    //------------------------------------------------------


    public bool Stopped;
    // Update is called once per frame
    void Update()
    {

        if (CurrentControlState == ControlState.Stop)
            Stopped = true;
        else
            Stopped = false;



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
