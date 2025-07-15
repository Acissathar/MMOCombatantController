using GamingIsLove.Makinom;

using Unity.Cinemachine;
using UnityEngine;

namespace MMOCombatantController.Runtime.Core
{
    public class MMOCombatantThirdPersonFollowCameraController : MonoBehaviour
    {
        #region Editor - Component References
        
        [SerializeField] private CinemachineCamera cinemachineCamera;

        [SerializeField] private Transform tilt;
        
        #endregion

        #region Editor - Settings
        
        [SerializeField] [Tooltip("How high up the camera is.")]
        private float height;

        [SerializeField] [Tooltip("Maximum distance from target.")]
        private float maxDistance;

        [SerializeField] [Tooltip("Maximum amount of camera tilt (X Axis)")]
        private float maxTilt;

        [SerializeField] [Tooltip("How fast to move the camera when auto adjusting.")]
        private float autoAdjustSpeed;

        [SerializeField] [Tooltip("Cushion offset for determining collisions.")]
        private float collisionCushion;
        
        [SerializeField] [Tooltip("Cushion offset for camera clip planes, used for determining collisions.")]
        private float clipPlaneCushion;
        
        [SerializeField] [Tooltip("Angle the camera should automatically adjust to when in Automatic mode.")]
        private float cameraAutoAdjustAngle;

        [SerializeField] [Tooltip("Number of rows for ray cast grid.")]
        private int collisionRayGridX;

        [SerializeField] [Tooltip("Number of columns for ray cast grid.")]
        private int collisionRayGridY;

        [SerializeField] [Tooltip("Layers the camera collides with.")]
        private LayerMask collisionMask;

        [SerializeField] [Tooltip("Multiplier for X input for camera movement.")]
        private float xInputSensitivity;

        [SerializeField] [Tooltip("Multiplier for Y input for camera movement.")]
        private float yInputSensitivity;

        [SerializeField] [Tooltip("Multiplier for zoom input for camera.")]
        private float zoomInputSensitivity;

        #endregion
        
        #region Properties

        public CombatantCameraState CombatantCameraState { get; private set; }

        public CombatantCameraThirdPersonFollowCameraInputs Inputs { get; set; }

        public int CombatantInputID
        {
            get
            {
                if (_target)
                {
                    return _target.Combatant.InputID;
                }
                
                return Maki.Control.InputID;
            }
        }
        
        public CinemachineCamera CinemachineCamera => cinemachineCamera;
        
        #endregion
        
        #region Private Fields
        
        private MMOCombatantCharacterController _target;
        
        // Collision info
        private float _adjustedDistance;
        private bool _adjustOnXAxis;
        private bool _adjustOnYAxis;
        private Vector3[] _cameraClip;
        private Ray _camRay;
        private RaycastHit _camRayHit;
        private Vector3[] _clipDirection;
        private float _currentDistance = 10.0f;

        // Current position/rotation
        private float _currentPan;
        private float _currentTilt = 10.0f;

        // Camera Smoothing
        private float _panAngle;
        private float _panOffset;
        private Vector3[] _playerClip;
        private bool[] _rayColHit;
        private Vector3[] _rayColOrigin;
        private Vector3[] _rayColPoint;
        private readonly float _xAutoRotCushion = 2.0f;
        private float _xAutoRotSpeed;
        private float _yAutoRotSpeed;
        private readonly float _yRotMax = 20.0f;
        private readonly float _yRotMin = 0.0f;

        #endregion
        
        #region Unity Lifecycle Methods

        protected void Start()
        {
            CombatantCameraState = CombatantCameraState.Automatic;
            SetCameraClipInfo();
        }

        protected void Update()
        {
            GetCameraInputs();

            if (collisionRayGridX * collisionRayGridY != _rayColOrigin.Length) SetCameraClipInfo();

            DetermineCameraCollisions();

            if (Inputs.LeftMouseClick || Inputs.RightMouseClick)
            {
                CombatantCameraState = CombatantCameraState.Manual;
            }
            else
            {
                CombatantCameraState = CombatantCameraState.Automatic;
                _adjustOnXAxis = true;
                _adjustOnYAxis = true;
            }
        }

        protected void LateUpdate()
        {
            if (_target != null) //Only execute if there is a target
            {
                _panAngle = Vector3.SignedAngle(
                    transform.forward,
                    _target.transform.forward,
                    Vector3.up
                );

                switch (CombatantCameraState)
                {
                    case CombatantCameraState.Automatic:
                        AutomaticCameraMovement();
                        break;

                    case CombatantCameraState.Manual:
                        ManualCameraMovement();
                        break;

                    default:
                        InternalDebug.LogWarningFormat(
                            $"CombatantCameraState not valid: {CombatantCameraState}"
                        );
                        break;
                }
            }
        }

        #endregion
        
        #region Internal Helper
        
        private void GetCameraInputs()
        {
            if (Inputs.LeftMouseClick || Inputs.RightMouseClick)
            {
                _currentPan += Inputs.CameraXRotate * xInputSensitivity;

                _currentTilt -= Inputs.CameraYRotate * yInputSensitivity;
                _currentTilt = Mathf.Clamp(_currentTilt, -maxTilt, maxTilt);
            }

            _currentDistance = Mathf.Lerp(
                _currentDistance,
                _currentDistance - Inputs.CameraZoom,
                zoomInputSensitivity * Time.deltaTime
            );
            _currentDistance = Mathf.Clamp(_currentDistance, 1.0f, maxDistance);
        }
        
        #endregion
        
        #region Public Methods
        
        public void UpdateCamTarget(Transform newTarget)
        {
            //Set camera height according to target height
            if (newTarget.TryGetComponent(out MMOCombatantCharacterController combatantController))
            {
                _target = combatantController;
                _target.MMOCombatantThirdPersonFollowCameraController = this;

                transform.position = _target.transform.position + Vector3.up * height;
                transform.rotation = _target.transform.rotation;

                tilt.eulerAngles = new Vector3(
                    _currentTilt,
                    transform.eulerAngles.y,
                    transform.eulerAngles.z
                );
                cinemachineCamera.transform.position += tilt.forward * -_currentDistance;
            }
            else
            {
                InternalDebug.LogFormat("MMOCombatantCharacterController not found.", newTarget);
            }
        }

        #endregion
        
        #region CombatantCameraState Processing

        private void AutomaticCameraMovement()
        {
            if (_target.MoveInputs.sqrMagnitude > 0.0f || _target.Inputs.Horizontal != 0)
            {
                AutoXAdjust();
                AutoYAdjust();
            }
            else
            {
                _xAutoRotSpeed = 0.0f;
                _yAutoRotSpeed = 0.0f;
            }

            SetCameraTransforms();
        }

        private void ManualCameraMovement()
        {
            _xAutoRotSpeed = 0.0f;
            _yAutoRotSpeed = 0.0f;

            SetCameraTransforms();

            // If right mouse is held, force rotation of character to match camera.
            if (Inputs.RightMouseClick) _target.SetRotation(transform.eulerAngles.y);

            _target.SetCameraAutoRun(Inputs.LeftMouseClick && Inputs.RightMouseClick);
        }

        private void SetCameraTransforms()
        {
            transform.position = _target.transform.position + Vector3.up * height;
            transform.eulerAngles = new Vector3(
                transform.eulerAngles.x,
                _currentPan,
                transform.eulerAngles.z
            );
            tilt.eulerAngles = new Vector3(_currentTilt, tilt.eulerAngles.y, tilt.eulerAngles.z);
            cinemachineCamera.transform.position =
                transform.position + tilt.forward * -_adjustedDistance;
        }

        #endregion

        #region Auto Adjustment

        private void AutoXAdjust()
        {
            if (_adjustOnXAxis)
            {
                _xAutoRotSpeed += Time.deltaTime * autoAdjustSpeed;

                if (Mathf.Abs(_panAngle) > _xAutoRotCushion)
                    _currentPan = Mathf.Lerp(_currentPan, _currentPan + _panAngle, _xAutoRotSpeed);
                else
                    _adjustOnXAxis = false;
            }
            else
            {
                _xAutoRotSpeed = 0.0f;

                _currentPan = _target.transform.eulerAngles.y;
            }
        }

        private void AutoYAdjust()
        {
            if (_adjustOnYAxis)
            {
                _yAutoRotSpeed += Time.deltaTime / 2.0f * autoAdjustSpeed;

                if (_currentTilt >= _yRotMax || _currentTilt <= _yRotMin)
                    _currentTilt = Mathf.Lerp(_currentTilt, _yRotMax / 2.0f, _yAutoRotSpeed);
                else if (_currentTilt < _yRotMax && _currentTilt > _yRotMin) _adjustOnYAxis = false;
            }
            else
            {
                _yAutoRotSpeed = 0.0f;
            }
        }

        #endregion

        #region Camera Collisions

        private void SetCameraClipInfo()
        {
            _cameraClip = new Vector3[4];

            var mainCam = Camera.main;
            mainCam?.CalculateFrustumCorners(
                new Rect(0.0f, 0.0f, 1.0f, 1.0f),
                mainCam.nearClipPlane,
                Camera.MonoOrStereoscopicEye.Mono,
                _cameraClip
            );

            _clipDirection = new Vector3[4];
            _playerClip = new Vector3[4];

            var rays = collisionRayGridX * collisionRayGridY;

            _rayColOrigin = new Vector3[rays];
            _rayColPoint = new Vector3[rays];
            _rayColHit = new bool[rays];
        }

        private void DetermineCameraCollisions()
        {
            var camDistance = _currentDistance + collisionCushion;

            for (var i = 0; i < _cameraClip.Length; i++)
            {
                var clipPoint =
                    cinemachineCamera.transform.up * _cameraClip[i].y
                    + cinemachineCamera.transform.right * _cameraClip[i].x;
                clipPoint =
                    clipPoint * clipPlaneCushion
                    + cinemachineCamera.transform.forward * _cameraClip[i].z
                    + (transform.position - tilt.forward * maxDistance);

                var playerPoint =
                    cinemachineCamera.transform.up * _cameraClip[i].y
                    + cinemachineCamera.transform.right * _cameraClip[i].x;
                playerPoint += transform.position;

                _clipDirection[i] = (clipPoint - playerPoint).normalized;
                _playerClip[i] = playerPoint;
            }

            var currentRay = 0;
            var isColliding = false;
            var rayX = collisionRayGridX - 1;
            var rayY = collisionRayGridY - 1;

            for (var x = 0; x < collisionRayGridX; x++)
            {
                var cuPoint = Vector3.Lerp(_clipDirection[1], _clipDirection[2], x / rayX);
                var clPoint = Vector3.Lerp(_clipDirection[0], _clipDirection[3], x / rayX);

                var puPoint = Vector3.Lerp(_playerClip[1], _playerClip[2], x / rayX);
                var plPoint = Vector3.Lerp(_playerClip[0], _playerClip[3], x / rayX);

                for (var y = 0; y < collisionRayGridY; y++)
                {
                    _camRay.origin = Vector3.Lerp(puPoint, plPoint, y / rayY);
                    _camRay.direction = Vector3.Lerp(cuPoint, clPoint, y / rayY);

                    _rayColOrigin[currentRay] = _camRay.origin;

                    if (Physics.Raycast(_camRay, out _camRayHit, camDistance, collisionMask))
                    {
                        isColliding = true;
                        _rayColHit[currentRay] = true;
                        _rayColPoint[currentRay] = _camRayHit.point;
                    }
                    else
                    {
                        _rayColHit[currentRay] = false;
                    }

                    currentRay++;
                }
            }

            if (isColliding)
            {
                var minRayDistance = float.MaxValue;
                currentRay = 0;

                for (var i = 0; i < _rayColHit.Length; i++)
                    if (_rayColHit[i])
                    {
                        var colDistance = Vector3.Distance(_rayColOrigin[i], _rayColPoint[i]);

                        if (colDistance < minRayDistance)
                        {
                            minRayDistance = colDistance;
                            currentRay = i;
                        }
                    }

                var clipCenter = transform.position - tilt.forward * _currentDistance;

                _adjustedDistance = Vector3.Dot(
                    -cinemachineCamera.transform.forward,
                    clipCenter - _rayColPoint[currentRay]
                );
                _adjustedDistance = _currentDistance - (_adjustedDistance + collisionCushion);
                _adjustedDistance = Mathf.Clamp(_adjustedDistance, 0.0f, maxDistance);
            }
            else
            {
                _adjustedDistance = _currentDistance;
            }
        }

        #endregion
    }

    public enum CombatantCameraState
    {
        Automatic,
        Manual
    }

    public struct CombatantCameraThirdPersonFollowCameraInputs
    {
        public bool LeftMouseClick;
        public bool RightMouseClick;
        public float CameraXRotate;
        public float CameraYRotate;
        public float CameraZoom;
    }
}