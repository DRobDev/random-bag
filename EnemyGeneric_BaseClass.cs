using System;
using UnityEngine;
using System.Collections;


public class EnemyGeneric_BaseClass : MonoBehaviour
{
    // Events
    public delegate void CrashSwapRagdoll();
    public CrashSwapRagdoll CrashSwapRagdollEvent; //Call when player crashes

    public delegate void RagdollStopped();
    public RagdollStopped RagDollStoppedEvent; //call when ragdoll stops moving


    //-------------------------------------------------------------------------------
    // PLAYER CREATION
    //-------------------------------------------------------------------------------
    /// <summary>
    /// Instantiate player-object, make player-object a child of this
    /// </summary>
    /// <param name="player">Player prefab to instantiate</param>
    public GameObject CreateEnemy(GameObject player)
    {
        return CreateEnemy(player, new Vector3(0, 0, 0), Quaternion.identity);
    }
    /// <summary>
    /// Instantiate player-object, make player-object a child of this, move player-object to position
    /// </summary>
    /// <param name="player">Player prefab to instantiate</param>
    /// <param name="spawnPoint">Position to spawn Player</param>
    public GameObject CreateEnemy(GameObject player, Vector3 spawnPoint)
    {
        return CreateEnemy(player, spawnPoint, Quaternion.identity);
    }
    /// <summary>
    /// Instantiate player-object, make player-object a child of this, move player-object to position
    /// </summary>
    /// <param name="player">Player prefab to instantiate</param>
    /// <param name="spawnPoint">Set Position to spawn Player</param>
    /// <param name="rotation">Set Rotation of spawned Player</param>
    public GameObject CreateEnemy(GameObject player, Vector3 spawnPoint, Quaternion rotation)
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
    /// Function that is called after the pair set is made; finish off ragdoll swap here
    /// </summary>
    /// <param name="player">player</param>
    /// <param name="ragdoll">ragdoll</param>
    /// <param name="pairSet">set of matching parts</param>
    /// <param name="secondPairSet">second pair of matching parts</param>
    private void CalledAfterPairSetMade(GameObject player, GameObject ragdoll, Transform[,] pairSet)   // do this after pair set made
    {
        RemoveAllColliders(player);                                                                                                //remove all colliders from player  
        MoveRagDollToAnimationPosition(pairSet, player);                                                                           //move ragdoll to player position and add velocity
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
    private GameObject _tempEnemyHolder;
    private GameObject _tempEnemyCollisionlessHolder;

    public bool SwappingInAnimation;
    /// <summary>
    /// Replace ragdoll with a playable game character 
    /// </summary>
    /// <param name="enemyPrefab">player to swap in</param>
    /// <param name="ragdollInstance">instance of ragdoll currently in scene</param>
    /// <param name="blendDuration">time used to blend ragdoll to animation</param>
    /// <returns>returns instance of game character</returns>
    public GameObject SwapInAnimation(GameObject enemyPrefab, GameObject ragdollInstance, float blendDuration)
    {
        SwappingInAnimation = true;
        _blendTime = blendDuration;
        _tempEnemyHolder = (GameObject)Instantiate(enemyPrefab, new Vector3(0, -1000, 0), Quaternion.identity); //store instance of complete player object; to be swapped out at the end


        GameObject enemyInstance = (GameObject)Instantiate(enemyPrefab, new Vector3(0, -1000, 0), Quaternion.identity); //create player instance to use without physics



        enemyInstance.transform.parent = transform;                                                                  //makes instance a child of this object
        _tempEnemyHolder.transform.parent = transform;
        RemovePhysics(enemyInstance);                                                                                //remove physics from instance


        RemoveMesh(enemyInstance);

        //print(playerInstance.name);
        MoveAnimationToRagdollPosition(enemyInstance, ragdollInstance, "Bip001");                                    //move instance to ragdoll position

        StartCoroutine(waitAwhile(enemyInstance, ragdollInstance)); //Set start
        return _tempEnemyHolder;
    }

    IEnumerator waitAwhile(GameObject playerInstance, GameObject ragdollInstance)
    {
        yield return new WaitForEndOfFrame();

        bool landedOnFront;
        if (LandedOnFront(ragdollInstance))
        {
            _tempEnemyHolder.GetComponent<Enemy_BugEnemy_State>().CurrentSideLanded =
                Enemy_BugEnemy_State.SideLanded.Front;
            playerInstance.GetComponent<Enemy_BugEnemy_State>().CurrentSideLanded =
                Enemy_BugEnemy_State.SideLanded.Front;
            landedOnFront = true;
        }
        else
        {
            _tempEnemyHolder.GetComponent<Enemy_BugEnemy_State>().CurrentSideLanded =
                Enemy_BugEnemy_State.SideLanded.Back;
            playerInstance.GetComponent<Enemy_BugEnemy_State>().CurrentSideLanded =
                Enemy_BugEnemy_State.SideLanded.Back;
            landedOnFront = false;
        }

        playerInstance.GetComponent<EnemyGeneric_StateFinder>().stateAssumePositionEvent(0, landedOnFront);                      //sets animation to start pose
        //>>>>>>>>>_tempEnemyHolder.GetComponent<Player_BugGuy_State>().CurrentSideLanded = playerInstance.GetComponent<Player_BugGuy_State>().CurrentSideLanded;

        if (playerInstance.GetComponent<Enemy_BugEnemy_State>().CurrentSideLanded == Enemy_BugEnemy_State.SideLanded.Front)
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
        _tempEnemyCollisionlessHolder = player;

    }
    /// <summary>
    /// This section of code called after blending
    /// </summary>
    private void CalledAfterBlending()
    {


        _tempEnemyHolder.GetComponent<EnemyGeneric_StateFinder>().stateStartRecoverEvent(); //fire state event indicating end of ragdoll blend
        Destroy(_tempRagdollHolder);
        _tempEnemyHolder.transform.position = _tempEnemyCollisionlessHolder.transform.position;
        _tempEnemyHolder.transform.rotation = _tempEnemyCollisionlessHolder.transform.rotation;


        Destroy(_tempEnemyCollisionlessHolder);
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
    /// <param name="enemyAnimation">Player animation version</param>
    /// <param name="enemyRagdoll">Player ragdoll version</param>
    private void MoveAnimationToRagdollPosition(GameObject enemyAnimation, GameObject enemyRagdoll, string nameOfSharedObject)
    {
        GameObject TempPositionHolder = new GameObject("TempPositionHolder");   //create temp object for moving Player Animation version

        //RaycastHit[] hitInfo;

        //find biped base and store position in TempPosition Holder
        foreach (Transform childTransform in enemyAnimation.GetComponentsInChildren<Transform>())
        {
            if (childTransform.gameObject.name == nameOfSharedObject)                             //if this object is matching object
            {
                TempPositionHolder.transform.position = childTransform.position;       //move position holder to base bipeds position and rotation
                TempPositionHolder.transform.rotation = childTransform.rotation;        //  

                TempPositionHolder.transform.parent = enemyAnimation.transform.parent.transform; //make temp-position-holder share the same parent as biped base
                enemyAnimation.transform.parent = TempPositionHolder.transform;                  //make animated game object a child of temp-position-holder
                break;
            }
        }
        //find ragdoll base and move temp-position-holder to that position
        foreach (Transform childTransform in enemyRagdoll.GetComponentsInChildren<Transform>())
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
                    if (rigidChild.isKinematic)
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
        enemyAnimation.transform.parent = transform; //re-assign parent
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
}

