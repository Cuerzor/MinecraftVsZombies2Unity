using MVZ2.Managers;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MVZ2.Models
{
    public class ModelManager : MonoBehaviour
    {
        public Sprite ShotIcon(Model model, int width, int height, Vector2 modelOffset, string name = null)
        {
            var pictureName = name ?? "ModelIcon";
            //激活摄像机与灯光
            modelShotRoot.gameObject.SetActive(true);

            //设置模型
            var modelInstance = Model.Create(model, modelShotPositionTransform, modelShotCamera).gameObject;
            modelInstance.transform.localPosition = Vector3.zero;


            //创建一个用于渲染图片的RenderTexture
            var colorFormat = GetSupportedColorFormat();
            var depthFormat = GetSupportedDepthFormat();
            RenderTexture renderTexture = new RenderTexture(width, height, colorFormat, depthFormat);
            renderTexture.antiAliasing = 2;
            modelShotCamera.targetTexture = renderTexture;
            modelShotCamera.enabled = false;

            // 相机渲染。
            modelShotCamera.orthographicSize = height * 0.005f;

            var localPos = modelShotPositionTransform.localPosition;
            localPos.x = modelOffset.x * 0.01f;
            localPos.y = modelOffset.y * 0.01f;
            modelShotPositionTransform.localPosition = localPos;

            SortingGroup.UpdateAllSortingGroups();

            // Create a standard request
            RenderPipeline.StandardRequest request = new RenderPipeline.StandardRequest();

            // Check if the request is supported by the active render pipeline
            if (RenderPipeline.SupportsRenderRequest(modelShotCamera, request))
            {
                // 2D Texture
                request.destination = renderTexture;
                // Render camera and fill texture2D with its view
                RenderPipeline.SubmitRenderRequest(modelShotCamera, request);
            }
            else
            {
                modelShotCamera.Render();
            }

            // 从Render Texture读取像素并保存为图片
            Texture2D texture = new Texture2D(width, height);
            RenderTexture.active = renderTexture;
            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture.Apply();
            texture.name = pictureName;
            RenderTexture.active = null; // 重置活动的Render Texture
            modelShotCamera.targetTexture = null;
            renderTexture.Release();
            DestroyImmediate(modelInstance);

            // 创建Sprite。
            Sprite sprite = main.ResourceManager.CreateSprite(texture, new Rect(0, 0, width, height), Vector2.one * 0.5f, pictureName, "modelIcon");

            return sprite;
        }
        private GraphicsFormat GetSupportedColorFormat()
        {
            if (confirmedColorFormat)
            {
                return supportedColorFormat;
            }
            Debug.Log("Checking supported color formats...");
            var format = SystemInfo.GetCompatibleFormat(GraphicsFormat.R8G8B8A8_SRGB, FormatUsage.Render);
            if (format == GraphicsFormat.None)
            {
                Debug.LogWarning("Cannot find a supported color format for device, using default color format.");
                return GraphicsFormat.R8G8B8A8_SRGB;
            }
            else
            {
                Debug.Log($"Found supported color format {format}.");
                confirmedColorFormat = true;
                supportedColorFormat = format;
                return format;
            }
        }
        private GraphicsFormat GetSupportedDepthFormat()
        {
            if (confirmedDepthFormat)
            {
                return supportedDepthFormat;
            }
            Debug.Log("Checking supported depth formats...");
            var format = SystemInfo.GetCompatibleFormat(GraphicsFormat.D32_SFloat_S8_UInt, FormatUsage.Render);
            if (format == GraphicsFormat.None)
            {
                Debug.LogWarning("Cannot find a supported depth format for device, using default depth format.");
                return GraphicsFormat.D32_SFloat_S8_UInt;
            }
            else
            {
                Debug.Log($"Found supported depth format {format}.");
                confirmedDepthFormat = true;
                supportedDepthFormat = format;
                return format;
            }
        }
        public MainManager Main => main;
        private bool confirmedColorFormat = false;
        private GraphicsFormat supportedColorFormat = GraphicsFormat.R8G8B8A8_SRGB;
        private bool confirmedDepthFormat = false;
        private GraphicsFormat supportedDepthFormat = GraphicsFormat.D32_SFloat_S8_UInt;
        [SerializeField]
        private MainManager main;
        [SerializeField]
        private Transform modelShotRoot;
        [SerializeField]
        private Transform modelShotPositionTransform;
        [SerializeField]
        private Camera modelShotCamera;
    }
}
