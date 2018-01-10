using UnityEngine;
using System.Collections;
[RequireComponent(typeof(Seeker))]
[RequireComponent(typeof(Rigidbody))]


//----------------------------------
// Simple move script that adds force to game object in the direction of the target.. using path finding
//----------------------------------

public class EnemyGeneric_MoveToTarget : MonoBehaviour
{
    //VARIABLES
    //---------------------------------------------------------
    public GameObject Target; //target for this to track
    public float MoveForce; //force to push current gameobject
    public float DistanceBeforeGoingToNextWaypoint;//



    private Seeker _seeker; //seeker class attached to this object
    private Vector3 _currentWaypint;//next waypoint in array to move towards
    private Vector3[] _waypoints; //collection of waypoints to target object
    private int _waypointIndexHolder;//holds current waypoints index

    //---------------------------------------------------------


    //Called once at start
    public void Start()
    {
        //Initialize-----
        _seeker = GetComponent<Seeker>();

        //Start process------
        _seeker.StartPath(transform.position, Target.transform.position); //generate path to target

        StartCoroutine(GeneratePath());
    }


    public void Update()
    {
        if (_waypointIndexHolder >= _waypoints.Length)
            return;
       MoveTowardsTarget();
    }


    //called after path has been created
    public void PathComplete(Vector3[] newPoints)
    {
        _waypoints = newPoints;
        _currentWaypint = _waypoints[0];
        _waypointIndexHolder = 0;
    }

    //Adds force to move object towards next waypoint
    private void MoveTowardsTarget()
    {
        rigidbody.AddForce(Vector3.Normalize(_currentWaypint - transform.position) * MoveForce);//adds force towards next waypoint target
        TargetNextWaypoint();
    }


    //Select next waypoint if close enough
    private void TargetNextWaypoint()
    {
        if(Vector3.Distance(transform.position, _currentWaypint)< DistanceBeforeGoingToNextWaypoint)
        {

            _waypointIndexHolder++;
            if (_waypointIndexHolder >= _waypoints.Length)
                return;
            _currentWaypint = _waypoints[_waypointIndexHolder];
        }
        return;
    }


    IEnumerator GeneratePath()
    {
        while (true)
        {
            Target = GameObject.Find("Player_BugGuyOnChair(Clone)");
            _seeker.StartPath(transform.position, Target.transform.position);
            yield return new WaitForSeconds(2);
        }
    }


}