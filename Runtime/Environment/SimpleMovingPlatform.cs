using UnityEngine;

namespace MMOCombatantController.Runtime.Environment
{
    // A simple linear moving platform example based on the Scivolo demo project
    public class SimpleMovingPlatform : BaseMovingPlatform
    {
        #region Editor - Settings
        
        [SerializeField]
        private float platformMovementSpeed = 2f;
        [SerializeField]
        private float platformRotationSpeed = 1f;
        [SerializeField]
        private Transform start;
        [SerializeField]
        private Transform end;

        #endregion
        
        #region Private Fields
        
        private Vector3 _deltaPosition;
        private Quaternion _deltaRotation;
        private bool _isMovingForward = true;
        
        #endregion
        
        #region Properties
        
        private Vector3 CurrentDestination => _isMovingForward ? end.position : start.position;

        private Vector3 UpDirection => transform.parent != null ? transform.parent.up : transform.up;
        
        #endregion
        
        #region Unity Lifecycle Methods
        
        protected void Start()
        {
            start.SetParent(transform.parent, true);
            end.SetParent(transform.parent, true);
        }

        protected void Update()
        {
            var deltaTime = Time.deltaTime;
            _deltaPosition = Vector3.MoveTowards(Vector3.zero, CurrentDestination - transform.position, platformMovementSpeed * deltaTime);
            _deltaRotation = Quaternion.AngleAxis(platformRotationSpeed * deltaTime, UpDirection);

            transform.SetPositionAndRotation(transform.position + _deltaPosition, _deltaRotation * transform.rotation);

            // Invert moving direction when it reaches the destination.
            if ((CurrentDestination - transform.position).sqrMagnitude < 1E-04f)
            {
                _isMovingForward = !_isMovingForward;
            }
        }

        #endregion
        
        #region BaseMovingPlatform Methods
        
        public override void GetDeltaPositionAndRotation(out Vector3 deltaPosition, out Quaternion deltaRotation)
        {
            deltaPosition = _deltaPosition;
            deltaRotation = _deltaRotation;
        }
        
        #endregion
    }
}

