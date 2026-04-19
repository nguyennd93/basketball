using Basketball.Event;
using UniRx;
using UnityEngine;

namespace Basketball.Interface
{
    public interface IGameplay
    {
        Camera Camera { get; }
        Transform StartPoint { get; }
        MeshCollider GoalCollider { get; }
        
        int Score { get; }

        ISubject<HitEvent> OnHit { get; }
        ISubject<ScoreEvent> OnScoreChanged { get; }

        Vector3 GetGoalPosition();
        void OnTriggerGoal(string colliderTag);
    }
}
