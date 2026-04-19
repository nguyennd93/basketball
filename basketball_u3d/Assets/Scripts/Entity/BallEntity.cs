using Basketball.Interface;
using Basketball.Utilities.Pool;
using UnityEngine;

namespace Basketball.Entity
{
    public class BallEntity : MonoBehaviour, ISpawn
    {
        [field: SerializeField] public Rigidbody Rigidbody { get; private set; }
        [field: SerializeField] public SphereCollider SphereCollider { get; private set; }

        private Vector3 _previousPosition;
        private int _goalCollisionCount;

        public bool HasScored { get; private set; }
        public bool EnteredGoalFromTop { get; private set; }

        private IGameplay _iGameplay;

        public void Initialize(IGameplay gameplay)
        {
            _iGameplay = gameplay;
            HoldAt(_iGameplay.StartPoint.position, _iGameplay.StartPoint.rotation);
            _goalCollisionCount = 0;
            HasScored = false;
            EnteredGoalFromTop = false;
        }

        public void HoldAt(Vector3 position, Quaternion rotation)
        {
            StopMotionIfNeeded();
            Rigidbody.isKinematic = true;
            Rigidbody.useGravity = false;
            transform.SetPositionAndRotation(position, rotation);
            _previousPosition = position;
        }

        public void Throw(Vector3 position, Vector3 initialVelocity)
        {
            transform.position = position;
            _previousPosition = position;
            Rigidbody.isKinematic = false;
            Rigidbody.useGravity = false;
            Rigidbody.linearVelocity = Vector3.zero;
            Rigidbody.angularVelocity = Vector3.zero;
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
            StopMotionIfNeeded();
            Rigidbody.isKinematic = true;
            Rigidbody.useGravity = false;
        }

        public void OnStore()
        {
            StopMotionIfNeeded();
            Rigidbody.isKinematic = true;
            Rigidbody.useGravity = false;
        }

        private void StopMotionIfNeeded()
        {
            if (Rigidbody.isKinematic)
            {
                return;
            }

            Rigidbody.linearVelocity = Vector3.zero;
            Rigidbody.angularVelocity = Vector3.zero;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider != null)
            {
                _iGameplay.OnTriggerGoal(collision.gameObject.tag);
            }
            
            if (!HasScored && collision.collider != null && !collision.collider.CompareTag("Mesh"))
            {
                Debug.Log("Collision with: " + collision.collider.name);
                _goalCollisionCount++;
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
