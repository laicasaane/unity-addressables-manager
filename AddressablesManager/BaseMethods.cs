using System;
using UnityEngine.SceneManagement;

namespace UnityEngine.AddressableAssets
{
    public static partial class AddressablesManager
    {
        public static void Initialize()
        {
            Clear();

            try
            {
                var operation = Addressables.InitializeAsync();
                operation.Completed += handle => OnInitializeCompleted(handle);
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);
            }
        }

        public static void LoadLocations(object key)
        {
            if (key == null)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw new InvalidKeyException(key);

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return;
            }

            try
            {
                var operation = Addressables.LoadResourceLocationsAsync(key);
                operation.Completed += handle => OnLoadLocationsCompleted(handle, key);
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);
            }
        }

        public static void LoadAsset<T>(string key) where T : Object
        {
            if (!GuardKey(key, out key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw new InvalidKeyException(key);

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return;
            }

            if (_assets.ContainsKey(key))
            {
                if (!(_assets[key] is T))
                {
                    if (!SuppressWarningLogs)
                        Debug.LogWarning(Exceptions.AssetKeyNotInstanceOf<T>(key));
                }

                return;
            }

            try
            {
                var operation = Addressables.LoadAssetAsync<T>(key);
                operation.Completed += handle => OnLoadAssetCompleted(handle, key, false);
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);
            }
        }

        public static void LoadAsset<T>(AssetReferenceT<T> reference) where T : Object
        {
            if (!GuardKey(reference, out var key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw Exceptions.InvalidReference;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return;
            }

            if (_assets.ContainsKey(key))
            {
                if (!(_assets[key] is T))
                {
                    if (!SuppressWarningLogs)
                        Debug.LogWarning(Exceptions.AssetReferenceNotInstanceOf<T>(key));
                }

                return;
            }

            try
            {
                var operation = reference.LoadAssetAsync<T>();
                operation.Completed += handle => OnLoadAssetCompleted(handle, key, true);
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);
            }
        }

        public static void LoadScene(string key,
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

                return;
            }

            if (_scenes.TryGetValue(key, out var scene))
            {
                if (activateOnLoad)
                {
                    var operation = scene.ActivateAsync();
                    operation.priority = priority;
                }

                return;
            }

            try
            {
                var operation = Addressables.LoadSceneAsync(key, loadMode, activateOnLoad, priority);
                operation.Completed += handle => OnLoadSceneCompleted(handle, key);
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

                return;
            }

            if (_scenes.TryGetValue(key, out var scene))
            {
                if (activateOnLoad)
                {
                    var operation = scene.ActivateAsync();
                    operation.priority = priority;
                }

                return;
            }

            try
            {
                var operation = reference.LoadSceneAsync(loadMode, activateOnLoad, priority);
                operation.Completed += handle => OnLoadSceneCompleted(handle, key);
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);
            }
        }


        public static void UnloadScene(string key, bool autoReleaseHandle = true)
        {
            if (!GuardKey(key, out key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw new InvalidKeyException(key);

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return;
            }

            if (!_scenes.TryGetValue(key, out var scene))
            {
                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.NoSceneKeyLoaded(key));

                return;
            }

            _scenes.Remove(key);

            try
            {
                Addressables.UnloadSceneAsync(scene, autoReleaseHandle);
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);
            }
        }

        public static void UnloadScene(AssetReference reference)
        {
            if (!GuardKey(reference, out var key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw Exceptions.InvalidReference;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return;
            }

            if (!_scenes.ContainsKey(key))
            {
                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.NoSceneReferenceLoaded(key));

                return;
            }

            _scenes.Remove(key);

            try
            {
                reference.UnLoadScene();
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

                return;
            }

            try
            {
                var operation = Addressables.InstantiateAsync(key, parent, inWorldSpace, trackHandle);
                operation.Completed += handle => OnInstantiateCompleted(handle, key, false);
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
                                       Transform parent = null,
                                       bool inWorldSpace = false)
        {
            if (!GuardKey(reference, out var key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw Exceptions.InvalidReference;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return;
            }

            try
            {
                var operation = reference.InstantiateAsync(parent, inWorldSpace);
                operation.Completed += handle => OnInstantiateCompleted(handle, key, true);
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