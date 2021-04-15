using System;
using System.Collections.Generic;

namespace UnityEngine.AddressableAssets
{
    using ResourceManagement.ResourceProviders;
    using ResourceManagement.ResourceLocations;

    public static partial class AddressablesManager
    {
        public enum ExceptionHandleType
        {
            Log, Throw, Suppress
        }

        private static readonly Dictionary<string, List<IResourceLocation>> _locations;
        private static readonly List<IResourceLocation> _noLocation;
        private static readonly Dictionary<string, Object> _assets;
        private static readonly Dictionary<string, SceneInstance> _scenes;
        private static readonly Dictionary<string, List<GameObject>> _instances;
        private static readonly Queue<List<GameObject>> _instanceListPool;
        private static readonly List<GameObject> _noInstanceList;
        private static readonly List<object> _keys;

        public static IReadOnlyList<object> Keys => _keys;

        public static ExceptionHandleType ExceptionHandle { get; set; }

        public static bool SuppressWarningLogs { get; set; }

        public static bool SuppressErrorLogs { get; set; }

        static AddressablesManager()
        {
            ExceptionHandle = ExceptionHandleType.Log;

            _locations = new Dictionary<string, List<IResourceLocation>>();
            _noLocation = new List<IResourceLocation>(0);
            _assets = new Dictionary<string, Object>();
            _scenes = new Dictionary<string, SceneInstance>();
            _instances = new Dictionary<string, List<GameObject>>();
            _instanceListPool = new Queue<List<GameObject>>();
            _noInstanceList = new List<GameObject>(0);
            _keys = new List<object>();
        }

        private static void Clear()
        {
            _keys.Clear();
            _locations.Clear();
            _assets.Clear();
            _scenes.Clear();
        }

        private static List<GameObject> GetInstanceList()
        {
            if (_instanceListPool.Count > 0)
                return _instanceListPool.Dequeue();

            return new List<GameObject>();
        }

        private static void PoolInstanceList(List<GameObject> list)
        {
            list.Clear();
            _instanceListPool.Enqueue(list);
        }

        private static bool GuardKey(string key, out string result)
        {
            result = key ?? string.Empty;

            return !string.IsNullOrEmpty(result);
        }

        private static bool GuardKey(AssetReference reference, out string result)
        {
            if (reference == null)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw new ArgumentNullException(nameof(reference));

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new ArgumentNullException(nameof(reference)));

                result = string.Empty;
            }
            else
            {
                result = reference.RuntimeKey.ToString();
            }

            return !string.IsNullOrEmpty(result);
        }

        public static bool ContainsAsset(string key)
            => _assets.ContainsKey(key) && _assets[key];

        public static bool ContainsKey(object key)
            => _keys.Contains(key);

        public static bool TryGetScene(string key, out SceneInstance scene)
        {
            scene = default;

            if (!GuardKey(key, out key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw new InvalidKeyException(key);

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return false;
            }

            if (_scenes.TryGetValue(key, out var value))
            {
                scene = value;
                return true;
            }

            if (!SuppressWarningLogs)
                Debug.LogWarning($"No scene with key={key} has been loaded through {nameof(AddressablesManager)}.");

            return false;
        }

        public static bool TryGetScene(AssetReference reference, out SceneInstance scene)
        {
            scene = default;

            if (!GuardKey(reference, out var key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw Exceptions.InvalidReference;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return false;
            }

            if (_scenes.TryGetValue(key, out var value))
            {
                scene = value;
                return true;
            }

            return false;
        }

        public static IReadOnlyList<IResourceLocation> GetLocations(string key)
        {
            if (!GuardKey(key, out key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw new InvalidKeyException(key);

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return _noLocation;
            }

            if (!_locations.TryGetValue(key, out var list))
                return _noLocation;

            return list;
        }

        public static T GetAsset<T>(string key) where T : Object
        {
            if (!GuardKey(key, out key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw new InvalidKeyException(key);

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return default;
            }

            return GetAssetInternal<T>(key);
        }

        public static T GetAsset<T>(AssetReference reference) where T : Object
        {
            if (!GuardKey(reference, out var key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw Exceptions.InvalidReference;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return default;
            }

            return GetAssetInternal<T>(key);
        }

        private static T GetAssetInternal<T>(string key) where T : Object
        {
            if (!_assets.ContainsKey(key))
            {
                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.CannotFindAssetByKey(key));

                return default;
            }

            if (_assets[key] is T asset)
                return asset;

            if (!SuppressWarningLogs)
                Debug.LogWarning(Exceptions.AssetKeyNotInstanceOf<T>(key));

            return default;
        }

        public static bool TryGetAsset<T>(string key, out T asset) where T : Object
        {
            asset = default;

            if (!GuardKey(key, out key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw new InvalidKeyException(key);

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return false;
            }

            return TryGetAssetInternal<T>(key, out asset);
        }

        public static bool TryGetAsset<T>(AssetReference reference, out T asset) where T : Object
        {
            asset = default;

            if (!GuardKey(reference, out var key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw Exceptions.InvalidReference;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return false;
            }

            return TryGetAssetInternal<T>(key, out asset);
        }

        private static bool TryGetAssetInternal<T>(string key, out T asset) where T : Object
        {
            asset = default;

            if (!_assets.ContainsKey(key))
            {
                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.CannotFindAssetByKey(key));

                return false;
            }

            if (_assets[key] is T assetT)
            {
                asset = assetT;
                return true;
            }

            if (!SuppressWarningLogs)
                Debug.LogWarning(Exceptions.AssetKeyNotInstanceOf<T>(key));

            return false;
        }

        public static void ReleaseAsset(string key)
        {
            if (!GuardKey(key, out key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw new InvalidKeyException(key);

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return;
            }

            if (!_assets.TryGetValue(key, out var asset))
                return;

            _assets.Remove(key);
            Addressables.Release(asset);
        }

        public static void ReleaseAsset(AssetReference reference)
        {
            if (!GuardKey(reference, out var key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw Exceptions.InvalidReference;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return;
            }

            if (!_assets.ContainsKey(key))
                return;

            _assets.Remove(key);
            reference.ReleaseAsset();
        }

        public static IReadOnlyList<GameObject> GetInstances(string key)
        {
            if (!GuardKey(key, out key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw new InvalidKeyException(key);

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return _noInstanceList;
            }

            if (_instances.TryGetValue(key, out var instanceList))
                return instanceList;

            return _noInstanceList;
        }

        public static IReadOnlyList<GameObject> GetInstances(AssetReference reference)
        {
            if (!GuardKey(reference, out var key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw Exceptions.InvalidReference;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return _noInstanceList;
            }

            if (_instances.TryGetValue(key, out var instanceList))
                return instanceList;

            return _noInstanceList;
        }

        public static void ReleaseInstances(string key)
        {
            if (!GuardKey(key, out key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw new InvalidKeyException(key);

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return;
            }

            ReleaseInstanceInternal(key);
        }

        public static void ReleaseInstances(AssetReference reference)
        {
            if (!GuardKey(reference, out var key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw Exceptions.InvalidReference;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return;
            }

            ReleaseInstanceInternal(key);
        }

        private static void ReleaseInstanceInternal(string key)
        {
            if (!_instances.TryGetValue(key, out var instanceList))
                return;

            _instances.Remove(key);

            foreach (var instance in instanceList)
            {
                Addressables.ReleaseInstance(instance);
            }

            PoolInstanceList(instanceList);
        }

        public static void ReleaseInstance(string key, GameObject instance)
        {
            if (!GuardKey(key, out key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw new InvalidKeyException(key);

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return;
            }

            ReleaseInstanceInternal(key, instance);
        }

        public static void ReleaseInstance(AssetReference reference, GameObject instance)
        {
            if (!GuardKey(reference, out var key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw Exceptions.InvalidReference;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return;
            }

            ReleaseInstanceInternal(key, instance);
        }

        private static void ReleaseInstanceInternal(string key, GameObject instance)
        {
            if (!instance)
                return;

            if (!_instances.TryGetValue(key, out var instanceList))
            {
                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.NoInstanceKeyInitialized(key), instance);

                return;
            }

            var index = instanceList.FindIndex(x => x.GetInstanceID() == instance.GetInstanceID());

            if (index < 0)
            {
                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.NoInstanceKeyInitialized(key), instance);

                return;
            }

            instanceList.RemoveAt(index);
            Addressables.ReleaseInstance(instance);

            if (instanceList.Count > 0)
                return;

            _instances.Remove(key);
            PoolInstanceList(instanceList);
        }
    }
}