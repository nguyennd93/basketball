using UnityEngine;

namespace Basketball.Event
{
    public struct HitEvent
    {
        public EHit Hit;
        public int BonusScore;
        public Vector2 CanvasPoint;
    }
}
