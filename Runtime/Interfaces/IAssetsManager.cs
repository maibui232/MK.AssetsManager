namespace MK.AssetsManager
{
    using System;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using UnityEngine.ResourceManagement.ResourceProviders;
    using Object = UnityEngine.Object;

    public interface IAssetsManager
    {
        T Load<T>(string key) where T : Object;

        UniTask<T> LoadAsync<T>(string key, IProgress<float> progress = null, CancellationToken cancellationToken = default, bool cancelImmediately = false, bool autoReleaseWhenCanceled = false) where T : Object;

        void ReleaseAsset(string key);

        void ReleaseAsset<T>(T asset) where T : Object;

        SceneInstance LoadScene(string sceneName, bool loadAdditive = false, bool activeOnLoad = false, bool releaseWhenUnload = true);

        UniTask<SceneInstance> LoadSceneAsync(string sceneName, bool loadAdditive = false, bool activeOnLoad = false, bool releaseWhenUnload = true, IProgress<float> progress = null, CancellationToken cancellationToken = default, bool cancelImmediately = false, bool autoReleaseWhenCanceled = false);

        void UnloadScene(string sceneName);
    }
}