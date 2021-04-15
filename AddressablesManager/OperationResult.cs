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

        public OperationResult(bool succeeded, in T value)
        {
            this.Succeeded = succeeded;
            this.Value = value;
        }

        public OperationResult(AsyncOperationStatus status, T value)
        {
            this.Succeeded = status == AsyncOperationStatus.Succeeded;
            this.Value = value;
        }

        public OperationResult(AsyncOperationStatus status, in T value)
        {
            this.Succeeded = status == AsyncOperationStatus.Succeeded;
            this.Value = value;
        }

        public void Deconstruct(out bool succeeded, out T value)
        {
            succeeded = this.Succeeded;
            value = this.Value;
        }

        public static implicit operator OperationResult<T>(in AsyncOperationHandle<T> handle)
            => new OperationResult<T>(handle.Status, handle.Result);

        public static implicit operator T(in OperationResult<T> result)
            => result.Value;
    }
}
