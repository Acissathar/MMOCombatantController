using UnityEngine;

namespace MMOCombatantController.Runtime.Environment
{
    // Moving Platform base class. The character controller needs GetDeltaPositionAndRotation to know how much the platform moved so it can apply it to the character as well.
    public abstract class BaseMovingPlatform : MonoBehaviour
    {
        public virtual void GetDeltaPositionAndRotation(out Vector3 deltaPosition, out Quaternion deltaRotation)
        {
            deltaPosition = Vector3.zero;
            deltaRotation = Quaternion.identity;
        }
    }
}