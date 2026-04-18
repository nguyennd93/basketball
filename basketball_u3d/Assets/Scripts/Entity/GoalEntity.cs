using UnityEngine;

namespace Basketball.Entity
{
    public class GoalEntity : MonoBehaviour
    {
        [field: SerializeField] public Animator Animator { get; private set; }

        private readonly int _animShakeHash = Animator.StringToHash("Shake");
        
        public void Initialize()
        {
            
        }

        public void TriggerShake()
        {
            Animator.SetTrigger("Shake");
        }
    }
}