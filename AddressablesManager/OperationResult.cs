namespace UnityEngine.AddressableAssets
{
    using ResourceManagement.AsyncOperations;

    public readonly struct OperationResult<T>
    {
        public readonly bool Succeeded;
        public readonly T Value;

        public OperationResult(bool succeeded, T value)
        {
            this.Succeeded = succeeded;
            this.Value = value;
        }

        public OperationResult(AsyncOperationStatus status, T value)
        {
            this.Succeeded = status == AsyncOperationStatus.Succeeded;
            this.Value = value;
        }

        public static implicit operator OperationResult<T>(AsyncOperationHandle<T> handle)
            => new OperationResult<T>(handle.Status, handle.Result);

        public static implicit operator T(in OperationResult<T> result)
            => result.Value;
    }
}
