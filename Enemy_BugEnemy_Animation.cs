using UnityEngine;
using System.Collections;
[RequireComponent(typeof(Enemy_BugEnemy_State))]
public class Enemy_BugEnemy_Animation : MonoBehaviour
{

    public GameObject AnimationRootObject;
    public AnimationClip Idle, Run, RecoverFront, BackRecover;

    private Enemy_BugEnemy_State.MotionState _currentMotionState;
    private Animation _animation;

    private bool alreadyPlayed = false;

    void Start()
    {
        GetComponent<Enemy_BugEnemy_State>().stateAssumePositionEvent += AssumeAnimationPosition;

        AnimationRootObject.AddComponent<Animation>();
        _animation = AnimationRootObject.animation;
        _animation.AddClip(Idle, Idle.name);
        _animation.AddClip(Run, Run.name);
        _animation.AddClip(RecoverFront, RecoverFront.name);
        _animation.AddClip(BackRecover, BackRecover.name);

        _animation.Play(Idle.name);


        _animation.playAutomatically = true;
    }

    void Update()
    {
        _currentMotionState = GetComponent<Enemy_BugEnemy_State>().CurrentMotionState;

        if (_currentMotionState == Enemy_BugEnemy_State.MotionState.Idle)
        {
            _animation.CrossFade(Idle.name);
        }
        if (_currentMotionState == Enemy_BugEnemy_State.MotionState.Running)
        {
            _animation.CrossFade(Run.name);
        }
        if (_currentMotionState == Enemy_BugEnemy_State.MotionState.NotYetImplimented)
        {
            _animation.Stop();
        }




        if (_currentMotionState == Enemy_BugEnemy_State.MotionState.RecoverFront)
        {
            if (!alreadyPlayed)
            {
                GetComponent<Enemy_BugEnemy_State>().CurrentControlState = Enemy_BugEnemy_State.ControlState.Stop;
                _animation.Play(RecoverFront.name);
                alreadyPlayed = true;
                StartCoroutine(ChangeStateAfterAnimation(RecoverFront, Enemy_BugEnemy_State.MotionState.Idle)); //change state after animation played
            }
        }
        if (_currentMotionState == Enemy_BugEnemy_State.MotionState.RecoverBack)
        {
            //print("BackAnimCalled");
            if (!alreadyPlayed)
            {
                GetComponent<Enemy_BugEnemy_State>().CurrentControlState = Enemy_BugEnemy_State.ControlState.Stop;
                _animation.Play(BackRecover.name);
                alreadyPlayed = true;
                StartCoroutine(ChangeStateAfterAnimation(BackRecover, Enemy_BugEnemy_State.MotionState.Idle)); //change state after animation played
            }
        }

    }

    IEnumerator ChangeStateAfterAnimation(AnimationClip clip, Enemy_BugEnemy_State.MotionState desiredMotionState)
    {
        yield return new WaitForSeconds(clip.length);
        GetComponent<Enemy_BugEnemy_State>().CurrentMotionState = desiredMotionState;
        GetComponent<Enemy_BugEnemy_State>().CurrentControlState = Enemy_BugEnemy_State.ControlState.Patrol;
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


