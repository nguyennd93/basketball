using Basketball.Interface;
using UnityEngine;

namespace Basketball.Controller
{
    public class AimController : MonoBehaviour
    {
        [field: SerializeField] public LineRenderer AimLine { get; private set; }
        [SerializeField] private bool showAimLine = true;

        [Header("Throw Tuning")]
        [SerializeField] private float aimPlaneZ;
        [SerializeField] private float aimLineWidth = 0.12f;
        [SerializeField] private int trajectorySteps = 24;
        [SerializeField] private float gravityMultiplier = 1.35f;
        [SerializeField] private float throwVelocityMultiplier = 1f;
        [SerializeField] private float arcHeight = 2.35f;
        [SerializeField] private float distanceArcMultiplier = 0.05f;
        [SerializeField] private float targetLift = 0.15f;
        [SerializeField] private Vector3 launchOffset = new Vector3(0f, 1.25f, 0f);
        [SerializeField] private Color aimLineStartColor = new Color(1f, 1f, 1f, 0.95f);
        [SerializeField] private Color aimLineEndColor = new Color(1f, 0.82f, 0.2f, 0.85f);

        public Vector3 EffectiveGravity => Physics.gravity * gravityMultiplier;

        private IGameplay _iGameplay;

        public void Initialize(IGameplay gameplayReference)
        {
            _iGameplay = gameplayReference;
            EnsureAimLine();
            SetAimLineActive(false);
            HideAim();
        }

        private void OnValidate()
        {
            EnsureAimLine();
            ApplyAimLineStyle();
        }

        private void EnsureAimLine()
        {
            AimLine.useWorldSpace = true;
            AimLine.positionCount = 2;
            AimLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            AimLine.receiveShadows = false;
            AimLine.alignment = LineAlignment.View;
            AimLine.numCapVertices = 8;
            AimLine.textureMode = LineTextureMode.Stretch;

            ApplyAimLineStyle();
        }

        private void ApplyAimLineStyle()
        {
            AimLine.positionCount = Mathf.Max(2, trajectorySteps);
            AimLine.startWidth = aimLineWidth;
            AimLine.endWidth = aimLineWidth * 0.72f;
            AimLine.startColor = aimLineStartColor;
            AimLine.endColor = aimLineEndColor;
        }

        public void ShowAim(Vector2 screenPosition)
        {
            if (!showAimLine)
            {
                SetAimLineActive(false);
                return;
            }

            if (!TryGetLaunchData(screenPosition, out Vector3 startPoint, out Vector3 initialVelocity, out float totalTime))
            {
                SetAimLineActive(false);
                return;
            }

            BuildTrajectory(startPoint, initialVelocity, totalTime);
            SetAimLineActive(true);
        }

        public void HideAim()
        {
            SetAimLineActive(false);
        }

        private void SetAimLineActive(bool isActive)
        {
            AimLine.enabled = isActive;
        }

        public bool TryGetLaunchData(Vector2 screenPosition, out Vector3 startPoint, out Vector3 initialVelocity, out float totalTime)
        {
            startPoint = _iGameplay.StartPoint.position;
            initialVelocity = Vector3.zero;
            totalTime = 0f;

            if (!TryGetAimPoint(screenPosition, out Vector3 aimPoint))
            {
                return false;
            }

            return BallisticArcUtility.TrySolveArc(
                startPoint,
                aimPoint,
                arcHeight,
                distanceArcMultiplier,
                EffectiveGravity,
                out initialVelocity,
                out totalTime) && TryApplyThrowVelocityScale(startPoint.y, aimPoint.y, ref initialVelocity, ref totalTime);
        }

        private bool TryGetAimPoint(Vector2 screenPosition, out Vector3 aimPoint)
        {
            Plane oxyPlane = new Plane(Vector3.forward, new Vector3(0f, 0f, aimPlaneZ));
            Ray ray = _iGameplay.Camera.ScreenPointToRay(screenPosition);

            if (!oxyPlane.Raycast(ray, out float enter))
            {
                aimPoint = Vector3.zero;
                return false;
            }

            aimPoint = ray.GetPoint(enter);
            aimPoint.z = aimPlaneZ;
            aimPoint.y += targetLift;
            return true;
        }

        private void BuildTrajectory(Vector3 startPoint, Vector3 initialVelocity, float totalTime)
        {
            int steps = Mathf.Max(2, trajectorySteps);
            AimLine.positionCount = steps;

            for (int i = 0; i < steps; i++)
            {
                float t = (i / (float)(steps - 1)) * totalTime;
                Vector3 position = BallisticArcUtility.EvaluatePosition(startPoint, initialVelocity, EffectiveGravity, t);
                AimLine.SetPosition(i, position);
            }
        }

        private bool TryApplyThrowVelocityScale(float startY, float targetY, ref Vector3 initialVelocity, ref float previewTime)
        {
            initialVelocity *= throwVelocityMultiplier;
            previewTime = ComputePreviewDuration(startY, targetY, initialVelocity.y, EffectiveGravity.y, previewTime);
            return previewTime > Mathf.Epsilon;
        }

        private static float ComputePreviewDuration(float startY, float targetY, float initialVelocityY, float gravityY, float fallbackTime)
        {
            float a = 0.5f * gravityY;
            float b = initialVelocityY;
            float c = startY - targetY;

            if (Mathf.Abs(a) <= Mathf.Epsilon)
            {
                return Mathf.Max(fallbackTime, 0.01f);
            }

            float discriminant = b * b - 4f * a * c;
            if (discriminant < 0f)
            {
                return Mathf.Max(fallbackTime, 0.01f);
            }

            float sqrtDiscriminant = Mathf.Sqrt(discriminant);
            float t1 = (-b + sqrtDiscriminant) / (2f * a);
            float t2 = (-b - sqrtDiscriminant) / (2f * a);
            float previewTime = Mathf.Max(t1, t2);

            if (previewTime <= Mathf.Epsilon)
            {
                previewTime = Mathf.Min(t1, t2);
            }

            return Mathf.Max(previewTime, 0.01f);
        }
    }
}
