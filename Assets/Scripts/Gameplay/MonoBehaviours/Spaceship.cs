using System;
using System.Collections;
using UnityEngine;

using Assets.Scripts.Data;
using Assets.Scripts.GameConstants;
using Assets.Scripts.Gameplay.Subsystems;
using Assets.Scripts.GameStructure.Communication.EventSystem;
using Assets.Scripts.GameStructure.Communication.Payloads;

namespace Assets.Scripts.Gameplay.MonoBehaviours
{
    /// <summary>
    /// Interface comprising of public methods to Spaceship
    /// </summary>
    public interface ISpaceship
    {
        /// <summary>
        /// Injects dependencies cached from gamemanager required by spaceship
        /// Method Injection instead of Contructor since MonoBehaviour
        /// </summary>
        /// <param name="spaceshipInjectionData"></param>
        void Inject(SpaceshipDependencyData spaceshipInjectionData);
        Transform GetTransform();
        void Initialize();
    }

    public readonly struct SpaceshipDependencyData
    {
        public readonly IScreenHandler ScreenHandler;
        public readonly IGameStatsHandler GameStatsHandler;
        public readonly float SafeDistance;

        public SpaceshipDependencyData(IScreenHandler screenHandler, IGameStatsHandler gameStatsHandler,
            float safeDistance)
        {
            ScreenHandler = screenHandler;
            GameStatsHandler = gameStatsHandler;
            SafeDistance = safeDistance;
        }
    }

    /// <summary>
    /// Class representing Player Spaceship GameObject.
    /// Utilizes its controller for move and shoot through turret.
    /// Responsible for its toggling on getting killed.
    /// </summary>
    public class Spaceship : ScreenWrappableObject, ISpaceship
    {
        #region SERIALIZED_FIELDS

        [SerializeField] private SpaceshipConfig spaceshipConfig;
        [SerializeField] private TurretConfig turretConfig;
        [SerializeField] private Transform turretReferencePoint;

        #endregion

        #region DEPENDENCIES

        private ISpaceshipConfig _spaceshipConfig;
        private ITurret _turret;
        private IScreenHandler _screenHandler;
        private ISpaceshipControllable _spaceshipController;
        private IGameStatsHandler _gameStatsHandler;

        #endregion

        #region UNITY_FUNCTIONS
        private void Update()
        {
            UpdateController();
        }
        private void FixedUpdate()
        {
            UpdateMovement();
        }

        private void OnDestroy()
        {
            EventBus.GetInstance().Unsubscribe(EventKey.PlayAgainPayload, RespawnAfterGame);
        }
        #endregion

        #region API
        public void Inject(SpaceshipDependencyData spaceshipInjectionData)
        {
            Logger.Info($"Injecting dependencies in {this.gameObject.name}");
            _safeDistance = spaceshipInjectionData.SafeDistance;
            _screenHandler = spaceshipInjectionData.ScreenHandler;
            _gameStatsHandler = spaceshipInjectionData.GameStatsHandler;
        }
        public Transform GetTransform()
        {
            return transform;
        }
        public override void Initialize()
        {
            Logger.Info($"Initialize {this.gameObject.name}");
            base.Initialize();
            CacheConfigsToDependencies();
            InitializeTurret();
            InitializeController();

            EventBus.GetInstance().Subscribe(EventKey.PlayAgainPayload, RespawnAfterGame);
        }
        #endregion

        private Vector2 _movementForce;
        private float _turnTorque;
        #region SPACESHIP_CONTROLS
        private Coroutine _alreadyRunningHyperspaceRoutine;
        private IEnumerator HyperspaceRoutine()
        {
            Logger.Info($"Hyperspace routine initiated");
            yield return new WaitForSeconds(_spaceshipConfig.GetWaitForHyperspace());
            transform.position = _screenHandler.GetRandomPointOnScreen();
            ToggleRendering(true);
            _alreadyRunningHyperspaceRoutine = null;
        }
        private void Shoot()
        {
            _turret?.Shoot(turretReferencePoint.position, transform.up);
        }
        private void Hyperspace()
        {
            PublishHyperspaceEvent();
            ToggleRendering(false);
            if (_alreadyRunningHyperspaceRoutine == null)
            {
                _alreadyRunningHyperspaceRoutine = StartCoroutine(HyperspaceRoutine());
            }
        }
        private void Move()
        {
            ObjectRigibody2D.AddForce(_movementForce);
            ObjectRigibody2D.velocity = Vector2.ClampMagnitude(ObjectRigibody2D.velocity, _spaceshipConfig.GetMaxVelocity());
        }
        private void Turn()
        {
            ObjectRigibody2D.SetRotation(ObjectRigibody2D.rotation + _turnTorque);
        }
        
        private void UpdateController()
        {
            if (_spaceshipController == null || !_spaceshipController.IsEnabled()) return;

            if (_spaceshipController.IsShooting()) Shoot();
            if (_spaceshipController.IsTriggeringHyperspace()) Hyperspace();

            var moveAxis = _spaceshipController.GetForwardAxis();
            _movementForce = moveAxis * _spaceshipConfig.GetMovementForce() * transform.up;
            var turnAxis = _spaceshipController.GetTurnAxis();
            _turnTorque = turnAxis * _spaceshipConfig.GetTurnTorque();
        }
        private void UpdateMovement()
        {
            if (_spaceshipController == null || !_spaceshipController.IsEnabled()) return;
            Move();
            Turn();
        }       
        
        #endregion

        protected override void OnEnteringTrigger()
        {
            Logger.Info($"{this.gameObject.name} entered trigger");

            base.OnEnteringTrigger();
            PublishSpaceshipExploded();
            ToggleRendering(false);
            if(CanRespawn())
            {
                DelayedRespawn();
            }
        }

        private void PublishHyperspaceEvent()
        {
            Logger.Info($"publishing hyperspace event");
            EventBus.GetInstance().Publish(new HyperspacePayload(EventKey.Hyperspace));
        }
        private void PublishSpaceshipExploded()
        {
            Logger.Info($"publishing spaceship explosion");
            EventBus.GetInstance().Publish(new SpaceshipExplodedPayload(transform.position));
        }

        private void CacheConfigsToDependencies()
        {
            _spaceshipConfig = spaceshipConfig;
        }
        private void InitializeController()
        {
            _spaceshipController = new SpaceshipController();
        }
        private void InitializeTurret()
        {
            _turret = new Turret(turretConfig, this);
        }

        private void DelayedRespawn()
        {
            StartCoroutine(RespawnRoutine());
        }
        private IEnumerator RespawnRoutine()
        {
            var safeSpawnTries = 0;
            Logger.Info($"Respawn routine triggered");
            yield return new WaitForSeconds(_spaceshipConfig.GetWaitBeforeRespawn());

            while (!IsSafeToSpawn() && safeSpawnTries <= GameConstants.Spaceship.NumberOfTriesForSafeSpawn)
            {
                Logger.Info($"unable to find safe place to spawn. trying again");
                yield return new WaitForSeconds(_spaceshipConfig.GetWaitForSafeSpawnDelay());
                safeSpawnTries++;
            }

            Respawn();
            
        }
        private void Respawn()
        {
            Logger.Info($"respawning spaceship");
            ResetShip();
            RePositionInCenter();
            
            ToggleTrigger(true);
            ToggleWrapping(true);
            ToggleRendering(true);
        }
        private void RePositionInCenter()
        {
            gameObject.transform.position = Vector3.zero;
        }
        private bool CanRespawn()
        {
            var remainingLives = _gameStatsHandler.GetLives();
            return (remainingLives > 0);
        }

        private void ResetShip()
        {
            ObjectRigibody2D.velocity = Vector2.zero;
            transform.rotation = Quaternion.identity;
        }
        private void ToggleRendering(bool status)
        {
            ObjectSpriteRenderer.enabled = status;
            ObjectCollider2D.enabled = status;
            _spaceshipController.ToggleController(status);
        }

        private float _safeDistance;
        private Collider2D[] _overlappingDamagableCollidersInRegion = new Collider2D[1];
        private bool IsSafeToSpawn()
        {
            var hasAnyAsteroidInRange = HasDamagingObjectInRangeOnLayer(1 << GameConstants.Layer.AsteroidLayer, ref _overlappingDamagableCollidersInRegion);
            _overlappingDamagableCollidersInRegion[0] = null;
            var hasAnyUFOInRange = HasDamagingObjectInRangeOnLayer(1 << GameConstants.Layer.UFOLayer, ref _overlappingDamagableCollidersInRegion);
            _overlappingDamagableCollidersInRegion[0] = null; 
            var hasAnyEnemtBulletInRange = HasDamagingObjectInRangeOnLayer(1 << GameConstants.Layer.EnemyBulletLayer, ref _overlappingDamagableCollidersInRegion);
            _overlappingDamagableCollidersInRegion[0] = null;

            var isSafe = !(hasAnyAsteroidInRange || hasAnyUFOInRange || hasAnyEnemtBulletInRange);
            return isSafe;
        }
        private bool HasDamagingObjectInRangeOnLayer(LayerMask layerMask, ref Collider2D[] damagingCollidersInRange)
        {
            Physics2D.OverlapCircleNonAlloc(new Vector2(0, 0), _safeDistance,
                damagingCollidersInRange, layerMask);

            return damagingCollidersInRange[0] != null;
        }
        private void RespawnAfterGame(IBasePayload basePayload)
        {
            RePositionInCenter();
            Respawn();
        }
    }
}