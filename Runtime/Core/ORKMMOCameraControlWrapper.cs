using GamingIsLove.ORKFramework.Components;
using GamingIsLove.Makinom;

using UnityEngine;

namespace MMOCombatantController.Runtime.Core
{
    public class ORKMMOCameraControlWrapper : BaseCameraControl
    {
        #region Editor - Input Keys

        [SerializeField]
        private InputKeyAsset cameraLeftMouse;

        [SerializeField]
        private InputKeyAsset cameraRightMouse;

        [SerializeField]
        private InputKeyAsset cameraZoom;

        [SerializeField]
        private InputKeyAsset cameraXRotate;

        [SerializeField]
        private InputKeyAsset cameraYRotate;
        
        #endregion
        
        #region Component References
        
        [SerializeField]
        private MMOCombatantThirdPersonFollowCameraController mmoCombatantThirdPersonFollowCameraController;
        
        #endregion

        #region Unity Lifecycle Methods
        
        protected void OnDisable()
        {
            if (mmoCombatantThirdPersonFollowCameraController)
            {
                mmoCombatantThirdPersonFollowCameraController.CinemachineCamera.Priority.Value = 0;
            }
        }

        protected void OnEnable()
        {
            if (mmoCombatantThirdPersonFollowCameraController)
            {
                if (CameraTarget != null)
                {
                    CameraTargetChanged(null, CameraTarget);
                }
                
                mmoCombatantThirdPersonFollowCameraController.CinemachineCamera.Priority.Value = 10;
            }
        }

        protected void Update()
        {
            if (mmoCombatantThirdPersonFollowCameraController)
            {
                var inputID = mmoCombatantThirdPersonFollowCameraController.CombatantInputID;
                var cameraInputs = new CombatantCameraThirdPersonFollowCameraInputs
                {
                    LeftMouseClick = InputKey.GetButton(cameraLeftMouse, inputID),
                    RightMouseClick = InputKey.GetButton(cameraRightMouse, inputID),
                    CameraXRotate =  InputKey.GetAxis(cameraXRotate, inputID),
                    CameraYRotate = InputKey.GetAxis(cameraYRotate, inputID),
                    CameraZoom = InputKey.GetAxis(cameraZoom, inputID)
                };
                
                mmoCombatantThirdPersonFollowCameraController.Inputs = cameraInputs;
            }
        }
        
        #endregion
        
        #region ORK Base Camera Control Override Methods

        /// <summary>
        ///     Changes the target of the wrapped camera control component, called when ORK changes the target.
        /// </summary>
        /// <param name="oldTarget">The game object of the old target.</param>
        /// <param name="newTarget">The game object of the new target.</param>
        public override void CameraTargetChanged(GameObject oldTarget, GameObject newTarget)
        {
            if (mmoCombatantThirdPersonFollowCameraController)
            {
                mmoCombatantThirdPersonFollowCameraController.UpdateCamTarget(newTarget.transform);
            }
        }

        #endregion
    }
}