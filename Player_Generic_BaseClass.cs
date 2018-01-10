using System;
using UnityEngine;
using System.Collections;


public class Player_Generic_BaseClass : MonoBehaviour
{
    // Events
    public delegate void CrashSwapRagdoll();
    public CrashSwapRagdoll CrashSwapRagdollEvent; //Call when player crashes

    public delegate void RagdollStopped();
    public RagdollStopped RagDollStoppedEvent; //call when ragdoll stops moving

    public delegate void NearChair();
    public NearChair NearChairEvent; //Call when player is near chair


    //-------------------------------------------------------------------------------
    // PLAYER CREATION
    //-------------------------------------------------------------------------------
    /// <summary>
    /// Instantiate player-object, make player-object a child of this
    /// </summary>
    /// <param name="player">Player prefab to instantiate</param>
    public GameObject CreatePlayer(GameObject player)
    {
        return CreatePlayer(player, new Vector3(0, 0, 0), Quaternion.identity);
    }
    /// <summary>
    /// Instantiate player-object, make player-object a child of this, move player-object to position
    /// </summary>
    /// <param name="player">Player prefab to instantiate</param>
    /// <param name="spawnPoint">Position to spawn Player</param>
    public GameObject CreatePlayer(GameObject player, Vector3 spawnPoint)
    {
        return CreatePlayer(player, spawnPoint, Quaternion.identity);
    }
    /// <summary>
    /// Instantiate player-object, make player-object a child of this, move player-object to position
    /// </summary>
    /// <param name="player">Player prefab to instantiate</param>
    /// <param name="spawnPoint">Set Position to spawn Player</param>
    /// <param name="rotation">Set Rotation of spawned Player</param>
    public GameObject CreatePlayer(GameObject player, Vector3 spawnPoint, Quaternion rotation)
    {
        GameObject instancedPlayer = (GameObject)Instantiate(player, spawnPoint, rotation);     //instantiate Player game-object
        instancedPlayer.transform.parent = transform;                                           //make Player game-object a child of this
        return instancedPlayer;
    }
    //-------------------------------------------------------------------------------

    //-------------------------------------------------------------------------------
    //SWAPPING TO RAGDOLL
    //-------------------------------------------------------------------------------


    /// <summary>
    /// Swap a player controlled game object with a ragdoll version
    /// </summary>
    /// <param name="playerToBeSwappedOut">player to swap</param>
    /// <param name="ragdollToBeSwappedIn">ragdoll to replace player</param>
    /// <returns>instance of ragdoll</returns>
    public GameObject SwapInRagdoll(GameObject playerToBeSwappedOut, GameObject ragdollToBeSwappedIn)
    {
        GameObject ragdollInstance = (GameObject)Instantiate(ragdollToBeSwappedIn, new Vector3(0, -1000, 0), Quaternion.identity); //create ragdoll way off in the distance
        ragdollInstance.transform.parent = transform;                                                                              //make child of current game object
        StartCoroutine(MakeRagDollPairSet(playerToBeSwappedOut, ragdollInstance));                                                 //make pair set of all game object in player and ragdoll
        return ragdollInstance;
    }
    /// <summary>
    /// Swap a player controlled game object with a ragdoll version
    /// </summary>
    /// <param name="playerToBeSwappedOut">player to swap</param>
    /// <param name="ragdollToBeSwappedIn">prefab ragdoll to replace player</param>
    /// <param name="ragdollToBeSwappedInSecondPart">prefab second ragdoll part</param>
    /// <returns>instance of ragdoll</returns>
    public GameObject SwapInRagdoll(GameObject playerToBeSwappedOut, GameObject ragdollToBeSwappedIn, GameObject ragdollToBeSwappedInSecondPart, out GameObject secondPartInstanceHolder)
    {
        GameObject ragdollInstance = (GameObject)Instantiate(ragdollToBeSwappedIn, new Vector3(0, -1000, 0), Quaternion.identity);
        GameObject ragDollSecondInstance = (GameObject)Instantiate(ragdollToBeSwappedInSecondPart, new Vector3(0, -1000, 0), Quaternion.identity);
        ragdollInstance.transform.parent = transform;
        ragDollSecondInstance.transform.parent = transform;
        secondPartInstanceHolder = ragDollSecondInstance;
        StartCoroutine(MakeRagDollPairSet(playerToBeSwappedOut, ragdollInstance, ragDollSecondInstance));
        return ragdollInstance;
    }



    /// <summary>
    /// Function that is called after the pair set is made; finish off ragdoll swap here
    /// </summary>
    /// <param name="player">player</param>
    /// <param name="ragdoll">ragdoll</param>
    /// <param name="pairSet">set of matching parts</param>
    private void CalledAfterPairSetMade(GameObject player, GameObject ragdoll, Transform[,] pairSet)   // do this after pair set made
    {
        CalledAfterPairSetMade(player, ragdoll, pairSet, new Transform[0, 0]);                                                                                                           //destroy player
    }
    /// <summary>
    /// Function that is called after the pair set is made; finish off ragdoll swap here
    /// </summary>
    /// <param name="player">player</param>
    /// <param name="ragdoll">ragdoll</param>
    /// <param name="pairSet">set of matching parts</param>
    /// <param name="secondPairSet">second pair of matching parts</param>
    private void CalledAfterPairSetMade(GameObject player, GameObject ragdoll, Transform[,] pairSet, Transform[,] secondPairSet)   // do this after pair set made
    {
        RemoveAllColliders(player);                                                                                                //remove all colliders from player  
        MoveRagDollToAnimationPosition(pairSet, player);                                                                           //move ragdoll to player position and add velocity
        if (secondPairSet.Length != 0)
        {
            MoveRagDollToAnimationPosition(secondPairSet, player);
        }
        Destroy(player);                                                                                                           //destroy player
    }



    /// <summary>
    /// Removes all colliders from game object and children
    /// </summary>
    /// <param name="player">game object to remove colliders from</param>
    private void RemoveAllColliders(GameObject player)
    {
        foreach (Transform playerPart in player.GetComponentsInChildren<Transform>())
        {
            if (playerPart.gameObject.collider)
                DestroyObject(playerPart.gameObject.collider);
        }
    }


    /// <summary>
    /// Makes a two dimensional array containing matching game objects from 'player' and 'ragdoll'
    /// </summary>
    /// <param name="player">player controlled game object</param>
    /// <param name="ragdoll">ragdoll to find pairs with</param>
    /// <returns></returns>
    IEnumerator MakeRagDollPairSet(GameObject player, GameObject ragdoll)
    {
        //if (player == null) yield break;
        Transform[] playerAnimationVersion = player.GetComponentsInChildren<Transform>();
        Transform[] ragdollToSwitchNext = ragdoll.GetComponentsInChildren<Transform>();

        Transform[,] PairArrayListHolder = new Transform[playerAnimationVersion.Length, 2]; //temporary holder for return array
        Transform[,] _pairArray = new Transform[0, 0];

        // create array of matching pairs
        int updateFinalArraySize = 0;
        int yieldOnOther = 0;
        foreach (Transform currentObject in playerAnimationVersion)
        {
            //create larger array to populate with pairs
            foreach (Transform checkThisObject in ragdollToSwitchNext)
            {
                if (currentObject.name == checkThisObject.name)
                {
                    PairArrayListHolder[updateFinalArraySize, 0] = currentObject;
                    PairArrayListHolder[updateFinalArraySize, 1] = checkThisObject;

                    updateFinalArraySize++;
                    break; //bails out of foreach if pair found
                }
            }

            //yield a couple of time to reduce hanging
            if (yieldOnOther < 10)
                yieldOnOther++;
            else
            {
                yieldOnOther = 0;
                yield return 0;
            }
        }

        //create refined size array
        _pairArray = new Transform[updateFinalArraySize, 2];
        for (int i = 0; i < updateFinalArraySize; i++)
        {
            _pairArray[i, 0] = PairArrayListHolder[i, 0];
            _pairArray[i, 1] = PairArrayListHolder[i, 1];
        }
        CalledAfterPairSetMade(player, ragdoll, _pairArray);
    }
    /// <summary>
    /// Makes a two dimensional array containing matching game objects from 'player' and 'ragdoll'
    /// </summary>
    /// <param name="player">player controlled game object</param>
    /// <param name="ragdoll">ragdoll to find pairs with</param>
    /// <param name="secondRagdoll">second ragdoll to find pairs with</param>
    /// <returns></returns>
    IEnumerator MakeRagDollPairSet(GameObject player, GameObject ragdoll, GameObject secondRagdoll)
    {
        Transform[] playerAnimationVersion = player.GetComponentsInChildren<Transform>();
        Transform[] ragdollToSwitchNext = ragdoll.GetComponentsInChildren<Transform>();
        Transform[] secondRagdollToSwitch = secondRagdoll.GetComponentsInChildren<Transform>();

        Transform[,] PairArrayListHolder = new Transform[playerAnimationVersion.Length, 2]; //temporary holder for return array
        Transform[,] SecondPairArrayListHolder = new Transform[playerAnimationVersion.Length, 2];//temporary holder for second return
        Transform[,] _pairArray = new Transform[0, 0];
        Transform[,] _secondPairArray = new Transform[0, 0];

        // create array of matching pairs
        int updateFinalArraySize = 0;
        int updateSecondFinalArraySize = 0;
        int yieldOnOther = 0;
        foreach (Transform currentObject in playerAnimationVersion)
        {
            //create larger array to populate with pairs
            foreach (Transform checkThisObject in ragdollToSwitchNext)
            {
                if (currentObject.name == checkThisObject.name)
                {
                    PairArrayListHolder[updateFinalArraySize, 0] = currentObject;
                    PairArrayListHolder[updateFinalArraySize, 1] = checkThisObject;

                    updateFinalArraySize++;
                    break; //bails out of foreach if pair found
                }

            }
            foreach (Transform checkThisObject in secondRagdollToSwitch)
            {
                if (currentObject.name == checkThisObject.name)
                {
                    SecondPairArrayListHolder[updateSecondFinalArraySize, 0] = currentObject;
                    SecondPairArrayListHolder[updateSecondFinalArraySize, 1] = checkThisObject;

                    updateSecondFinalArraySize++;
                    break; //bails out of foreach if pair found
                }
            }

            //yield a couple of time to reduce hanging
            if (yieldOnOther < 10)
                yieldOnOther++;
            else
            {
                yieldOnOther = 0;
                yield return 0;
            }
        }

        //create refined size array
        _pairArray = new Transform[updateFinalArraySize, 2];
        for (int i = 0; i < updateFinalArraySize; i++)
        {
            _pairArray[i, 0] = PairArrayListHolder[i, 0];
            _pairArray[i, 1] = PairArrayListHolder[i, 1];
        }
        //create second refined size array
        _secondPairArray = new Transform[updateSecondFinalArraySize, 2];
        for (int i = 0; i < updateSecondFinalArraySize; i++)
        {
            _secondPairArray[i, 0] = SecondPairArrayListHolder[i, 0];
            _secondPairArray[i, 1] = SecondPairArrayListHolder[i, 1];
        }

        CalledAfterPairSetMade(player, ragdoll, _pairArray, _secondPairArray);
    }
    /// <summary>
    /// Moves objects from 'a' to 'b' in a two dimensional array of transforms
    /// </summary>
    /// <param name="animationAndRagDollPositions">[,0]=animation transforms [,1]=ragdoll transforms</param>
    /// <param name="player">used to transfer velocity</param>
    private void MoveRagDollToAnimationPosition(Transform[,] animationAndRagDollPositions, GameObject player)
    {
        for (int i = 0; i < animationAndRagDollPositions.Length / 2; i++)
        {
            // move ragdoll part position to animation part position
            animationAndRagDollPositions[i, 1].transform.position =
                animationAndRagDollPositions[i, 0].transform.position;
            // move ragdoll part rotation to animation part rotation
            animationAndRagDollPositions[i, 1].transform.rotation =
                animationAndRagDollPositions[i, 0].transform.rotation;
            // move assign ragdoll part rigid body velocity
            if (animationAndRagDollPositions[i, 1].rigidbody)
                animationAndRagDollPositions[i, 1].rigidbody.velocity = player.rigidbody.velocity;
        }
    }

    //-------------------------------------------------------------------------------

    //-------------------------------------------------------------------------------
    // SWAPPING TO ANIMATION
    //-------------------------------------------------------------------------------



    //TODO create player instance               <DONE>
    //TODO remove all colliders from player     <DONE>
    //TODO remove controls                      
    //TODO hide mesh                            <DONE>
    //TODO align player to ragdoll              <DONE> 
    //TODO create pair set for ragdoll          <DONE>
    //TODO remove ragdoll physics               <DONE>
    //TODO blend ragdoll towards player         <DONE>
    //TODO destroy ragdoll                      <DONE>
    //TODO create second player instance        <DONE>
    //TODO align with no-collider version       <DONE>
    //TODO destroy no-collider version          <DONE>
    private float _blendTime = 0;
    private GameObject _tempRagdollHolder;
    private GameObject _tempPlayerChairlessHolder;
    private GameObject _tempPlayerChairlessCollisionlessHolder;

    public bool SwappingInAnimation;
    /// <summary>
    /// Replace ragdoll with a playable game character 
    /// </summary>
    /// <param name="playerPrefab">player to swap in</param>
    /// <param name="ragdollInstance">instance of ragdoll currently in scene</param>
    /// <param name="blendDuration">time used to blend ragdoll to animation</param>
    /// <returns>returns instance of game character</returns>
    public GameObject SwapInAnimation(GameObject playerPrefab, GameObject ragdollInstance, float blendDuration)
    {
        SwappingInAnimation = true;
        _blendTime = blendDuration;
        _tempPlayerChairlessHolder = (GameObject)Instantiate(playerPrefab, new Vector3(0, -1000, 0), Quaternion.identity); //store instance of complete player object; to be swapped out at the end


        GameObject playerInstance = (GameObject)Instantiate(playerPrefab, new Vector3(0, -1000, 0), Quaternion.identity); //create player instance to use without physics



        playerInstance.transform.parent = transform;                                                                  //makes instance a child of this object
        _tempPlayerChairlessHolder.transform.parent = transform;
        RemovePhysics(playerInstance);                                                                                //remove physics from instance


        RemoveMesh(playerInstance);

        //print(playerInstance.name);
        MoveAnimationToRagdollPosition(playerInstance, ragdollInstance, "Bip001");                                    //move instance to ragdoll position

        StartCoroutine(waitAwhile(playerInstance, ragdollInstance)); //Set start
        //StartCoroutine(MakeAnimationPairSet(playerInstance, ragdollInstance));                                        //make pair-set for instance
        return _tempPlayerChairlessHolder;
    }

    IEnumerator waitAwhile(GameObject playerInstance, GameObject ragdollInstance)
    {
        yield return new WaitForEndOfFrame();

        bool landedOnFront;
        if (LandedOnFront(ragdollInstance))
        {
            _tempPlayerChairlessHolder.GetComponent<Player_BugGuy_State>().CurrentSideLanded =
                Player_BugGuy_State.SideLanded.Front;
            playerInstance.GetComponent<Player_BugGuy_State>().CurrentSideLanded =
                Player_BugGuy_State.SideLanded.Front;
            landedOnFront = true;
        }
        else
        {
            _tempPlayerChairlessHolder.GetComponent<Player_BugGuy_State>().CurrentSideLanded =
                Player_BugGuy_State.SideLanded.Back;
            playerInstance.GetComponent<Player_BugGuy_State>().CurrentSideLanded =
                Player_BugGuy_State.SideLanded.Back;
            landedOnFront = false;
        }

        playerInstance.GetComponent<Player_Generic_StateFinder>().stateAssumePositionEvent(0, landedOnFront);                      //sets animation to start pose
        //>>>>>>>>>_tempPlayerChairlessHolder.GetComponent<Player_BugGuy_State>().CurrentSideLanded = playerInstance.GetComponent<Player_BugGuy_State>().CurrentSideLanded;

        if (playerInstance.GetComponent<Player_BugGuy_State>().CurrentSideLanded == Player_BugGuy_State.SideLanded.Front)
        {
            playerInstance.transform.rotation = Quaternion.Euler(playerInstance.transform.rotation.eulerAngles.x,
                                                                 -playerInstance.transform.rotation.eulerAngles.y,
                                                                 playerInstance.transform.rotation.eulerAngles.z);

        }



        StartCoroutine(MakeAnimationPairSet(playerInstance, ragdollInstance));                                      //make pair set of animation position
    }

    /// <summary>
    /// Function that is called after the pair set is made; begin position blend from here
    /// </summary>
    /// <param name="player">player</param>
    /// <param name="ragdoll">ragdoll</param>
    /// <param name="pairSet">set of matching parts</param>
    /// <param name="secondPairSet">second pair of matching parts</param>
    private void CalledAfterAnimationPairSetMade(GameObject player, GameObject ragdoll, Transform[,] pairSet)                      // do this after pair set made
    {
        RemovePhysics(ragdoll);                                                                                         //remove radoll physics; so it doesn't collide with animation
        StartCoroutine(BlendToPosition(pairSet));
        _tempRagdollHolder = ragdoll;
        _tempPlayerChairlessCollisionlessHolder = player;

    }
    /// <summary>
    /// This section of code called after blending
    /// </summary>
    private void CalledAfterBlending()
    {
        

        _tempPlayerChairlessHolder.GetComponent<Player_Generic_StateFinder>().stateStartRecoverEvent(); //fire state event indicating end of ragdoll blend
        Destroy(_tempRagdollHolder);
        _tempPlayerChairlessHolder.transform.position = _tempPlayerChairlessCollisionlessHolder.transform.position;
        _tempPlayerChairlessHolder.transform.rotation = _tempPlayerChairlessCollisionlessHolder.transform.rotation;


        Destroy(_tempPlayerChairlessCollisionlessHolder);
        SwappingInAnimation = false;
    }


    /// <summary>
    /// Makes a two dimensional array containing matching game objects from 'player' and 'ragdoll'
    /// </summary>
    /// <param name="player">player controlled game object</param>
    /// <param name="ragdoll">ragdoll to find pairs with</param>
    /// <returns></returns>
    IEnumerator MakeAnimationPairSet(GameObject player, GameObject ragdoll)
    {


        Transform[] playerAnimationVersion = player.GetComponentsInChildren<Transform>();
        Transform[] ragdollToSwitchNext = ragdoll.GetComponentsInChildren<Transform>();

        Transform[,] PairArrayListHolder = new Transform[playerAnimationVersion.Length, 2];
        //temporary holder for return array
        Transform[,] _pairArray = new Transform[0, 0];

        // create array of matching pairs
        int updateFinalArraySize = 0;
        int yieldOnOther = 0;


        foreach (Transform currentObject in playerAnimationVersion)
        {
            //create larger array to populate with pairs
            foreach (Transform checkThisObject in ragdollToSwitchNext)
            {
                if (currentObject.name == checkThisObject.name)
                {
                    PairArrayListHolder[updateFinalArraySize, 0] = currentObject;
                    PairArrayListHolder[updateFinalArraySize, 1] = checkThisObject;

                    updateFinalArraySize++;
                    break; //bails out of foreach if pair found
                }
            }

            //yield a couple of time to reduce hanging
            if (yieldOnOther < 10)
                yieldOnOther++;
            else
            {
                yieldOnOther = 0;
                yield return 0;

            }
        }

        //create refined size array
        _pairArray = new Transform[updateFinalArraySize, 2];
        for (int i = 0; i < updateFinalArraySize; i++)
        {
            _pairArray[i, 0] = PairArrayListHolder[i, 0];
            _pairArray[i, 1] = PairArrayListHolder[i, 1];
        }

        CalledAfterAnimationPairSetMade(player, ragdoll, _pairArray);
    }

    /// <summary>
    /// blends using an array of matching transforms
    /// </summary>
    /// <param name="pairSet"></param>
    /// <returns></returns>
    IEnumerator BlendToPosition(Transform[,] pairSet)
    {
        float startTime = Time.time;
        float waitTime = _blendTime;

        Vector3[] startPosition = new Vector3[pairSet.Length / 2];
        Quaternion[] startRotation = new Quaternion[pairSet.Length / 2];
        for (int i = 0; i < pairSet.Length / 2; i++)
        {
            startPosition[i] = new Vector3(pairSet[i, 1].position.x, pairSet[i, 1].position.y, pairSet[i, 1].position.z);
            startRotation[i] = new Quaternion(pairSet[i, 1].rotation.x, pairSet[i, 1].rotation.y,
                                              pairSet[i, 1].rotation.z, pairSet[i, 1].rotation.w);
        }

        while (startTime + waitTime > Time.time)
        {
            float blendTime = waitTime - ((startTime + waitTime) - Time.time);
            blendTime = blendTime / waitTime;
            //print(blendTime);
            for (int i = 0; i < pairSet.Length / 2; i++)
            {
                //blend towards target position
                pairSet[i, 1].position = Vector3.Slerp(startPosition[i],
                                                       pairSet[i, 0].position,
                                                       blendTime);
                //blend towards target rotation
                pairSet[i, 1].rotation = Quaternion.Slerp(startRotation[i],
                                                          pairSet[i, 0].rotation,
                                                          blendTime);
            }
            yield return new WaitForEndOfFrame();
        }
        CalledAfterBlending();
    }



    /// <summary>
    /// Removes or disables all physics based components
    /// </summary>
    /// <param name="player">object to remove physics from</param>
    private void RemovePhysics(GameObject player)
    {
        foreach (Transform child in player.GetComponentsInChildren<Transform>())
        {
            if (child.gameObject.rigidbody)
                child.gameObject.rigidbody.isKinematic = true;
            if (child.gameObject.hingeJoint)
                Destroy(child.gameObject.hingeJoint);
            if (child.gameObject.collider)
                Destroy(child.gameObject.collider);
        }
    }


    /// <summary>
    /// Moves animation model to rigid body model
    /// </summary>
    /// <param name="playerAnimation">Player animation version</param>
    /// <param name="playerRagdoll">Player ragdoll version</param>
    private void MoveAnimationToRagdollPosition(GameObject playerAnimation, GameObject playerRagdoll, string nameOfSharedObject)
    {
        GameObject TempPositionHolder = new GameObject("TempPositionHolder");   //create temp object for moving Player Animation version

        //RaycastHit[] hitInfo;

        //find biped base and store position in TempPosition Holder
        foreach (Transform childTransform in playerAnimation.GetComponentsInChildren<Transform>())
        {
            if (childTransform.gameObject.name == nameOfSharedObject)                             //if this object is matching object
            {
                TempPositionHolder.transform.position = childTransform.position;       //move position holder to base bipeds position and rotation
                TempPositionHolder.transform.rotation = childTransform.rotation;        //  

                TempPositionHolder.transform.parent = playerAnimation.transform.parent.transform; //make temp-position-holder share the same parent as biped base
                playerAnimation.transform.parent = TempPositionHolder.transform;                  //make animated game object a child of temp-position-holder
                break;
            }
        }
        //find ragdoll base and move temp-position-holder to that position
        foreach (Transform childTransform in playerRagdoll.GetComponentsInChildren<Transform>())
        {
            if (childTransform.gameObject.name == nameOfSharedObject)                             //if this object is biped base
            {
                //TempPositionHolder.transform.position = childTransform.position;        //move position holder (containing Player Animation version) to this position and rotation

                //TODO ------------------------------------------------------------------------------------------------------------------------------------------
                // TempPositionHolder.transform.position = new Vector3(childTransform.position.x, 1.05f, childTransform.position.z); //TODO Make this less retarded
                //TODO ------------------------------------------------------------------------------------------------------------------------------------------
                TempPositionHolder.transform.position = new Vector3(childTransform.position.x, childTransform.position.y + .7f, childTransform.position.z);
                foreach (Rigidbody rigidChild in TempPositionHolder.GetComponentsInChildren<Rigidbody>())
                {
                    if(rigidChild.isKinematic)
                        continue; 
                    rigidChild.velocity = Vector3.zero;
                }



                Vector3 tempRotationHolder = new Vector3(325.1524f, -childTransform.rotation.eulerAngles.y, 93.49226f); //only update rotation around the 'y' axis (floats used to counter act the initial import offset)
                TempPositionHolder.transform.rotation = Quaternion.Euler(tempRotationHolder); //childTransform.rotation;
                TempPositionHolder.transform.DetachChildren();                          //detach position holder
                DestroyObject(TempPositionHolder);
                break;
            }
        }
        playerAnimation.transform.parent = transform; //re-assign parent
    }


    private bool LandedOnFront(GameObject ragdoll)
    {
        Vector3 centreTransformPosition = transform.position;
        Quaternion centerTransformRotation = transform.rotation;
        foreach (Transform child in ragdoll.GetComponentsInChildren<Transform>())
        {
            if (child.gameObject.name == "Bip001")
            {
                centreTransformPosition = child.position;
                centerTransformRotation = child.rotation;
                break;
            }

        }
        // find out which side is on the ground
        RaycastHit[] hitInfoForward = Physics.RaycastAll(centreTransformPosition, centerTransformRotation * Vector3.left);
        RaycastHit[] hitInfoBackwards = Physics.RaycastAll(centreTransformPosition, centerTransformRotation * Vector3.right);
        float hitFrontDistance = 10000;
        float hitBackDistance = 10000;

        foreach (RaycastHit raycastHit in hitInfoForward)
        {
            //print("hit+" + raycastHit.collider.gameObject.name);
            if (raycastHit.collider.gameObject.tag == "Player")
                continue;

            if (raycastHit.collider.gameObject.tag == "Ground")
            {
                hitFrontDistance = raycastHit.distance;
                break;
            }
        }
        foreach (RaycastHit raycastHit in hitInfoBackwards)
        {
            if (raycastHit.collider.gameObject.tag == "Player")
                continue;
            if (raycastHit.collider.gameObject.tag == "Ground")
            {
                hitBackDistance = raycastHit.distance;
                break;
            }
        }

        //print(hitFrontDistance + " " + hitBackDistance);

        if (hitBackDistance < hitFrontDistance)
        {
            return false;
        }
        else
        {
            return true;
        }
    }


    private void RemoveMesh(GameObject player)
    {
        foreach (Transform child in player.GetComponentsInChildren<Transform>())
        {
            if (child.renderer)
                Destroy(child.renderer);
        }

    }
    //-------------------------------------------------------------------------------

    //-------------------------------------------------------------------------------
    // CLIMBING ON CHAIR
    //-------------------------------------------------------------------------------

    //TODO disable player controls
    //TODO create physicless instance of bugguy on wheels
    //TODO Move chairless to player position

    //TODO Blend wheel ragdoll to recovery onWheels state

    private GameObject _tempRagdollChairHolder;
    private GameObject _tempPlayerOnChair;

    private Transform[,] _chairAnimationRagdollPairHolder;

    public GameObject GetOnChair(GameObject PlayerInstanceNoChair, GameObject RagdollChairInstance, AnimationClip RecoverClip, GameObject BugOnChairPrefab)
    {
        if (SwappingInAnimation)
            print("WARNING WAS STILL SWAPPING IN ANIMATION");
        PlayerInstanceNoChair.GetComponent<Player_BugGuy_State>().CurrentControlState = //disable player controls
            Player_BugGuy_State.ControlState.Disabled;

        _tempPlayerOnChair = (GameObject)Instantiate(BugOnChairPrefab, new Vector3(10, -1000, 0), Quaternion.identity); //create bugOnChairInstance
        _tempPlayerOnChair.transform.parent = transform;


        _tempPlayerChairlessHolder = PlayerInstanceNoChair;                                 //store player instance in temp holder
        _tempRagdollChairHolder = RagdollChairInstance;                                     //store ragdoll chair instance in holder

        StartCoroutine(PlayeRecoverStartAnimation(PlayerInstanceNoChair, RecoverClip));
        return _tempPlayerOnChair;

    }

    private void CalledAfterRecoverStart()
    {
        StartCoroutine(BlendChairToPosition(_chairAnimationRagdollPairHolder));

        RemovePhysics(_tempPlayerChairlessHolder);                                      //remove physics from chair-less instance and wheels
        RemovePhysics(_tempRagdollChairHolder);

        foreach (Renderer child in _tempPlayerOnChair.GetComponentsInChildren<Renderer>()) //disable chair visibility
        {
            if (child.gameObject.name == "Chair")
            {
                child.enabled = false;
                break;
            }
        }

        foreach (Rigidbody child in _tempPlayerOnChair.GetComponentsInChildren<Rigidbody>()) //zero velocity
        {
            child.velocity = Vector3.zero;
        }

        _tempPlayerOnChair.transform.position = _tempPlayerChairlessHolder.transform.position;  //align new on-chair guy to chair-less; position and rotation
        _tempPlayerOnChair.transform.rotation = _tempPlayerChairlessHolder.transform.rotation;


        _tempPlayerOnChair.GetComponent<Player_BugGuy_State>().CurrentControlState =            //disable player controls
            Player_BugGuy_State.ControlState.Disabled;

        _tempPlayerOnChair.GetComponent<Player_BugGuy_State>().CurrentMotionState =
            Player_BugGuy_State.MotionState.JumpingOnChair;

        Destroy(_tempPlayerChairlessHolder);




        //print("yey");   
    }

    IEnumerator PlayeRecoverStartAnimation(GameObject PlayerInstanceNoChari, AnimationClip recoverClip)
    {

        StartCoroutine(MakeChairPairSet(_tempPlayerOnChair, _tempRagdollChairHolder));
        //_chairAnimationRagdoll = MakeRagDollPairSet(_tempPlayerOnChair, _tempRagdollChairHolder);

        PlayerInstanceNoChari.GetComponent<Player_BugGuy_State>().CurrentMotionState =     //start jumping on chair; cross fades crouch animation for begining of jump
            Player_BugGuy_State.MotionState.JumpingOnChair;
        yield return new WaitForSeconds(recoverClip.length);
        CalledAfterRecoverStart(); ;
    }

    /// <summary>
    /// blends using an array of matching transforms
    /// </summary>
    /// <param name="pairSet"></param>
    /// <returns></returns>
    IEnumerator BlendChairToPosition(Transform[,] pairSet)
    {
        float startTime = Time.time;
        float waitTime = _blendTime;

        Vector3[] startPosition = new Vector3[pairSet.Length / 2];
        Quaternion[] startRotation = new Quaternion[pairSet.Length / 2];
        for (int i = 0; i < pairSet.Length / 2; i++)
        {
            startPosition[i] = new Vector3(pairSet[i, 1].position.x, pairSet[i, 1].position.y, pairSet[i, 1].position.z);
            startRotation[i] = new Quaternion(pairSet[i, 1].rotation.x, pairSet[i, 1].rotation.y,
                                              pairSet[i, 1].rotation.z, pairSet[i, 1].rotation.w);
        }

        while (startTime + waitTime > Time.time)
        {
            float blendTime = waitTime - ((startTime + waitTime) - Time.time);
            blendTime = blendTime / waitTime;
            //print(blendTime);
            for (int i = 0; i < pairSet.Length / 2; i++)
            {
                //blend towards target position
                pairSet[i, 1].position = Vector3.Slerp(startPosition[i],
                                                       pairSet[i, 0].position,
                                                       blendTime);
                //blend towards target rotation
                pairSet[i, 1].rotation = Quaternion.Slerp(startRotation[i],
                                                          pairSet[i, 0].rotation,
                                                          blendTime);
            }
            yield return new WaitForEndOfFrame();
        }
        // after blending
        //print("destruction");
        Destroy(_tempRagdollChairHolder); //destroy ragdol
        foreach (Renderer child in _tempPlayerOnChair.GetComponentsInChildren<Renderer>())
        {
            if (child.gameObject.name == "Chair")
                child.enabled = true;
        }


    }


    /// <summary>
    /// Makes a two dimensional array containing matching game objects from 'player' and 'ragdoll'
    /// </summary>
    /// <param name="player">player controlled game object</param>
    /// <param name="ragdoll">ragdoll to find pairs with</param>
    /// <returns></returns>
    IEnumerator MakeChairPairSet(GameObject player, GameObject ragdoll)
    {
        Transform[] playerAnimationVersion = player.GetComponentsInChildren<Transform>();
        Transform[] ragdollToSwitchNext = ragdoll.GetComponentsInChildren<Transform>();

        Transform[,] PairArrayListHolder = new Transform[playerAnimationVersion.Length, 2]; //temporary holder for return array
        Transform[,] _pairArray = new Transform[0, 0];

        // create array of matching pairs
        int updateFinalArraySize = 0;
        int yieldOnOther = 0;
        foreach (Transform currentObject in playerAnimationVersion)
        {
            //create larger array to populate with pairs
            foreach (Transform checkThisObject in ragdollToSwitchNext)
            {
                if (currentObject.name == checkThisObject.name)
                {
                    PairArrayListHolder[updateFinalArraySize, 0] = currentObject;
                    PairArrayListHolder[updateFinalArraySize, 1] = checkThisObject;

                    updateFinalArraySize++;
                    break; //bails out of foreach if pair found
                }
            }

            //yield a couple of time to reduce hanging
            if (yieldOnOther < 10)
                yieldOnOther++;
            else
            {
                yieldOnOther = 0;
                yield return 0;
            }
        }

        //create refined size array
        _pairArray = new Transform[updateFinalArraySize, 2];
        for (int i = 0; i < updateFinalArraySize; i++)
        {
            _pairArray[i, 0] = PairArrayListHolder[i, 0];
            _pairArray[i, 1] = PairArrayListHolder[i, 1];
        }

        _chairAnimationRagdollPairHolder = _pairArray;
    }
}

