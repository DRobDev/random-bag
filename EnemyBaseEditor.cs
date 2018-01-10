using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(EnemyBase))] //custom editor for EnemyBase.cs

public class EnemyBaseEditor : Editor
{
    //VARIABLES-----------------------------------------------
    public MainTabType CurrentTab = MainTabType.Start; //the current tab open in the editor
    //--------------------------------------------------------


    // called once when script is enabled---------------------
    public void OnEnable()
    {

    }
    //--------------------------------------------------------



    // updates that happen in the inspector-------------------
    public override void OnInspectorGUI()
    {
        // set skin to normal
        GUI.skin = null;

        // Initialize playerBaseTarget to access component
        EnemyBase enemyBaseTarget = target as EnemyBase; //get access to 'EnemyBase.cs'


        // Toggle debugging display
        enemyBaseTarget.ShowDebugging = GUILayout.Toggle(enemyBaseTarget.ShowDebugging, "Toggle Debugging Display?"); //displays icons and other debugging info
        // Toggle automatic ragdoll swapping override
        if (enemyBaseTarget.ShowDebugging) _overrideAutomaticRagdollSwapping = GUILayout.Toggle(_overrideAutomaticRagdollSwapping, "Override automatic Ragdoll swapping?");
        // Display buttons for overriding ragdoll swapping
        if (_overrideAutomaticRagdollSwapping) ManualRagdollSwappingButtons(enemyBaseTarget);
        EditorGUILayout.Separator();



        // Main Tabs ------------------------------------------------
        // Creates three main tabs at the top of the inspector pane
        CurrentTab = (MainTabType)GUILayout.Toolbar((int)CurrentTab, new string[3] { "Start", "Movement", "Animation" }); //list main tabs across the top of the editor; current tab equals the main tab clicked
        // Fills the tab based on current selection
        switch (CurrentTab)
        {
            case MainTabType.Start:
                StartTab(enemyBaseTarget); //Contain all necessary start-up requirements
                break;
            case MainTabType.Movement:
                MovementTab(enemyBaseTarget);
                break;
            case MainTabType.Animation:
                AnimationTab(enemyBaseTarget);
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
        //EnemyBase playerBaseTarget = target as EnemyBase;
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
    public void StartTab(EnemyBase playerBaseTarget)
    {
        //Display tip if no 'Main Enemy' game object assigned 
        if (playerBaseTarget.MainPlayer == null)
            GUILayout.Label("\nPlace the controllable Enemy prefab here\n(This will be the first character instance to spawn)", EditorStyles.miniLabel);
        //Object field for main Enemy to be assigned
        playerBaseTarget.MainPlayer = (GameObject)EditorGUILayout.ObjectField("Main Enemy", playerBaseTarget.MainPlayer,
                     typeof(GameObject));

        //Display tip if no 'Main Enemy Ragdoll' has been assigned
        if (playerBaseTarget.MainPlayerRagdoll == null)
            GUILayout.Label("\nPlace the prefab Ragdoll used for the Main Enemy here\n(The ragdoll must have matching bone names of Main Enemy)", EditorStyles.miniLabel);
        //Object field for 'Main Enemy Ragdoll' to be assigned
        playerBaseTarget.MainPlayerRagdoll =
            (GameObject)EditorGUILayout.ObjectField("Main Enemy Ragdoll", playerBaseTarget.MainPlayerRagdoll,
                                                     typeof(GameObject));

        //Display tip if no 'Main Enemy Center' assigned
        if (playerBaseTarget.MainPlayerCenterName == "")
            GUILayout.Label("\nType Center name here\n(This must be contained in both Controllable Enemy and Ragdoll e.g. 'Bip001'", EditorStyles.miniLabel);
        playerBaseTarget.MainPlayerCenterName = EditorGUILayout.TextField("Main Enemy Center Object", playerBaseTarget.MainPlayerCenterName);
    }

    /// <summary>
    /// This function hold all information displayed when 'MovementTab' is selected
    /// </summary>
    /// <param name="playerBaseTarget"></param>
    public void MovementTab(EnemyBase playerBaseTarget)
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
    public void AnimationTab(EnemyBase playerBaseTarget)
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
        GUILayout.Label("Current Enemy States", EditorStyles.boldLabel);
        //display current Enemy type (ragdoll, controllable or other)
        GUILayout.BeginHorizontal();
        GUILayout.Label("Current Enemy Type");
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
    void ManualRagdollSwappingButtons(EnemyBase pbTarget)
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Swap To Ragdoll")) //when 'swap to ragdoll' pressed
        {
            if (pbTarget.PlayerInstance == null)//if Enemy exists in scene
            {
                Debug.Log("No Instance of Controllable Enemy In Scene");
            }
            else if (pbTarget.CurrentPlayerTypeUsed == EnemyBase.CurrentPlayerType.Active)
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
            else if (pbTarget.CurrentPlayerTypeUsed == EnemyBase.CurrentPlayerType.Ragdoll)
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
