using System;

using UnityEngine;

using Assets.Scripts.GameConstants;
using Assets.Scripts.Gameplay.MonoBehaviours;
using Assets.Scripts.GameStructure.Communication.EventSystem;
using Assets.Scripts.GameStructure.Communication.Payloads;
using Assets.Scripts.GameStructure.Pooling;
using System.Collections;

public class VFXSpawner
{
    private readonly float vfxWaitBeforeRelease = 1f;
    public readonly IBaseObjectPool<IVfxItem> VFXPool;
    private MonoBehaviour _coroutineReferenceMonoBehaviour;

    #region CONSTRUCTOR

    public VFXSpawner(MonoBehaviour coroutineReferenceMonoBehaviour)
    {
        _coroutineReferenceMonoBehaviour = coroutineReferenceMonoBehaviour;
        VFXPool = new BaseObjectPool<IVfxItem>(Prefabs.VFXExplosion, InitialPoolSize.VfxExplosion);
        SubscribeEvents();
    }
    ~VFXSpawner()
    {
        UnsubscribeEvents();
    }

    #endregion

    private void Spawn(Vector2 spawnPosition)
    {
        //Logger.Info($"Spawning explosion at {spawnPosition}");
        var obtainedItem = VFXPool.GetFreeItem();
        obtainedItem.SetPosition(spawnPosition);
        obtainedItem.Play();
        _coroutineReferenceMonoBehaviour.StartCoroutine(ReleaseRoutine(obtainedItem));
    }
    private IEnumerator ReleaseRoutine(IVfxItem obtainedItem)
    {
        yield return new WaitForSeconds(vfxWaitBeforeRelease);
        obtainedItem.Release();
    }

    private void SubscribeEvents()
    {
        EventBus.GetInstance().Subscribe(EventKey.AsteroidExploded, OnAsteroidExplosion);
        EventBus.GetInstance().Subscribe(EventKey.SpaceshipExploded, OnSpaceshipExploded);
        EventBus.GetInstance().Subscribe(EventKey.UFOExploded, OnUFOExploded);
        EventBus.GetInstance().Subscribe(EventKey.GameOver, OnGameOver);
    }
    private void UnsubscribeEvents()
    {
        EventBus.GetInstance().Unsubscribe(EventKey.AsteroidExploded, OnAsteroidExplosion);
        EventBus.GetInstance().Unsubscribe(EventKey.SpaceshipExploded, OnSpaceshipExploded);
        EventBus.GetInstance().Unsubscribe(EventKey.UFOExploded, OnUFOExploded);
        EventBus.GetInstance().Unsubscribe(EventKey.GameOver, OnGameOver);
    }
    private void OnAsteroidExplosion(IBasePayload basePayload)
    {
        AsteroidExplodedPayload payload = (AsteroidExplodedPayload) basePayload;
        Spawn(payload.Position);
    }
    private void OnSpaceshipExploded(IBasePayload basePayload)
    {
        SpaceshipExplodedPayload payload = (SpaceshipExplodedPayload)basePayload;
        Spawn(payload.Position);
    }
    private void OnUFOExploded(IBasePayload basePayload)
    {
        UfoExplodedPayload payload = (UfoExplodedPayload)basePayload;
        Spawn(payload.Position);
    }

    private void OnGameOver(IBasePayload basePayload)
    {
        VFXPool.ReleaseAll();
    }
}
