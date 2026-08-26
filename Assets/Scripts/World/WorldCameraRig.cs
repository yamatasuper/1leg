using UnityEngine;

namespace NinetyMinutes.World
{
    public sealed class WorldCameraRig : MonoBehaviour
    {
        public Transform Target;
        public float Distance = 11.2f;
        public float Height = 9.4f;
        public float LookHeight = 0.55f;
        public float Yaw;
        public bool InputEnabled = true;

        public Vector3 PlanarForward
        {
            get
            {
                var f = transform.forward;
                f.y = 0f;
                if (f.sqrMagnitude < 0.001f) f = Vector3.forward;
                return f.normalized;
            }
        }

        public Vector3 PlanarRight
        {
            get
            {
                var r = transform.right;
                r.y = 0f;
                if (r.sqrMagnitude < 0.001f) r = Vector3.right;
                return r.normalized;
            }
        }

        void LateUpdate()
        {
            if (Target == null) return;

            if (InputEnabled)
            {
                if (Input.GetMouseButton(1))
                    Yaw += Input.GetAxis("Mouse X") * 3.4f;
                if (Input.GetKey(KeyCode.Q))
                    Yaw -= 80f * Time.deltaTime;
                if (Input.GetKey(KeyCode.Z))
                    Yaw += 80f * Time.deltaTime;
            }

            var rot = Quaternion.Euler(0f, Yaw, 0f);
            var desired = Target.position + Vector3.up * Height + rot * new Vector3(0f, 0f, -Distance);
            transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-10f * Time.deltaTime));
            transform.LookAt(Target.position + Vector3.up * LookHeight);
        }

        public void Snap()
        {
            if (Target == null) return;
            var rot = Quaternion.Euler(0f, Yaw, 0f);
            transform.position = Target.position + Vector3.up * Height + rot * new Vector3(0f, 0f, -Distance);
            transform.LookAt(Target.position + Vector3.up * LookHeight);
        }
    }
}
