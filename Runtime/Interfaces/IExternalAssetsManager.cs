namespace MK.AssetsManager
{
    using System;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using UnityEngine;

    public interface IExternalAssetsManager
    {
        UniTask<Texture2D> DownloadTextureAsync(string uri, IProgress<float> progress = null, CancellationToken cancellationToken = default);

        UniTask<Sprite> DownloadSpriteAsync(string uri, IProgress<float> progress = null, CancellationToken cancellationToken = default);

        UniTask<AudioClip> DownloadAudioClipAsync(string uri, AudioType audioType = AudioType.OGGVORBIS, IProgress<float> progress = null, CancellationToken cancellationToken = default);

        UniTask<string> DownloadTextAsync(string uri, IProgress<float> progress = null, CancellationToken cancellationToken = default);

        UniTask<AssetBundle> DownloadAssetBundleAsync(string uri, IProgress<float> progress = null, CancellationToken cancellationToken = default);

        void ReleaseAsset(string uri);
    }
}