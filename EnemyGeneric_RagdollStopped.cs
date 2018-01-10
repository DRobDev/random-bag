using UnityEngine;
using System.Collections;

public class EnemyGeneric_RagdollStopped : MonoBehaviour
{
    public float MinimumMovementAllowed = 5;
    public float CurrentMovement;

    // Use this for initialization
    void Start()
    {
        _timeDelay = Time.time;
    }

    private float _timeDelay;
    private bool _alreadyCalled = false;

    //Update is called once per frame
    void FixedUpdate()
    {


        if (Time.time - _timeDelay < 1)
            return;

        if (transform.root.gameObject.GetComponent<EnemyGeneric_BaseClass>() == null)
            return;

        if (rigidbody == null)
            return;

        CurrentMovement = rigidbody.velocity.magnitude;

        if (rigidbody.velocity.magnitude < MinimumMovementAllowed && !_alreadyCalled)                                   //if ragdoll moves less than specified
        {
            transform.root.gameObject.GetComponent<EnemyGeneric_BaseClass>().RagDollStoppedEvent();   //fire ragdoll stopped event
            //transform.root.gameObject.GetComponent<Player_Generic_BaseClass>().TestEvent();
            _alreadyCalled = true;
        }
    }
}