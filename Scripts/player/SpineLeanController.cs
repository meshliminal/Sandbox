using UnityEngine;

namespace sandbox
{
    public class SpineLeanController : MonoBehaviour
    {
        [Header("References")]
        public Transform spine;

        [Header("Lean Amount")]
        public float forwardLean = 12f;
        public float backwardLean = 6f;
        public float sideLean = 10f;

        [Header("Smooth Time")]
        [Tooltip("Kisebb = gyorsabb reakció")]
        public float smoothTime = 0.12f;

        private Quaternion startRotation;

        private float currentPitch;
        private float currentRoll;

        private float pitchVelocity;
        private float rollVelocity;

        void Start()
        {
            if (spine == null)
                spine = transform;

            startRotation = spine.localRotation;
        }

        void LateUpdate()
        {
            float targetPitch = 0f;
            float targetRoll = 0f;

            bool w = Input.GetKey(KeyCode.W);
            bool s = Input.GetKey(KeyCode.S);
            bool a = Input.GetKey(KeyCode.A);
            bool d = Input.GetKey(KeyCode.D);

            // Előre / hátra
            if (w)
                targetPitch = -forwardLean;
            else if (s)
                targetPitch = backwardLean;

            // Bal / jobb
            if (a)
                targetRoll = sideLean;
            else if (d)
                targetRoll = -sideLean;

            currentPitch = Mathf.SmoothDampAngle(
                currentPitch,
                targetPitch,
                ref pitchVelocity,
                smoothTime);

            currentRoll = Mathf.SmoothDampAngle(
                currentRoll,
                targetRoll,
                ref rollVelocity,
                smoothTime);

            spine.localRotation =
                startRotation *
                Quaternion.Euler(
                    currentPitch,
                    0f,
                    currentRoll);
        }
    }
}