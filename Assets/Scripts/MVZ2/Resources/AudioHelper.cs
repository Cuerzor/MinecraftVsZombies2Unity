#nullable enable

using System.IO;
using Cysharp.Threading.Tasks;
using PVZEngine.Tools.Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace MVZ2.Resources
{
    public static class AudioHelper
    {
        public static AudioType GetAudioTypeFromPath(string path)
        {
            string extension = Path.GetExtension(path).ToLower();
            switch (extension)
            {
                case ".mp3": return AudioType.MPEG;
                case ".ogg": return AudioType.OGGVORBIS;
                case ".wav": return AudioType.WAV;
                default:
                    Log.LogWarning($"未知音频格式: {extension}，尝试使用 UNKNOWN 类型");
                    return AudioType.UNKNOWN;
            }
        }
        public static async UniTask<AudioClip?> LoadAudioClip(string url, AudioType type, bool streaming = false)
        {
            using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, type))
            {
                var handler = (DownloadHandlerAudioClip)request.downloadHandler;

                // 开启流式音频
                handler.streamAudio = streaming;

                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Log.LogError($"音频加载失败: {request.error}");
                    return null;
                }
                else
                {
                    return DownloadHandlerAudioClip.GetContent(request);
                }
            }
        }
    }
}