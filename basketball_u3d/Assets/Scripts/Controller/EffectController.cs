using Basketball.Entity;
using Basketball.Event;
using Basketball.Interface;
using Basketball.Utilities.Pool;
using UniRx;
using UnityEngine;

namespace Basketball.Controller
{
    public class EffectController : MonoBehaviour
    {
        [field: SerializeField] public HitEffect PrefabHit { get; private set; }
        [field: SerializeField] public ExplodeEffect PrefabExplode { get; private set; }

        private IGameplay _iGameplay;
        private ObjectPool<HitEffect> _poolHits;
        private ObjectPool<ExplodeEffect> _poolExplode;
        private readonly CompositeDisposable _disposable = new CompositeDisposable();

        public void Initialize(GameplayController gameplay)
        {
            _iGameplay = gameplay;
            _poolHits = new ObjectPool<HitEffect>(PrefabHit, gameplay.UIController.CanvasRoot);
            _poolExplode = new ObjectPool<ExplodeEffect>(PrefabExplode, this.transform);
            _iGameplay.OnHit.Subscribe(OnHit).AddTo(_disposable);
        }

        public void Dispose()
        {
            _disposable?.Dispose();
        }

        private void OnHit(HitEvent evt)
        {
            var hit = _poolHits.Get();
            ((RectTransform)hit.transform).anchoredPosition = evt.CanvasPoint;
            hit.Play(evt.Hit, evt.BonusScore, ele => _poolHits.Store(ele));

            if (evt.Hit != EHit.Miss)
            {
                var explode = _poolExplode.Get();
                explode.Play(_iGameplay.GetGoalPosition(), ele => _poolExplode.Store(ele));
            }
        }
    }
}
