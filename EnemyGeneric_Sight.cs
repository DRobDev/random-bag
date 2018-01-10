using UnityEngine;
using System.Collections;

public class EnemyGeneric_Sight : MonoBehaviour
{
    //if player enters 'sight zone'
      //look for player
        //if player is visible
          //fire 'track player' event
        //else
          //keep looking
    //if player exits sight zone
      //fire 'lost sight' event

    private EnemyGeneric_StateFinder _genState; // holds state finder component of parent object
    public bool _lookingForPlayer; //true when looking for player

    // Use this for initialization
    void Start()
    {
        //initialize _genstate
        _genState = transform.parent.gameObject.GetComponent<EnemyGeneric_StateFinder>();

        StartCoroutine(ScaleSightZone());
    }
    

    // Update is called once per frame

    IEnumerator ScaleSightZone()
    {
        Vector3 origScale = transform.localScale;
        while (true)
        {
            yield return new WaitForSeconds(1);
            if(!_playerEntered) continue;
            

            transform.localScale = new Vector3(.1f,.1f,.1f);
            yield return new WaitForSeconds(.2f);
            transform.localScale = origScale;
        }
        
    }

    public bool _playerEntered = false;
    void OnTriggerEnter(Collider colInfo)
    {
        if (colInfo.gameObject.tag == "Player" && !_playerEntered)
        {
            //print("Hi player");
            if (_genState == null) return;

            _playerEntered = true;
            _lookingForPlayer = true;
            StartCoroutine(LookForPlayer(colInfo));
        }
    }

    void OnTriggerExit(Collider colInfo)
    {
        if (colInfo.gameObject.tag == "Player" && _playerEntered)
        {
            //print("Bye player");
            if (_genState == null) return;
            _genState.stateLostSightEvent(); //fire lost sight event
            _playerEntered = false;
            _lookingForPlayer = false;
        }
    }


    IEnumerator LookForPlayer(Collider colInfo)
    {
        RaycastHit[] hitInfo;

        // loop runs whilst player is inside sight zone
        while (_lookingForPlayer)
        {

            //Debug.DrawRay(transform.parent.position + new Vector3(0, 1, 0),
            //              colInfo.gameObject.transform.position - transform.parent.position);
            if(colInfo == null)
            {
                _lookingForPlayer = false;
                break;
            }

            //raycast from this position towards player
            hitInfo = Physics.RaycastAll(transform.parent.position + new Vector3(0, 1, 0), colInfo.gameObject.transform.position - transform.parent.position);

            //check if player is behind wall--------------------
            float playerdistance = 10000;
            float closestobjdistane = 10000;
            //if player hit track player
            foreach (RaycastHit hit in hitInfo)
            {
                if (hit.collider.gameObject.tag == "Enemy") //ignore hits tagged 'enemy'
                    continue;


                if (hit.collider.gameObject.tag == "Player")//record player distance when hit
                {
                    playerdistance = hit.distance;
                    continue;
                }

                if (hit.distance < closestobjdistane)       //record closest object distance
                    closestobjdistane = hit.distance;
            }

            if(playerdistance < closestobjdistane)         //if closest object isn't closer than player
            {
                                //print("gotcha");
                _genState.stateTrackPlayerEvent();        //track player
                _lookingForPlayer = false;
            }
            //-------------------------------------------------

            yield return new WaitForSeconds(.2f);
        }

    }

}
