namespace Basketball.Event
{
    public struct ScoreEvent
    {
        public EHit Hit;
        public int NewScore;
        public bool Animation;
        
        public ScoreEvent(EHit hit, int newScore, bool animation)
        {
            Hit = hit;
            NewScore = newScore;
            Animation = animation;
        }
    }
}