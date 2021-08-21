using System;
using System.Collections.Generic;

namespace UnityEngine.AddressableAssets
{
    using ResourceManagement.AsyncOperations;
    using ResourceManagement.ResourceProviders;
    using ResourceManagement.ResourceLocations;
    using ResourceLocators;

    public static partial class AddressablesManager
    {
        private static void OnInitializeCompleted(AsyncOperationHandle<IResourceLocator> handle,
                                                  Action onSucceeded = null,
                                                  Action onFailed = null)
        {
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                onFailed?.Invoke();
                return;
            }

            _keys.AddRange(handle.Result.Keys);
            onSucceeded?.Invoke();
        }

        private static void OnLoadLocationsCompleted(AsyncOperationHandle<IList<IResourceLocation>> handle,
                                                     object key,
                                                     Action<object> onSucceeded = null,
                                                     Action<object> onFailed = null)
        {
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                onFailed?.Invoke(key);
                return;
            }

            var succeeded = false;

            foreach (var location in handle.Result)
            {
                var primaryKey = location.PrimaryKey;

                if (!GuardKey(primaryKey, out primaryKey))
                    continue;

                if (!_locations.ContainsKey(primaryKey))
                    _locations.Add(primaryKey, new List<IResourceLocation>());

                var list = _locations[primaryKey];
                var index = list.FindIndex(x => string.Equals(x.InternalId, location.InternalId));

                if (index < 0)
                {
                    list.Add(location);
                    succeeded = true;
                }
            }

            if (succeeded)
                onSucceeded?.Invoke(key);
        }

        private static void OnLoadAssetCompleted<T>(AsyncOperationHandle<T> handle,
                                                    string key,
                                                    bool useReference,
                                                    Action<string, T> onSucceeded = null,
                                                    Action<string> onFailed = null)
            where T : Object
        {
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                onFailed?.Invoke(key);
                return;
            }

            if (!handle.Result)
            {
                if (!SuppressErrorLogs)
                    Debug.LogError(useReference ? Exceptions.CannotLoadAssetReference<T>(key) : Exceptions.CannotLoadAssetKey<T>(key));

                onFailed?.Invoke(key);
                return;
            }

            if (_assets.ContainsKey(key))
            {
                if (!(_assets[key] is T))
                {
                    if (!SuppressErrorLogs)
                    {
                        if (useReference)
                            Debug.LogError(Exceptions.AssetReferenceExist(_assets[key].GetType(), key));
                        else
                            Debug.LogError(Exceptions.AssetKeyExist(_assets[key].GetType(), key));
                    }

                    onFailed?.Invoke(key);
                    return;
                }
            }
            else
            {
                _assets.Add(key, handle.Result);
            }

            onSucceeded?.Invoke(key, handle.Result);
        }

        private static void OnInstantiateCompleted(AsyncOperationHandle<GameObject> handle,
                                                   string key,
                                                   bool useReference,
                                                   Action<string, GameObject> onSucceeded = null,
                                                   Action<string> onFailed = null)
        {
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                onFailed?.Invoke(key);
                return;
            }

            if (!handle.Result)
            {
                if (!SuppressErrorLogs)
                {
                    if (useReference)
                        Debug.LogError(Exceptions.CannotInstantiateReference(key));
                    else
                        Debug.LogError(Exceptions.CannotInstantiateKey(key));
                }

                onFailed?.Invoke(key);
                return;
            }

            if (!_instances.ContainsKey(key))
                _instances.Add(key, GetInstanceList());

            _instances[key].Add(handle.Result);
            onSucceeded?.Invoke(key, handle.Result);
        }

        private static void OnLoadSceneCompleted(AsyncOperationHandle<SceneInstance> handle,
                                                 string key,
                                                 Action<SceneInstance> onSucceeded = null,
                                                 Action<string> onFailed = null)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _scenes.Add(key, handle.Result);
                onSucceeded?.Invoke(handle.Result);
            }
            else if (handle.Status == AsyncOperationStatus.Failed)
            {
                onFailed?.Invoke(key);
            }
        }

        private static void OnUnloadSceneCompleted(AsyncOperationHandle<SceneInstance> handle,
                                                   string key,
                                                   Action<string> onSucceeded,
                                                   Action<string> onFailed)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                onSucceeded?.Invoke(key);
            }
            else if (handle.Status == AsyncOperationStatus.Failed)
            {
                onFailed?.Invoke(key);
            }
        }
    }
}