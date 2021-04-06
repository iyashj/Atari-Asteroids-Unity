using System.Data;

namespace Assets.Scripts.GameConstants
{
    /// <summary>
    /// Constants to be used with EventBus Communcation
    /// </summary>
    public class EventKey
    {
        public const string AsteroidExploded = "AsteroidExploded";
        public const string ToggleScreenWrapping = "ToggleScreenWrapping";
        public const string SpaceshipExploded = "SpaceshipExploded";
        public const string UFOExploded = "UFOExploded";
        public const string Shoot = "Shoot";
        public const string Hyperspace = "Hyperspace";

        public const string PlayAgainPayload = "PlayAgainPayload";
        public const string GameOver = "GameOver";
        public const string SpaceshipSpawned = "SpaceshipSpawned";
        public const string ChangeLives = "ChangeLives";
        public const string ChangeScores = "ChangeScores";
    }

    /// <summary>
    /// Constants specifying layers used in the game for various gameObjects
    /// </summary>
    public class Layer
    {
        public const int SpaceshipLayer = 8;
        public const int UFOLayer = 9;
        public const int PlayerBulletLayer = 10;
        public const int AsteroidLayer = 11;
        public const int EnemyBulletLayer = 12;
    }

    /// <summary>
    /// Constants used to get addressable path for prefabs 
    /// </summary>
    public class Prefabs
    {
        public const string PrefabLocation = "Prefabs/";

        public const string SpaceshipSpawnKey = "Spaceship";
        public const string UFOSmallKey = "UFOSmall";
        public const string UFOLargeKey = "UFOLarge";

        public const string AsteroidLevel0 = "AsteroidLevel0";
        public const string AsteroidLevel1 = "AsteroidLevel1";
        public const string AsteroidLevel2 = "AsteroidLevel2";


        public const string PlayerBullet = "PlayerBullet";
        public const string EnemyBullet = "EnemyBullet";
        public const string VFXExplosion = "Explosion";
    }

    public class Asteroids
    {
        public const int Level3Id = 2;
        public const int Level3AsteroidWeight = 7;
    }

    public class Spaceship
    {
        public const int NumberOfTriesForSafeSpawn = 4;
    }
    
    public class UFO
    {
        public const float DelayBetweenSubsequentSpawns = 10f;
    }

    /// <summary>
    /// Constants used to get default pool sizes
    /// </summary>
    public class InitialPoolSize
    {
        public const int VfxExplosion = 10;
        public const int SfxAudioSource = 10;
        public const int Bullet = 10;

        public const int Asteroids = 10;
        public const int AppendLengthOnExhaustion = 5;
    }
}