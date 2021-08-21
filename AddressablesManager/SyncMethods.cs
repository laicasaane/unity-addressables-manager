#if ADDRESSABLES_1_17

using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace UnityEngine.AddressableAssets
{
    using ResourceManagement.ResourceProviders;
    using AddressableAssets.ResourceLocators;

    public static partial class AddressablesManager
    {
        public static IResourceLocator InitializeSync()
        {
            Clear();

            try
            {
                var operation = Addressables.InitializeAsync();
                var result = operation.WaitForCompletion();
                OnInitializeCompleted(operation);

                return result;
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

        public static bool TryInitializeSync(out IResourceLocator result)
        {
            Clear();

            try
            {
                var operation = Addressables.InitializeAsync();
                result = operation.WaitForCompletion();
                OnInitializeCompleted(operation);

                return result != null;
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                result = default;
                return false;
            }
        }

        public static bool TryInitializeSync()
        {
            Clear();

            try
            {
                var operation = Addressables.InitializeAsync();
                operation.WaitForCompletion();
                OnInitializeCompleted(operation);

                return true;
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                return false;
            }
        }

        public static IList<IResourceLocation> LoadLocationsSync(object key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            try
            {
                var operation = Addressables.LoadResourceLocationsAsync(key);
                var result = operation.WaitForCompletion();
                OnLoadLocationsCompleted(operation, key);

                return result;
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

        public static bool TryLoadLocationsSync(object key, out IList<IResourceLocation> result)
        {
            if (key == null)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw new InvalidKeyException(key);

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                result = default;
                return false;
            }

            try
            {
                var operation = Addressables.LoadResourceLocationsAsync(key);
                result = operation.WaitForCompletion();
                OnLoadLocationsCompleted(operation, key);

                return result != null;
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                result = default;
                return false;
            }
        }

        public static T LoadAssetSync<T>(string key) where T : Object
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
                if (_assets[key] is T assetT)
                    return assetT;


                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.AssetKeyNotInstanceOf<T>(key));

                return default;
            }

            try
            {
                var operation = Addressables.LoadAssetAsync<T>(key);
                var result = operation.WaitForCompletion();
                OnLoadAssetCompleted(operation, key, false);

                return result;
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

        public static bool TryLoadAssetSync<T>(string key, out T result) where T : Object
        {
            if (!GuardKey(key, out key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw new InvalidKeyException(key);

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                result = default;
                return false;
            }

            if (_assets.ContainsKey(key))
            {
                if (_assets[key] is T assetT)
                {
                    result = assetT;
                    return true;
                }


                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.AssetKeyNotInstanceOf<T>(key));

                result = default;
                return false;
            }

            try
            {
                var operation = Addressables.LoadAssetAsync<T>(key);
                result = operation.WaitForCompletion();
                OnLoadAssetCompleted(operation, key, false);

                return result;
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                result = default;
                return false;
            }
        }

        public static T LoadAssetSync<T>(AssetReferenceT<T> reference) where T : Object
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
                if (_assets[key] is T assetT)
                    return assetT;


                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.AssetReferenceNotInstanceOf<T>(key));

                return default;
            }

            try
            {
                var operation = reference.LoadAssetAsync<T>();
                var result = operation.WaitForCompletion();
                OnLoadAssetCompleted(operation, key, true);

                return result;
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

        public static bool TryLoadAssetSync<T>(AssetReferenceT<T> reference, out T result) where T : Object
        {
            if (!GuardKey(reference, out var key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw Exceptions.InvalidReference;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                result = default;
                return false;
            }

            if (_assets.ContainsKey(key))
            {
                if (_assets[key] is T assetT)
                {
                    result = assetT;
                    return true;
                }

                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.AssetReferenceNotInstanceOf<T>(key));

                result = default;
                return false;
            }

            try
            {
                var operation = reference.LoadAssetAsync<T>();
                result = operation.WaitForCompletion();
                OnLoadAssetCompleted(operation, key, true);

                return result;
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                result = default;
                return false;
            }
        }

        private static void ActivateSceneSync(in SceneInstance instance, int priority)
        {
            var operation = instance.ActivateAsync();
            operation.priority = priority;
            operation.WaitForCompletion();
        }

        public static SceneInstance LoadSceneSync(string key,
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
                    ActivateSceneSync(scene, priority);

                return scene;
            }

            try
            {
                var operation = Addressables.LoadSceneAsync(key, loadMode, activateOnLoad, priority);
                var result = operation.WaitForCompletion();
                OnLoadSceneCompleted(operation, key);

                return result;
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

        public static bool TryLoadSceneSync(string key, out SceneInstance result,
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

                result = default;
                return false;
            }

            if (_scenes.TryGetValue(key, out var scene))
            {
                if (activateOnLoad)
                    ActivateSceneSync(scene, priority);

                result = scene;
                return true;
            }

            try
            {
                var operation = Addressables.LoadSceneAsync(key, loadMode, activateOnLoad, priority);
                result = operation.WaitForCompletion();
                OnLoadSceneCompleted(operation, key);

                return true;
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                result = default;
                return false;
            }
        }

        public static SceneInstance LoadSceneSync(AssetReference reference,
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
                    ActivateSceneSync(scene, priority);

                return scene;
            }

            try
            {
                var operation = reference.LoadSceneAsync(loadMode, activateOnLoad, priority);
                var result = operation.WaitForCompletion();
                OnLoadSceneCompleted(operation, key);

                return result;
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

        public static bool LoadSceneSync(AssetReference reference, out SceneInstance result,
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

                result = default;
                return false;
            }

            if (_scenes.TryGetValue(key, out var scene))
            {
                if (activateOnLoad)
                    ActivateSceneSync(scene, priority);

                result = scene;
                return true;
            }

            try
            {
                var operation = reference.LoadSceneAsync(loadMode, activateOnLoad, priority);
                result = operation.WaitForCompletion();
                OnLoadSceneCompleted(operation, key);

                return true;
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                result = default;
                return false;
            }
        }

        [Obsolete]
        public static bool TryLoadSceneSync(string key, LoadSceneMode loadMode, out SceneInstance result,
                                            bool activateOnLoad = true, int priority = 100)
        {
            if (!GuardKey(key, out key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw new InvalidKeyException(key);

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                result = default;
                return false;
            }

            if (_scenes.TryGetValue(key, out var scene))
            {
                if (activateOnLoad)
                    ActivateSceneSync(scene, priority);

                result = scene;
                return true;
            }

            try
            {
                var operation = Addressables.LoadSceneAsync(key, loadMode, activateOnLoad, priority);
                result = operation.WaitForCompletion();
                OnLoadSceneCompleted(operation, key);

                return true;
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                result = default;
                return false;
            }
        }

        [Obsolete]
        public static bool LoadSceneSync(AssetReference reference, LoadSceneMode loadMode, out SceneInstance result,
                                         bool activateOnLoad = true, int priority = 100)
        {
            if (!GuardKey(reference, out var key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw Exceptions.InvalidReference;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                result = default;
                return false;
            }

            if (_scenes.TryGetValue(key, out var scene))
            {
                if (activateOnLoad)
                    ActivateSceneSync(scene, priority);

                result = scene;
                return true;
            }

            try
            {
                var operation = reference.LoadSceneAsync(loadMode, activateOnLoad, priority);
                result = operation.WaitForCompletion();
                OnLoadSceneCompleted(operation, key);

                return true;
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                result = default;
                return false;
            }
        }

        public static SceneInstance UnloadSceneSync(string key, bool autoReleaseHandle = true)
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
                var result = operation.WaitForCompletion();
                return result;
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                return scene;
            }
        }

        public static bool TryUnloadSceneSync(string key, out SceneInstance result,
                                              bool autoReleaseHandle = true)
        {
            if (!GuardKey(key, out key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw new InvalidKeyException(key);

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                result = default;
                return false;
            }

            if (!_scenes.TryGetValue(key, out var scene))
            {
                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.NoSceneKeyLoaded(key));

                result = default;
                return false;
            }

            _scenes.Remove(key);

            try
            {
                var operation = Addressables.UnloadSceneAsync(scene, autoReleaseHandle);
                result = operation.WaitForCompletion();
                return true;
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                result = scene;
                return false;
            }
        }

        public static SceneInstance UnloadSceneSync(AssetReference reference)
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
                var result = operation.WaitForCompletion();
                return result;
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                return scene;
            }
        }

        public static bool TryUnloadSceneSync(AssetReference reference, out SceneInstance result)
        {
            if (!GuardKey(reference, out var key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw Exceptions.InvalidReference;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                result = default;
                return false;
            }

            if (!_scenes.TryGetValue(key, out var scene))
            {
                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.NoSceneReferenceLoaded(key));

                result = default;
                return false;
            }

            _scenes.Remove(key);

            try
            {
                var operation = reference.UnLoadScene();
                result = operation.WaitForCompletion();
                return true;
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                result = scene;
                return false;
            }
        }

        public static GameObject InstantiateSync(string key,
                                                 Transform parent = null,
                                                 bool inWorldSpace = false,
                                                 bool trackHandle = true)
        {
            if (!GuardKey(key, out key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw Exceptions.InvalidReference;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return default;
            }

            try
            {
                var operation = Addressables.InstantiateAsync(key, parent, inWorldSpace, trackHandle);
                var result = operation.WaitForCompletion();
                OnInstantiateCompleted(operation, key, false);

                return result;
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

        public static bool TryInstantiateSync(string key, out GameObject result,
                                              Transform parent = null,
                                              bool inWorldSpace = false,
                                              bool trackHandle = true)
        {
            if (!GuardKey(key, out key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw Exceptions.InvalidReference;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                result = default;
                return false;
            }

            try
            {
                var operation = Addressables.InstantiateAsync(key, parent, inWorldSpace, trackHandle);
                result = operation.WaitForCompletion();
                OnInstantiateCompleted(operation, key, false);

                return result;
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                result = default;
                return false;
            }
        }

        public static GameObject InstantiateSync(AssetReference reference,
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
                var result = operation.WaitForCompletion();
                OnInstantiateCompleted(operation, key, true);

                return result;
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

        public static bool TryInstantiateSync(AssetReference reference, out GameObject result,
                                              Transform parent = null,
                                              bool inWorldSpace = false)
        {
            if (!GuardKey(reference, out var key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw Exceptions.InvalidReference;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                result = default;
                return false;
            }

            try
            {
                var operation = reference.InstantiateAsync(parent, inWorldSpace);
                result = operation.WaitForCompletion();
                OnInstantiateCompleted(operation, key, true);

                return result;
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                result = default;
                return false;
            }
        }
    }

    internal static partial class AsyncOperationExtensions
    {
        public static void WaitForCompletion(this AsyncOperation operation)
        {
            new SyncOperationAwaiter(operation).WaitForCompletion();
        }
    }

    internal readonly struct SyncOperationAwaiter
    {
        private readonly AsyncOperation operation;

        public SyncOperationAwaiter(AsyncOperation operation)
        {
            this.operation = operation;
        }

        public bool IsCompleted
        {
            get
            {
                if (this.operation == null)
                    return true;

                return this.operation.isDone;
            }
        }

        public void WaitForCompletion()
        {
            while (!this.IsCompleted) { }
        }
    }
}

#endif