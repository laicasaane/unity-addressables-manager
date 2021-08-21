using System;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace UnityEngine.AddressableAssets
{
    public static partial class AddressablesManager
    {
        public static void Initialize(Action onSucceeded, Action onFailed = null)
        {
            Clear();

            try
            {
                var operation = Addressables.InitializeAsync();
                operation.Completed += handle => OnInitializeCompleted(handle, onSucceeded, onFailed);
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);
            }
        }

        public static void LoadLocations(object key,
                                         Action<object> onSucceeded,
                                         Action<object> onFailed = null)
        {
            if (key == null)
            {
                onFailed?.Invoke(key);
                return;
            }

            try
            {
                var operation = Addressables.LoadResourceLocationsAsync(key);
                operation.Completed += handle => OnLoadLocationsCompleted(handle, key, onSucceeded, onFailed);
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);
            }
        }

        public static void LoadAsset<T>(string key,
                                        Action<string, T> onSucceeded,
                                        Action<string> onFailed = null)
            where T : Object
        {
            if (!GuardKey(key, out key))
            {
                onFailed?.Invoke(key);
                return;
            }

            if (_assets.ContainsKey(key))
            {
                if (_assets[key] is T asset)
                {
                    onSucceeded?.Invoke(key, asset);
                    return;
                }

                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.AssetKeyNotInstanceOf<T>(key));

                onFailed?.Invoke(key);
                return;
            }

            try
            {
                var operation = Addressables.LoadAssetAsync<T>(key);
                operation.Completed += handle => OnLoadAssetCompleted(handle, key, false, onSucceeded, onFailed);
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);
            }
        }

        public static void LoadAsset<T>(AssetReferenceT<T> reference,
                                        Action<string, T> onSucceeded,
                                        Action<string> onFailed = null)
            where T : Object
        {
            if (!GuardKey(reference, out var key))
            {
                onFailed?.Invoke(key);
                return;
            }

            if (_assets.ContainsKey(key))
            {
                if (_assets[key] is T asset)
                {
                    onSucceeded?.Invoke(key, asset);
                    return;
                }


                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.AssetReferenceNotInstanceOf<T>(key));

                onFailed?.Invoke(key);
                return;
            }

            try
            {
                var operation = reference.LoadAssetAsync<T>();
                operation.Completed += handle => OnLoadAssetCompleted(handle, key, true, onSucceeded, onFailed);
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);
            }
        }

        private static void ActivateScene(SceneInstance scene, int priority, Action<SceneInstance> onSucceeded)
        {
            var operation = scene.ActivateAsync();
            operation.priority = priority;
            operation.completed += _ => onSucceeded?.Invoke(scene);
        }

        public static void LoadScene(string key,
                                     Action<SceneInstance> onSucceeded,
                                     Action<string> onFailed = null,
                                     LoadSceneMode loadMode = LoadSceneMode.Single,
                                     bool activateOnLoad = true,
                                     int priority = 100)
        {
            if (!GuardKey(key, out key))
            {
                onFailed?.Invoke(key);
                return;
            }

            if (_scenes.TryGetValue(key, out var scene))
            {
                if (activateOnLoad)
                    ActivateScene(scene, priority, onSucceeded);
                else
                    onSucceeded?.Invoke(scene);

                return;
            }

            try
            {
                var operation = Addressables.LoadSceneAsync(key, loadMode, activateOnLoad, priority);
                operation.Completed += handle => OnLoadSceneCompleted(handle, key, onSucceeded, onFailed);
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);
            }
        }

        public static void LoadScene(AssetReference reference,
                                     Action<SceneInstance> onSucceeded,
                                     Action<string> onFailed = null,
                                     LoadSceneMode loadMode = LoadSceneMode.Single,
                                     bool activateOnLoad = true,
                                     int priority = 100)
        {
            if (!GuardKey(reference, out var key))
            {
                onFailed?.Invoke(key);
                return;
            }

            if (_scenes.TryGetValue(key, out var scene))
            {
                if (activateOnLoad)
                    ActivateScene(scene, priority, onSucceeded);
                else
                    onSucceeded?.Invoke(scene);

                return;
            }

            try
            {
                var operation = reference.LoadSceneAsync(loadMode, activateOnLoad, priority);
                operation.Completed += handle => OnLoadSceneCompleted(handle, key, onSucceeded, onFailed);
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);
            }
        }

        public static void UnloadScene(string key,
                                       Action<string> onSucceeded = null,
                                       Action<string> onFailed = null,
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

            try
            {
                var operation = Addressables.UnloadSceneAsync(scene, autoReleaseHandle);
                operation.Completed += handle => OnUnloadSceneCompleted(handle, key, onSucceeded, onFailed);
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);
            }
        }

        public static void UnloadScene(AssetReference reference,
                                       Action<string> onSucceeded = null,
                                       Action<string> onFailed = null)
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

            try
            {
                var operation = reference.UnLoadScene();
                operation.Completed += handle => OnUnloadSceneCompleted(handle, key, onSucceeded, onFailed);
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);
            }
        }

        public static void Instantiate(string key,
                                       Action<string, GameObject> onSucceeded,
                                       Action<string> onFailed = null,
                                       Transform parent = null,
                                       bool inWorldSpace = false,
                                       bool trackHandle = true)
        {
            if (!GuardKey(key, out key))
            {
                onFailed?.Invoke(key);
                return;
            }

            try
            {
                var operation = Addressables.InstantiateAsync(key, parent, inWorldSpace, trackHandle);
                operation.Completed += handle => OnInstantiateCompleted(handle, key, false, onSucceeded, onFailed);
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);
            }
        }

        public static void Instantiate(AssetReference reference,
                                       Action<string, GameObject> onSucceeded,
                                       Action<string> onFailed = null,
                                       Transform parent = null,
                                       bool inWorldSpace = false)
        {
            if (!GuardKey(reference, out var key))
            {
                onFailed?.Invoke(key);
                return;
            }

            try
            {
                var operation = reference.InstantiateAsync(parent, inWorldSpace);
                operation.Completed += handle => OnInstantiateCompleted(handle, key, true, onSucceeded, onFailed);
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);
            }
        }
    }
}