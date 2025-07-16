using GamingIsLove.Makinom;
using GamingIsLove.ORKFramework;

using MenteBacata.ScivoloCharacterController;

using MMOCombatantController.Runtime.Environment;

using UnityEngine;

namespace MMOCombatantController.Runtime.Core
{
    public class MMOCombatantCharacterController : MonoBehaviour, IMovementComponent
    {
        #region Component References

        private Animator _animator;
        private CharacterCapsule _characterCapsule;
        private CharacterMover _characterMover;
        private GroundDetector _groundDetector;
        private BaseMovingPlatform _baseMovingPlatform;    
        
        #endregion
        
        #region Editor - Settings

        [SerializeField] private bool allowDoubleJump;
        [SerializeField] private float jumpHeight;
        [SerializeField] private float gravity;
        [SerializeField] private float minVerticalSpeed;
        [SerializeField] private float rotationSpeed;
        [SerializeField] private float swimLevel;
        [SerializeField] private float swimStrength;
        private MoveSpeed<GameObjectSelection> swimMoveSpeed;
        [SerializeField] private bool useHeadIK;
        
        #endregion
        
        #region Properties
        
        public Combatant Combatant { get; private set; }
        
        public CombatantControllerInputs Inputs { get; set; }
        
        public MMOCombatantThirdPersonFollowCameraController MMOCombatantThirdPersonFollowCameraController { get; set; }

        public float Height => _characterCapsule.Height;

        public Vector3 MoveInputs => _moveInput;
        
        public bool InWater { get; set; }

        public float CurrentWaterSurfaceLevel { get; set; }

        private bool IsOnMovingPlatform { get; set; }
        
        private float WalkSpeed => Combatant.Setting.moveSettings.MoveSpeed.GetWalkSpeed(Combatant);
        private float RunSpeed => Combatant.Setting.moveSettings.MoveSpeed.GetRunSpeed(Combatant);
        private float SwimSpeed => swimMoveSpeed.GetCombatantSpeed(Combatant);
        
        #endregion
        
        #region Private Fields
        
        private enum MoveState
        {
            Grounded,
            Airborne,
            Swimming,
            Sliding
        }
        
        private Vector3 _moveInput;
        private float _currentSpeed; 
        private MoveState _currentMoveState;
        private bool _autoRun;
        private bool _cameraAutoRun;
        private bool _walking; 
        private bool _groundDetected = true;
        private float _verticalSpeed = -10;
        private bool _canDoubleJump;
        private float _distanceFromWaterSurface;
        private float _weightIK;
        
        #endregion
        
        #region Scivolo Variables
        
        private readonly Collider[] _overlaps = new Collider[5];
        private int _overlapCount;
        private float _nextUngroundedTime = -1.0f; // Time after which the character should be considered ungrounded.
        private const float TimeBeforeUngrounded = 0.02f; // Time before the character is set to ungrounded from the last time he was safely grounded.
        private readonly MoveContact[] _moveContacts = CharacterMover.NewMoveContactArray;
        private int _contactCount;
        
        #endregion

        #region Animator Hashes
        
        private readonly int _groundHash = Animator.StringToHash("Grounded");
        private readonly int _jumpHash = Animator.StringToHash("Jump");
        private readonly int _xAxisMovementHash = Animator.StringToHash("X Axis Movement");
        private readonly int _zAxisMovementHash = Animator.StringToHash("Z Axis Movement");
        private readonly int _rotationHash = Animator.StringToHash("Rotation");
        private readonly int _movementSpeedHash = Animator.StringToHash("Movement Speed");
        private readonly int _swimmingHash = Animator.StringToHash("Swimming");
        private readonly int _yAxisMovementHash = Animator.StringToHash("Y Axis Movement");
        
        #endregion
        
        #region Unity Lifecycle Methods

        protected void Awake()
        {
            if (!TryGetComponent(out _characterCapsule))
            {
                InternalDebug.LogErrorFormat($"Character Capsule not found on {gameObject.name}");
            }
            
            if (!TryGetComponent(out _characterMover))
            {
                InternalDebug.LogErrorFormat($"Character Mover not found on {gameObject.name}");
            }
            
            if (!TryGetComponent(out _groundDetector))
            {
                InternalDebug.LogErrorFormat($"Ground Detector not found on {gameObject.name}");
            }
            
            if (!TryGetComponent(out _animator))
            {
                InternalDebug.LogWarning($"Animator not found on {gameObject.name}");
            }

            Inputs = new CombatantControllerInputs();
        }

        protected void Start()
        {
            Combatant = ORKComponentHelper.GetCombatant(gameObject);
            if (Combatant == null)
            {
                InternalDebug.LogWarning("Combatant not found.", gameObject);
            }
            
            if (InWater)
            {
                _currentMoveState = MoveState.Swimming;
            }
            else
            {
                HandleOverlaps();
                _groundDetected = DetectGroundAndCheckIfGrounded(
                    out bool isGrounded,
                    out GroundInfo groundInfo
                );

                if (_groundDetected && !isGrounded)
                {
                    _currentMoveState = MoveState.Sliding;
                }
                else if (isGrounded)
                {
                    _currentMoveState = MoveState.Grounded;
                }
                else
                {
                    _currentMoveState = MoveState.Airborne;
                }

                _currentMoveState = isGrounded ? MoveState.Grounded : MoveState.Airborne;
            }
        }

        protected void Update()
        {
            IsOnMovingPlatform = false;
            
            GetControllerInputs();
            HandleOverlaps();
            CheckWaterLevel();

            if (_currentMoveState == MoveState.Swimming)
            {
                SwimmingLocomotion();
            }
            else
            {
                _animator.SetBool(_swimmingHash, false);
                GroundedLocomotion();
            }
        }
        
        protected void LateUpdate()
        {
            if (IsOnMovingPlatform)
            {
                ApplyPlatformMovement(_baseMovingPlatform);
            }
        }
        
        #endregion

        #region Unity Callbacks
        
        private void OnAnimatorIK(int layerIndex)
        {
            //From landing animation, slowly build weight back up to give it smoothing time
            if (_weightIK < 0.75f)
            {
                _weightIK = Mathf.Clamp(_weightIK + Time.deltaTime, 0, 0.75f);
            }
            
            if (useHeadIK && MMOCombatantThirdPersonFollowCameraController != null)
            {
                //Set combatant's IK look position to where the camera is looking
                _animator.SetLookAtWeight(_weightIK, _weightIK / 3, 0.75f, 1, 0.5f);
                _animator.SetLookAtPosition(
                    transform.position
                        + new Vector3(0, 1.5f, 0)
                        + MMOCombatantThirdPersonFollowCameraController.transform.forward
                );
            }
        }
        
        #endregion

        #region Movement Methods

        private void GroundedLocomotion()
        {
            var rotation =
                transform.eulerAngles + new Vector3(0.0f, Inputs.Horizontal * rotationSpeed, 0.0f);
            transform.eulerAngles = rotation;

            _groundDetected = DetectGroundAndCheckIfGrounded(
                out bool isGrounded,
                out GroundInfo groundInfo
            );

            _animator.SetBool(_groundHash, isGrounded);
            
            if (_groundDetected && !isGrounded)
            {
                //var slopeAngle = Vector3.Angle(groundInfo.normal, transform.forward) - 90.0f;
                _currentMoveState = MoveState.Sliding;
            }

            if (isGrounded || (allowDoubleJump && _canDoubleJump) )
            {
                _currentMoveState = isGrounded ? MoveState.Grounded : MoveState.Airborne;

                if (Inputs.Jump)
                {
                    if (!isGrounded && _canDoubleJump)
                    {
                        _canDoubleJump = false;
                        _animator.SetBool(_jumpHash, true); //Play double jumping animation
                        _verticalSpeed = jumpHeight * 1.1f; // Set gravity a little higher with a double jump
                    }
                    else
                    {
                        _animator.SetBool(_jumpHash, true); //Play jumping animation
                        _verticalSpeed = jumpHeight; //Set current gravity force to jump height
                    }

                    _nextUngroundedTime = -1.0f;
                    isGrounded = false;
                }
            }
            
            if (isGrounded)
            {
                _canDoubleJump = true;
                _characterMover.mode = CharacterMover.Mode.Walk;
                _verticalSpeed = 0;
                
                if (_groundDetected)
                {
                    IsOnMovingPlatform = groundInfo.collider.TryGetComponent(out _baseMovingPlatform);
                }
            }
            else
            {
                _characterMover.mode = CharacterMover.Mode.SimpleSlide;

                BounceDownIfTouchedCeiling();

                _verticalSpeed += gravity * Time.deltaTime;

                if (_verticalSpeed < minVerticalSpeed)
                {
                    _verticalSpeed = minVerticalSpeed;
                }
            }

            // Check auto run status. If the combatant gets a vertical movement, cancel auto run.
            // If auto run is still active, then simulate full forward movement and keep autorun on.
            if (_moveInput.z != 0)
            {
                _autoRun = false;
            }

            if (_autoRun || _cameraAutoRun)
            {
                _moveInput.z = 1.0f;
            }

            // If in water, slow down speed towards walk speed as we get deeper, unless already walking...
            if (InWater)
            {
                if (_distanceFromWaterSurface >= swimLevel)
                {
                    _animator.SetBool(_swimmingHash, true);
                    _currentMoveState = MoveState.Swimming;
                    _currentSpeed = SwimSpeed;
                }
                else
                {
                    _animator.SetBool(_swimmingHash, false);
                    if (_walking)
                    {
                        _currentSpeed = WalkSpeed;
                    }
                    else
                    {
                        _currentSpeed = Mathf.Lerp(
                            RunSpeed,
                            WalkSpeed,
                            _distanceFromWaterSurface / swimLevel
                        );
                    }
                }
            }
            else if (_walking || _moveInput.z < 0.0f)
            {
                _currentSpeed = WalkSpeed;
            }
            else
            {
                _currentSpeed = RunSpeed;
            }

            // Remove time from the delta dampening so that if we are stopping movement its more immediate, rather than a slow stop.
            _animator.SetFloat(
                _xAxisMovementHash,
                _moveInput.x,
                0.1f,
                (2.5f - _moveInput.magnitude) * Time.deltaTime
            );
            _animator.SetFloat(
                _zAxisMovementHash,
                _moveInput.z,
                0.1f,
                (2.5f - _moveInput.magnitude) * Time.deltaTime
            );
            _animator.SetFloat(_rotationHash, Inputs.Horizontal, 0.1f, Time.deltaTime);

            // Normalize movement (prevents moving faster diagonally) and apply current speed
            // Only normalize if above 1 as we want to allow for analog movement to be less than 1.0 on a given axis
            Vector3 velocity =
                _moveInput.magnitude > 1.0
                    ? _moveInput.normalized * _currentSpeed
                    : _moveInput * _currentSpeed;
            velocity.y = _verticalSpeed; //Set our movement.y to current gravity force

            // We don't want to count vertical movement for movement speed in the animator
            var animatorSpeed = new Vector2(velocity.x, velocity.z);
            _animator.SetFloat(_movementSpeedHash, animatorSpeed.magnitude, 0.1f, Time.deltaTime);

            _characterMover.Move(
                transform.rotation * velocity * Time.deltaTime,
                _groundDetected,
                groundInfo,
                _overlapCount,
                _overlaps,
                _moveContacts,
                out _contactCount
            );
        }

        private void SwimmingLocomotion()
        {
            var rotation =
                transform.eulerAngles + new Vector3(0.0f, Inputs.Horizontal * rotationSpeed, 0.0f);
            transform.eulerAngles = rotation;

            _groundDetected = DetectGroundAndCheckIfGrounded(
                out var isGrounded,
                out var groundInfo
            );

            _verticalSpeed = 0.0f;
            var jumpingFromWater = false;
            if (InWater)
            {
                _canDoubleJump = true;

                if (Inputs.Jump && _distanceFromWaterSurface - 0.075f <= swimLevel)
                {
                    jumpingFromWater = true;

                    if (!InWater && allowDoubleJump && _canDoubleJump)
                    {
                        _canDoubleJump = false;
                        _animator.SetBool(_jumpHash, true);
                        _verticalSpeed = jumpHeight * 1.1f; // Set gravity a little higher with a double jump
                    }
                    else
                    {
                        _animator.SetBool(_jumpHash, true);
                        _verticalSpeed = jumpHeight * 1.25f; // Increase jump height to counteract water and to help jump out of the water
                    }

                    _nextUngroundedTime = -1.0f;
                }
                else if (Inputs.SwimUp && _distanceFromWaterSurface > swimLevel)
                {
                    _verticalSpeed = swimStrength;
                }
                else if (Inputs.SwimDown)
                {
                    _verticalSpeed = -swimStrength;
                    _animator.SetFloat(_yAxisMovementHash, -1.0f, 0.1f, Time.deltaTime);
                }
            }
            _animator.SetFloat(_yAxisMovementHash, _verticalSpeed, 0.1f, Time.deltaTime);

            // Check auto run status. If the combatant gives a vertical movement, cancel auto run.
            // If auto run is still active, then simulate full forward movement and keep autorun on.
            if (_moveInput.z != 0)
            {
                _autoRun = false;
            }

            if (_autoRun || _cameraAutoRun)
            {
                _moveInput.z = 1.0f;
            }

            _currentSpeed = SwimSpeed;

            // Remove time from the delta dampening so that if we are stopping movement its more immediate, rather than a slow stop.
            _animator.SetFloat(
                _xAxisMovementHash,
                _moveInput.x,
                0.1f,
                (2.5f - _moveInput.magnitude) * Time.deltaTime
            );
            _animator.SetFloat(
                _zAxisMovementHash,
                _moveInput.z,
                0.1f,
                (2.5f - _moveInput.magnitude) * Time.deltaTime
            );

            // Normalize movement (prevents moving faster diagonally) and apply current speed
            // Only normalize if above 1 as we want to allow for analog movement to be less than 1.0 on a given axis
            Vector3 velocity =
                _moveInput.magnitude > 1.0
                    ? _moveInput.normalized * _currentSpeed
                    : _moveInput * _currentSpeed;
            velocity.y = _verticalSpeed; // Set our movement.y to current gravity force

            // We don't want to count vertical movement for movement speed in the animator
            var animatorSpeed = new Vector2(velocity.x, velocity.z);
            _animator.SetFloat(_movementSpeedHash, animatorSpeed.magnitude, 0.1f, Time.deltaTime);

            _characterMover.mode = CharacterMover.Mode.SimpleSlide;
            _characterMover.Move(
                transform.rotation * velocity * Time.deltaTime,
                _groundDetected,
                groundInfo,
                _overlapCount,
                _overlaps,
                _moveContacts,
                out _contactCount
            );

            if (!jumpingFromWater)
            {
                // Hard set swim position just to help maintain some consistency
                transform.position = new Vector3(
                    transform.position.x,
                    Mathf.Clamp(
                        transform.position.y,
                        float.MinValue,
                        CurrentWaterSurfaceLevel - swimLevel
                    ),
                    transform.position.z
                );
            }
        }

        #endregion

        #region Internal Helper Methods

        private void CheckWaterLevel()
        {
            _distanceFromWaterSurface = CurrentWaterSurfaceLevel - transform.position.y;

            if (InWater && _distanceFromWaterSurface >= swimLevel)
            {
                _animator.SetBool(_swimmingHash, true);
                _currentMoveState = MoveState.Swimming;
            }
            else
            {
                _animator.SetBool(_swimmingHash, false);
                _groundDetected = DetectGroundAndCheckIfGrounded(
                    out var isGrounded,
                    out var groundInfo
                );

                _animator.SetBool(_groundHash, isGrounded);
                _currentMoveState = isGrounded ? MoveState.Grounded : MoveState.Airborne;
            }
        }
        
        private void GetControllerInputs()
        {
            if (MMOCombatantThirdPersonFollowCameraController != null)
            {
                switch (MMOCombatantThirdPersonFollowCameraController.CombatantCameraState)
                {
                    case CombatantCameraState.Automatic:
                        _moveInput = new Vector3(
                            Inputs.Strafe,
                            0.0f,
                            Inputs.Vertical
                        );
                        break;

                    case CombatantCameraState.Manual:
                        if ( (MMOCombatantThirdPersonFollowCameraController.Inputs.LeftMouseClick && Inputs.Vertical != 0.0f) || MMOCombatantThirdPersonFollowCameraController.Inputs.RightMouseClick )
                        {
                            _moveInput = new Vector3(
                                Mathf.Clamp(
                                    Inputs.Strafe + Inputs.Horizontal,
                                    -1.0f,
                                    1.0f
                                ),
                                0.0f,
                                Inputs.Vertical
                            );
                        }
                        else
                        {
                            _moveInput = Vector3.zero;
                        }
                        break;

                    default:
                        InternalDebug.LogWarningFormat(
                            $"CombatantCameraState not valid: {MMOCombatantThirdPersonFollowCameraController.CombatantCameraState}"
                        );
                        break;
                }
            }

            if (Inputs.AutoRunToggle)
            {
                _autoRun = !_autoRun;
            }

            if (Inputs.WalkToggle)
            {
                _walking = !_walking;
            }
        }

        private void ApplyPlatformMovement(BaseMovingPlatform baseMovingPlatform)
        {
            GetMovementFromMovingPlatform(baseMovingPlatform, out Vector3 movement, out float upRotation);

            transform.Translate(movement, Space.World);
            transform.Rotate(0f, upRotation, 0f, Space.Self);
        }
        
        private void GetMovementFromMovingPlatform(BaseMovingPlatform baseMovingPlatform, out Vector3 movement, out float deltaAngleUp)
        {
            baseMovingPlatform.GetDeltaPositionAndRotation(out Vector3 platformDeltaPosition, out Quaternion platformDeltaRotation);
            Vector3 localPosition = transform.position - baseMovingPlatform.transform.position;
            movement = platformDeltaPosition + platformDeltaRotation * localPosition - localPosition;

            platformDeltaRotation.ToAngleAxis(out float platformDeltaAngle, out Vector3 axis);
            float axisDotUp = Vector3.Dot(axis, transform.up);

            if (-0.1f < axisDotUp && axisDotUp < 0.1f)
            {
                deltaAngleUp = 0f;
            }
            else
            {
                deltaAngleUp = platformDeltaAngle * Mathf.Sign(axisDotUp);
            }
        }
        
        #endregion

        #region Public Methods

        public void InitSettings(MMOCombatantCharacterControllerComponentSetting settings)
        {
            if (settings != null)
            {
                allowDoubleJump = settings.AllowDoubleJump;
                jumpHeight = settings.JumpHeight;
                gravity = settings.Gravity;
                minVerticalSpeed = settings.MinVerticalSpeed;
                rotationSpeed = settings.RotationSpeed;
                swimLevel = settings.SwimLevel;
                swimStrength = settings.SwimStrength;
                swimMoveSpeed = settings.SwimSpeed;
                useHeadIK = settings.UseHeadIK;
            }
        }
        
        public void SetRotation(float rot)
        {
            transform.rotation = Quaternion.Euler(0, rot, 0);
        }

        public void SetCameraAutoRun(bool setFlag)
        {
            _cameraAutoRun = setFlag;
        }
        
        #endregion

        #region Scivolo Character Controller Helper Methods
        
        private void HandleOverlaps()
        {
            if (_characterCapsule && _characterCapsule.TryResolveOverlap())
            {
                _overlapCount = 0;
            }
            else
            {
                _overlapCount = _characterCapsule.CollectOverlaps(_overlaps);
            }
        }

        private bool DetectGroundAndCheckIfGrounded(out bool isGrounded, out GroundInfo groundInfo)
        {
            var groundDetected = _groundDetector.DetectGround(out groundInfo);

            if (groundDetected)
            {
                if (groundInfo.isOnFloor && _verticalSpeed < 0.1f)
                {
                    _nextUngroundedTime = Time.time + TimeBeforeUngrounded;
                }
            }
            else
            {
                _nextUngroundedTime = -1f;
            }

            isGrounded = Time.time < _nextUngroundedTime;
            return groundDetected;
        }

        private void BounceDownIfTouchedCeiling()
        {
            for (int i = 0; i < _contactCount; i++)
            {
                if (Vector3.Dot(_moveContacts[i].normal, transform.up) < -0.7f)
                {
                    _verticalSpeed = -0.25f * _verticalSpeed;
                    break;
                }
            }
        }
        
        #endregion

        #region IMovementComponent Methods
        
        public void Move(Vector3 change)
        {
            InternalDebug.LogWarning("Move not yet implemented.");
        }

        public bool MoveTo(ref Vector3 position, float speed)
        {
            InternalDebug.LogWarning("MoveTo not yet implemented.");
            return false;
        }

        public bool MoveTo(Vector3 position, float speed, params Notify[] notifyOnFinished)
        {
            InternalDebug.LogWarning("MoveTo notify not yet implemented.");
            return false;
        }

        public void SetPosition(Vector3 position)
        {
            InternalDebug.LogWarning("SetPosition not yet implemented.");
        }

        public void Stop()
        {
            InternalDebug.LogWarning("Stop not yet implemented.");
        }
        
        #endregion
    }

    public struct CombatantControllerInputs
    {
        public float Horizontal;
        public float Vertical;
        public float Strafe;
        public bool Jump;
        public bool SwimUp;
        public bool SwimDown;
        public bool AutoRunToggle;
        public bool WalkToggle;
    }
}
