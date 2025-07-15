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
        [EditorTitleLabel("Jump")]
        [EditorHelp(
            "Allow Double Jump",
            "Allows the combatant to do a double jump if input provided.",
            ""
        )]
        public bool AllowDoubleJump = false;
        
        [EditorHelp(
            "Jump Height",
            "How high the combatant can jump.",
            ""
        )]
        public float JumpHeight = 5.0f;
        
        [EditorHelp(
            "Gravity",
            "How fast the combatant falls down when not grounded or swimming.",
            ""
        )]
        public float Gravity = 15.0f;
        
        [EditorSeparator]
        [EditorTitleLabel("Movement")]
        [EditorHelp(
            "Rotation Speed",
            "How fast the combatant turns as a multiplier of the Horizontal input.",
            ""
        )]
        public float RotationSpeed = 0.75f;

        [EditorSeparator]
        [EditorTitleLabel("Swimming")]
        [EditorHelp(
            "Swim Level",
            "How deep into water the character should be before considering them actually swimming.",
            ""
        )]
        public float SwimLevel = 1.25f;

        [EditorHelp(
            "Swim Strength",
            "How much force to apply when swimming up or down for a combatant.",
            ""
        )]
        public float SwimStrength = 5.0f;
        
        public MoveSpeed<GameObjectSelection> SwimSpeed = new (MoveSpeedType.Walk, 5);
        
        [EditorSeparator]
        [EditorTitleLabel("Animation")]
        [EditorHelp(
            "Use Head IK",
            "When activated will rotate combatant's head to look at camera direction if applicable or point of interest.",
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

                _combatantController.Settings = setting;
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
}