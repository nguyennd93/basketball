using UnityEngine;
using System.Collections.Generic;

namespace Basketball.Entity
{
    public class GoalEntity : MonoBehaviour
    {
        [field: SerializeField] public Animator Animator { get; private set; }
        [field: SerializeField] public Cloth GoalMesh { get; private set; }

        private readonly int _animShakeHash = Animator.StringToHash("Shake");

        public void TriggerShake()
        {
            Animator.SetTrigger(_animShakeHash);
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
    }
}
