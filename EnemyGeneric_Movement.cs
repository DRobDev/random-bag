using UnityEngine;
using System.Collections;
[RequireComponent(typeof(Seeker))]
[RequireComponent(typeof(Rigidbody))]

// A collection of move related functions
    //to be inherited from by the code that controls the actual movement


public class EnemyGeneric_Movement : MonoBehaviour
{

    //-----------------------------------------------------------------------------
    //MOVEMENT
    //-----------------------------------------------------------------------------    
    public float Speed = 1000;
    public float SpeedCap = 1000;
    //-----------------------------------------------------------------------------

    //-----------------------------------------------------------------------------
    // ROTATION
    //-----------------------------------------------------------------------------
    public float MinimumMoveSpeedBeforeTurning = 1;
    public float SmoothTurnAmmount = 4;
    /// <summary>
    /// Rotate object to face direction based on movement
    /// </summary>
    /// <param name="objectToRotate">object to rotate</param>
    public void RotateBasedOnVelocity(GameObject objectToRotate)
    {
        Vector3 velocityBasedDirection = Vector3.Normalize(new Vector3(rigidbody.velocity.x, 0, rigidbody.velocity.z)); // get direction based on velocity zeroing-out the 'y' axis
        objectToRotate.transform.LookAt(transform.position + velocityBasedDirection); // use 'LookAt' to convert the normal into a Quaternion rotation
    }
    //-----------------------------------------------------------------------------


    //-----------------------------------------------------------------------------

    //-----------------------------------------------------------------------------
    /// <summary>
    /// Smooth rotate towards target object rotation using default values
    /// </summary>
    /// <param name="rotateTowardsObject">game-object holding desired rotation</param>
    public void SmoothRotateTowards(GameObject rotateTowardsObject)
    {
        SmoothRotateTowards(rotateTowardsObject, .5f, 4);
    }
    /// <summary>
    /// Smooth rotate towards target object rotation
    /// </summary>
    /// <param name="rotateTowardsObject">game-object holding desired rotation</param>
    /// <param name="minimumMovementSpeedBeforeUpdatingDirection">minimum velocity before updating; default 1</param>
    /// <param name="smoothTurning">amount of smoothing to apply to rotation; default 4</param>
    public void SmoothRotateTowards(GameObject rotateTowardsObject, float minimumMovementSpeedBeforeUpdatingDirection, float smoothTurning)
    {
        Vector3 velocityMinusY = rigidbody.velocity;
        Quaternion desiredRotationHolder = new Quaternion();
        velocityMinusY.y = 0;

        // if update only if target is moving past a defined speed
        //if (ParentRigidBody.rigidbody.velocity.magnitude > MinimumVelocity)
        if (velocityMinusY.magnitude > minimumMovementSpeedBeforeUpdatingDirection)
            desiredRotationHolder = rotateTowardsObject.transform.rotation;

        // smooth rotate towards target
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotationHolder,
                                              Time.deltaTime * smoothTurning);
    }
    //-----------------------------------------------------------------------------


    //-----------------------------------------------------------------------------
    // Check velocity change
    //-----------------------------------------------------------------------------
    private Vector3 _lastCheckedVelocity = Vector3.zero;
    public bool CheckForVelocityChange(Rigidbody rigidBodyUsed, float xzTollerence, float yTollerance)
    {
        //initialize first check
        if (_lastCheckedVelocity == Vector3.zero)
        {
            _lastCheckedVelocity = rigidBodyUsed.velocity;
            return false;
        }

        // Create four vector3s to store velocity
        Vector3 velocityMinusY = rigidBodyUsed.velocity; //one without the 'y' axis
        Vector3 velocityYOnly = velocityMinusY;          //and one with only the 'y'
        Vector3 oldVelocityMinusy = _lastCheckedVelocity;//hold last checked velocity
        Vector3 oldVelocityYOnly = _lastCheckedVelocity;//
        oldVelocityMinusy.y = 0;
        oldVelocityYOnly.x = 0;
        oldVelocityYOnly.z = 0;
        velocityMinusY.y = 0;
        velocityYOnly.x = 0;
        velocityYOnly.z = 0;
        _lastCheckedVelocity = rigidBodyUsed.velocity; //update last checked for next call

        if (Mathf.Abs(Mathf.Abs(velocityMinusY.magnitude) - Mathf.Abs(oldVelocityMinusy.magnitude)) > xzTollerence ||
            Mathf.Abs(Mathf.Abs(velocityYOnly.magnitude) - Mathf.Abs(oldVelocityYOnly.magnitude)) > yTollerance)
        {
            //print("difference" + Mathf.Abs(Mathf.Abs(velocityMinusY.magnitude) - Mathf.Abs(oldVelocityMinusy.magnitude)));
            return true;
        }

        return false;

    }
    //-----------------------------------------------------------------------------

    //adds downward force----------------------------------------------------------
    public void AddMoreGravity(float amount)
    {
        rigidbody.AddForce(Vector3.down * amount);
    }
    //-----------------------------------------------------------------------------
}
