using UnityEngine;

namespace Basketball.Interface
{
    public interface IInput
    {
        void OnTouchBegin(Vector2 screenPos);
        void OnTouchMove(Vector2 screenPos);
        void OnTouchEnd(Vector2 screenPos);
    }
}