using UnityEngine;
using System.Collections;

public class Player_NearChair : MonoBehaviour
{

    public float DetectionDelay = 7;
    private float _startTime;
    public bool _alreadyCalled = false;
    void Start()
    {
        _startTime = Time.time;
    }


    void OnTriggerEnter(Collider colInfo)
    {
        if(_alreadyCalled)
            return;

        if (Time.time - _startTime < DetectionDelay)
            return;

        if (colInfo.gameObject.tag == "Player")                                         // if object with tag 'Player' comes near to chair
        {
            if (transform.root.gameObject.GetComponent<Player_Generic_BaseClass>() != null) // and if it's a child of the player controller root object
            {
                //print("FiredNearChair");
                transform.root.gameObject.GetComponent<Player_Generic_BaseClass>().NearChairEvent();// fire near chair event

                _alreadyCalled = true;
                StartCoroutine(UnCall());
            }
        }
    }

    //bug: once in a while player no longer seems able to jump on chair.. having a hard time recreating it here's a temp patch
    IEnumerator UnCall()
    {
        yield return new WaitForSeconds(5);                 //after five seconds player should already be back on chair and this class should no longer exist
        Debug.LogWarning("Something is wrong here");        // if it does then something is wrong
        _alreadyCalled = false;                             //set already called back to false so player can call the chair jump again; TODO find real problem
    }
}
