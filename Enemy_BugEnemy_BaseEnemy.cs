using UnityEngine;
using System.Collections;

//---------------------------------------------------------------------------------------------------
// This class is used to swap to different player states; (ragdoll or controllable object)
//---------------------------------------------------------------------------------------------------


public class Enemy_BugEnemy_BaseEnemy : EnemyGeneric_BaseClass
{
    public GameObject PrefabEnemy_BugEnemy, PrefabEnemyBugEnemyRagdoll; //player prefabs and ragdoll prefabs
    public GameObject SpawnPoint;


    private GameObject _enemy_BugEnemy, _enemyBugEnemyRagRagdoll; //holders for instances of prefabs


    // Use this for initialization----------------------------------------------------------------------
    void Start()
    {
        //if spawn point hasn't been set; use this objects position as spwan point
        if (SpawnPoint == null) SpawnPoint = gameObject;


        //create start character
        _enemy_BugEnemy = CreateEnemy(PrefabEnemy_BugEnemy, SpawnPoint.transform.position);
        //subscribe to relative events
        GetComponent<EnemyGeneric_BaseClass>().CrashSwapRagdollEvent += CalledWhenCrash;
        GetComponent<EnemyGeneric_BaseClass>().RagDollStoppedEvent += CalledWhenRagdollStops;
    }
    //---------------------------------------------------------------------------------------------------


    // Update is called once per frame; used for debugging-----------------------------------------------
    void Update()
    {
        TestCrashing(); //Uncomment this line if you want to use manual switching controls;(space = turn to ragdoll, R = turn to animation, C = Climb on chair)
    }
    //---------------------------------------------------------------------------------------------------


    //Called when player movement detects a crash--------------------------------------------------------
    private void CalledWhenCrash()
    {
            _enemyBugEnemyRagRagdoll = SwapInRagdoll(_enemy_BugEnemy, PrefabEnemyBugEnemyRagdoll);
    }
    //---------------------------------------------------------------------------------------------------


    //Called if Ragdoll Stops----------------------------------------------------------------------------
    private void CalledWhenRagdollStops()
    {
        StartCoroutine(Delayed()); //Needs to be delayed so it doesn't kick-off a bunch of other errors.. god knows why
    }
    IEnumerator Delayed()
    {
        yield return new WaitForEndOfFrame();
        _enemy_BugEnemy = SwapInAnimation(PrefabEnemy_BugEnemy, _enemyBugEnemyRagRagdoll, .5f);
    }
    //---------------------------------------------------------------------------------------------------



    //USED FOR TESTING ----------------------------------------------------------------------------------
    private void TestCrashing()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
                _enemyBugEnemyRagRagdoll = SwapInRagdoll(_enemy_BugEnemy, PrefabEnemyBugEnemyRagdoll);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            _enemy_BugEnemy = SwapInAnimation(PrefabEnemy_BugEnemy, _enemyBugEnemyRagRagdoll, .5f);
        }
    }
    //---------------------------------------------------------------------------------------------------
}
