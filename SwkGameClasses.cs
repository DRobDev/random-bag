using System.Linq;
using UnityEngine;
using System.Collections;
/// <summary>
/// SwkGameClasses.cs is a collection of static classes for controlling single-instance game components
/// </summary>
public abstract class SwkGameClasses : MonoBehaviour
{
    //Public Static
    public static SimplePlatformTypes CurrentPlatform;
    public static PlayableAreaBouns PlayableArea;
    public static GameState CurrentGameState;
    public static InputData CurrentInput;
    public static CameraData CurrentCamera;
    public static SwkSwork CurrentTargetSwork;
    //Private Static

    /// <summary>
    /// Initializes generalized game variables
    /// </summary>
    /// <param name="boundsMesh">mesh to calculate bounds from</param>
    /// <param name="numberOfSworks">number of sworks to be used this level</param>
    /// <param name="sworkPrefab">prefab of swork to sapwn</param>
    /// <param name="holdingArea">off map position to store dead sworks</param>
    /// <param name="cameraUsed">the main camera used</param>
    public static void ReadyGame(MeshRenderer boundsMesh, int numberOfSworks, GameObject sworkPrefab, Transform holdingArea, Camera cameraUsed)
    {
        //initialize protected classes
        SworkControl.ReadySworkControl(numberOfSworks, sworkPrefab, holdingArea);
        InputControl.ReadyInput(Application.platform);
        CameraControls.ReadyCamera(cameraUsed);

        //set game start state
        CurrentGameState.CurrentPlayState = GameState.PlayState.Play;
        CurrentGameState.CurrentControlState = GameState.ControlState.ManualControl;

        CalculateBound(boundsMesh);
    }


    ////////////////////////////////////////////////////////////////////////////////
    //Handles gathering of platform specific input---------------------------------
    protected class InputControl
    {
        //Private Static
        private static float _startTime;
        //Private Static Temp
        private static float _scrollAmount;
        private static Vector3 _desiredPosition;

        //Called Once
        //ready input for desired type
        public static void ReadyInput(RuntimePlatform desiredPlatform)
        {
            CurrentPlatform = SimplePlatformTypes.Pc;
        }

        //Called Each Frame
        //checks for input from specific platform type
        public static void CollectInput()
        {
            //'StartInput'       true for one frame at start of input
            //'ContinuousInput'  true while input held
            //'EndInput'         true for one frame at end of input
            //'FullClick'        true for one frame if pressed and released faster than 'ClickLength'
            switch (CurrentPlatform)
            {
                case SimplePlatformTypes.Pc://--------------------------------------PC
                    //main input
                    CurrentInput.Start = false;
                    CurrentInput.Continuous = false;
                    CurrentInput.End = false;
                    CurrentInput.Clicked = false;
                    if (Input.GetMouseButtonDown(0))
                    {
                        CurrentInput.Start = true;
                        CurrentInput.Continuous = true;
                        _desiredPosition = Input.mousePosition;//get input world position
                        _desiredPosition.z = -CurrentCamera.ZPosition;
                        CurrentInput.WorldPosition = CurrentCamera.Cam.ScreenToWorldPoint(_desiredPosition);
                        CurrentInput.FirstPosition = CurrentInput.WorldPosition;
                        _startTime = Time.time;
                    }
                    else if (Input.GetMouseButton(0))
                    {
                        CurrentInput.Continuous = true;
                        _desiredPosition = Input.mousePosition;//get input world position
                        _desiredPosition.z = -CurrentCamera.ZPosition;
                        CurrentInput.WorldPosition = CurrentCamera.Cam.ScreenToWorldPoint(_desiredPosition);
                    }
                    if (Input.GetMouseButtonUp(0))
                    {
                        CurrentInput.End = true;
                        _desiredPosition = Input.mousePosition;//get input world position
                        _desiredPosition.z = -CurrentCamera.ZPosition;
                        CurrentInput.WorldPosition = CurrentCamera.Cam.ScreenToWorldPoint(_desiredPosition);
                        CurrentInput.EndPosition = CurrentInput.WorldPosition;
                        if (Time.time - _startTime < SwkGame.Tweaks.Input.ClickLength)
                            CurrentInput.Clicked = true;
                    }
                    //zoom
                    _scrollAmount = Input.GetAxisRaw("Mouse ScrollWheel");
                    if (_scrollAmount != 0) //zoom out
                        CurrentInput.ZoomAdjust = _scrollAmount * 10;
                    else
                        CurrentInput.ZoomAdjust = 0;

                    break;
            }
        }
    }
    ////////////////////////////////////////////////////////////////////////////////

    ///////////////////////////////////////////////////////////////////////////////
    //Controls that act on all sworks---------------------------------------------
    protected class SworkControl
    {
        //Public Static
        public static SwkSwork[] AllSworks;
        public static SwkJumpPad[] AllJumpPads;

        public static Queue StandBySworksQueue;
        public static SwkSwork StoredTarget;
        public static SwkJumpPad StoredJumpPad;
        //Private Static
        private static float _startTime;
        private static float _nearestDistance;
        private static SwkSwork _nearestSworkSoFar;
        private static SwkJumpPad _recentlyPickedUp;
        //Private Static Temp
        private static float _distance;
        private static Vector3 _desiredPos;

        //Called Once//
        //Adds desired number of Sworks to scene
        public static void ReadySworkControl(int numberOfSworks, GameObject sworkPrefab, Transform holdingAreaTran)
        {
            //initialize required components
            AllJumpPads = (SwkJumpPad[])FindObjectsOfType(typeof(SwkJumpPad));
            AllSworks = new SwkSwork[numberOfSworks];
            StandBySworksQueue = new Queue(AllSworks.Length);

            GameObject tempGo;
            for (int i = 0; i < numberOfSworks; i++)
            {
                tempGo = (GameObject)Instantiate(sworkPrefab, holdingAreaTran.position, Quaternion.identity);
                AllSworks[i] = tempGo.GetComponent<SwkSwork>();
                AllSworks[i].LocalInitialize();
                AllSworks[i].MakeStandby(holdingAreaTran.position);
                StandBySworksQueue.Enqueue(AllSworks[i]);
            }

            for (int i = 0; i < AllJumpPads.Length; i++)
            {
                AllJumpPads[i].LocalInitialize();
            }
        }
        //find swork to target
        public static void TargetNearestSwork()
        {
            if (CurrentTargetSwork) return;                 //end; swork already targetted   
            //find nearest swork in range
            _nearestDistance = SwkGame.Tweaks.Game.ManualSelectionRange;
            foreach (SwkSwork swork in AllSworks)
            {
                _distance = Vector3.Distance(swork.SwkTran.position, CurrentInput.WorldPosition);
                if (_distance < _nearestDistance)
                {
                    _nearestDistance = _distance;
                    _nearestSworkSoFar = swork;
                }
            }

            if (_nearestDistance == SwkGame.Tweaks.Game.ManualSelectionRange) return;//end; no Swork in range
            //target nearest
            CurrentTargetSwork = _nearestSworkSoFar;
            CurrentTargetSwork.CurrentState = SwkSwork.SworkStates.TargetInPlay;
            CurrentGameState.CurrentControlState = GameState.ControlState.TargetAquired;
        }
        //explode swork
        public static void ExplodeTargetSwork(Transform holdingArea)
        {
            //explode targer
            if (CurrentTargetSwork.Carrying)
            {
                if (CurrentTargetSwork.OnGround)
                { }//drop carrying
                else
                { }//float carrying
            }
            CurrentGameState.CurrentControlState = GameState.ControlState.ManualControl;
            CurrentCamera.Focused = false;

            //explode nearby sworks
            foreach (SwkSwork swork in AllSworks)
            {
                if (swork.CurrentState != SwkSwork.SworkStates.TargetInPlay) continue; //check if possible for swork to explode
                if (Vector3.Distance(CurrentTargetSwork.SwkTran.position, swork.SwkTran.position) > SwkGame.Tweaks.Swork.SworkExplosionEffectRange) continue; //check if within range
                if (swork.GetInstanceID() == CurrentTargetSwork.GetInstanceID()) continue;//chat that 'isn't' target swork

                if (swork.Carrying)
                {
                    //drop carrying
                }

                swork.MakeStandby(holdingArea.position);
            }

            //explode nearby jumppads
            foreach (SwkJumpPad jumpPad in AllJumpPads)
            {
                if (jumpPad.CurrentState == SwkJumpPad.JumpPadStates.Floating) continue;
                if (Vector3.Distance(CurrentTargetSwork.SwkTran.position, jumpPad.JmpPTran.position) > SwkGame.Tweaks.Swork.SworkExplosionEffectRange) continue;

                jumpPad.MakeFloating();
                StoredJumpPad = jumpPad;

                CurrentGameState.CurrentControlState = GameState.ControlState.WaitingForNextPickup;
            }


            StandBySworksQueue.Enqueue(CurrentTargetSwork);
            CurrentTargetSwork.MakeStandby(holdingArea.position);

            CurrentTargetSwork = null;



        }

        //Called Each Frame//
        //Runs 'LocalUpdate' for all sworks and jumps in scene
        public static void UpdateLocals()
        {
            foreach (SwkSwork swork in AllSworks)
                swork.LocalUpdate();
            foreach (SwkJumpPad jumpPad in AllJumpPads)
                jumpPad.LocalUpdate();
        }
        //make swork jump if hit's active jumpPad
        public static void MakeSworkJump()
        {}
        //make swork pick-up floating jumpPad
        public static void PickUpFloatingPad()
        {
            foreach (SwkSwork swork in AllSworks)
            {
                if(swork.CurrentState != SwkSwork.SworkStates.TargetInPlay) continue; //skip swork if not in play
                if(swork.Carrying) continue;
                foreach (SwkJumpPad jumpPad in AllJumpPads)
                {
                    if(jumpPad.CurrentState != SwkJumpPad.JumpPadStates.Floating) continue; //skip jumpPad if not floating
                    if(Vector3.Distance(jumpPad.JmpPTran.position, swork.SwkTran.position) > SwkGame.Tweaks.JumpPad.ActivateJumpPadRange) continue;//skip jumppad if out-of-range
                    
                    PickUpJumpPad(swork, jumpPad);//pick up jump
                    _recentlyPickedUp = jumpPad;//
                    return;
                }
            }
        }
        //target first swork that hits 'StoredJumpPad'
        public static void TargetPickupSwork()
        {
            if(_recentlyPickedUp.GetInstanceID() == StoredJumpPad.GetInstanceID())
            {
                
            }
        }
        //Checks if a Swork is out-of-bounds and moves it to the holding area
        public static void CollectOutOfBoundsSworks(Transform holdingArea)
        {
            for (int i = 0; i < AllSworks.Length; i++)
            {
                if (AllSworks[i].InStandby)
                    continue;
                //dead if 'Out-of-Bounds'
                if (OutOfBounds(AllSworks[i].SwkTran.position))//check if out-of-bounds
                {
                    if (CurrentTargetSwork != null && //if it's 'Target'
                        AllSworks[i].GetInstanceID() == CurrentTargetSwork.GetInstanceID())
                    {
                        StoredTarget = CurrentTargetSwork;
                        CurrentTargetSwork.CurrentState = SwkSwork.SworkStates.TargetOutOfBounds;
                        CurrentCamera.Focused = false;
                        CurrentTargetSwork = null;
                    }
                    else
                    {
                        StandBySworksQueue.Enqueue(AllSworks[i]);
                    }
                    AllSworks[i].MakeStandby(holdingArea.position);
                }
            }
        }
        //Spawns 'standby' Sworks at regular intervals
        public static void SpawnStandbySwork(Transform spawnPoint, float spawnDelay)
        {
            if (Time.time - _startTime < spawnDelay) return;            //end; not enough time passed
            if (StandBySworksQueue.Count == 0) return; //end; no sworks in 'standby'

            var swork = (SwkSwork)StandBySworksQueue.Dequeue(); //remove 'standby' swork from queue

            swork.SpawnLocal(spawnPoint.position); //reset local swork conditions
            _startTime = Time.time;

        }
        //Spawns 'StoredTarget' after focusing
        public static void SpawnStoredWhenFocused(Transform spawnPoint)
        {
            _startTime = Time.time;

            if (CurrentCamera.Focused)
            {
                CurrentTargetSwork = StoredTarget; //set as target
                CurrentTargetSwork.SpawnLocal(spawnPoint.position);
                CurrentTargetSwork.CurrentState = SwkSwork.SworkStates.TargetInPlay;
            }
        }


        // makes 'jumpPad' a child of 'swork' and moves into position
        private static void PickUpJumpPad(SwkSwork swork, SwkJumpPad jumpPad)
        {
            jumpPad.JmpPTran.parent = swork.transform;
            _desiredPos = Vector3.zero;
            _desiredPos.x = SwkGame.Tweaks.JumpPad.CarryPositionX;
            _desiredPos.y = SwkGame.Tweaks.JumpPad.CarryPositionY;
            jumpPad.JmpPTran.localPosition = _desiredPos;

            swork.Carrying = true;
            jumpPad.MakeCarrying();
        }
    }
    ///////////////////////////////////////////////////////////////////////////////

    ///////////////////////////////////////////////////////////////////////////////
    //Controls relating to camera function----------------------------------------
    protected class CameraControls
    {
        ////Notes
        // * -if explode
        // * --switch to manual
        // * --if is carrying
        // * ---wait for next pickup
        // * ---if player input during wait
        // * ----switch to manual
        // * ---if picked up
        // * ----follow pickup
        // */

        //Public static


        //private static
        private static bool _startedFocusing;
        private static float _lerpPosition;
        private static float _zoomLevel;
        private static float _startTime;
        private static Vector3 _camStartPos;
        private static Vector3 _offsetInputStartPos;
        //Private static temp
        private static bool _restricted;
        private static float _speed;
        private static Vector3 _desiredPos;
        private static Vector3 _targetPos;
        private static Vector3 _offset;

        //Called Once//
        //position camera
        public static void ReadyCamera(Camera camera)
        {
            CurrentCamera.Cam = camera;
            CurrentCamera.CamTran = CurrentCamera.Cam.transform;
            CurrentCamera.ZPosition = CurrentCamera.CamTran.position.z;
            _zoomLevel = CurrentCamera.Cam.orthographicSize;
        }

        //Called Each Frame//
        //click and drag to manually move camera
        public static void ManualMove()
        {
            if (CurrentInput.Start)//get starting point for camera and input
            {
                _camStartPos = CurrentCamera.CamTran.position;
                _offsetInputStartPos = CurrentInput.FirstPosition;
            }
            if (CurrentInput.Continuous)//move camera with input
            {
                _offset = (_offsetInputStartPos - CurrentInput.WorldPosition) - (_camStartPos - CurrentCamera.CamTran.position);//get offset
                _desiredPos = _camStartPos + _offset;
            }

            if (KeepInsidePlayArea(ref _desiredPos, true, false))//increase manouverablity when being restricted
                _offsetInputStartPos.x = CurrentInput.WorldPosition.x;
            if (KeepInsidePlayArea(ref _desiredPos, false, true))
                _offsetInputStartPos.y = CurrentInput.WorldPosition.y;

            _desiredPos.z = CurrentCamera.ZPosition;//apply to camera position
            CurrentCamera.CamTran.position = _desiredPos;


        }
        //Zoom
        public static void ManualZoom()
        {
            if (CurrentInput.ZoomAdjust == 0) return; //end; no zoom required'

            _zoomLevel += -CurrentInput.ZoomAdjust;               //apply adjustment todo: make smooth

            if (_zoomLevel < SwkGame.Tweaks.Camera.MinZoom)     //cap lower zoom
                _zoomLevel = SwkGame.Tweaks.Camera.MinZoom;
            if (_zoomLevel > SwkGame.Tweaks.Camera.MaxZoom)     //cap upper zoom
                _zoomLevel = SwkGame.Tweaks.Camera.MaxZoom;

            CurrentCamera.Cam.orthographicSize = _zoomLevel;  //update camear zoom

            _desiredPos = CurrentCamera.CamTran.position; //keep camera inside bounds
            KeepInsidePlayArea(ref _desiredPos, true, true);
            CurrentCamera.CamTran.position = _desiredPos;
        }
        //follow target swork 
        public static void FollowTarget()
        {
            //follow target
            _targetPos = CurrentTargetSwork.SwkTran.position;
            _targetPos.z = CurrentCamera.ZPosition;
            _desiredPos = _targetPos;
            if (!CurrentCamera.Focused)//focus on target
            {
                KeepInsidePlayArea(ref _targetPos);
                FocusOnTarget(_targetPos, ref _desiredPos);
            }
            KeepInsidePlayArea(ref _desiredPos);

            CurrentCamera.CamTran.position = _desiredPos;
        }
        //smoothly moves camera over target
        public static void FocusOnTarget(Vector3 targetPos)
        {
            KeepInsidePlayArea(ref targetPos);
            FocusOnTarget(targetPos, ref _desiredPos);
            KeepInsidePlayArea(ref _desiredPos);

            CurrentCamera.CamTran.position = _desiredPos;
        }

        //Internal Functions//
        //smoothly moves camera over target
        private static void FocusOnTarget(Vector3 targetPos, ref Vector3 desiredPos)
        {
            targetPos.z = CurrentCamera.ZPosition;

            if (!_startedFocusing)                         //runs once at start of focus
            {
                _startedFocusing = true;
                _camStartPos = CurrentCamera.CamTran.position;
                _startTime = Time.time;
                _speed = Vector3.Distance(_camStartPos, targetPos);
            }

            _lerpPosition = Time.time - _startTime;          //calculate speed based on distance to target
            _lerpPosition = _lerpPosition * SwkGame.Tweaks.Camera.FocusSpeedMultiplyer;
            _lerpPosition = _lerpPosition / _speed;

            if (_lerpPosition <= 1)                          //runs every frame until time passed
            {
                desiredPos = Vector3.Slerp(_camStartPos, targetPos, _lerpPosition);

            }
            else
            {
                _startedFocusing = false;
                CurrentCamera.Focused = true;
            }
        }
        //restrict camera to bounds X
        private static void KeepInsidePlayArea(ref Vector3 restrictedPos)
        {
            KeepInsidePlayArea(ref restrictedPos, true, true);
            return;
        }
        private static bool KeepInsidePlayArea(ref Vector3 restrictedPos, bool x, bool y)
        {
            _restricted = false;
            //restrict bounds along 'x' axis
            if (x)
            {
                if (restrictedPos.x > PlayableArea.TopRight.x - (CurrentCamera.Cam.orthographicSize * CurrentCamera.Cam.aspect))
                {
                    restrictedPos.x = PlayableArea.TopRight.x - (CurrentCamera.Cam.orthographicSize * CurrentCamera.Cam.aspect);
                    _restricted = true;
                }
                else if (restrictedPos.x < PlayableArea.BottomLeft.x + (CurrentCamera.Cam.orthographicSize * CurrentCamera.Cam.aspect))
                {
                    restrictedPos.x = PlayableArea.BottomLeft.x + (CurrentCamera.Cam.orthographicSize * CurrentCamera.Cam.aspect);
                    _restricted = true;
                }
            }
            //restrict bounds along 'y' axis
            if (y)
            {
                if (restrictedPos.y > PlayableArea.TopRight.y - CurrentCamera.Cam.orthographicSize)
                {
                    restrictedPos.y = PlayableArea.TopRight.y - CurrentCamera.Cam.orthographicSize;
                    _restricted = true;
                }
                else if (restrictedPos.y < PlayableArea.BottomLeft.y + CurrentCamera.Cam.orthographicSize)
                {
                    restrictedPos.y = PlayableArea.BottomLeft.y + CurrentCamera.Cam.orthographicSize;
                    _restricted = true;
                }
            }
            return _restricted;
        }

    }
    ///////////////////////////////////////////////////////////////////////////////


    /// <summary>
    /// Check if submitted position is out of playable area
    /// </summary>
    /// <param name="position">position to check</param>
    /// <returns>true if out of bounds</returns>
    private static bool OutOfBounds(Vector3 position)
    {
        if (position.x > PlayableArea.TopRight.x)
            return true;
        if (position.x < PlayableArea.BottomLeft.x)
            return true;
        if (position.y > PlayableArea.TopRight.y)
            return true;
        if (position.y < PlayableArea.BottomLeft.y)
            return true;
        return false;
    }
    /// <summary>
    /// calculate playable area
    /// </summary>
    /// <param name="boundsMesh">mesh representing playable area</param>
    private static void CalculateBound(MeshRenderer boundsMesh)
    {
        //calculate bounds mesh
        if (!boundsMesh)
            PlayableArea = new PlayableAreaBouns
            {
                BottomLeft = -new Vector3(float.MaxValue, float.MaxValue),
                TopRight = new Vector3(float.MaxValue, float.MaxValue)
            };
        else
            PlayableArea = new PlayableAreaBouns
            {
                BottomLeft = boundsMesh.bounds.min,
                TopRight = boundsMesh.bounds.max
            };

        PlayableArea.TopRight.z = 0;
        PlayableArea.BottomLeft.z = 0;
    }

    public struct InputData
    {
        public bool Start;
        public bool Continuous;
        public bool End;
        public bool Clicked;
        public float ZoomAdjust;
        public Vector3 FirstPosition;
        public Vector3 WorldPosition;
        public Vector3 EndPosition;
        public static bool StartInput;//true for one frame at start of input
        public static bool EndInput;  //true for one frame at end of input
        public static bool FullClick; //true for one frame at the end of a click
        public static bool ContinuousInput; //true for every frame input held
    }
    public struct CameraData
    {
        public bool Focused;
        public float ZPosition;
        public Camera Cam;
        public Transform CamTran;
    }
    public struct AllTweaks
    {
        public SworkTweaks Swork;
        public JumpPadTweaks JumpPad;
        public CameraTweaks Camera;
        public InputTweaks Input;
        public GameTweaks Game;

        public struct JumpPadTweaks
        {
            public float ActivateJumpPadRange;
            public float CarryPositionX, CarryPositionY;
        }

        public struct SworkTweaks
        {
            public float MovementSpeed;
            public float GroundDetectionRange;
            public float SworkExplosionEffectRange;
        }

        public struct CameraTweaks
        {
            public float MaxZoom;
            public float MinZoom;
            public float FocusSpeedMultiplyer;
        }

        public struct InputTweaks
        {
            public float ClickLength;
        }

        public struct GameTweaks
        {
            public float ManualSelectionRange;
        }

    }
    public struct PlayableAreaBouns
    {
        public Vector3 TopRight;
        public Vector3 BottomLeft;
    }
    public struct GameState
    {
        public PlayState CurrentPlayState;
        public ControlState CurrentControlState;

        public enum ControlState
        {
            TargetAquired,
            ManualControl,
            TargetOutOfBounds,
            WaitingForNextPickup,
            WaitingForNextJump,
            Other
        }
        public enum PlayState
        {
            Play,
            Paused,
            Other
        }
    }
    public enum SimplePlatformTypes
    {
        Pc,
        Other
    }

}
