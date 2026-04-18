using UnityEngine;

namespace Basketball.Entity
{
    public class ExplodeEffect : MonoBehaviour
    {
        [field: SerializeField] public Transform Main { get; private set; }
        
        private System.Action<ExplodeEffect> _onComplete = null;
        
        public void Play(Vector3 worldPos, System.Action<ExplodeEffect> cb = null)
        {
            this.transform.position = worldPos;
            _onComplete = cb;
        }

        public void OnAnimComplete()
        {
            _onComplete?.Invoke(this);
        }
    }
}