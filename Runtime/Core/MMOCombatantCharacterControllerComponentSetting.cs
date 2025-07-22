using GamingIsLove.Makinom;
using GamingIsLove.ORKFramework;
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
            "Swim Level Buffer",
            MMOCombatantCharacterControllerTooltips.SwimLevelBufferTT,
            ""
        )]
        public float SwimLevelBuffer = 0.0075f;
        
        [EditorHelp(
            "Dive Strength",
            MMOCombatantCharacterControllerTooltips.DiveStrengthTT,
            ""
        )]
        public float DiveStrength = 2.5f;
        
        public MoveSpeed<GameObjectSelection> SwimSpeed = new (MoveSpeedType.Walk, 5);
        
        [EditorHelp("Use Swim Stamina", MMOCombatantCharacterControllerTooltips.UseSwimStaminaTT, "")]
        public bool UseSwimStamina = false;
        
        [EditorHelp("Swim Stamina", MMOCombatantCharacterControllerTooltips.SwimStaminaTT, "")]
        [EditorCondition("UseSwimStamina", true)]
        public AssetSelection<StatusValueAsset> SwimStaminaStatusValue = new AssetSelection<StatusValueAsset>();

        [EditorHelp("Swim Stamina Tick Time", MMOCombatantCharacterControllerTooltips.SwimStaminaTickTimeTT, "")]
        public float SwimStaminaTickTime = 1.0f;
        
        [EditorHelp("Only When Diving", MMOCombatantCharacterControllerTooltips.OnlyWhenDivingTT, "")]
        public bool OnlyWhenDiving = false;
        
        [EditorTitleLabel("(Optional) Drowning Machine")]
        [EditorHelp("Drowning Schematic", MMOCombatantCharacterControllerTooltips.DrowningSchematicTT, "")]
        public MakinomSchematicAsset DrowningSchematic;

        [EditorHelp("Is Blocking", MMOCombatantCharacterControllerTooltips.DrowningSchematicIsBlockingTT, "")]
        [EditorEndCondition]
        public bool drowningIsBlocking = false;
        
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
        public const string SwimLevelBufferTT = "Buffer to apply to swim level checks to help account for float imprecision and different pivot points on characters.";
        public const string DiveStrengthTT = "How much force to apply when swimming up or diving down for a combatant.";
        public const string UseSwimStaminaTT = "Toggle to control whether or not to track swim stamina. When disabled, swim and dive time is not limited.";
        public const string SwimStaminaTT = "Status value used for how long swimming can be done. A value of 1 is subtracted for each swim tick.";
        public const string SwimStaminaTickTimeTT = "How many in seconds between each check of swim stamina. Use a value of 0 for every frame.";
        public const string OnlyWhenDivingTT = "Only subtract swim stamina when diving (underwater). When unchecked, stamina will also be subtracted when at the water surface.";
        public const string DrowningSchematicTT = "Schematic to run every tick when there is no more swim stamina remaining.";
        public const string DrowningSchematicIsBlockingTT = "Use true if this is a blocking schematic, otherwise false.\nOnly one blocking schematic can be executed at a time.";
        public const string UseHeadIKTT =
            "When activated will rotate combatant's head to look at camera direction if applicable or point of interest.";
    }
}