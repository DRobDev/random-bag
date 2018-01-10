using UnityEngine;
using System.Collections;

public class Player_Generic_Animation : MonoBehaviour
{
    public Player_Generic_Animation()
    {
        InitializeAnimations();
    }


    public delegate void AnimationRun();
    public AnimationRun AnimRun = delegate { };

    public delegate void AnimationIdle();
    public AnimationIdle AnimIdle = delegate { };

   public void InitializeAnimations()
    {
        AnimRun = new AnimationRun(RunAnimation);
        AnimIdle = new AnimationIdle(IdleAnimation);
    }

    private void RunAnimation()
    {
        print("Running");
    }
    private void IdleAnimation()
    {
        print("idling");
    }
    
    //------------------------------------------------------------------------
    // GET MOVEMENT STATE
    //------------------------------------------------------------------------

    public void GetMovementState()
    {
    }
    //------------------------------------------------------------------------

}
