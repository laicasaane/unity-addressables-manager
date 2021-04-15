using System;

namespace UnityEngine.AddressableAssets
{
    internal static class Exceptions
    {
        private const string _cannotFindAssetByKey = "Cannot find any asset by key={0}.";
        private const string _noInstanceKeyInitialized = "No instance with key={0} has been instantiated through AddressablesManager.";
        private const string _noInstanceReferenceInitialized = "No instance with key={0} has been instantiated through AddressablesManager.";
        private const string _assetKeyNotInstanceOf = "The asset with key={0} is not an instance of {1}.";
        private const string _assetReferenceNotInstanceOf = "The asset with reference={0} is not an instance of {1}.";
        private const string _noSceneKeyLoaded = "No scene with key={0} has been loaded through AddressablesManager.";
        private const string _noSceneReferenceLoaded = "No scene with reference={0} has been loaded through AddressablesManager.";
        private const string _cannotLoadAssetKey = "Cannot load any asset of type={0} by key={1}.";
        private const string _cannotLoadAssetReference = "Cannot load any asset of type={0} by reference={1}.";
        private const string _assetKeyExist = "An asset of type={0} has been already registered with key={1}.";
        private const string _assetReferenceExist = "An asset of type={0} has been already registered with reference={1}.";
        private const string _cannotInstantiateKey = "Cannot instantiate key={0}.";
        private const string _cannotInstantiateReference = "Cannot instantiate reference={0}.";

        public static readonly InvalidKeyException InvalidReference = new InvalidKeyException("Reference is invalid.");

        public static string CannotFindAssetByKey(string key)
            => string.Format(_cannotFindAssetByKey, key);

        public static string NoInstanceKeyInitialized(string key)
            => string.Format(_noInstanceKeyInitialized, key);

        public static string NoInstanceReferenceInitialized(string key)
            => string.Format(_noInstanceReferenceInitialized, key);

        public static string AssetKeyNotInstanceOf<T>(string key)
            => string.Format(_assetKeyNotInstanceOf, key, typeof(T));

        public static string AssetReferenceNotInstanceOf<T>(string key)
            => string.Format(_assetReferenceNotInstanceOf, key, typeof(T));

        public static string NoSceneKeyLoaded(string key)
            => string.Format(_noSceneKeyLoaded, key);

        public static string NoSceneReferenceLoaded(string key)
            => string.Format(_noSceneReferenceLoaded, key);

        public static string CannotLoadAssetKey<T>(string key)
            => string.Format(_cannotLoadAssetKey, typeof(T), key);

        public static string CannotLoadAssetReference<T>(string key)
            => string.Format(_cannotLoadAssetReference, typeof(T), key);

        public static string AssetKeyExist(Type type, string key)
            => string.Format(_assetKeyExist, type, key);

        public static string AssetReferenceExist(Type type, string key)
            => string.Format(_assetReferenceExist, type, key);

        public static string CannotInstantiateKey(string key)
            => string.Format(_cannotInstantiateKey, key);

        public static string CannotInstantiateReference(string key)
            => string.Format(_cannotInstantiateReference, key);
    }
}