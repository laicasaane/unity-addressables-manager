#if ADDRESSABLES_UNITASK

using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

namespace UnityEngine.AddressableAssets
{
    using ResourceManagement.AsyncOperations;
    using ResourceManagement.ResourceProviders;
    using AddressableAssets.ResourceLocators;

    public static partial class AddressablesManager
    {
        public static async UniTask<OperationResult<IResourceLocator>> InitializeAsync()
        {
            Clear();

            var operation = Addressables.InitializeAsync();
            await operation;

            OnInitializeCompleted(operation);
            return operation;
        }

        public static async UniTask<OperationResult<object>> LoadLocationsAsync(object key)
        {
            if (key == null)
                return new OperationResult<object>(false, key);

            var operation = Addressables.LoadResourceLocationsAsync(key);
            await operation;

            OnLoadLocationsCompleted(operation, key);
            return new OperationResult<object>(operation.Status == AsyncOperationStatus.Succeeded, key);
        }

        public static async UniTask<OperationResult<T>> LoadAssetAsync<T>(string key) where T : Object
        {
            if (!GuardKey(key, out key))
                return default;

            if (!_assets.ContainsKey(key))
            {
                var operation = Addressables.LoadAssetAsync<T>(key);
                await operation;

                OnLoadAssetCompleted(operation, key);
                return operation;
            }

            if (_assets[key] is T asset)
                return new OperationResult<T>(true, asset);

            Debug.LogWarning($"The asset with key={key} is not an instance of {typeof(T)}.");
            return default;
        }

        public static async UniTask<OperationResult<T>> LoadAssetAsync<T>(AssetReferenceT<T> reference) where T : Object
        {
            if (!GuardKey(reference, out var key))
                return default;

            if (!_assets.ContainsKey(key))
            {
                var operation = reference.LoadAssetAsync<T>();
                await operation;

                OnLoadAssetCompleted(operation, key);
                return operation;
            }

            if (_assets[key] is T asset)
                return new OperationResult<T>(true, asset);

            Debug.LogWarning($"The asset with key={key} is not an instance of {typeof(T)}.");
            return default;
        }

        public static async UniTask<OperationResult<SceneInstance>> LoadSceneAsync(string key,
            LoadSceneMode loadMode, bool activeOnLoad = true, int priority = 100)
        {
            if (!GuardKey(key, out key))
                return default;

            if (_scenes.ContainsKey(key))
                return new OperationResult<SceneInstance>(true, _scenes[key]);

            var operation = Addressables.LoadSceneAsync(key, loadMode, activeOnLoad, priority);
            await operation;

            OnLoadSceneCompleted(operation, key);
            return operation;
        }

        public static async UniTask<OperationResult<SceneInstance>> LoadSceneAsync(AssetReference reference,
            LoadSceneMode loadMode, bool activeOnLoad = true, int priority = 100)
        {
            if (!GuardKey(reference, out var key))
                return default;

            if (_assets.ContainsKey(key))
                return new OperationResult<SceneInstance>(true, _scenes[key]);

            var operation = reference.LoadSceneAsync(loadMode, activeOnLoad, priority);
            await operation;

            OnLoadSceneCompleted(operation, key);
            return operation;
        }

        public static async UniTask<OperationResult<SceneInstance>> UnloadSceneAsync(string key, bool autoReleaseHandle = true)
        {
            if (!GuardKey(key, out key))
                return default;

            if (!_scenes.TryGetValue(key, out var scene))
                return default;

            _scenes.Remove(key);

            var operation = Addressables.UnloadSceneAsync(scene, autoReleaseHandle);
            await operation;

            return operation;
        }

        public static async UniTask<OperationResult<SceneInstance>> UnloadSceneAsync(AssetReference reference)
        {
            if (!GuardKey(reference, out var key))
                return default;

            if (!_scenes.ContainsKey(key))
                return default;

            _scenes.Remove(key);

            var operation = reference.UnLoadScene();
            await operation;

            return operation;
        }

        public static async UniTask<OperationResult<GameObject>> InstantiateAsync(string key,
            Transform parent = null, bool inWorldSpace = false, bool trackHandle = true)
        {
            if (!GuardKey(key, out key))
                return default;

            var operation = Addressables.InstantiateAsync(key, parent, inWorldSpace, trackHandle);
            await operation;

            OnInstantiateCompleted(operation, key);
            return operation;
        }

        public static async UniTask<OperationResult<GameObject>> InstantiateAsync(AssetReference reference,
            Transform parent = null, bool inWorldSpace = false)
        {
            if (!GuardKey(reference, out var key))
                return default;

            var operation = reference.InstantiateAsync(parent, inWorldSpace);
            await operation;

            OnInstantiateCompleted(operation, key);
            return operation;
        }
    }
}

#endif