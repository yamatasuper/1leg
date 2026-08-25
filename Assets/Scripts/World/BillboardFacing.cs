using UnityEngine;

namespace NinetyMinutes.World
{
    public sealed class BillboardFacing : MonoBehaviour
    {
        void LateUpdate()
        {
            var cam = Camera.main;
            if (cam == null) return;
            var target = cam.transform.position;
            target.y = transform.position.y;
            if ((target - transform.position).sqrMagnitude < 0.0001f) return;
            transform.LookAt(target);
        }
    }
}
