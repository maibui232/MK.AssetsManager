namespace MK.AssetsManager
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using MK.Extensions;
    using MK.Log;
    using UnityEngine;
    using UnityEngine.Networking;
    using ILogger = MK.Log.ILogger;
    using Object = UnityEngine.Object;

    internal sealed class ExternalAssetManager : IExternalAssetsManager
    {
        private readonly Dictionary<string, object> uriToObjectMap = new();
        private readonly ILogger                    logger;

        public ExternalAssetManager(ILoggerManager loggerManager)
        {
            this.logger = loggerManager.GetLogger(this);
        }

        async UniTask<Texture2D> IExternalAssetsManager.DownloadTextureAsync(string uri, IProgress<float> progress, CancellationToken cancellationToken)
        {
            if (this.uriToObjectMap.TryGetValue(uri, out var obj)) return (Texture2D)obj;
            var request = UnityWebRequestTexture.GetTexture(uri);
            var result  = DownloadHandlerTexture.GetContent(await request.SendWebRequest());
            this.uriToObjectMap.Add(uri, result);

            return result;
        }

        async UniTask<Sprite> IExternalAssetsManager.DownloadSpriteAsync(string uri, IProgress<float> progress, CancellationToken cancellationToken)
        {
            var spriteUri = $"sprite://{uri}";

            if (this.uriToObjectMap.TryGetValue(spriteUri, out var obj)) return (Sprite)obj;
            var texture = await ((IExternalAssetsManager)this).DownloadTextureAsync(uri, progress, cancellationToken);
            var sprite  = texture.ToSprite();
            this.uriToObjectMap.Add(spriteUri, sprite);

            return sprite;
        }

        async UniTask<AudioClip> IExternalAssetsManager.DownloadAudioClipAsync(string uri, AudioType audioType, IProgress<float> progress, CancellationToken cancellationToken)
        {
            if (this.uriToObjectMap.TryGetValue(uri, out var obj)) return (AudioClip)obj;
            var request = UnityWebRequestMultimedia.GetAudioClip(uri, audioType);
            var result  = DownloadHandlerAudioClip.GetContent(await request.SendWebRequest());
            this.uriToObjectMap.Add(uri, result);

            return result;
        }

        async UniTask<string> IExternalAssetsManager.DownloadTextAsync(string uri, IProgress<float> progress, CancellationToken cancellationToken)
        {
            if (this.uriToObjectMap.TryGetValue(uri, out var obj)) return (string)obj;
            var request = UnityWebRequest.Get(uri);
            await request.SendWebRequest();
            var result = request.downloadHandler.text;
            this.uriToObjectMap.Add(uri, result);

            return result;
        }

        async UniTask<AssetBundle> IExternalAssetsManager.DownloadAssetBundleAsync(string uri, IProgress<float> progress, CancellationToken cancellationToken)
        {
            if (this.uriToObjectMap.TryGetValue(uri, out var obj)) return (AssetBundle)obj;
            var request = UnityWebRequestAssetBundle.GetAssetBundle(uri);
            var result  = DownloadHandlerAssetBundle.GetContent(await request.SendWebRequest());
            this.uriToObjectMap.Add(uri, result);

            return result;
        }

        void IExternalAssetsManager.ReleaseAsset(string uri)
        {
            if (!this.uriToObjectMap.TryGetValue(uri, out var obj))
            {
                this.logger.Error($"Asset already released or it doesn't exist: {uri}");

                return;
            }

            if (obj == null) return;
            this.uriToObjectMap.Remove(uri);
            if (obj is Object unityObject) Object.Destroy(unityObject);
        }
    }
}