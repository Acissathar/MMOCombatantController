using MMOCombatantController.Runtime.Core;

using UnityEngine;

namespace MMOCombatantController.Runtime.Environment
{
    public class WaterBody : MonoBehaviour
    {
        [SerializeField]
        private LayerMask collisionMask;
    
        private void OnTriggerStay(Collider other)
        {
            if ((collisionMask.value & (1 << other.transform.gameObject.layer)) > 0)
            {
                if (other.TryGetComponent(out MMOCombatantCharacterController combatant))
                {
                    combatant.InWater = true;
                    combatant.CurrentWaterSurfaceLevel = transform.position.y;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if ((collisionMask.value & (1 << other.transform.gameObject.layer)) > 0)
            {
                if (other.TryGetComponent(out MMOCombatantCharacterController combatant))
                {
                    combatant.InWater = false;
                    combatant.CurrentWaterSurfaceLevel = 0.0f;
                }
            }
        }
    }
}
