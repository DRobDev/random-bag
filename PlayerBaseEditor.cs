using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(PlayerBase))] //custom editor for PlayerBase.cs

public class PlayerBaseEditor : Editor
{
    //VARIABLES-----------------------------------------------
    public MainTabType CurrentTab = MainTabType.Start; //the current tab open in the editor
    private bool _moreThanOneCopy;
    //--------------------------------------------------------


    // called once when script is enabled---------------------
    public void OnEnable()
    {
        // checks for duplicate instances of 'PlayerBase'
        PlayerBase[] playerBases = FindObjectsOfType(typeof(PlayerBase)) as PlayerBase[];
        if (playerBases.Length > 1)
        {
            _moreThanOneCopy = true;             //disable inspector
            foreach (PlayerBase playerBase in playerBases)
            {
                Debug.LogWarning("PlayerBase found in : " + playerBase.gameObject.name);
            }
        }
    }
    //--------------------------------------------------------



    // updates that happen in the inspector-------------------
    public override void OnInspectorGUI()
    {
        // Display warning if more than one instance of 'PlayerBase' exists in the scene
        if (_moreThanOneCopy)
        {
            GUILayout.Label("More than one copy of PlayerBase exists in the scene", EditorStyles.notificationBackground);
            GUILayout.Label("Please make sure to use only one copy of 'PlayerBase' > check Console for details",
                            EditorStyles.miniLabel);
            return;
        }

        // set skin to normal
        GUI.skin = null;

        // Initialize playerBaseTarget to access component
        PlayerBase playerBaseTarget = target as PlayerBase; //get access to 'PlayerBase.cs'


        // Toggle debugging display
        playerBaseTarget.ShowDebugging = GUILayout.Toggle(playerBaseTarget.ShowDebugging, "Toggle Debugging Display?"); //displays icons and other debugging info
        // Toggle automatic ragdoll swapping override
        if (playerBaseTarget.ShowDebugging) _overrideAutomaticRagdollSwapping = GUILayout.Toggle(_overrideAutomaticRagdollSwapping, "Override automatic Ragdoll swapping?");
        // Display buttons for overriding ragdoll swapping
        if (_overrideAutomaticRagdollSwapping) ManualRagdollSwappingButtons(playerBaseTarget);
        EditorGUILayout.Separator();



        // Main Tabs ------------------------------------------------
        // Creates three main tabs at the top of the inspector pane
        CurrentTab = (MainTabType)GUILayout.Toolbar((int)CurrentTab, new string[3] { "Start", "Movement", "Animation" }); //list main tabs across the top of the editor; current tab equals the main tab clicked
        // Fills the tab based on current selection
        switch (CurrentTab)
        {
            case MainTabType.Start:
                StartTab(playerBaseTarget); //Contain all necessary start-up requirements
                break;
            case MainTabType.Movement:
                MovementTab(playerBaseTarget);
                break;
            case MainTabType.Animation:
                AnimationTab(playerBaseTarget);
                break;
        }
        //-----------------------------------------------------------

        if (GUI.changed) //apply changes made in editor ??I think.. TODO find out more about this
        {
            EditorUtility.SetDirty(target);
        }
    }
    //--------------------------------------------------------



    // updates that happen in the editor scene view-----------
    public void OnSceneGUI()
    {
        // Initialize------------------------------------------------
        //PlayerBase playerBaseTarget = target as PlayerBase;
        //-----------------------------------------------------------


        //REPAINT!!
        Repaint();
        HandleUtility.Repaint();
    }
    //--------------------------------------------------------



    //MAIN TABS-----------------------------------------------
    // contains all options displayed when its tab is active
    /// <summary>
    /// This function holds all information displayed when 'StartTab' is selected
    /// </summary>
    /// <param name="playerBaseTarget"></param>
    public void StartTab(PlayerBase playerBaseTarget)
    {
        //Display tip if no 'Main Player' game object assigned 
        if (playerBaseTarget.MainPlayer == null)
            GUILayout.Label("\nPlace the controllable player prefab here\n(This will be the first character instance to spawn)", EditorStyles.miniLabel);
        //Object field for main player to be assigned
        playerBaseTarget.MainPlayer = (GameObject)EditorGUILayout.ObjectField("Main Player", playerBaseTarget.MainPlayer,
                     typeof(GameObject));

        //Display tip if no 'Main Player Ragdoll' has been assigned
        if (playerBaseTarget.MainPlayerRagdoll == null)
            GUILayout.Label("\nPlace the prefab Ragdoll used for the Main Player here\n(The ragdoll must have matching bone names of Main Player)", EditorStyles.miniLabel);
        //Object field for 'Main Player Ragdoll' to be assigned
        playerBaseTarget.MainPlayerRagdoll =
            (GameObject)EditorGUILayout.ObjectField("Main Player Ragdoll", playerBaseTarget.MainPlayerRagdoll,
                                                     typeof(GameObject));

        //Display tip if no 'Main Player Center' assigned
        if (playerBaseTarget.MainPlayerCenterName == "")
            GUILayout.Label("\nType Center name here\n(This must be contained in both Controllable player and Ragdoll e.g. 'Bip001'", EditorStyles.miniLabel);
        playerBaseTarget.MainPlayerCenterName = EditorGUILayout.TextField("Main Player Center Object", playerBaseTarget.MainPlayerCenterName);

    }

    /// <summary>
    /// This function hold all information displayed when 'MovementTab' is selected
    /// </summary>
    /// <param name="playerBaseTarget"></param>
    public void MovementTab(PlayerBase playerBaseTarget)
    {
        // display label
        GUILayout.Label("Navigation Tweaking", EditorStyles.boldLabel);
        // Int for movement speed
        playerBaseTarget.MoveSpeed = EditorGUILayout.IntField("Movement Speed", playerBaseTarget.MoveSpeed);
        // Inf for jump amount
        playerBaseTarget.JumpForce = EditorGUILayout.IntField("Jump Amount", playerBaseTarget.JumpForce);

        // display label
        GUILayout.Label("Turning", EditorStyles.boldLabel);
        // float for minimum speed before turning
        playerBaseTarget.SpeedToReachBeforeTurning = EditorGUILayout.FloatField("Speed To Reach Before Turning",
                                                                                playerBaseTarget.
                                                                                    SpeedToReachBeforeTurning);
        // float for turn smoothing 
        playerBaseTarget.SmoothTurnAmount = EditorGUILayout.FloatField("Smooth Turning Amount",
                                                                       playerBaseTarget.SmoothTurnAmount);
    }

    /// <summary>
    /// This function holds all information displayed when 'Animation' tab is active
    /// </summary>
    /// <param name="playerBaseTarget"></param>
    public void AnimationTab(PlayerBase playerBaseTarget)
    {
        //display label--------
        GUILayout.Label("Animation Tweaking", EditorStyles.boldLabel);
        playerBaseTarget.SpeedToReachBeforeRunning = EditorGUILayout.IntField("Speed to reach before Running",
                                                                                playerBaseTarget.
                                                                                    SpeedToReachBeforeRunning);
        //display label--------
        EditorGUILayout.Separator();
        GUILayout.Label("Animation Clips", EditorStyles.boldLabel);
        playerBaseTarget.AnimRunning = (AnimationClip)EditorGUILayout.ObjectField("Run Animation", playerBaseTarget.AnimRunning, typeof(AnimationClip));
        playerBaseTarget.AnimWalk = (AnimationClip)EditorGUILayout.ObjectField("Walk Animation", playerBaseTarget.AnimWalk, typeof(AnimationClip));
        playerBaseTarget.AnimIdle = (AnimationClip)EditorGUILayout.ObjectField("Idle Animation", playerBaseTarget.AnimIdle, typeof(AnimationClip));
        //display label--------
        GUILayout.Label("Recover Animations", EditorStyles.miniLabel);
        playerBaseTarget.AnimRecoverFront = (AnimationClip)EditorGUILayout.ObjectField("Front Recover Animation", playerBaseTarget.AnimRecoverFront, typeof(AnimationClip));
        playerBaseTarget.AnimRecoverFrontSampleTime = EditorGUILayout.FloatField("Sample Time",
                                                                                 playerBaseTarget.
                                                                                     AnimRecoverFrontSampleTime, EditorStyles.miniLabel);
        playerBaseTarget.AnimRecoverBack = (AnimationClip)EditorGUILayout.ObjectField("Back Recover Animation", playerBaseTarget.AnimRecoverBack, typeof(AnimationClip));
        playerBaseTarget.AnimRecoverBackSampleTime = EditorGUILayout.FloatField("Sample Time",
                                                                                 playerBaseTarget.
                                                                                    AnimRecoverBackSampleTime, EditorStyles.miniLabel);
        playerBaseTarget.AnimRecoverStanding = (AnimationClip)EditorGUILayout.ObjectField("Standing Recover Animation", playerBaseTarget.AnimRecoverStanding, typeof(AnimationClip));
        playerBaseTarget.AnimRecoverStandingSampleTime = EditorGUILayout.FloatField("Sample Time",
                                                                                 playerBaseTarget.
                                                                                     AnimRecoverStandingSampleTime, EditorStyles.miniLabel);

        //display label--------
        EditorGUILayout.Separator();
        GUILayout.Label("Current Player States", EditorStyles.boldLabel);
        //display current player type (ragdoll, controllable or other)
        GUILayout.BeginHorizontal();
        GUILayout.Label("Current Player Type");
        GUILayout.Label(playerBaseTarget.CurrentPlayerTypeUsed.ToString());
        GUILayout.EndHorizontal();
        //display current looping animation
        GUILayout.BeginHorizontal();
        GUILayout.Label("Current looping anim");
        GUILayout.Label(playerBaseTarget.CurrentAnimationStateLooping.ToString());
        GUILayout.EndHorizontal();
    }
    //-------------------------------------------------------
    //-------------------------------------------------------
    public void OnGUI()
    {

    }


    //Viewport Icons-----------------------------------------
    // a collection of small functions that display custom 
    //icons in the view-port window

    public void OnDrawGizmos()
    { }
    //-------------------------------------------------------






    //Main tabs that run across the top of the inspector pane-
    public enum MainTabType
    {
        Start,
        Movement,
        Animation
    }
    //--------------------------------------------------------


    // Debugging----------------------------------------------
    private bool _overrideAutomaticRagdollSwapping; //weather or not to override automatic ragdoll swapping
    /// <summary>
    /// Disables automatic ragdoll swapping; replacing it with manual buttons
    /// </summary>
    void ManualRagdollSwappingButtons(PlayerBase pbTarget)
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Swap To Ragdoll")) //when 'swap to ragdoll' pressed
        {
            if (pbTarget.PlayerInstance == null)//if player exists in scene
            {
                Debug.Log("No Instance of Controllable Player In Scene");
            }
            else if (pbTarget.CurrentPlayerTypeUsed == PlayerBase.CurrentPlayerType.Controllable)
            {
                pbTarget.SwapToRagdoll();//swap to ragdoll
            }
            else
            {
                Debug.Log("Can only call when controlling character");
            }
        }
        if (GUILayout.Button("Swap To Controllable"))
        {
            if (pbTarget.RagdollInstance == null)//if ragdoll exists in scene
            {
                Debug.Log("No Instance of Ragdoll In Scene");
            }
            else if (pbTarget.CurrentPlayerTypeUsed == PlayerBase.CurrentPlayerType.Ragdoll)
            {
                pbTarget.SwapToControllable();//swap to controllable
            }
            else
            {
                Debug.Log("Can only call when ragdoll is in use");
            }
        }
        GUILayout.EndHorizontal();

    }
    //--------------------------------------------------------

}
