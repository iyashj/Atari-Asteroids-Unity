using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using Assets.Scripts.GameConstants;

public class SpawnerUtility
{
    public static AsyncOperationHandle<GameObject> GetAsyncAssetLoad(string prefabKey)
    {
        prefabKey = string.Concat(Prefabs.PrefabLocation, prefabKey);
        return Addressables.LoadAssetAsync<GameObject>(prefabKey);
    }
    public static AsyncOperationHandle<GameObject> InstantiateGameObject(string prefabKey)
    {
        return InstantiateGameObject(prefabKey, Vector3.zero, Quaternion.identity);
    }
    public static AsyncOperationHandle<GameObject> InstantiateGameObject(string prefabKey, Vector3 position, Quaternion rotation)
    {
        return Addressables.InstantiateAsync(string.Concat(Prefabs.PrefabLocation, prefabKey), position, rotation);
    }
}