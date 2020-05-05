using System;
using UnityEngine.SceneManagement;

namespace UnityEngine.AddressableAssets
{
    public static partial class AddressablesManager
    {
        public static void Initialize(Action onSucceeded, Action onFailed = null)
        {
            Clear();

            var operation = Addressables.InitializeAsync();
            operation.Completed += handle => OnInitializeCompleted(handle, onSucceeded, onFailed);
        }

        public static void LoadLocations(object key, Action<object> onSucceeded, Action<object> onFailed = null)
        {
            if (key == null)
            {
                onFailed?.Invoke(key);
                return;
            }

            var operation = Addressables.LoadResourceLocationsAsync(key);
            operation.Completed += handle => OnLoadLocationsCompleted(handle, key, onSucceeded, onFailed);
        }

        public static void LoadAsset<T>(string key, Action<string, T> onSucceeded, Action<string> onFailed = null) where T : Object
        {
            if (!GuardKey(key, out key))
            {
                onFailed?.Invoke(key);
                return;
            }

            if (!_assets.ContainsKey(key))
            {
                var operation = Addressables.LoadAssetAsync<T>(key);
                operation.Completed += handle => OnLoadAssetCompleted(handle, key, onSucceeded, onFailed);
                return;
            }

            if (_assets[key] is T asset)
            {
                onSucceeded?.Invoke(key, asset);
            }
            else
            {
                Debug.LogWarning($"The asset with key={key} is not an instance of {typeof(T)}.");
                onFailed?.Invoke(key);
            }
        }

        public static void LoadAsset<T>(AssetReferenceT<T> reference, Action<string, T> onSucceeded,
            Action<string> onFailed = null) where T : Object
        {
            if (!GuardKey(reference, out var key))
            {
                onFailed?.Invoke(key);
                return;
            }

            if (!_assets.ContainsKey(key))
            {
                var operation = reference.LoadAssetAsync<T>();
                operation.Completed += handle => OnLoadAssetCompleted(handle, key, onSucceeded, onFailed);
                return;
            }

            if (_assets[key] is T asset)
            {
                onSucceeded?.Invoke(key, asset);
            }
            else
            {
                Debug.LogWarning($"The asset with key={key} is not an instance of {typeof(T)}.");
                onFailed?.Invoke(key);
            }
        }

        public static void LoadScene(string key, Action<Scene> onSucceeded, Action<string> onFailed = null,
            LoadSceneMode loadMode = LoadSceneMode.Single, bool activeOnLoad = true, int priority = 100)
        {
            if (!GuardKey(key, out key))
            {
                onFailed?.Invoke(key);
                return;
            }

            if (_scenes.ContainsKey(key))
            {
                onSucceeded?.Invoke(_scenes[key].Scene);
                return;
            }

            var operation = Addressables.LoadSceneAsync(key, loadMode, activeOnLoad, priority);
            operation.Completed += handle => OnLoadSceneCompleted(handle, key, onSucceeded, onFailed);
        }

        public static void LoadScene(AssetReference reference, Action<Scene> onSucceeded, Action<string> onFailed = null,
            LoadSceneMode loadMode = LoadSceneMode.Single, bool activeOnLoad = true, int priority = 100)
        {
            if (!GuardKey(reference, out var key))
            {
                onFailed?.Invoke(key);
                return;
            }

            if (_assets.ContainsKey(key))
            {
                onSucceeded?.Invoke(_scenes[key].Scene);
                return;
            }

            var operation = reference.LoadSceneAsync(loadMode, activeOnLoad, priority);
            operation.Completed += handle => OnLoadSceneCompleted(handle, key, onSucceeded, onFailed);
        }

        public static void UnloadScene(string key, Action<string> onSucceeded = null, Action<string> onFailed = null,
            bool autoReleaseHandle = true)
        {
            if (!GuardKey(key, out key))
            {
                onFailed?.Invoke(key);
                return;
            }

            if (!_scenes.TryGetValue(key, out var scene))
            {
                onFailed?.Invoke(key);
                return;
            }

            _scenes.Remove(key);

            var operation = Addressables.UnloadSceneAsync(scene, autoReleaseHandle);
            operation.Completed += handle => OnUnloadSceneCompleted(handle, key, onSucceeded, onFailed);
        }

        public static void UnloadScene(AssetReference reference, Action<string> onSucceeded = null, Action<string> onFailed = null)
        {
            if (!GuardKey(reference, out var key))
            {
                onFailed?.Invoke(key);
                return;
            }

            if (!_scenes.ContainsKey(key))
            {
                onFailed?.Invoke(key);
                return;
            }

            _scenes.Remove(key);

            var operation = reference.UnLoadScene();
            operation.Completed += handle => OnUnloadSceneCompleted(handle, key, onSucceeded, onFailed);
        }

        public static void Instantiate(string key, Action<string, GameObject> onSucceeded, Action<string> onFailed = null,
            Transform parent = null, bool inWorldSpace = false, bool trackHandle = true)
        {
            if (!GuardKey(key, out key))
            {
                onFailed?.Invoke(key);
                return;
            }

            var operation = Addressables.InstantiateAsync(key, parent, inWorldSpace, trackHandle);
            operation.Completed += handle => OnInstantiateCompleted(handle, key, onSucceeded, onFailed);
        }

        public static void Instantiate(AssetReference reference, Action<string, GameObject> onSucceeded,
            Action<string> onFailed = null, Transform parent = null, bool inWorldSpace = false)
        {
            if (!GuardKey(reference, out var key))
            {
                onFailed?.Invoke(key);
                return;
            }

            var operation = reference.InstantiateAsync(parent, inWorldSpace);
            operation.Completed += handle => OnInstantiateCompleted(handle, key, onSucceeded, onFailed);
        }
    }
}