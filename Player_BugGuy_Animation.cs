using UnityEngine;
using System.Collections;
[RequireComponent(typeof(Player_BugGuy_State))]

public class Player_BugGuy_Animation : MonoBehaviour
{
    public GameObject AnimationRootObject;
    public AnimationClip Idle, Run, ChairJump;




    private Player_BugGuy_State.MotionState _currentMotionState;
    private Animation _animation;

    private bool alreadyPlayed = false;




    void Start()
    {
        AnimationRootObject.AddComponent<Animation>();
        _animation = AnimationRootObject.animation;
        _animation.AddClip(Idle, Idle.name);
        _animation.AddClip(Run, Run.name);
        _animation.AddClip(ChairJump, ChairJump.name);

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
        if (_currentMotionState == Player_BugGuy_State.MotionState.JumpingOnChair)
        {
            if(!alreadyPlayed)
            {
                _animation.Rewind(ChairJump.name);
                _animation.Play(ChairJump.name);
                alreadyPlayed = true;
                StartCoroutine(SwitchStateAfterAnimationLength(ChairJump));
            }
            

            //GetComponent<Player_BugGuy_State>().CurrentMotionState = Player_BugGuy_State.MotionState.Idle;

        }
        if (_currentMotionState == Player_BugGuy_State.MotionState.NotYetImplimented)
        {
            _animation.Stop();
        }

    }

    IEnumerator SwitchStateAfterAnimationLength(AnimationClip animationClip)
    {
        yield return new WaitForSeconds(animationClip.length);
        //print("switch");
        GetComponent<Player_BugGuy_State>().CurrentMotionState = Player_BugGuy_State.MotionState.Idle;
        GetComponent<Player_BugGuy_State>().CurrentControlState = Player_BugGuy_State.ControlState.Enabled;
    }
}
