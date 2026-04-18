using Basketball.Interface;
using Basketball.Utilities.Pool;
using UnityEngine;

namespace Basketball.Entity
{
    public class BallEntity : MonoBehaviour, ISpawn
    {
        [field: SerializeField] public Rigidbody Rigidbody { get; private set; }

        private Vector3 _previousPosition;
        private int _goalCollisionCount;

        public bool HasScored { get; private set; }
        public bool EnteredGoalFromTop { get; private set; }

        private IGameplay _iGameplay;

        public void Initialize(IGameplay gameplay)
        {
            _iGameplay = gameplay;
            transform.position = _previousPosition = _iGameplay.StartPoint.position;
            Rigidbody.isKinematic = true;
            Rigidbody.useGravity = false;
            _goalCollisionCount = 0;
            HasScored = false;
            EnteredGoalFromTop = false;
        }

        public void Throw(Vector3 position, Vector3 initialVelocity)
        {
            transform.position = position;
            Rigidbody.isKinematic = false;
            Rigidbody.useGravity = false;
            Rigidbody.AddForce(initialVelocity, ForceMode.VelocityChange);
        }

        public void ApplyCustomGravity(Vector3 gravity)
        {
            Rigidbody.AddForce(gravity, ForceMode.Acceleration);
        }

        public bool TryResolveScore(out EHit hit)
        {
            Bounds goalBounds = _iGameplay.GoalCollider.bounds;
            Vector3 currentPosition = transform.position;
            bool insideHorizontalBounds = IsInsideHorizontalBounds(_previousPosition, goalBounds) ||
                                          IsInsideHorizontalBounds(currentPosition, goalBounds);

            if (!EnteredGoalFromTop && _previousPosition.y > goalBounds.max.y &&
                currentPosition.y <= goalBounds.max.y && insideHorizontalBounds)
            {
                EnteredGoalFromTop = true;
            }

            bool passedBelowGoal = EnteredGoalFromTop
                                   && _previousPosition.y >= goalBounds.min.y
                                   && currentPosition.y < goalBounds.min.y
                                   && insideHorizontalBounds;

            _previousPosition = currentPosition;

            if (passedBelowGoal && !HasScored)
            {
                HasScored = true;
                hit = ResolveHitFromCollisionCount();
                return true;
            }

            hit = default;
            return false;
        }

        public void OnSpawn()
        {
            Rigidbody.isKinematic = true;
            Rigidbody.useGravity = false;
        }

        public void OnStore()
        {
            Rigidbody.isKinematic = true;
            Rigidbody.useGravity = false;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!HasScored && collision.collider.transform.IsChildOf(_iGameplay.GoalCollider.transform))
            {
                _goalCollisionCount++;
                _iGameplay?.OnTriggerGoal();
            }
        }

        private bool IsInsideHorizontalBounds(Vector3 position, Bounds bounds)
        {
            return position.x >= bounds.min.x
                   && position.x <= bounds.max.x
                   && position.z >= bounds.min.z
                   && position.z <= bounds.max.z;
        }

        private EHit ResolveHitFromCollisionCount()
        {
            if (_goalCollisionCount == 0)
            {
                return EHit.Perfect;
            }

            if (_goalCollisionCount == 1)
            {
                return EHit.Great;
            }

            return EHit.Good;
        }
    }
}