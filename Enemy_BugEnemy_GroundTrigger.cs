using UnityEngine;
using System.Collections;

/// <summary>
/// Fires event every frame when on ground
/// </summary>

public class Enemy_BugEnemy_GroundTrigger : MonoBehaviour
{
    public GameObject ParentObject; //Parent object with State class

    public bool IsOnGround; //true if player on ground


    private Enemy_BugEnemy_State _enemyState; //holds delegate
    private Collider _colInfoHolder; //stores recently collided objects


    private Collider[] _collidersToIgnore; //colliders to ignore when calculating collision

    void Start()
    {
        // Initialize
        _enemyState = ParentObject.GetComponent<Enemy_BugEnemy_State>();

        // create list of colliders contained in this game object; in order to ignore self collisions
        if (ParentObject)
        {
            _collidersToIgnore = ParentObject.GetComponentsInChildren<Collider>();
        }
    }



    void FixedUpdate()
    {
        //Fire 'OnGround' State event when on ground
        if (IsOnGround)
        {
            _enemyState.stateOnGroundEvent(_colInfoHolder);
        }
    }


    // Fire StateOnGround event when object is on ground
    void OnTriggerEnter(Collider colinfo)
    {
        if (colinfo.isTrigger)
            return;

        foreach (Collider thisCollider in _collidersToIgnore)
        {
            if (thisCollider == colinfo)
                return;
        }

        _colInfoHolder = colinfo;
        IsOnGround = true;
    }
    void OnTriggerStay(Collider colinfo)
    {
        foreach (Collider thisCollider in _collidersToIgnore)
        {
            if (thisCollider == colinfo)
                return;
        }
        IsOnGround = true;
        _colInfoHolder = colinfo;
    }
    void OnTriggerExit(Collider colinfo)
    {
        foreach (Collider thisCollider in _collidersToIgnore)
        {
            if (thisCollider == colinfo)
                return;
        }

        IsOnGround = false;
    }

}
