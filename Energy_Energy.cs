using UnityEngine;
using System.Collections;

//---------------------------------------------------------
// Attached to energy objects, this script increments player or enemy holders by one
//---------------------------------------------------------

public class Energy_Energy : MonoBehaviour
{
    private bool alreadyCollected;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider colInfo)
    {
        if(alreadyCollected)
        {
            Destroy(gameObject);
            return;
        }

        if(colInfo.isTrigger) return;

        if (colInfo.gameObject.transform.root.GetComponent<Energy_Holder_Player>())
        {
            colInfo.gameObject.transform.root.GetComponent<Energy_Holder_Player>().CurrentEnergy++;
            alreadyCollected = true;
            Destroy(gameObject);
        }

        if (colInfo.gameObject.transform.root.GetComponent<Energy_Holder_Enemy>())
        {
            colInfo.gameObject.transform.root.GetComponent<Energy_Holder_Enemy>().CurrentEnergy++;
            alreadyCollected = true;
            Destroy(gameObject);
        }

    }
}
