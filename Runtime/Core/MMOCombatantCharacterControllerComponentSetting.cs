using GamingIsLove.Makinom;

using UnityEngine;

namespace MMOCombatantController.Runtime.Core
{
    [EditorSettingInfo(
        "MMO Combatant Character Controller",
        "Classic MMO style inspired character controller. "
    )]
    public class MMOCombatantCharacterControllerComponentSetting : BaseMovementComponentSetting
    {
        [EditorHelp(
            "Add Component",
            "The component will be added to the game object, if it isn't added already.\n"
            + "If disabled, the component must already be attached to the game object.",
            ""
        )]
        public bool CompAdd = false;

        [EditorSeparator]
        [EditorTitleLabel("Base Movement")]
        [EditorHelp(
            "Rotation Speed",
            MMOCombatantCharacterControllerTooltips.RotationSpeedTT,
            ""
        )]
        public float RotationSpeed = 0.75f;
        
        [EditorHelp(
            "Gravity",
            MMOCombatantCharacterControllerTooltips.GravityTT,
            ""
        )]
        public float Gravity = -15.0f;
        
        [EditorHelp(
            "Min Vertical Speed",
            MMOCombatantCharacterControllerTooltips.MinVerticalSpeedTT,
            ""
        )]
        public float MinVerticalSpeed = -7.5f;
        
        [EditorSeparator]
        [EditorTitleLabel("Jump")]
        [EditorHelp(
            "Allow Double Jump",
            MMOCombatantCharacterControllerTooltips.AllowDoubleJumpTT,
            ""
        )]
        public bool AllowDoubleJump = false;
        
        [EditorHelp(
            "Jump Height",
            MMOCombatantCharacterControllerTooltips.JumpHeightTT,
            ""
        )]
        public float JumpHeight = 5.0f;

        [EditorSeparator]
        [EditorTitleLabel("Swimming")]
        [EditorHelp(
            "Swim Level",
            MMOCombatantCharacterControllerTooltips.SwimLevelTT,
            ""
        )]
        public float SwimLevel = 1.25f;

        [EditorHelp(
            "Swim Strength",
            MMOCombatantCharacterControllerTooltips.SwimStrengthTT,
            ""
        )]
        public float SwimStrength = 2.5f;
        
        public MoveSpeed<GameObjectSelection> SwimSpeed = new (MoveSpeedType.Walk, 5);
        
        [EditorSeparator]
        [EditorTitleLabel("Animation")]
        [EditorHelp(
            "Use Head IK",
            MMOCombatantCharacterControllerTooltips.UseHeadIKTT,
            ""
        )]
        public bool UseHeadIK = false;
        
        public override IMovementComponent Init(GameObject gameObject)
        {
            return new MoveComponent(gameObject, this);
        }
        
        public class MoveComponent : IMovementComponent
        {
            private readonly MMOCombatantCharacterController _combatantController;

            public MoveComponent(
                GameObject gameObject,
                MMOCombatantCharacterControllerComponentSetting setting
            )
            {
                if (!gameObject.transform.root.TryGetComponent(out _combatantController))
                {
                    if (setting.CompAdd)
                    {
                        _combatantController = gameObject.AddComponent<MMOCombatantCharacterController>();
                    }
                    else
                    {
                        InternalDebug.LogWarningFormat($"Combatant Controller not found on {gameObject.name} but Add Component is not checked.");
                    }
                }

                _combatantController.InitSettings(setting);
            }

            public virtual void Move(Vector3 change)
            {
                if (_combatantController != null) _combatantController.Move(change);
            }

            public virtual bool MoveTo(ref Vector3 position, float speed)
            {
                if (_combatantController != null)
                {
                    _combatantController.MoveTo(ref position, speed);
                    return true;
                }

                return false;
            }

            public virtual void SetPosition(Vector3 position)
            {
                if (_combatantController != null) _combatantController.SetPosition(position);
            }

            public virtual void Stop()
            {
                if (_combatantController != null) _combatantController.Stop();
            }
        }
    }
    
    public static class MMOCombatantCharacterControllerTooltips
    {
        public const string RotationSpeedTT = "How fast the combatant turns as a multiplier of the Horizontal input.";
        public const string GravityTT = "How fast the combatant falls down when not grounded or swimming.";
        public const string MinVerticalSpeedTT = "Minimum amount of speed that should be used when gravity is starting to be applied. Most notable for helping keep clamped down on moving platforms.";
        public const string AllowDoubleJumpTT = "Allows the combatant to do a double jump if input provided.";
        public const string JumpHeightTT = "How high the combatant can jump.";
        public const string SwimLevelTT = "How deep into water the character should be before considering them actually swimming.";
        public const string SwimStrengthTT = "How much force to apply when swimming up or down for a combatant.";
        public const string UseHeadIKTT =
            "When activated will rotate combatant's head to look at camera direction if applicable or point of interest.";
    }
}