using UnityEngine;
using System.Collections;

//---------------------------------------------------------------------------------------------------
// This class is used to swap to different player states; (ragdoll or controllable object)
//---------------------------------------------------------------------------------------------------


public class Player_BugGuy_BasePlayer : Player_Generic_BaseClass
{
    public GameObject PrefabBugGuyOnChair, PrefabBugGuyNoChair, PrefabBugGuyRagdoll, PrefabBugGuyChairRagdoll; //player prefabs and ragdoll prefabs
    public GameObject SpawnPoint;


    private GameObject _bugGuyOnChair, _bugGuyNoChair, _bugGuyRagdoll, _bugGuyChairRagdoll; //holders for instances of prefabs


    // Use this for initialization----------------------------------------------------------------------
    void Start()
    {
        //create start character
        _bugGuyOnChair = CreatePlayer(PrefabBugGuyOnChair, SpawnPoint.transform.position);
        //subscribe to relative events
        GetComponent<Player_Generic_BaseClass>().CrashSwapRagdollEvent += CalledWhenCrash;
        GetComponent<Player_Generic_BaseClass>().RagDollStoppedEvent += CalledWhenRagdollStops;
        GetComponent<Player_Generic_BaseClass>().NearChairEvent += CalledWhenNearChair;
    }
    //---------------------------------------------------------------------------------------------------


    // Update is called once per frame; used for debugging-----------------------------------------------
    void Update()
    {
        TestCrashing(); //Uncomment this line if you want to use manual switching controls;(space = turn to ragdoll, R = turn to animation, C = Climb on chair)
    }
    //---------------------------------------------------------------------------------------------------


    //Called when player movement detects a crash--------------------------------------------------------
    private bool _alreadyCrashing = false;
    private void CalledWhenCrash()
    {
        if (_alreadyCrashing) return;
        StartCoroutine(CrashCallDelay());
        //print("CRASHCALL");
        if (!_bugGuyChairRagdoll)
        {
            _bugGuyRagdoll = SwapInRagdoll(_bugGuyOnChair, PrefabBugGuyRagdoll, PrefabBugGuyChairRagdoll,
                                           out _bugGuyChairRagdoll);
        }
        else
        {
            _bugGuyRagdoll = SwapInRagdoll(_bugGuyNoChair, PrefabBugGuyRagdoll);
        }
    }

    //fix; put a delay to get rid of multiple crash calls error
    IEnumerator CrashCallDelay()
    {
        _alreadyCrashing = true;
        yield return new WaitForSeconds(.2f);
        _alreadyCrashing = false;
    }
    //---------------------------------------------------------------------------------------------------


    //Called if Ragdoll Stops----------------------------------------------------------------------------
    private void CalledWhenRagdollStops()
    {
        //print("eh");
        //_bugGuyNoChair = SwapInAnimation(PrefabBugGuyNoChair, _bugGuyRagdoll, .5f);
        StartCoroutine(Delayed()); //Needs to be delayed so it doesn't kick-off a bunch of other errors.. god knows why
    }
    IEnumerator Delayed()
    {
        yield return new WaitForEndOfFrame();
        _bugGuyNoChair = SwapInAnimation(PrefabBugGuyNoChair, _bugGuyRagdoll, .5f);
    }
    //---------------------------------------------------------------------------------------------------


    //Called if NearChair--------------------------------------------------------------------------------
    private void CalledWhenNearChair()
    {

        //BUGFIX
        if(SwappingInAnimation)
        {
            StartCoroutine(DelayedGetOn());
            return;
        }

        //temp fix TODO Find the real problem
        //if(_bugGuyNoChair == null || _bugGuyChairRagdoll == null || _bugGuyNoChair == null) return;

        _bugGuyOnChair = GetOnChair(_bugGuyNoChair, _bugGuyChairRagdoll, _bugGuyNoChair.GetComponentInChildren<Animation>().GetClip("Player_BugGuy_NoChair_StartChairJump"), PrefabBugGuyOnChair);
    }

    IEnumerator DelayedGetOn()
    {
        while (SwappingInAnimation)
        {
            yield break;
        }
        CalledWhenNearChair();
    }
    //---------------------------------------------------------------------------------------------------





    //USED FOR TESTING ----------------------------------------------------------------------------------
    private void TestCrashing()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if (!_bugGuyChairRagdoll)
            {
                _bugGuyRagdoll = SwapInRagdoll(_bugGuyOnChair, PrefabBugGuyRagdoll, PrefabBugGuyChairRagdoll,
                                               out _bugGuyChairRagdoll);
            }
            else
            {
                _bugGuyRagdoll = SwapInRagdoll(_bugGuyNoChair, PrefabBugGuyRagdoll);
            }
        }
        if(Input.GetKeyDown(KeyCode.R))
        {
            _bugGuyNoChair = SwapInAnimation(PrefabBugGuyNoChair, _bugGuyRagdoll, .5f);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            _bugGuyOnChair = GetOnChair(_bugGuyNoChair, _bugGuyChairRagdoll, _bugGuyNoChair.GetComponentInChildren<Animation>().GetClip("Player_BugGuy_NoChair_StartChairJump"), PrefabBugGuyOnChair);
        }
    }
    //---------------------------------------------------------------------------------------------------
}
