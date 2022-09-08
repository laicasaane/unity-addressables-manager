namespace UnityEngine.AddressableAssets
{
    using ResourceManagement.AsyncOperations;

    public readonly struct OperationResult<T>
    {
        public readonly bool Succeeded;
        public readonly object Key;
        public readonly T Value;

        public OperationResult(bool succeeded, object key, T value)
        {
            this.Succeeded = succeeded;
            this.Key = key;
            this.Value = value;
        }

        public OperationResult(bool succeeded, object key, in T value)
        {
            this.Succeeded = succeeded;
            this.Key = key;
            this.Value = value;
        }

        public OperationResult(in AsyncOperationHandle<T> handle) : this()
        {
            this.Succeeded = handle.Status == AsyncOperationStatus.Succeeded;
            this.Value = handle.Result;
        }

        public OperationResult(object key, in AsyncOperationHandle<T> handle)
        {
            this.Succeeded = handle.Status == AsyncOperationStatus.Succeeded;
            this.Key = key;
            this.Value = handle.Result;
        }

        public void Deconstruct(out bool succeeded, out T value)
        {
            succeeded = this.Succeeded;
            value = this.Value;
        }

        public void Deconstruct(out bool succeeded, out object key, out T value)
        {
            succeeded = this.Succeeded;
            key = this.Key;
            value = this.Value;
        }

        public static implicit operator T(in OperationResult<T> result)
            => result.Value;
    }
}
