using UnityEngine;

namespace sandbox
{
    public class WeaponSnap : MonoBehaviour
    {
        [Header("Target")]
        public Transform handTarget;

        [Header("Position")]
        public Vector3 positionOffset;
        public float positionSmooth = 20f;

        [Header("Rotation")]
        public Vector3 rotationOffset;
        public float rotationSmooth = 20f;

        [Header("Advanced")]
        public bool useLateUpdate = true;

        Vector3 velocity;

        void Update()
        {
            if (!useLateUpdate)
                SnapWeapon();
        }

        void LateUpdate()
        {
            if (useLateUpdate)
                SnapWeapon();
        }

        void SnapWeapon()
        {
            if (handTarget == null)
                return;

            Vector3 targetPos =
                handTarget.position +
                handTarget.TransformDirection(positionOffset);

            Quaternion targetRot =
                handTarget.rotation *
                Quaternion.Euler(rotationOffset);

            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPos,
                ref velocity,
                1f / positionSmooth
            );

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSmooth * Time.deltaTime
            );
        }
    }
}