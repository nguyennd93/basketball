using UnityEngine;

namespace Basketball.Info
{
    [System.Serializable]
    public class ScoreInfo
    {
        [field: SerializeField] public EHit Type { get; private set; }
        [field: SerializeField] public int BonusScore { get; private set; }
    }
}