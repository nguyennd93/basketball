using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Basketball.Entity
{
    public class HitEffect : MonoBehaviour
    {
        [System.Serializable]
        public struct HitInfo
        {
            public EHit Hit;
            public Color Color;
        }
        
        [field: SerializeField] public TMP_Text TextHit { get; private set; }
        [field: SerializeField] public TMP_Text TextScore { get; private set; }
        [field: SerializeField] public List<HitInfo> HitInfos { get; private set; }

        private System.Action<HitEffect> _onComplete = null;
        
        public void Play(EHit hit, int score, System.Action<HitEffect> cb = null)
        {
            SetHit(hit);
            SetScore(score);
            _onComplete = cb;
        }

        private void SetHit(EHit hit)
        {
            var hitInfo = HitInfos.Find(x => x.Hit == hit);
            TextHit.color = hitInfo.Color;
            TextHit.text = hitInfo.Hit.ToString();
        }
        
        private void SetScore(int score)
        {
            TextScore.gameObject.SetActive(score > 0);
            TextScore.text = $"+{score}";
        }

        public void OnAnimComplete()
        {
            _onComplete?.Invoke(this);
        }
    }
}