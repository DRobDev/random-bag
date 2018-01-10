using UnityEngine;
using System.Collections;

//-----------------------------------------------------------------
// This scrip turns a colliding player into a ragdoll then adds force to ragdoll and all nearby rigid bodies
//-----------------------------------------------------------------

public class EnemyGeneric_ShuntPlayer : MonoBehaviour
{

    public GameObject ActivePlayerType; //prefab of controllable player; must be child of base player
    public GameObject ActivePlayerHolder; //holder for the current instance of the in game playable character

    public float ExplosionRadious;      //radius of effect
    public float ExplosionForce;        //amount of force applied to rigidbodies



    // Use this for initialization------------------------------------
    void Start()
    {
        StartCoroutine(CallEverySecond()); //starts checks to be made every second
    }
    //-----------------------------------------------------------------


    // Called when trigger-zone entered--------------------------------
    void OnTriggerEnter(Collider colInfo)
    {

        if (ActivePlayerHolder == null) return; //end if no active player

        if(colInfo.gameObject.GetInstanceID() == ActivePlayerHolder.GetInstanceID())                                //if the active player enters the trigger zone
        {
            if (ActivePlayerHolder.transform.parent.gameObject.GetComponent<Player_Generic_BaseClass>() != null)    // 
            {
                //print("CALLED");
                ActivePlayerHolder.transform.parent.gameObject.GetComponent<Player_Generic_BaseClass>().            //fire crash event in the player generic class; causing the player to turn to a ragdoll
                    CrashSwapRagdollEvent();                                                                        //
                ActivePlayerHolder = null;//set active player to null so as not to be called again

                StartCoroutine(AddExplosiveForceToNearbyRigidBodies());                                             //add explosive force to all nearby rigidbodies
            }
            else
                Debug.LogWarning("ActivePlayerType must be a child of basePlayer");
        }
    }
    //-----------------------------------------------------------------

    
    //Adds force to nearby rigid bodies--------------------------------
    private IEnumerator AddExplosiveForceToNearbyRigidBodies()
    {
        yield return new WaitForSeconds(.2f);                                                                               //wait a while for player to be switched to ragdoll

        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, ExplosionRadious);                           //get all colliders near game object

        foreach (Collider nearbyCollider in nearbyColliders)                                                                //for each collider
        {
            if (nearbyCollider.gameObject.rigidbody == null) continue;                                                      //with a rigid body

            if (nearbyCollider.gameObject.tag == "Player")                                                                  //if tagged player
                nearbyCollider.gameObject.rigidbody.AddExplosionForce(ExplosionForce / 8, transform.position, ExplosionRadious);//reduce the amount of force; (this is done to balance the weight; if left undevided the player ragdoll gets too much collective force)
            else                                                                                                            //else
                nearbyCollider.gameObject.rigidbody.AddExplosionForce(ExplosionForce, transform.position, ExplosionRadious);//add full force to rigid body
        }
        yield return 0;
    }
    //-----------------------------------------------------------------


    //Check that using the active player-------------------------------
    IEnumerator CallEverySecond()
    {
        while (true) //infinite loop
        {                                                                                           //-------------------------------
            if(ActivePlayerHolder == null)                                                          //if the current active player has been destroyed
            {                                                                                       
                //print(ActivePlayerType.name + "(Clone)");                                           
                if (GameObject.Find(ActivePlayerType.name + "(Clone)") != null)                     //look for a clone of the active player type
                {                                                                                   
                    ActivePlayerHolder = GameObject.Find(ActivePlayerType.name + "(Clone)");        //if clone found; set it as the current active player
                }
                else
                {
                    ActivePlayerHolder = null;                                                      //else; set active player to null; so it doesn't try to access a destroyed instance 
                }                                                                                   //-------------------------------
            }
            yield return new WaitForSeconds(1);
        }
    }
}
