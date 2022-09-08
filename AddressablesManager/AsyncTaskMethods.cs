#if !ADDRESSABLES_UNITASK

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using UnityEngine.SceneManagement;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace UnityEngine.AddressableAssets
{
    using ResourceManagement.ResourceProviders;
    using AddressableAssets.ResourceLocators;

    public static partial class AddressablesManager
    {
        public static async Task<OperationResult<IResourceLocator>> InitializeAsync(bool autoReleaseHandle = true)
        {
            Clear();

            try
            {
                var operation = Addressables.InitializeAsync(false);
                await operation.Task;

                OnInitializeCompleted(operation);

                var result = new OperationResult<IResourceLocator>(operation);

                if (autoReleaseHandle)
                {
                    Addressables.Release(operation);
                }

                return result;
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                return new OperationResult<IResourceLocator>(false, default, default);
            }
        }

        public static async Task<OperationResult<IList<IResourceLocation>>> LoadLocationsAsync(object key, Type type = null)
        {
            if (key == null)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw new InvalidKeyException(key);

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return new OperationResult<IList<IResourceLocation>>(false, key, default);
            }

            try
            {
                var operation = Addressables.LoadResourceLocationsAsync(key, type);
                await operation.Task;

                OnLoadLocationsCompleted(operation, key);
                return new OperationResult<IList<IResourceLocation>>(key, operation);
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                return new OperationResult<IList<IResourceLocation>>(false, key, default);
            }
        }

        public static async Task<OperationResult<T>> LoadAssetAsync<T>(string key) where T : Object
        {
            if (!GuardKey(key, out key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw new InvalidKeyException(key);

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return new OperationResult<T>(false, key, default);
            }

            if (_assets.ContainsKey(key))
            {
                if (_assets[key] is T asset)
                    return new OperationResult<T>(true, key, asset);

                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.AssetKeyNotInstanceOf<T>(key));

                return new OperationResult<T>(false, key, default);
            }

            try
            {
                var operation = Addressables.LoadAssetAsync<T>(key);
                await operation.Task;

                OnLoadAssetCompleted(operation, key, false);
                return new OperationResult<T>(key, operation);
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                return new OperationResult<T>(false, key, default);
            }
        }

        public static async Task<OperationResult<T>> LoadAssetAsync<T>(AssetReferenceT<T> reference) where T : Object
        {
            if (!GuardKey(reference, out var key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw Exceptions.InvalidReference;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return new OperationResult<T>(false, reference, default);
            }

            if (_assets.ContainsKey(key))
            {
                if (_assets[key] is T asset)
                    return new OperationResult<T>(true, reference, asset);

                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.AssetReferenceNotInstanceOf<T>(key));

                return new OperationResult<T>(false, reference, default);
            }

            try
            {
                var operation = reference.LoadAssetAsync<T>();
                await operation.Task;

                OnLoadAssetCompleted(operation, key, true);
                return new OperationResult<T>(reference, operation);
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                return new OperationResult<T>(false, reference, default);
            }
        }

        public static async Task ActivateSceneAsync(SceneInstance scene, int priority)
        {
            var operation = scene.ActivateAsync();
            operation.priority = priority;

            await operation;
        }

        public static async Task<OperationResult<SceneInstance>> LoadSceneAsync(string key,
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

                return new OperationResult<SceneInstance>(false, key, default);
            }

            if (_scenes.TryGetValue(key, out var scene))
            {
                if (activateOnLoad)
                    await ActivateSceneAsync(scene, priority);

                return new OperationResult<SceneInstance>(true, key, in scene);
            }

            try
            {
                var operation = Addressables.LoadSceneAsync(key, loadMode, activateOnLoad, priority);
                await operation.Task;

                OnLoadSceneCompleted(operation, key);
                return new OperationResult<SceneInstance>(key, operation);
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                return new OperationResult<SceneInstance>(false, key, default);
            }
        }

        public static async Task<OperationResult<SceneInstance>> LoadSceneAsync(AssetReference reference,
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

                return new OperationResult<SceneInstance>(false, reference, default);
            }

            if (_scenes.TryGetValue(key, out var scene))
            {
                if (activateOnLoad)
                    await ActivateSceneAsync(scene, priority);

                return new OperationResult<SceneInstance>(true, reference, in scene);
            }

            try
            {
                var operation = reference.LoadSceneAsync(loadMode, activateOnLoad, priority);
                await operation.Task;

                OnLoadSceneCompleted(operation, key);
                return new OperationResult<SceneInstance>(reference, operation);
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                return new OperationResult<SceneInstance>(false, reference, default);
            }
        }

        public static async Task<OperationResult<SceneInstance>> UnloadSceneAsync(string key, bool autoReleaseHandle = true)
        {
            if (!GuardKey(key, out key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw new InvalidKeyException(key);

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return new OperationResult<SceneInstance>(false, key, default);
            }

            if (!_scenes.TryGetValue(key, out var scene))
            {
                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.NoSceneKeyLoaded(key));

                return new OperationResult<SceneInstance>(false, key, default);
            }

            _scenes.Remove(key);

            try
            {
                var operation = Addressables.UnloadSceneAsync(scene, autoReleaseHandle);
                await operation.Task;

                return new OperationResult<SceneInstance>(key, operation);
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                return new OperationResult<SceneInstance>(false, key, in scene);
            }
        }

        public static async Task<OperationResult<SceneInstance>> UnloadSceneAsync(AssetReference reference)
        {
            if (!GuardKey(reference, out var key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw Exceptions.InvalidReference;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return new OperationResult<SceneInstance>(false, reference, default);
            }

            if (!_scenes.TryGetValue(key, out var scene))
            {
                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.NoSceneReferenceLoaded(key));

                return new OperationResult<SceneInstance>(false, reference, default);
            }

            _scenes.Remove(key);

            try
            {
                var operation = reference.UnLoadScene();
                await operation.Task;

                return new OperationResult<SceneInstance>(reference, operation);
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                return new OperationResult<SceneInstance>(false, reference, scene);
            }
        }

        public static async Task<OperationResult<GameObject>> InstantiateAsync(string key,
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

                return new OperationResult<GameObject>(false, key, default);
            }

            try
            {
                var operation = Addressables.InstantiateAsync(key, parent, inWorldSpace, trackHandle);
                await operation.Task;

                OnInstantiateCompleted(operation, key, false);
                return new OperationResult<GameObject>(key, operation);
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                return new OperationResult<GameObject>(false, key, default);
            }
        }

        public static async Task<OperationResult<GameObject>> InstantiateAsync(AssetReference reference,
                                                                               Transform parent = null,
                                                                               bool inWorldSpace = false)
        {
            if (!GuardKey(reference, out var key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw Exceptions.InvalidReference;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return new OperationResult<GameObject>(false, reference, default);
            }

            try
            {
                var operation = reference.InstantiateAsync(parent, inWorldSpace);
                await operation.Task;

                OnInstantiateCompleted(operation, key, true);
                return new OperationResult<GameObject>(reference, operation);
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                return new OperationResult<GameObject>(false, reference, default);
            }
        }
    }

    internal static partial class AsyncOperationExtensions
    {
        public static AsyncOperationAwaiter GetAwaiter(this AsyncOperation operation)
        {
            return new AsyncOperationAwaiter(operation);
        }
    }

    internal readonly struct AsyncOperationAwaiter : INotifyCompletion
    {
        private readonly AsyncOperation operation;

        public AsyncOperationAwaiter(AsyncOperation operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public bool IsCompleted
            => this.operation.isDone;

        public void OnCompleted(Action continuation)
            => this.operation.completed += _ => continuation?.Invoke();

        public void GetResult() { }
    }
}

#endif