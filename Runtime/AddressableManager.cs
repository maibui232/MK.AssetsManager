namespace MK.AssetsManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using MK.Log;
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;
    using UnityEngine.ResourceManagement.ResourceProviders;
    using UnityEngine.SceneManagement;

    public class AddressableManager : IAssetsManager
    {
        private readonly struct AsyncOperationHandleSceneInstance
        {
            internal AsyncOperationHandle<SceneInstance> OperationHandle   { get; }
            internal bool                                ReleaseWhenUnload { get; }

            public AsyncOperationHandleSceneInstance(AsyncOperationHandle<SceneInstance> operationHandle, bool releaseWhenUnload)
            {
                this.OperationHandle   = operationHandle;
                this.ReleaseWhenUnload = releaseWhenUnload;
            }

            internal void Unload()
            {
                Addressables.UnloadSceneAsync(this.OperationHandle, this.ReleaseWhenUnload);
            }
        }

        private readonly Dictionary<string, AsyncOperationHandle>              keyToLoadAssetHandle  = new();
        private readonly Dictionary<string, AsyncOperationHandleSceneInstance> nameToLoadSceneHandle = new();

        private readonly ILogger logger;

        public AddressableManager(ILoggerManager loggerManager)
        {
            this.logger = loggerManager.GetDefaultLogger();
        }

        T IAssetsManager.Load<T>(string key)
        {
            return ((IAssetsManager)this).LoadAsync<T>(key).GetAwaiter().GetResult();
        }

        async UniTask<T> IAssetsManager.LoadAsync<T>(string key, IProgress<float> progress, CancellationToken cancellationToken, bool cancelImmediately, bool autoReleaseWhenCanceled)
        {
            if (this.keyToLoadAssetHandle.TryGetValue(key, out var handle))
            {
                return (T)handle.Result;
            }

            var operationHandle = Addressables.LoadAssetAsync<T>(key);
            this.keyToLoadAssetHandle.Add(key, operationHandle);

            return await operationHandle.ToUniTask(progress, PlayerLoopTiming.Update, cancellationToken, cancelImmediately, autoReleaseWhenCanceled);
        }

        void IAssetsManager.ReleaseAsset(string key)
        {
            if (!this.keyToLoadAssetHandle.Remove(key, out var handle))
            {
                this.logger.Fatal(new Exception($"Already release asset with key: {key}"));

                return;
            }

            handle.Release();
        }

        void IAssetsManager.ReleaseAsset<T>(T asset)
        {
            var handle = this.keyToLoadAssetHandle.Values.FirstOrDefault(loadAssetHandle => loadAssetHandle.Result.Equals(asset));
            if (handle.Equals(default))
            {
                this.logger.Fatal($"Couldn't find {nameof(AsyncOperationHandle)} for asset: {asset.name}");

                return;
            }

            handle.Release();
        }

        SceneInstance IAssetsManager.LoadScene(string sceneName, bool loadAdditive, bool activeOnLoad, bool releaseWhenUnload)
        {
            return ((IAssetsManager)this).LoadSceneAsync(sceneName, loadAdditive, activeOnLoad, releaseWhenUnload).GetAwaiter().GetResult();
        }

        async UniTask<SceneInstance> IAssetsManager.LoadSceneAsync(string sceneName, bool loadAdditive, bool activeOnLoad, bool releaseWhenUnload, IProgress<float> progress, CancellationToken cancellationToken, bool cancelImmediately, bool autoReleaseWhenCanceled)
        {
            if (this.nameToLoadSceneHandle.TryGetValue(sceneName, out var handle))
            {
                return handle.OperationHandle.Result;
            }

            var loadMode                 = loadAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single;
            var releaseMode              = releaseWhenUnload ? SceneReleaseMode.ReleaseSceneWhenSceneUnloaded : SceneReleaseMode.OnlyReleaseSceneOnHandleRelease;
            var loadSceneOperationHandle = Addressables.LoadSceneAsync(sceneName, loadMode, activeOnLoad, 100, releaseMode);

            this.nameToLoadSceneHandle.Add(sceneName, new AsyncOperationHandleSceneInstance(loadSceneOperationHandle, releaseWhenUnload));

            return await loadSceneOperationHandle.ToUniTask(progress, PlayerLoopTiming.Update, cancellationToken, cancelImmediately, autoReleaseWhenCanceled);
        }

        void IAssetsManager.UnloadScene(string sceneName)
        {
            if (!this.nameToLoadSceneHandle.TryGetValue(sceneName, out var handle))
            {
                this.logger.Fatal(new Exception($"Not found scene with name :{sceneName} to unload!"));

                return;
            }

            handle.Unload();
            if (handle.ReleaseWhenUnload)
            {
                this.nameToLoadSceneHandle.Remove(sceneName);
            }
        }
    }
}