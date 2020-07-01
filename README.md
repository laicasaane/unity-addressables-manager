# Unity Addressables Manager

- The APIs provided by `AddressablesManager` are equivalent to that of `Addressables`, with 3 kinds of overloading: callback, coroutine, and async.
- Loaded assets, scenes, and instances will be cached for later uses.

# Changelog

## 1.1.0

- Use UniTask when it is included in the project
- Add InitializeAsync methods
- Breaking change: Rename AsyncResult<T> to OperationResult<T>