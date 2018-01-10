using UnityEngine;
using System.Collections;
[RequireComponent(typeof(Enemy_BugEnemy_State))]
[RequireComponent(typeof(Seeker))]
[RequireComponent(typeof(Rigidbody))]

//-----------------------------------------------------------
// Handles player controlled movement for specific player type
//   Also controls facing direction
//-----------------------------------------------------------

public class Enemy_BugEnemy_Movement : EnemyGeneric_Movement
{
    //VARIABLES----------------------------------------------
    public GameObject MovementDirectionFinder; //game object that holds the current movement direction

    public float IfXZVelocityChangesMoreThanThisAmountCrash = 6; //rapid changes in velocity causes character to 'crash' or 'swap to ragdoll'
    public float IfYVelocityChangesMoreThanThisAmountCrash = 10; //

    public float ExtraGravity = 0; //add downward force to tweak fall speed

    private bool _playerOnGround = false;
    private Vector3 _lastVelocity;// used to calculate velocity change
    private float _timeDelayForCrash = 2;// time delay before being able to crash
    private float _creatioinTime;        //

    //Path Finding VARIABLES---------------------------------
    public GameObject PlayerTypeToLookFor, SecondaryPlayerTypeToLookFor; //prefabs of the player controlled character
    public float DistanceBeforeMovingToTheNextWaypoint; //when player this close to current waypoint; move on to the next

    public GameObject CurrentActivePlayerHolder; //used to access the current active player
    private Seeker _seeker; //used to access seeker component
    private Vector3[] _waypoints; //holds list of array points that form path
    private int _waypointsIndexHolder; //stores the index of the current array point
    private Vector3 _startPosition; //stores the start location of the enemy; used for patroling
    private bool _patrolling; //indicates weather currently patrolling

    private float _lookForPlayerDelay = 3; //amount of time to look for player before returning to patrol
    private bool _lookForPlayerDelayInProgress;

    //-------------------------------------------------------


    // Use this for initialization---------------------------
    void Start()
    {
        _creatioinTime = Time.time; //record creation time; to calculate delay
        PathFindingStart(); //contains initializing of path-finding variables


        //subscribe to state events
        GetComponent<Enemy_BugEnemy_State>().stateOnGroundEvent += IsOnGround;
    }
    //-------------------------------------------------------


    //Called when player is on ground------------------------
    void IsOnGround(Collider colInfo)
    {
        _playerOnGround = true;
    }
    //-------------------------------------------------------


    // Update is called once per frame-----------------------
    void Update()
    {
        //Swap to ragdoll if velocity changes dramatically---
        if (Time.time < _timeDelayForCrash + _creatioinTime) return; //return if delay not reached

        if (CheckForVelocityChange(rigidbody, IfXZVelocityChangesMoreThanThisAmountCrash,
                                   IfYVelocityChangesMoreThanThisAmountCrash))
        {
            if (transform.parent == null)
                return;
            transform.parent.gameObject.GetComponent<EnemyGeneric_BaseClass>().CrashSwapRagdollEvent();
        }
        //---------------------------------------------------


    }
    //-------------------------------------------------------


    // Called 60 times a second------------------------------
    void FixedUpdate()
    {
        PathfindingFixedUpdate(); //contains updates used for path finding
        // rotate direction finder based on this objects current velocity
        RotateBasedOnVelocity(MovementDirectionFinder);
        // rotate this object smoothly towards direction finder
        SmoothRotateTowards(MovementDirectionFinder, MinimumMoveSpeedBeforeTurning, SmoothTurnAmmount);

        // add extra gravity
        if (!_playerOnGround)
        {
            AddMoreGravity(ExtraGravity);
            return;
        }

        // All code below only gets called when enemy is on ground
        //_____________________________________________________




        _playerOnGround = false; //gets turned back to true by event call
    }
    //-------------------------------------------------------


    //-------------------------------------------------------
    // Path finding
    //-------------------------------------------------------
    // Called once at start----------------------------------
    private void PathFindingStart()
    {
        _seeker = GetComponent<Seeker>(); //get Seeker component
        StartCoroutine(CalledEverySecond()); //starts infinite loop coroutine containing path-finding checks that only need to be called once a second
        _startPosition = transform.position;
    }
    //-------------------------------------------------------

    // Called once a second----------------------------------
    IEnumerator CalledEverySecond()
    {
        while (true)
        {
            // Look for player if holder is empty-
            if (CurrentActivePlayerHolder == null)
            {
                // print("look");
                FindTarget(PlayerTypeToLookFor, SecondaryPlayerTypeToLookFor);
            }
            //-

            if (GetComponent<Enemy_BugEnemy_State>().CurrentControlState == Enemy_BugEnemy_State.ControlState.Stop)
                _waypoints = null;
            // Generate path if holder is not null; and if player hasn't been lost sight of
            if (CurrentActivePlayerHolder != null && GetComponent<Enemy_BugEnemy_State>().CurrentControlState == Enemy_BugEnemy_State.ControlState.HuntPlayer)
            {
                GeneratePath();
            }

            yield return new WaitForSeconds(.2f);
        }
    }
    //-------------------------------------------------------

    
    // Called 60 times a second------------------------------
    private void PathfindingFixedUpdate()
    {
        // switch to 'patrol' if looking for palyer exceeds specified time
        if (!_lookForPlayerDelayInProgress && GetComponent<Enemy_BugEnemy_State>().CurrentControlState == Enemy_BugEnemy_State.ControlState.LookForPlayer) StartCoroutine(LookForPlayerDelay());

        // if enemy is hunting, or looking for player; move
        if (GetComponent<Enemy_BugEnemy_State>().CurrentControlState == Enemy_BugEnemy_State.ControlState.HuntPlayer || GetComponent<Enemy_BugEnemy_State>().CurrentControlState == Enemy_BugEnemy_State.ControlState.LookForPlayer)
        {
            if (rigidbody.velocity.magnitude < .1f && _waypoints != null) StartCoroutine(FixStuck()); //add force if character is stuck
            MoveToWaypoint();
            CheckTooClose();
        }

        // if enemy is patrolling; move towards the player then back to it's original location over time
        if(GetComponent<Enemy_BugEnemy_State>().CurrentControlState == Enemy_BugEnemy_State.ControlState.Patrol)
        {
            if (!_patrolling)
                StartCoroutine(Patrol());
        }


    }
    //-------------------------------------------------------

    //Look for player; delay before patrolling
    IEnumerator LookForPlayerDelay()
    {
        _lookForPlayerDelayInProgress = true;
        float startTime = Time.time;
        
        //wait whilst enemy is looking for player and delayed time hasn't passed
        while (startTime + _lookForPlayerDelay > Time.time && GetComponent<Enemy_BugEnemy_State>().CurrentControlState == Enemy_BugEnemy_State.ControlState.LookForPlayer)
        {
            yield return new WaitForEndOfFrame();
        }

        //set to partol if player not found
        GetComponent<Enemy_BugEnemy_State>().CurrentControlState = Enemy_BugEnemy_State.ControlState.Patrol;
        _lookForPlayerDelayInProgress = false;
        yield return 0;
    }

    //Patrol
    IEnumerator Patrol()
    {
        _patrolling = true;
        GeneratePath();

        float startTime = Time.time;
        while (startTime + 4 > Time.time)
        {
            MoveToWaypoint();
            yield return new WaitForEndOfFrame();
            CheckTooClose();
        }
        startTime = Time.time;
        GenerateCustomPath(transform.position, _startPosition);
        while (startTime + 4 > Time.time)
        {
            MoveToWaypoint();
            yield return new WaitForEndOfFrame();
            CheckTooClose();
        }
        _patrolling = false;
        yield return 0;
    }


    //adds force to rigidbody in the direction of the next waypoint location
    private void MoveToWaypoint()
    {
        if(_waypoints == null) return;
        if (_waypoints.Length <= _waypointsIndexHolder)
        {
            GetComponent<Enemy_BugEnemy_State>().CurrentControlState = Enemy_BugEnemy_State.ControlState.Patrol;
            return;
        }
        rigidbody.AddForce(Vector3.Normalize(_waypoints[_waypointsIndexHolder] - transform.position) * Speed); //adds force towards first waypoint
    }
    //-------------------------------------------------------

    //adds random force when player is stuck and selects the next waypoint in the array
    private bool _fixingStuck;
    private int _nextWaypointAfter; //runs through the check a specified number of times before selecting the next waypoint
    IEnumerator FixStuck()
    {
        //Adds force in random direction for *seconds
        if (_fixingStuck) yield break; //end if already fixing
        _fixingStuck = true; // set to fixing


        float oldVelocity = rigidbody.velocity.magnitude;
        yield return new WaitForSeconds(.2f);
        float newVelocity = rigidbody.velocity.magnitude;

        if(newVelocity > oldVelocity)
        {
            _fixingStuck = false;
            yield break;
        }


        Vector3 RandomDirection = Vector3.Normalize(new Vector3(Random.Range(-1f,1f),0,Random.Range(-1f,1f))); //get random direction
        float startTime = Time.time;   
        while (startTime + .5f > Time.time)
        {
            rigidbody.AddForce(RandomDirection * Speed * 1.5f);
            yield return 0;
        }

        //Adds no force for *seconds
        startTime = Time.time;
        while (startTime + .5f > Time.time)
        {
            yield return 0;
        }

        //Selects next waypoint after #calls

        {
                   if (_waypoints.Length <= _waypointsIndexHolder)
                //if indexer it at the end of array; stop and return                                                                  
            {
                GetComponent<Enemy_BugEnemy_State>().CurrentControlState = Enemy_BugEnemy_State.ControlState.Stop;
                _fixingStuck = false;
                yield break;
            }
            if (_nextWaypointAfter == 2)
            {
                _waypointsIndexHolder++;
                _nextWaypointAfter = 0;
            }
            else
                _nextWaypointAfter++;
        }

        _fixingStuck = false;       
    }


    //if too close to the current waypoint, move on to the next
    private void CheckTooClose()
    {
        if(_waypoints == null) return;
        if (_waypoints.Length <= _waypointsIndexHolder)
        {
            GetComponent<Enemy_BugEnemy_State>().CurrentControlState = Enemy_BugEnemy_State.ControlState.Stop;
            return;
        }
        if (Vector3.Distance(transform.position, _waypoints[_waypointsIndexHolder]) < DistanceBeforeMovingToTheNextWaypoint) //if distance is less than specified
        {
            if (_waypoints.Length <= _waypointsIndexHolder) //if indexer it at the end of array; stop and return                                                                  
            {
                GetComponent<Enemy_BugEnemy_State>().CurrentControlState = Enemy_BugEnemy_State.ControlState.Stop;
                return;
            }

            _waypointsIndexHolder++;                                                                                        //increment waypoint indexer; this will change the target waypoint to the next

        }
        return;
    }


    //find instance of player--------------------------------
    private void FindTarget(GameObject target, GameObject secondaryTarget)
    {
        if (GameObject.Find(target.name + "(Clone)") != null)
        {
            CurrentActivePlayerHolder = GameObject.Find(target.name + "(Clone)");
            return;
        }
        if (CurrentActivePlayerHolder != null) return;

        if (GameObject.Find(secondaryTarget.name + "(Clone)") != null)
        {
            CurrentActivePlayerHolder = GameObject.Find(secondaryTarget.name + "(Clone)");
            return;
        }
        CurrentActivePlayerHolder = null;
    }
    //-------------------------------------------------------


    //Generate path to player--------------------------------
    private void GenerateCustomPath(Vector3 start, Vector3 end)
    {
        _seeker.StartPath(start, end);
    }
    private void GeneratePath()
    {
        _seeker.StartPath(transform.position, CurrentActivePlayerHolder.transform.position); //create path from here to the target
    }
    private void PathComplete(Vector3[] waypoints)
    {
        //print("PATHCOMP");
        _waypoints = waypoints;
        //_waypointsIndexHolder = _waypoints.Length > 1 ? 1 : 0;
        _waypointsIndexHolder = 0;
    }

    //-------------------------------------------------------


}
