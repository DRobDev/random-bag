using System;
using UnityEngine;
using System.Collections;

public class Player_Generic_StateFinder : MonoBehaviour
{


    public delegate void StateOnGroundDelegate(Collider colinfo);
    public StateOnGroundDelegate stateOnGroundEvent;

    public delegate void StateAssumePosition(float animationSamplePosition, bool ragdollIsOnFront);
    public StateAssumePosition stateAssumePositionEvent;

    public delegate void StateStartRecover();
    public StateStartRecover stateStartRecoverEvent;

    public delegate void StateEndRecover();
    public StateEndRecover stateEndRecoverEvent;

    public delegate void StateStartChairJump();
    public StateStartChairJump stateStartChairJumpEvent;






}
