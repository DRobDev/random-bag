using UnityEngine;
using System.Collections;
[RequireComponent(typeof(Player_BugGuy_State))]
public class Player_BugGuy_noChair_Animation : MonoBehaviour
{

    public GameObject AnimationRootObject;
    public AnimationClip Idle, Run, RecoverFront, BackRecover, StartChairRecover;

    private Player_BugGuy_State.MotionState _currentMotionState;
    private Animation _animation;

    private bool alreadyPlayed = false;

    void Start()
    {
        GetComponent<Player_BugGuy_State>().stateAssumePositionEvent += AssumeAnimationPosition;

        AnimationRootObject.AddComponent<Animation>();
        _animation = AnimationRootObject.animation;
        _animation.AddClip(Idle, Idle.name);
        _animation.AddClip(Run, Run.name);
        _animation.AddClip(RecoverFront, RecoverFront.name);
        _animation.AddClip(BackRecover, BackRecover.name);
        StartChairRecover.wrapMode = WrapMode.ClampForever;
        _animation.AddClip(StartChairRecover, StartChairRecover.name);


        _animation.Play(Idle.name);


        _animation.playAutomatically = true;
    }

    void Update()
    {
        _currentMotionState = GetComponent<Player_BugGuy_State>().CurrentMotionState;

        if (_currentMotionState == Player_BugGuy_State.MotionState.Idle)
        {
            _animation.CrossFade(Idle.name);
        }
        if (_currentMotionState == Player_BugGuy_State.MotionState.Running)
        {
            _animation.CrossFade(Run.name);
        }
        if (_currentMotionState == Player_BugGuy_State.MotionState.NotYetImplimented)
        {
            _animation.Stop();
        }
        if (_currentMotionState == Player_BugGuy_State.MotionState.JumpingOnChair)
        {
            _animation.CrossFade(StartChairRecover.name);
        }




        if (_currentMotionState == Player_BugGuy_State.MotionState.RecoverFront)
        {
            if (!alreadyPlayed)
            {
                GetComponent<Player_BugGuy_State>().CurrentControlState = Player_BugGuy_State.ControlState.Disabled;
                _animation.Play(RecoverFront.name);
                alreadyPlayed = true;
                StartCoroutine(ChangeStateAfterAnimation(RecoverFront, Player_BugGuy_State.MotionState.Idle)); //change state after animation played
            }
        }
        if (_currentMotionState == Player_BugGuy_State.MotionState.RecoverBack)
        {
            //print("BackAnimCalled");
            if (!alreadyPlayed)
            {
                GetComponent<Player_BugGuy_State>().CurrentControlState = Player_BugGuy_State.ControlState.Disabled;
                _animation.Play(BackRecover.name);
                alreadyPlayed = true;
                StartCoroutine(ChangeStateAfterAnimation(BackRecover, Player_BugGuy_State.MotionState.Idle)); //change state after animation played
            }
        }

    }

    IEnumerator ChangeStateAfterAnimation(AnimationClip clip, Player_BugGuy_State.MotionState desiredMotionState)
    {
        yield return new WaitForSeconds(clip.length);
        GetComponent<Player_BugGuy_State>().CurrentMotionState = desiredMotionState;
        GetComponent<Player_BugGuy_State>().CurrentControlState = Player_BugGuy_State.ControlState.Enabled;
    }

    void AssumeAnimationPosition(float animationSamplePosition, bool ragdollOnFront)
    {
        _animation.Stop();
        //print("assumePosition");
        if (!ragdollOnFront)
        {
            //print("Back");
            _animation.gameObject.SampleAnimation(BackRecover, animationSamplePosition);
        }
        else
        {
            _animation.gameObject.SampleAnimation(RecoverFront, animationSamplePosition);
            //print("Front");
        }
        
    }
}


