using Basketball.Event;
using Basketball.Interface;
using PrimeTween;
using TMPro;
using UniRx;
using UnityEngine;

namespace Basketball.Controller
{
    public class UIController : MonoBehaviour
    {
        [field: SerializeField] public TMP_Text TextScore { get; private set; }

        public RectTransform CanvasRoot => (RectTransform)TextScore.transform.parent;
        public Canvas Canvas => CanvasRoot.GetComponentInParent<Canvas>();

        private IGameplay _iGameplay;
        
        private readonly CompositeDisposable _disposable = new CompositeDisposable();
        
        public void Initialize(GameplayController gameplay)
        {
            _iGameplay = gameplay;
            _iGameplay.OnScoreChanged.Subscribe(OnScoreChanged).AddTo(_disposable);
            TextScore.text = _iGameplay.Score.ToString();
        }

        public void Dispose()
        {
            _disposable?.Dispose();
        }
        
        private void OnScoreChanged(ScoreEvent evt)
        {
            TextScore.text = evt.NewScore.ToString();
            if (evt.Animation)
            {
                Tween.Scale(TextScore.transform, 1.3f, 0.15f, Ease.OutBack, 2, CycleMode.Yoyo);
            }
        }

        public Vector2 WorldToCanvasPoint(Vector3 worldPosition)
        {
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(_iGameplay.Camera, worldPosition);
            Camera uiCamera = Canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Canvas.worldCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(CanvasRoot, screenPoint, uiCamera, out Vector2 canvasPoint);
            return canvasPoint;
        }
    }
}
