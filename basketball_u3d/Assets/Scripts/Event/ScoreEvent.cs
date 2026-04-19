namespace Basketball.Event
{
    public struct ScoreEvent
    {
        public EHit Hit;
        public int NewScore;
        public int PrevScore;
        public bool Animation;
        
        public ScoreEvent(EHit hit, int newScore, int prevScore, bool animation)
        {
            Hit = hit;
            PrevScore = prevScore;
            NewScore = newScore;
            Animation = animation;
        }
    }
}