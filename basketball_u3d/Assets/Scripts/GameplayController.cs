using System;
using System.Collections;
using System.Collections.Generic;
using Basketball.Controller;
using Basketball.Entity;
using Basketball.Event;
using Basketball.Interface;
using Basketball.Utilities.Pool;
using UniRx;
using UnityEngine;

namespace Basketball
{
    public class GameplayController : MonoBehaviour, IGameplay, IInput
    {
        [field: Header("Controller")]
        [field: SerializeField]
        public UIController UIController { get; private set; }

        [field: SerializeField] public EffectController EffectController { get; private set; }
        [field: SerializeField] public InputController InputController { get; private set; }
        [field: SerializeField] public AimController AimController { get; private set; }

        [field: Header("Gameplay")]
        [field: SerializeField]
        public Camera Camera { get; private set; }

        [field: SerializeField] public GoalEntity GoalEntity { get; private set; }
        [field: SerializeField] public MeshCollider GoalCollider { get; private set; }
        [field: SerializeField] public Transform LimitLine { get; private set; }
        [field: SerializeField] public Transform StartPoint { get; private set; }
        [field: SerializeField] public BallEntity PrefabBall { get; private set; }

        #region Properties

        public int Score { get; private set; } = 0;
        public ISubject<HitEvent> OnHit { get; private set; } = new Subject<HitEvent>();
        public ISubject<ScoreEvent> OnScoreChanged { get; private set; } = new Subject<ScoreEvent>();

        #endregion

        private BallEntity _currentBall;
        private readonly List<BallEntity> _activeBalls = new();
        private readonly List<BallEntity> _storeBalls = new();
        private ObjectPool<BallEntity> _poolBalls;

        private bool _isInitialized = false;
        private bool _isAiming;
        private bool _canThrow;

        #region Private Methods

        private void Awake()
        {
            var ballEntity = GameObject.Instantiate(PrefabBall, transform);
            _poolBalls = new ObjectPool<BallEntity>(ballEntity, transform);
        }

        private void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            UIController.Initialize(this);
            EffectController.Initialize(this);
            InputController.Initialize(this);
            AimController.Initialize(this);

            Score = 0;
            SpawnReadyBall();
            _isInitialized = true;
        }

        private void Dispose()
        {
            UIController.Dispose();
            EffectController.Dispose();
        }

        #endregion

        #region Unity Methods

        private void Start()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            Dispose();
        }

        private void FixedUpdate()
        {
            for (int i = 0; i < _activeBalls.Count; i++)
            {
                _activeBalls[i].ApplyCustomGravity(AimController.EffectiveGravity);
            }
        }

        private void LateUpdate()
        {
            for (int i = _activeBalls.Count - 1; i >= 0; i--)
            {
                BallEntity ball = _activeBalls[i];
                if (_storeBalls.Contains(ball)) continue;

                if (!ball.HasScored && ball.TryResolveScore(out EHit hit))
                {
                    AddScore(hit, 1, GoalCollider.transform.position + new Vector3(0f, 0.5f, 0f), true);
                }

                if (ball.transform.position.y < LimitLine.position.y && ball.Rigidbody.linearVelocity.y < 0f)
                {
                    _storeBalls.Add(ball);

                    if (!ball.HasScored) AddScore(EHit.Miss, 0, ball.transform.position, true);
                    StartCoroutine(CRStoreBall(ball));
                }
            }
        }

        #endregion

        #region IGameplay

        public Vector3 GetGoalPosition() => GoalCollider.transform.position;

        public void AddScore(EHit hit, int score, Vector3 worldPosition, bool anim = false)
        {
            Score += score;
            OnScoreChanged?.OnNext(new ScoreEvent(hit, Score, anim));

            Vector2 canvasPoint = UIController.WorldToCanvasPoint(worldPosition);
            OnHit?.OnNext(new HitEvent() { Hit = hit, BonusScore = score, CanvasPoint = canvasPoint });
        }

        public void OnTriggerGoal()
        {
            GoalEntity.TriggerShake();
        }

        #endregion

        #region IInput

        public void OnTouchBegin(Vector2 screenPos)
        {
            if (!_isInitialized || !_canThrow)
            {
                return;
            }

            _isAiming = true;
            AimController.ShowAim(screenPos);
        }

        public void OnTouchMove(Vector2 screenPos)
        {
            if (!_isInitialized || !_isAiming)
            {
                return;
            }

            AimController.ShowAim(screenPos);
        }

        public void OnTouchEnd(Vector2 screenPos)
        {
            if (!_isInitialized || !_isAiming)
            {
                return;
            }

            ThrowBall(screenPos);
            AimController.HideAim();
            _isAiming = false;
        }

        #endregion

        private void SpawnReadyBall()
        {
            _currentBall = _poolBalls.Get();
            _currentBall.Initialize(this);
            _canThrow = true;
        }

        private void ThrowBall(Vector2 screenPos)
        {
            if (!AimController.TryGetLaunchData(screenPos, out Vector3 launchOrigin, out Vector3 initialVelocity,
                    out _))
            {
                return;
            }

            BallEntity thrownBall = _currentBall;
            thrownBall.Throw(launchOrigin, initialVelocity);

            _activeBalls.Add(thrownBall);

            _currentBall = null;
            _canThrow = false;
            StartCoroutine(CRSpawnBall());
        }

        private IEnumerator CRSpawnBall()
        {
            yield return new WaitForSeconds(0.5f);
            SpawnReadyBall();
        }

        private IEnumerator CRStoreBall(BallEntity ball)
        {
            yield return new WaitForSeconds(2.0f);
            _activeBalls.Remove(ball);
            _storeBalls.Remove(ball);
            _poolBalls.Store(ball);
        }
    }
}