using GamingIsLove.Makinom;

using UnityEngine;

namespace MMOCombatantController.Runtime.Core
{
    public class ORKMMOPlayerControlWrapper : MonoBehaviour
    {
        #region Editor - Input Keys
        
        [SerializeField]
        private InputKeyAsset horizontalMove;

        [SerializeField]
        private InputKeyAsset verticalMove;

        [SerializeField]
        private InputKeyAsset strafeMove;

        [SerializeField]
        private InputKeyAsset jump;

        [SerializeField]
        private InputKeyAsset walkToggle;

        [SerializeField]
        private InputKeyAsset autoRunToggle;

        [SerializeField]
        private InputKeyAsset swimUp;

        [SerializeField]
        private InputKeyAsset swimDown;
        
        #endregion

        private int InputID => _mmoCombatantCharacterController?.Combatant?.InputID ?? Maki.Control.InputID;
        private MMOCombatantCharacterController _mmoCombatantCharacterController;
        
        #region Unity Methods

        private void OnEnable()
        {
            if (!TryGetComponent(out _mmoCombatantCharacterController))
            {
                InternalDebug.LogWarningFormat($"Combatant Controller not found on {gameObject.name}");
            }
        }

        private void Update()
        {
            if (_mmoCombatantCharacterController)
            {
                var controllerInputs = new CombatantControllerInputs
                {
                    Horizontal = InputKey.GetAxis(horizontalMove, InputID),
                    Vertical = InputKey.GetAxis(verticalMove, InputID),
                    Strafe =  InputKey.GetAxis(strafeMove, InputID),
                    Jump = InputKey.GetButton(jump, InputID),
                    SwimUp = InputKey.GetButton(swimUp, InputID),
                    SwimDown = InputKey.GetButton(swimDown, InputID),
                    AutoRunToggle = InputKey.GetButton(autoRunToggle, InputID),
                    WalkToggle = InputKey.GetButton(walkToggle, InputID)
                };
                
                _mmoCombatantCharacterController.Inputs = controllerInputs;
            }
        }

        #endregion
    }
}