# Unity Addressables Manager

- The APIs provided by `AddressablesManager` are equivalent to that of `Addressables`, with 3 kinds of overloading: callback, coroutine, and async.
- Loaded assets, scenes, and instances will be cached for later uses.

# Changelog

## 1.3.1

- Correct all async APIs

## 1.3.0

- Fix exception: Attempting to use an invalid operation handle
- Add `onFailed` invocation at the end of `catch` blocks
- Improve `OperationResult<T>` struct and the way async APIs return the result
- BREAKING CHANGE: some constructors are removed from `OperationResult<T>` as deemed redundant

## 1.2.2

- `LoadScene` methods will now activate scene if `activateOnLoad` param is `true`
- **Breaking changes:** Correct the signature of `onSucceeded` callbacks on `LoadSceneCoroutine` and some `LoadScene` methods.
- **Note:** Regarding the behaviour of `activateOnLoad`, please read this documentation:
https://docs.unity3d.com/Packages/com.unity.addressables@1.16/manual/LoadSceneAsync.html


## 1.2.0

- Support synchronous APIs in Addressables 1.17
- Improve exceptions and logs handling
- Exceptions and logs handling behaviours can be changed via `AddressablesManager.ExceptionHandle`, `AddressablesManager.SuppressErrorLogs` and `AddressablesManager.SuppressWarningLogs` properties

## 1.1.0

- Use UniTask when it is included in the project
- Add InitializeAsync methods
- Breaking change: Rename AsyncResult<T> to OperationResult<T>