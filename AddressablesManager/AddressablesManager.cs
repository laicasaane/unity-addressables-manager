using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace UnityEngine.AddressableAssets
{
    using ResourceManagement.ResourceProviders;
    using ResourceManagement.ResourceLocations;

    public static partial class AddressablesManager
    {
        private static readonly Dictionary<string, List<IResourceLocation>> _locations;
        private static readonly List<IResourceLocation> _noLocation;
        private static readonly Dictionary<string, Object> _assets;
        private static readonly Dictionary<string, SceneInstance> _scenes;
        private static readonly Dictionary<string, List<GameObject>> _instances;
        private static readonly Queue<List<GameObject>> _instanceListPool;
        private static readonly List<GameObject> _noInstanceList;
        private static readonly List<object> _keys;
        private static readonly string[] _filters;

        public static IReadOnlyList<object> Keys
            => _keys;

        static AddressablesManager()
        {
            _locations = new Dictionary<string, List<IResourceLocation>>();
            _noLocation = new List<IResourceLocation>(0);
            _assets = new Dictionary<string, Object>();
            _scenes = new Dictionary<string, SceneInstance>();
            _instances = new Dictionary<string, List<GameObject>>();
            _instanceListPool = new Queue<List<GameObject>>();
            _noInstanceList = new List<GameObject>(0);
            _keys = new List<object>();
            _filters = new[] { "\n", "\r" };
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

            for (var i = 0; i < _filters.Length; i++)
            {
                result = result.Replace(_filters[i], string.Empty);
            }

            return !string.IsNullOrEmpty(result);
        }

        private static bool GuardKey(AssetReference reference, out string result)
        {
            if (reference == null)
            {
                Debug.LogException(new System.ArgumentNullException(nameof(reference)));
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

        public static void Initialize()
        {
            Clear();

            var operation = Addressables.InitializeAsync();
            operation.Completed += handle => OnInitializeCompleted(handle);
        }

        public static void LoadLocations(object key)
        {
            if (key == null)
                return;

            var operation = Addressables.LoadResourceLocationsAsync(key);
            operation.Completed += handle => OnLoadLocationsCompleted(handle, key);
        }

        public static void LoadAsset<T>(string key) where T : Object
        {
            if (!GuardKey(key, out key))
                return;

            if (!_assets.ContainsKey(key))
            {
                var operation = Addressables.LoadAssetAsync<T>(key);
                operation.Completed += handle => OnLoadAssetCompleted(handle, key);
                return;
            }

            if (!(_assets[key] is T))
            {
                Debug.LogWarning($"The asset with key={key} is not an instance of {typeof(T)}.");
            }
        }

        public static void LoadAsset<T>(AssetReferenceT<T> reference) where T : Object
        {
            if (!GuardKey(reference, out var key))
                return;

            if (!_assets.ContainsKey(key))
            {
                var operation = reference.LoadAssetAsync<T>();
                operation.Completed += handle => OnLoadAssetCompleted(handle, key);
                return;
            }

            if (!(_assets[key] is T))
            {
                Debug.LogWarning($"The asset with key={key} is not an instance of {typeof(T)}.");
            }
        }

        public static void LoadScene(string key, LoadSceneMode loadMode, bool activeOnLoad = true, int priority = 100)
        {
            if (!GuardKey(key, out key))
                return;

            if (_scenes.ContainsKey(key))
                return;

            var operation = Addressables.LoadSceneAsync(key, loadMode, activeOnLoad, priority);
            operation.Completed += handle => OnLoadSceneCompleted(handle, key);
        }

        public static void LoadScene(AssetReference reference, LoadSceneMode loadMode, bool activeOnLoad = true,
            int priority = 100)
        {
            if (!GuardKey(reference, out var key))
                return;

            if (_scenes.ContainsKey(key))
                return;

            var operation = reference.LoadSceneAsync(loadMode, activeOnLoad, priority);
            operation.Completed += handle => OnLoadSceneCompleted(handle, key);
        }

        public static bool TryGetScene(string key, out SceneInstance scene)
        {
            scene = default;

            if (!GuardKey(key, out key))
                return false;

            if (_scenes.TryGetValue(key, out var value))
            {
                scene = value;
                return true;
            }

            return false;
        }

        public static bool TryGetScene(AssetReference reference, out SceneInstance scene)
        {
            scene = default;

            if (!GuardKey(reference, out var key))
                return false;

            if (_scenes.TryGetValue(key, out var value))
            {
                scene = value;
                return true;
            }

            return false;
        }

        public static void UnloadScene(string key, bool autoReleaseHandle = true)
        {
            if (!GuardKey(key, out key))
                return;

            if (!_scenes.TryGetValue(key, out var scene))
                return;

            _scenes.Remove(key);
            Addressables.UnloadSceneAsync(scene, autoReleaseHandle);
        }

        public static void UnloadScene(AssetReference reference)
        {
            if (!GuardKey(reference, out var key))
                return;

            if (!_scenes.ContainsKey(key))
                return;

            _scenes.Remove(key);
            reference.UnLoadScene();
        }

        public static void Instantiate(string key, Transform parent = null, bool inWorldSpace = false, bool trackHandle = true)
        {
            if (!GuardKey(key, out key))
                return;

            var operation = Addressables.InstantiateAsync(key, parent, inWorldSpace, trackHandle);
            operation.Completed += handle => OnInstantiateCompleted(handle, key);
        }

        public static void Instantiate(AssetReference reference, Transform parent = null, bool inWorldSpace = false)
        {
            if (!GuardKey(reference, out var key))
                return;

            var operation = reference.InstantiateAsync(parent, inWorldSpace);
            operation.Completed += handle => OnInstantiateCompleted(handle, key);
        }

        public static IReadOnlyList<IResourceLocation> GetLocations(string key)
        {
            if (!GuardKey(key, out key))
                return _noLocation;

            if (!_locations.TryGetValue(key, out var list))
                return _noLocation;

            return list;
        }

        public static T GetAsset<T>(string key) where T : Object
        {
            if (!GuardKey(key, out key))
                return default;

            return GetAssetInternal<T>(key);
        }

        public static T GetAsset<T>(AssetReference reference) where T : Object
        {
            if (!GuardKey(reference, out var key))
                return default;

            return GetAssetInternal<T>(key);
        }

        private static T GetAssetInternal<T>(string key) where T : Object
        {
            if (!_assets.ContainsKey(key))
            {
                Debug.LogWarning($"Cannot find any asset by key={key}.");
                return default;
            }

            if (_assets[key] is T asset)
                return asset;

            Debug.LogWarning($"The asset with key={key} is not an instance of {typeof(T)}.");
            return default;
        }

        public static bool TryGetAsset<T>(string key, out T asset) where T : Object
        {
            asset = default;

            if (!GuardKey(key, out key))
                return false;

            return TryGetAssetInternal<T>(key, out asset);
        }

        public static bool TryGetAsset<T>(AssetReference reference, out T asset) where T : Object
        {
            asset = default;

            if (!GuardKey(reference, out var key))
                return false;

            return TryGetAssetInternal<T>(key, out asset);
        }

        private static bool TryGetAssetInternal<T>(string key, out T asset) where T : Object
        {
            asset = default;

            if (!_assets.ContainsKey(key))
            {
                Debug.LogWarning($"Cannot find any asset by key={key}.");
                return false;
            }

            if (_assets[key] is T assetT)
            {
                asset = assetT;
                return true;
            }

            Debug.LogWarning($"The asset with key={key} is not an instance of {typeof(T)}.");
            return false;
        }

        public static void ReleaseAsset(string key)
        {
            if (!GuardKey(key, out key))
                return;

            if (!_assets.TryGetValue(key, out var asset))
                return;

            _assets.Remove(key);
            Addressables.Release(asset);
        }

        public static void ReleaseAsset(AssetReference reference)
        {
            if (!GuardKey(reference, out var key))
                return;

            if (!_assets.ContainsKey(key))
                return;

            _assets.Remove(key);
            reference.ReleaseAsset();
        }

        public static IReadOnlyList<GameObject> GetInstances(string key)
        {
            if (!GuardKey(key, out key))
                return _noInstanceList;

            if (_instances.TryGetValue(key, out var instanceList))
                return instanceList;

            return _noInstanceList;
        }

        public static IReadOnlyList<GameObject> GetInstances(AssetReference reference)
        {
            if (!GuardKey(reference, out var key))
                return _noInstanceList;

            if (_instances.TryGetValue(key, out var instanceList))
                return instanceList;

            return _noInstanceList;
        }

        public static void ReleaseInstances(string key)
        {
            if (!GuardKey(key, out key))
                return;

            ReleaseInstanceInternal(key);
        }

        public static void ReleaseInstances(AssetReference reference)
        {
            if (!GuardKey(reference, out var key))
                return;

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
                return;

            ReleaseInstanceInternal(key, instance);
        }

        public static void ReleaseInstance(AssetReference reference, GameObject instance)
        {
            if (!GuardKey(reference, out var key))
                return;

            ReleaseInstanceInternal(key, instance);
        }

        private static void ReleaseInstanceInternal(string key, GameObject instance)
        {
            if (!instance)
                return;

            if (!_instances.TryGetValue(key, out var instanceList))
                return;

            var index = instanceList.FindIndex(x => x.GetInstanceID() == instance.GetInstanceID());

            if (index < 0)
            {
                Debug.LogWarning($"The instance was not instantiated through {nameof(AddressablesManager)} therefore it cannot be unloaded by this method.", instance);
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