using System.Collections;

using UnityEngine;

using Assets.Scripts.Data;
using Assets.Scripts.GameConstants;
using Assets.Scripts.Gameplay.MonoBehaviours;
using Assets.Scripts.Gameplay.Spawners;
using Assets.Scripts.Gameplay.Subsystems;
using Assets.Scripts.GameStructure.Communication.EventSystem;
using Assets.Scripts.GameStructure.Communication.Payloads;
using UnityEngine.UI;

/// <summary>
/// Traditional GameManager class deals with
/// initializing spawners & subsystems
/// injecting dependencies
/// starting game and acting as a monobehaviour reference for starting coroutines
/// </summary>
public class GameManager : MonoBehaviour
{
    #region SERIALIZED_FIELDS

    [Header("Game Configs")]
    [SerializeField] private LevelingConfig levelingConfig;
    [SerializeField] private ScoreConfig scoreConfig;
    [SerializeField] private SFXConfig sfxConfig;
    [SerializeField] private int _maxLives = 3;
    [SerializeField] private int _startLevel = 1;
    [SerializeField] private float _waitBeforeTryAgainMenu = 2.5f;

    [Space(5)]
    [Header("UI")]
    [SerializeField] private TMPro.TextMeshProUGUI _scoreText;
    [SerializeField] private TMPro.TextMeshProUGUI _livesText;
    [SerializeField] private TMPro.TextMeshProUGUI _gameOverText;
    [SerializeField] private GameObject _playAgainMenu;
    [SerializeField] private Button _playAgainButton;
    [SerializeField] private Button _QuitGameButton;

    [Space(5)]
    [Header("Camera")]
    [SerializeField] private Camera _camera;

    #endregion

    #region DEPENDENCIES

    private ILevelingConfig _levelingConfig;
    private IScoreConfig _scoreConfig;

    private IAsteroidSpawner _asteroidSpawner;
    private ISpaceshipSpawner _spaceshipSpawner;
    private IUFOSpawner _iufoSpawner;

    private IScreenHandler _screenHandler;
    private ILevelHandler _levelHandler;
    private IScreenWrapper _screenWrapper;
    private IGameStatsHandler _gameStatsHandler;
    private ISfxSpawner _sfxSpawner;
    private ISpaceship _spaceship;

    #endregion

    #region UNITY_FUNCTION
    private void Start()
    {
        Application.targetFrameRate = 60;
        AttachMethodsToGameMenu();

        SetPhysicsLayer();
        CacheScriptableObjectConfigsToDependencies();
        InitializeSpawners();
        InitializeSubsystems();
        
        _iufoSpawner = new UFOSpawner(_screenHandler.GetScreenRect(), this);
        
        PreloadSpawnerDepdendencies();
        SubscribeToGameEvents();
        
        StartCoroutine(StartGameOnSpawersPreloadComplete());
    }
    private void Update()
    {
        _screenWrapper.Update();
    }
    private void OnDisable()
    {
        UnsubscribeFromGameEvents();
    }
    #endregion

    private void SubscribeToGameEvents()
    {
        EventBus.GetInstance().Subscribe(EventKey.SpaceshipSpawned, OnSpaceshipSpawned);
        EventBus.GetInstance().Subscribe(EventKey.GameOver, OnGameOver);
    }
    private void UnsubscribeFromGameEvents()
    {
        EventBus.GetInstance().Unsubscribe(EventKey.SpaceshipSpawned, OnSpaceshipSpawned);
        EventBus.GetInstance().Unsubscribe(EventKey.GameOver, OnGameOver);
    }
    private void OnSpaceshipSpawned(IBasePayload basePayload)
    {
        Logger.Info($"Spawned spaceship");
        SpaceshipSpawnedPayload spaceshipSpawnedPayload = (SpaceshipSpawnedPayload)basePayload;
        _spaceship = spaceshipSpawnedPayload.spaceshipBehaviour;
    }
    private void OnGameOver(IBasePayload basePayload)
    {
        Logger.Info($"GameOver Reached: GameManager");
        StartCoroutine(DelayedPlayAgainMenu());
    }

    
    /// <summary>
    /// Caches configs from serialized field to dependencies
    /// </summary>
    private void CacheScriptableObjectConfigsToDependencies()
    {
        Logger.Info($"Cache scriptable object configs to dependencies");
        _levelingConfig = levelingConfig;
        _scoreConfig = scoreConfig;
    }

    /// <summary>
    /// Initializes gameplay spawners for Asteroids, UFOs and Spaceship
    /// </summary>
    private void InitializeSpawners()
    {
        Logger.Info($"Initializing spawners");

        _spaceshipSpawner = new SpaceshipSpawner();
        _asteroidSpawner = new AsteroidSpawner();
    }
    
    /// <summary>
    /// Preloads the associated asset(s)/prefab(s) for spawning object(s) later in the gameplay
    /// </summary>
    private void PreloadSpawnerDepdendencies()
    {
        Logger.Info($"preloading dependencies for spawners");

        _spaceshipSpawner.Preload();
        _asteroidSpawner.Preload();
        _iufoSpawner.Preload();
    }

    /// <summary>
    /// Initializes & injects the required dependency in the various subsystems
    /// </summary>
    private void InitializeSubsystems()
    {
        InitializeSfxSpawner();
        InitializeVfxSpawner();
        InitializeScreenHandler();
        InitializeScreenWrapper();
        InitializeLevelHandler();
        InitializeUIHandler();
        InitializeGameStatsHandler();
    }

    private void InitializeSfxSpawner()
    {
        var sfxHandlerConstructionData = new SfxSpawnerConstructionData(InitialPoolSize.SfxAudioSource, sfxConfig, this);
        _sfxSpawner = new SfxSpawner(sfxHandlerConstructionData);
    }
    private void InitializeVfxSpawner()
    {
        var vfxSpawner = new VFXSpawner(this);
    }
    private void InitializeScreenHandler()
    {
        _screenHandler = new ScreenHandler(_camera);
    }
    private void InitializeScreenWrapper()
    {
        _screenWrapper = new ScreenWrapper(_screenHandler);
    }
    private void InitializeLevelHandler()
    {
        var levelHandlerConstructionData =
            new LevelHandlerConstructionData(_asteroidSpawner, _screenHandler, _levelingConfig, this);
        _levelHandler = new LevelHandler(levelHandlerConstructionData);
    }
    private void InitializeUIHandler()
    {
        new UIHandler( _scoreText, _livesText,_gameOverText);
    }
    private void InitializeGameStatsHandler()
    {
        _gameStatsHandler = new GameStatsHandler(_maxLives, _scoreConfig);
    }
    
    /// <summary>
    /// Returns true if all spawners are loaded
    /// </summary>
    /// <returns></returns>
    private bool AreAllSpawnersLoaded()
    {
        var allAsteridPoolsReady = _asteroidSpawner.IsReady();
        var spaceshipSpawnerReady = _spaceshipSpawner.IsReady();
        var ufoSpawnerReady = _iufoSpawner.IsReady();
        return spaceshipSpawnerReady && allAsteridPoolsReady && ufoSpawnerReady;
    }

    /// <summary>
    /// Routine that starts the game once all the spawners are loaded
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartGameOnSpawersPreloadComplete()
    {
        yield return new WaitWhile(() => !AreAllSpawnersLoaded());
        SetupGame();
        StartGame();
    }

    private void SetupGame()
    {
        Logger.Info($"Setting up Game");
        _spaceshipSpawner.Spawn();
        SpaceshipDependencyData injectionData = new SpaceshipDependencyData(_screenHandler, _gameStatsHandler,
            _levelingConfig.GetSafeDistanceFromPlayerShip());
        _spaceship.Inject(injectionData);
        _iufoSpawner.Spawn();
    }
    private void StartGame()
    {
        Logger.Info($"Starting Game");
        _levelHandler.Start(_startLevel);
        
    }

    /// <summary>
    /// Configure physics2D layer for customized collision matrix
    /// </summary>
    private void SetPhysicsLayer()
    {
        Logger.Info($"Setting physics layer");

        Physics2D.IgnoreLayerCollision(Layer.SpaceshipLayer, Layer.SpaceshipLayer);
        Physics2D.IgnoreLayerCollision(Layer.UFOLayer, Layer.UFOLayer);
        Physics2D.IgnoreLayerCollision(Layer.PlayerBulletLayer, Layer.PlayerBulletLayer);
        Physics2D.IgnoreLayerCollision(Layer.AsteroidLayer, Layer.AsteroidLayer);
        Physics2D.IgnoreLayerCollision(Layer.EnemyBulletLayer, Layer.EnemyBulletLayer);

        Physics2D.IgnoreLayerCollision(Layer.SpaceshipLayer, Layer.PlayerBulletLayer);
        Physics2D.IgnoreLayerCollision(Layer.UFOLayer, Layer.EnemyBulletLayer);
        Physics2D.IgnoreLayerCollision(Layer.PlayerBulletLayer, Layer.EnemyBulletLayer);
    }
    /// <summary>
    /// Routine to enable play again menu after timed delay
    /// </summary>
    /// <returns></returns>
    private IEnumerator DelayedPlayAgainMenu()
    {
        yield return new WaitForSeconds(_waitBeforeTryAgainMenu);
        EnablePlayAgainMenu();
    }
    /// <summary>
    /// Enable try again menu on gameover
    /// </summary>
    
    private void EnablePlayAgainMenu()
    {
        _playAgainMenu.gameObject.SetActive(true);
    }
    private void AttachMethodsToGameMenu()
    {
        _playAgainButton.onClick.AddListener(PlayAgain);
        _QuitGameButton.onClick.AddListener(QuitGame);
    }
    private void PlayAgain()
    {
        _playAgainMenu.gameObject.SetActive(false);
        _gameOverText.gameObject.SetActive(false);
        StartGame();
        EventBus.GetInstance().Publish(new PlayAgainPayload(EventKey.PlayAgainPayload));
    }
    private void QuitGame()
    {
        Application.Quit();
    }
}