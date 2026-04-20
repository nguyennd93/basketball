using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Basketball.Entity
{
    public class GoalEntity : MonoBehaviour
    {
        [field: SerializeField] public Animator Animator { get; private set; }
        [field: SerializeField] public Cloth GoalMesh { get; private set; }

        private readonly int _animShakeHash = Animator.StringToHash("Shake");
        private static readonly int _baseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int _colorId = Shader.PropertyToID("_Color");
        private static readonly int _emissionColorId = Shader.PropertyToID("_EmissionColor");

        private static readonly Color _scoreGlowTint = new(1f, 0.55f, 0.1f, 1f);

        private const float ScoreGlowDuration = 0.28f;
        private const float ScoreGlowColorBlend = 0.55f;
        private const float ScoreGlowEmissionStrength = 3.5f;

        private MeshRenderer _goalColliderRenderer;
        private Material _goalColliderMaterial;
        private Coroutine _scoreGlowCoroutine;
        private Color _defaultBaseColor = Color.white;
        private Color _defaultEmissionColor = Color.black;

        public void TriggerShake()
        {
            Animator.SetTrigger(_animShakeHash);
        }

        public void TriggerScoreGlow(MeshCollider goalCollider)
        {
            if (!TryBindGoalCollider(goalCollider))
            {
                return;
            }

            if (_scoreGlowCoroutine != null)
            {
                StopCoroutine(_scoreGlowCoroutine);
                RestoreGoalColliderVisual();
            }

            _scoreGlowCoroutine = StartCoroutine(CRTriggerScoreGlow());
        }

        public void AddBallCollider(SphereCollider sphereCollider)
        {
            var sphereColliders = new List<ClothSphereColliderPair>(GoalMesh.sphereColliders);
            sphereColliders.Add(new ClothSphereColliderPair(sphereCollider));
            GoalMesh.sphereColliders = sphereColliders.ToArray();
        }

        public void RemoveBallCollider(SphereCollider sphereCollider)
        {
            var sphereColliders = new List<ClothSphereColliderPair>(GoalMesh.sphereColliders);

            for (int i = sphereColliders.Count - 1; i >= 0; i--)
            {
                if (sphereColliders[i].first == sphereCollider || sphereColliders[i].second == sphereCollider)
                {
                    sphereColliders.RemoveAt(i);
                }
            }

            GoalMesh.sphereColliders = sphereColliders.ToArray();
        }

        private bool TryBindGoalCollider(MeshCollider goalCollider)
        {
            if (goalCollider == null)
            {
                return false;
            }

            var renderer = goalCollider.GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                return false;
            }

            if (_goalColliderRenderer == renderer && _goalColliderMaterial != null)
            {
                return true;
            }

            _goalColliderRenderer = renderer;
            _goalColliderMaterial = renderer.material;
            CacheDefaultGoalColliderVisual();
            return true;
        }

        private void CacheDefaultGoalColliderVisual()
        {
            if (_goalColliderMaterial == null)
            {
                return;
            }

            if (_goalColliderMaterial.HasColor(_baseColorId))
            {
                _defaultBaseColor = _goalColliderMaterial.GetColor(_baseColorId);
            }
            else if (_goalColliderMaterial.HasColor(_colorId))
            {
                _defaultBaseColor = _goalColliderMaterial.GetColor(_colorId);
            }

            if (_goalColliderMaterial.HasColor(_emissionColorId))
            {
                _defaultEmissionColor = _goalColliderMaterial.GetColor(_emissionColorId);
            }
        }

        private IEnumerator CRTriggerScoreGlow()
        {
            float elapsed = 0f;

            while (elapsed < ScoreGlowDuration)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(elapsed / ScoreGlowDuration);
                float pulse = Mathf.Sin(normalizedTime * Mathf.PI);
                ApplyGoalColliderGlow(pulse);
                yield return null;
            }

            RestoreGoalColliderVisual();
            _scoreGlowCoroutine = null;
        }

        private void ApplyGoalColliderGlow(float intensity)
        {
            if (_goalColliderMaterial == null)
            {
                return;
            }

            Color glowColor = Color.Lerp(_defaultBaseColor, _scoreGlowTint, intensity * ScoreGlowColorBlend);
            Color emissionColor = _defaultEmissionColor + (_scoreGlowTint * (intensity * ScoreGlowEmissionStrength));

            if (_goalColliderMaterial.HasColor(_baseColorId))
            {
                _goalColliderMaterial.SetColor(_baseColorId, glowColor);
            }

            if (_goalColliderMaterial.HasColor(_colorId))
            {
                _goalColliderMaterial.SetColor(_colorId, glowColor);
            }

            if (_goalColliderMaterial.HasColor(_emissionColorId))
            {
                _goalColliderMaterial.EnableKeyword("_EMISSION");
                _goalColliderMaterial.SetColor(_emissionColorId, emissionColor);
            }
        }

        private void RestoreGoalColliderVisual()
        {
            if (_goalColliderMaterial == null)
            {
                return;
            }

            if (_goalColliderMaterial.HasColor(_baseColorId))
            {
                _goalColliderMaterial.SetColor(_baseColorId, _defaultBaseColor);
            }

            if (_goalColliderMaterial.HasColor(_colorId))
            {
                _goalColliderMaterial.SetColor(_colorId, _defaultBaseColor);
            }

            if (_goalColliderMaterial.HasColor(_emissionColorId))
            {
                _goalColliderMaterial.SetColor(_emissionColorId, _defaultEmissionColor);

                if (_defaultEmissionColor.maxColorComponent <= 0.001f)
                {
                    _goalColliderMaterial.DisableKeyword("_EMISSION");
                }
            }
        }
    }
}
