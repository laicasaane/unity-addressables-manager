#if ADDRESSABLES_UNITASK

using System;
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

            try
            {
                var operation = Addressables.InitializeAsync();
                await operation;

                OnInitializeCompleted(operation);
                return operation;
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                return default;
            }
        }

        /// <summary>
        /// Loads the resource locations specified by a key.
        /// </summary>
        /// <param name="key">The key for the locations.</param>
        /// <returns></returns>
        /// <exception cref="InvalidKeyException"></exception>
        /// <exception cref="Exception"></exception>
        public static async UniTask<OperationResult<object>> LoadLocationsAsync(object key)
        {
            if (key == null)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw new InvalidKeyException((object) null);

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException((object) null));

                return new OperationResult<object>(false, null);
            }

            try
            {
                var operation = Addressables.LoadResourceLocationsAsync(key);
                await operation;

                OnLoadLocationsCompleted(operation, key);
                return new OperationResult<object>(operation.Status == AsyncOperationStatus.Succeeded, key);
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                return new OperationResult<object>(false, key);
            }
        }

        public static async UniTask<OperationResult<T>> LoadAssetAsync<T>(string key) where T : Object
        {
            if (!GuardKey(key, out key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw new InvalidKeyException(key);

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return default;
            }

            if (_assets.ContainsKey(key))
            {
                if (_assets[key] is T asset)
                    return new OperationResult<T>(true, asset);


                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.AssetKeyNotInstanceOf<T>(key));

                return default;
            }

            try
            {
                var operation = Addressables.LoadAssetAsync<T>(key);
                await operation;

                OnLoadAssetCompleted(operation, key, false);
                return operation;
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                return default;
            }
        }

        /// <summary>
        /// Load the referenced asset as type T. This cannot be used a second time until the first load is released.
        /// and pass your key im.
        /// </summary>
        /// <param name="reference"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidKeyException"></exception>
        /// <exception cref="Exception"></exception>
        public static async UniTask<OperationResult<T>> LoadAssetAsync<T>(AssetReferenceT<T> reference) where T : Object
        {
            if (!GuardKey(reference, out var key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw Exceptions.InvalidReference;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return default;
            }

            if (_assets.ContainsKey(key))
            {
                if (_assets[key] is T asset)
                    return new OperationResult<T>(true, asset);


                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.AssetReferenceNotInstanceOf<T>(key));

                return default;
            }

            try
            {
                var operation = reference.LoadAssetAsync<T>();
                await operation;

                OnLoadAssetCompleted(operation, key, true);
                return operation;
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                return default;
            }
        }

        public static async UniTask ActivateSceneAsync(SceneInstance scene, int priority)
        {
            var operation = scene.ActivateAsync();
            operation.priority = priority;

            await operation;
        }

        /// <summary>
        /// Loads an Addressable Scene asset.
        /// </summary>
        /// <param name="key">The key of the location of the scene to load.</param>
        /// <param name="loadMode">Scene load mode.</param>
        /// <param name="activateOnLoad">If false, the scene will load but not activate (for background loading). The SceneInstance returned has an Activate() method that can be called to do this at a later point.</param>
        /// <param name="priority">Async operation priority for scene loading.</param>
        /// <returns></returns>
        /// <exception cref="InvalidKeyException">Exception to encapsulate invalid key errors.</exception>
        /// <exception cref="Exception">Other exception</exception>
        public static async UniTask<OperationResult<SceneInstance>> LoadSceneAsync(string key,
            LoadSceneMode loadMode = LoadSceneMode.Single,
            bool activateOnLoad = true,
            int priority = 100)
        {
            if (!GuardKey(key, out key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw new InvalidKeyException(key);

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return default;
            }

            if (_scenes.TryGetValue(key, out var scene))
            {
                if (activateOnLoad)
                    await ActivateSceneAsync(scene, priority);

                return new OperationResult<SceneInstance>(true, in scene);
            }

            try
            {
                var operation = Addressables.LoadSceneAsync(key, loadMode, activateOnLoad, priority);
                await operation;

                OnLoadSceneCompleted(operation, key);
                return operation;
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                return default;
            }
        }

        /// <summary>
        /// Loads the reference as a scene. This cannot be used a second time until the first load is unloaded.
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="loadMode"></param>
        /// <param name="activateOnLoad"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        /// <exception cref="InvalidKeyException">Exception to encapsulate invalid key errors.</exception>
        /// <exception cref="Exception">Other exception</exception>
        public static async UniTask<OperationResult<SceneInstance>> LoadSceneAsync(AssetReference reference,
            LoadSceneMode loadMode = LoadSceneMode.Single,
            bool activateOnLoad = true,
            int priority = 100)
        {
            if (!GuardKey(reference, out var key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw Exceptions.InvalidReference;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return default;
            }

            if (_scenes.TryGetValue(key, out var scene))
            {
                if (activateOnLoad)
                    await ActivateSceneAsync(scene, priority);

                return new OperationResult<SceneInstance>(true, in scene);
            }

            try
            {
                var operation = reference.LoadSceneAsync(loadMode, activateOnLoad, priority);
                await operation;

                OnLoadSceneCompleted(operation, key);
                return operation;
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                return default;
            }
        }

        /// <summary>
        /// Release the scene instance.
        /// </summary>
        /// <param name="key">The key of the location of the scene instance</param>
        /// <param name="autoReleaseHandle">If true, the handle will be released automatically when complete.</param>
        /// <returns></returns>
        /// <exception cref="InvalidKeyException">Exception to encapsulate invalid key errors.</exception>
        /// <exception cref="Exception">Other exception</exception>
        public static async UniTask<OperationResult<SceneInstance>> UnloadSceneAsync(string key,
            bool autoReleaseHandle = true)
        {
            if (!GuardKey(key, out key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw new InvalidKeyException(key);

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return default;
            }

            if (!_scenes.TryGetValue(key, out var scene))
            {
                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.NoSceneKeyLoaded(key));

                return default;
            }

            _scenes.Remove(key);

            try
            {
                var operation = Addressables.UnloadSceneAsync(scene, autoReleaseHandle);
                await operation;

                return operation;
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                return new OperationResult<SceneInstance>(false, in scene);
            }
        }

        /// <summary>
        /// Unloads the reference as a scene.
        /// </summary>
        /// <param name="reference">Reference to an addressable asset. This can be used in script to provide fields that can be easily set in the editor and loaded dynamically at runtime. To determine if the reference is set, use RuntimeKeyIsValid().</param>
        /// <returns></returns>
        /// <exception cref="InvalidKeyException">Exception to encapsulate invalid key errors.</exception>
        /// <exception cref="Exception">Other exception</exception>
        public static async UniTask<OperationResult<SceneInstance>> UnloadSceneAsync(AssetReference reference)
        {
            if (!GuardKey(reference, out var key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw Exceptions.InvalidReference;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return default;
            }

            if (!_scenes.TryGetValue(key, out var scene))
            {
                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.NoSceneReferenceLoaded(key));

                return default;
            }

            _scenes.Remove(key);

            try
            {
                var operation = reference.UnLoadScene();
                await operation;

                return operation;
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                return new OperationResult<SceneInstance>(false, scene);
            }
        }

        /// <summary>
        /// Instantiate a single game object and cache it int the AddressableManager for future use.
        /// </summary>
        /// <param name="key">The key of the location of the Object to instantiate.</param>
        /// <param name="parent">Parent transform of instantiated object.</param>
        /// <param name="inWorldSpace">Option to retain world space when instantiated with a parent.</param>
        /// <param name="trackHandle">If true, Addressables will track this request to allow it to be released via the result object.</param>
        /// <returns></returns>
        /// <exception cref="InvalidKeyException">Exception to encapsulate invalid key errors.</exception>
        /// <exception cref="Exception">Other exception</exception>
        public static async UniTask<OperationResult<GameObject>> InstantiateAsync(string key,
            Transform parent = null,
            bool inWorldSpace = false,
            bool trackHandle = true)
        {
            if (!GuardKey(key, out key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw new InvalidKeyException(key);

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return default;
            }

            try
            {
                var operation = Addressables.InstantiateAsync(key, parent, inWorldSpace, trackHandle);
                await operation;

                OnInstantiateCompleted(operation, key, false);
                return operation;
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                return default;
            }
        }

        /// <summary>
        /// Instantiate a single game object and cache it int the AddressableManager for future use.
        /// </summary>
        /// <param name="key">The key of the location of the Object to instantiate.</param>
        /// <param name="position">The position of the instantiated object.</param>
        /// <param name="rotation">The rotation of the instantiated object.</param>
        /// <param name="parent">Parent transform of instantiated object.</param>
        /// <param name="trackHandle">If true, Addressables will track this request to allow it to be released via the result object.</param>
        /// <returns></returns>
        /// <exception cref="InvalidKeyException">Exception to encapsulate invalid key errors.</exception>
        /// <exception cref="Exception">Other exception</exception>
        public static async UniTask<OperationResult<GameObject>> InstantiateAsync(string key, Vector3 position,
            Quaternion rotation,
            Transform parent = null,
            bool trackHandle = true)
        {
            if (!GuardKey(key, out key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw new InvalidKeyException(key);

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return default;
            }

            try
            {
                var operation = Addressables.InstantiateAsync(key, position, rotation, parent, trackHandle);
                await operation;

                OnInstantiateCompleted(operation, key, false);
                return operation;
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                return default;
            }
        }

        /// <summary>
        /// Instantiate a single game object and cache it int the AddressableManager for future use.
        /// </summary>
        /// <param name="reference">Reference to an addressable asset. This can be used in script to provide fields that can be easily set in the editor and loaded dynamically at runtime. To determine if the reference is set, use RuntimeKeyIsValid().</param>
        /// <param name="parent">The parent of the instantiated object</param>
        /// <param name="inWorldSpace">Option to retain world space when instantiated with a parent</param>
        /// <returns></returns>
        /// <exception cref="InvalidKeyException">Exception to encapsulate invalid key errors.</exception>
        /// <exception cref="Exception">Other exception</exception>
        public static async UniTask<OperationResult<GameObject>> InstantiateAsync(AssetReference reference,
            Transform parent = null,
            bool inWorldSpace = false)
        {
            if (!GuardKey(reference, out var key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw Exceptions.InvalidReference;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return default;
            }

            try
            {
                var operation = reference.InstantiateAsync(parent, inWorldSpace);
                await operation;

                OnInstantiateCompleted(operation, key, true);
                return operation;
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                return default;
            }
        }

        /// <summary>
        /// Instantiate a single game object and cache it int the AddressableManager for future use.
        /// </summary>
        /// <param name="reference">Reference to an addressable asset. This can be used in script to provide fields that can be easily set in the editor and loaded dynamically at runtime. To determine if the reference is set, use RuntimeKeyIsValid().</param>
        /// <param name="position">The position of the instantiated object.</param>
        /// <param name="rotation">The rotation of the instantiated object.</param>
        /// <param name="parent">The parent of the instantiated object.</param>
        /// <returns></returns>
        /// <exception cref="InvalidKeyException">Exception to encapsulate invalid key errors.</exception>
        /// <exception cref="Exception">Other exception</exception>
        public static async UniTask<OperationResult<GameObject>> InstantiateAsync(AssetReference reference,
            Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (!GuardKey(reference, out var key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw Exceptions.InvalidReference;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return default;
            }

            try
            {
                var operation = reference.InstantiateAsync(position, rotation, parent);
                await operation;

                OnInstantiateCompleted(operation, key, true);
                return operation;
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                return default;
            }
        }
    }
}

#endif