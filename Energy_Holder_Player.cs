using UnityEngine;
using System.Collections;

//----------------------------------------------
// Holds current collected energy;
// Constantly reduces energy amount
//----------------------------------------------

public class Energy_Holder_Player : MonoBehaviour
{
    public float EnergyDepleteRate; //how fast to deplete energy
    public float CurrentEnergy; //current amount of energy


    private bool _hasEnergy, _alreadyDepleting;
    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        _hasEnergy = CurrentEnergy > 0;

        if (_hasEnergy && !_alreadyDepleting)   //if has energy  
            StartCoroutine(DepleteEnergy());    //start depleting energy if not already doing so

        if (CurrentEnergy < 0) CurrentEnergy = 0;
    }

    IEnumerator DepleteEnergy()
    {
        _alreadyDepleting = true;                  //set 'depleting' to true
        while (_hasEnergy)                          
        {
            CurrentEnergy -= EnergyDepleteRate / 1000;  //reduce energy
            yield return new WaitForSeconds(.2f);       
        }
        _alreadyDepleting = false;                //when player has run out of energy, stop depleting
    }
}
