using System.Collections;
using UnityEngine;

using Assets.Scripts.Data;
using Assets.Scripts.Gameplay.Subsystems;
using Assets.Scripts.GameStructure.Communication.EventSystem;
using Assets.Scripts.GameStructure.Communication.Payloads;

namespace Assets.Scripts.Gameplay.MonoBehaviours
{
    /// <summary>
    /// Interface comprising of public methods to UFO
    /// </summary>
    public interface IUFO
    {
        void CacheScreenRect(Rect rect);
        void Enable();
        void ToggleShipRenderer(bool status);
        void Initialize();
    }

    /// <summary>
    /// Class representing UFO GameObject.
    /// Defines method for move and shoot through its turret.
    /// Publishes its explosion event.
    /// </summary>
    public class UFO : ScreenWrappableObject, IUFO
    {
        #region SERIALIZED_FIELDS

        [SerializeField] private AudioSource audioSource;
        [SerializeField] private int ufoLevel;
        [SerializeField] private Transform turretReferencePoint;
        [SerializeField] private TurretConfig turretConfig;
        [SerializeField] private UFOConfig ufoConfig;
        

        #endregion

        #region DEPENDENCIES

        private IUFOConfig _ufoConfig;
        private ITurretConfig _turretConfig;
        private ITurret _turret;

        #endregion

        #region UNITY_FUNCTIONS

        private void OnDisable()
        {
            UnsubscribeFromGameEvents();
        }
        #endregion

        #region API
        public void CacheScreenRect(Rect rect)
        {
            _rect = rect;
        }

        private void StartRepeatedShooting()
        {
            Logger.Info($"{this.gameObject.name} started repeated shooting");

            _turret.Toggle(true);
            _shootingRoutine = StartCoroutine(RepeatedShooting(_turretConfig.GetCooldown()));
        }
        private void StopRepeatedShooting()
        {
            Logger.Info($"{this.gameObject.name} stopped repeated shooting");

            _turret.Toggle(false);
            if (_shootingRoutine != null)
            {
                StopCoroutine(_shootingRoutine);
            }
        }

        public void Enable()
        {
            Logger.Info($"{this.gameObject.name} enabled");

            var randomizedSign = GetRandomSign();
            RePositionOnScreenEdges(randomizedSign);
            ToggleShipRenderer(true);
            ToggleTrigger(true);
            ToggleWrapping(true);
            ApplyVelocity(randomizedSign);
            StartRepeatedShooting();
        }
        public void ToggleShipRenderer(bool status)
        {
            Logger.Info($"{this.gameObject.name} rendering toggled to {status}");

            audioSource.enabled = status;
            ObjectRigibody2D.velocity = Vector2.zero;
            ObjectSpriteRenderer.enabled = status;
            ObjectCollider2D.enabled = status;
            _bIsShipEnabled = status;
            gameObject.SetActive(status);
        }
        public override void Initialize()
        {
            Logger.Info($"{this.gameObject.name} Initialized");

            base.Initialize();

            CacheConfigsToDependencies();

            SubscribeToGameEvents();
            InitializeTurret();
            
        }
        #endregion

        protected override void OnEnteringTrigger()
        {
            Logger.Info($"{this.gameObject.name} has entered trigger");
            base.OnEnteringTrigger();
            Explode();
        }


        private Coroutine _shootingRoutine;
        private Transform _spaceshipTransform;
        private bool _bIsShipEnabled; 
        private Rect _rect;

        private void SubscribeToGameEvents()
        {
            EventBus.GetInstance().Subscribe(GameConstants.EventKey.SpaceshipSpawned, OnSpaceshipLoaded);
        }
        private void UnsubscribeFromGameEvents()
        {
            EventBus.GetInstance().Unsubscribe(GameConstants.EventKey.SpaceshipSpawned, OnSpaceshipLoaded);
        }
        private void OnSpaceshipLoaded(IBasePayload basePayload)
        {
            SpaceshipSpawnedPayload spawnedPayload = (SpaceshipSpawnedPayload)basePayload;
            _spaceshipTransform = spawnedPayload.spaceshipBehaviour.GetTransform();
        }
        private void PublishUFOExplosionEvent()
        {
            EventBus.GetInstance().Publish(new UfoExplodedPayload(ufoLevel, transform.position));
        }

        private void CacheConfigsToDependencies()
        {
            _ufoConfig = ufoConfig;
            _turretConfig = turretConfig;
        }
        private void InitializeTurret()
        {
            _turret = new Turret(_turretConfig, this);
        }
        private void ApplyVelocity(int velocityXDirection)
        {
            if (velocityXDirection == 0) velocityXDirection = 1;
            ObjectRigibody2D.AddForce(this.transform.right * velocityXDirection * _ufoConfig.GetMovementForce());
        }
       
        private void Shoot()
        {
            //Logger.Info($"{this.gameObject.name} Shooting");

            if (!_bIsShipEnabled) return;
            var shootDirection = GetShootDirection();
            _turret?.Shoot(turretReferencePoint.position, shootDirection);
        }
        
        private Vector2 GetShootDirection()
        {
            var directionToSpaceship = GetDirectionToSpaceship();
            var directionToShoot = GetRandomlySkewedDirection(directionToSpaceship);
            return directionToSpaceship;
        }
        private Vector2 GetRandomlySkewedDirection(Vector2 originalDirection)
        {
            var precision = Random.Range(_ufoConfig.GetMinAngle(), _ufoConfig.GetMaxAngle());
            return Quaternion.Euler(0.0f, 0.0f, precision) * originalDirection;
        }
        private Vector2 GetDirectionToSpaceship()
        {
            Vector3 directionToSpaceship = default;
            if (_spaceshipTransform != null)
            {
                directionToSpaceship = _spaceshipTransform.transform.position - transform.position;
                directionToSpaceship.Normalize();
            }

            return directionToSpaceship;
        }

        IEnumerator RepeatedShooting(float delay)
        {
            while (true)
            {
                yield return new WaitForSeconds(delay);
                Shoot();
            }
        }

        private void Explode()
        {
            StopRepeatedShooting();
            ToggleShipRenderer(false);
            PublishUFOExplosionEvent();    
        }
        
        private void RePositionOnScreenEdges(int randomizedSpawnSide)
        {
            var edgeCoordinatedX = GetEdgeCoordinateXBasedOnObject();
            var randomizedHeight = GetEdgeCoordinateYBasedOnRect();

            transform.position = new Vector3(randomizedSpawnSide * edgeCoordinatedX, randomizedHeight);
        }
        private float GetEdgeCoordinateYBasedOnRect()
        {
            var rectHeight = _rect.y;
            var ufoHeightRandomizer = Random.Range(-0.5f, 0.5f);
            var randomizedHeight = ufoHeightRandomizer * rectHeight;
            return randomizedHeight;
        }
        private float GetEdgeCoordinateXBasedOnObject()
        {
            var spriteOffsetEdgeX = GetObjectBounds().size.x / 2;
            var rectWidth = _rect.x;

            var edgeCoordinatedX = rectWidth + spriteOffsetEdgeX;
            return edgeCoordinatedX;
        }
        private int GetRandomSign()
        {
            return (Random.Range(0, 1f) < 0.5f) ? 1 : -1;
        }
        
    }
}

