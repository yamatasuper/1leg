using UnityEngine;

namespace NinetyMinutes.World
{
    public sealed class BillboardFacing : MonoBehaviour
    {
        void LateUpdate()
        {
            var cam = Camera.main;
            if (cam == null) return;
            var toCam = cam.transform.position - transform.position;
            if (toCam.sqrMagnitude < 0.0001f) return;
            transform.rotation = Quaternion.LookRotation(toCam.normalized, cam.transform.up);
        }
    }
}
