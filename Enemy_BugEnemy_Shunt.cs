using UnityEngine;
using System.Collections;

//-----------------------------------------------------------------
// This scrip turns a colliding player into a ragdoll then adds force to ragdoll and all nearby rigid bodies
//-----------------------------------------------------------------

public class Enemy_BugEnemy_Shunt : MonoBehaviour
{
    public float ExplosionRadious;      //radius of effect
    public float ExplosionForce;        //amount of force applied to rigidbodies


    public GameObject ActivePlayerHolder;//holder for active player
    // Use this for initialization------------------------------------
    void Start()
    {
    }
    //-----------------------------------------------------------------

    // CalledEveryFrame
    void Update()
    {
        ActivePlayerHolder = GetComponent<Enemy_BugEnemy_Movement>().CurrentActivePlayerHolder;
    }

    // Called when trigger-zone entered--------------------------------
    void OnCollisionEnter(Collision colInfo)
    {
        if (ActivePlayerHolder == null) return; //end if no active player

        if (colInfo.gameObject.GetInstanceID() == ActivePlayerHolder.GetInstanceID())                                //if the active player enters the trigger zone
        {
            if (ActivePlayerHolder.transform.parent.gameObject.GetComponent<Player_Generic_BaseClass>() != null)    // 
            {
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

}
