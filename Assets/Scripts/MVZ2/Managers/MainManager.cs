using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MukioI18n;
using MVZ2.Almanacs;
using MVZ2.Assets.Scripts.MVZ2.Managers;
using MVZ2.Audios;
using MVZ2.Cameras;
using MVZ2.Collisions;
using MVZ2.Cursors;
using MVZ2.GameContent.Effects;
using MVZ2.Games;
using MVZ2.IO;
using MVZ2.Level;
using MVZ2.Level.Components;
using MVZ2.Localization;
using MVZ2.Modding;
using MVZ2.Models;
using MVZ2.Options;
using MVZ2.Save;
using MVZ2.Saves;
using MVZ2.Scenes;
using MVZ2.Supporters;
using MVZ2Logic;
using MVZ2Logic.Games;
using PVZEngine;
using PVZEngine.Level.Collisions;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace MVZ2.Managers
{
    public class MainManager : MonoBehaviour, IMainManager
    {
        public async Task Initialize()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            Application.targetFrameRate = 60;

            LogGraphics();

            InitSerializable();

            Global.Init(this);
            Game = new Game(BuiltinNamespace, LanguageManager, SaveManager, ResourceManager);

            OptionsManager.InitOptions();
            OptionsManager.LoadOptions();

            await ModManager.LoadMods(Game);

            // 在MOD信息加载之后
            await ResourceManager.LoadAllModResources();
            await LanguageManager.InitLanguagePacks();

            // 在MOD资源加载之后
            ModManager.LoadModLogics(Game);

            // 在MOD逻辑加载之后
            SaveManager.Load();

            if (IsFastMode())
            {
                var pipeline = new TaskPipeline();
                await InitLoad(pipeline);
            }

            Scene.Init();

            ModManager.PostGameInit();
        }
        public bool IsMobile()
        {
#if UNITY_EDITOR
            switch (platformMode)
            {
                case PlatformMode.Mobile:
                    return true;
                case PlatformMode.Standalone:
                    return false;
            }
#endif

#if UNITY_ANDROID || UNITY_IOS
            return true;
#else
            return false;
#endif
        }
        public bool IsFastMode()
        {
#if UNITY_EDITOR
            return fastMode;
#else
            return false;
#endif
        }
        public async Task InitLoad(TaskPipeline pipeline)
        {
            var sponsorProgress = new TaskProgress(TASK_LOAD_SPONSORS);
            pipeline.AddTask(sponsorProgress);

            pipeline.SetCurrentTask(0);
            await SponsorManager.PullSponsors(sponsorProgress);
            pipeline.SetCurrentTask(1);
        }
        public Sprite GetFinalSprite(SpriteReference spriteRef)
        {
            if (!SpriteReference.IsValid(spriteRef))
                return null;
            if (!OptionsManager.HasBloodAndGore())
            {
                var id = spriteRef.id;
                var censoredId = new NamespaceID(id.SpaceName, $"{id.Path}_censored");
                SpriteReference censoredRef;
                if (spriteRef.isSheet)
                {
                    censoredRef = new SpriteReference(censoredId, spriteRef.index);
                }
                else
                {
                    censoredRef = new SpriteReference(censoredId);
                }
                var sprite = LanguageManager.GetCurrentLanguageSprite(censoredRef) ?? ResourceManager.GetSprite(censoredRef);
                if (sprite)
                    return sprite;
            }
            return LanguageManager.GetCurrentLanguageSprite(spriteRef) ?? ResourceManager.GetSprite(spriteRef);
        }
        public Sprite GetFinalSprite(Sprite sprite)
        {
            var spriteID = ResourceManager.GetSpriteReference(sprite);
            if (!SpriteReference.IsValid(spriteID))
                return sprite;
            return GetFinalSprite(spriteID) ?? sprite;
        }
        public Sprite GetFinalSprite(SpriteReference spriteRef, string language)
        {
            if (!OptionsManager.HasBloodAndGore())
            {
                var id = spriteRef.id;
                var censoredId = new NamespaceID(id.SpaceName, $"{id.Path}_censored");
                SpriteReference censoredRef;
                if (spriteRef.isSheet)
                {
                    censoredRef = new SpriteReference(censoredId, spriteRef.index);
                }
                else
                {
                    censoredRef = new SpriteReference(censoredId);
                }
                var sprite = LanguageManager.GetLocalizedSprite(censoredRef, language) ?? ResourceManager.GetSprite(censoredRef);
                if (sprite)
                    return sprite;
            }
            return LanguageManager.GetLocalizedSprite(spriteRef, language) ?? ResourceManager.GetSprite(spriteRef);
        }
        public Sprite GetFinalSprite(Sprite sprite, string language)
        {
            var spriteID = ResourceManager.GetSpriteReference(sprite);
            if (!SpriteReference.IsValid(spriteID))
                return sprite;
            return GetFinalSprite(spriteID, language) ?? sprite;
        }
        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
            }
            else
            {
                throw new DuplicateInstanceException(name);
            }
        }
        private void OnApplicationQuit()
        {
            SaveManager.SaveModDatas();
        }
        private void InitSerializable()
        {
            SerializeHelper.init(BuiltinNamespace);
            // MVZ2
            SerializeHelper.RegisterClass<SerializableUserDataList>();
            SerializeHelper.RegisterClass<SerializableSaveDataMeta>();
            SerializeHelper.RegisterClass<SerializableAdviceComponent>();
            SerializeHelper.RegisterClass<SerializableArtifactComponent>();
            SerializeHelper.RegisterClass<SerializableLightComponent>();
            SerializeHelper.RegisterClass<SerializableUIComponent>();
            SerializeHelper.RegisterClass<SerializableSoundComponent>();
            SerializeHelper.RegisterClass<SerializableBlueprintComponent>();
            SerializeHelper.RegisterClass<EmptySerializableLevelComponent>();

            SerializeHelper.RegisterClass<SerializableLevelControllerPart>();
            SerializeHelper.RegisterClass<SerializableLevelBlueprintController>();
            SerializeHelper.RegisterClass<SerializableLevelBlueprintChooseController>();

            SerializeHelper.RegisterClass<SerializableBlueprintController>();
            SerializeHelper.RegisterClass<SerializableClassicBlueprintController>();
            SerializeHelper.RegisterClass<SerializableConveyorBlueprintController>();

            SerializeHelper.RegisterClass<SerializableModelData>();
            SerializeHelper.RegisterClass<SerializableSpriteModelData>();
            SerializeHelper.RegisterClass<SerializableUIModelData>();

            SerializeHelper.RegisterClass<SerializableModelGraphicGroup>();
            SerializeHelper.RegisterClass<SerializableModelRendererGroup>();
            SerializeHelper.RegisterClass<SerializableModelImageGroup>();

            SerializeHelper.RegisterClass<SerializableGraphicElement>();
            SerializeHelper.RegisterClass<SerializableRendererElement>();
            SerializeHelper.RegisterClass<SerializableImageElement>();

            SerializeHelper.RegisterClass<SerializableUnityCollisionSystem>();
            SerializeHelper.RegisterClass<SerializableUnityCollisionEntity>();
            SerializeHelper.RegisterClass<SerializableUnityEntityCollider>();
        }
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            if (e.Exception == null)
            {
                Debug.LogError(e);
                return;
            }
            if (e.Exception.InnerException != null)
            {
                Debug.LogError(e.Exception.InnerException);
                return;
            }
            if (e.Exception.InnerExceptions != null)
            {
                foreach (var exception in e.Exception.InnerExceptions)
                {
                    Debug.LogError(exception);
                }
                return;
            }
            Debug.LogError(e.Exception);
        }
        private void LogGraphics()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Quality: {QualitySettings.names[QualitySettings.GetQualityLevel()]}");

            sb.AppendLine("系统信息：");
            sb.AppendLine($"deviceModel: {SystemInfo.deviceModel}");
            sb.AppendLine($"deviceType: {SystemInfo.deviceType}");
            sb.AppendLine($"operatingSystem: {SystemInfo.operatingSystem}");
            sb.AppendLine($"operatingSystemFamily: {SystemInfo.operatingSystemFamily}");
            sb.AppendLine($"processorCount: {SystemInfo.processorCount}");
            sb.AppendLine($"processorFrequency: {SystemInfo.processorFrequency}");
            sb.AppendLine($"processorType: {SystemInfo.processorType}");
            sb.AppendLine($"supportsAccelerometer: {SystemInfo.supportsAccelerometer}");
            sb.AppendLine($"supportsAudio: {SystemInfo.supportsAudio}");
            sb.AppendLine($"supportsGyroscope: {SystemInfo.supportsGyroscope}");
            sb.AppendLine($"supportsLocationService: {SystemInfo.supportsLocationService}");
            sb.AppendLine($"supportsVibration: {SystemInfo.supportsVibration}");
            sb.AppendLine($"systemMemorySize: {SystemInfo.systemMemorySize}");

            sb.AppendLine();
            sb.AppendLine("显示设备信息：");
            sb.AppendLine($"graphicsDeviceID: {SystemInfo.graphicsDeviceID}");
            sb.AppendLine($"graphicsDeviceName: {SystemInfo.graphicsDeviceName}");
            sb.AppendLine($"graphicsDeviceType: {SystemInfo.graphicsDeviceType}");
            sb.AppendLine($"graphicsDeviceVendor: {SystemInfo.graphicsDeviceVendor}");
            sb.AppendLine($"graphicsDeviceVendorID: {SystemInfo.graphicsDeviceVendorID}");
            sb.AppendLine($"graphicsDeviceVersion: {SystemInfo.graphicsDeviceVersion}");
            sb.AppendLine($"graphicsMemorySize: {SystemInfo.graphicsMemorySize}");
            sb.AppendLine($"graphicsMultiThreaded: {SystemInfo.graphicsMultiThreaded}");
            sb.AppendLine($"graphicsShaderLevel: {SystemInfo.graphicsShaderLevel}");
            sb.AppendLine($"graphicsUVStartsAtTop: {SystemInfo.graphicsUVStartsAtTop}");
            sb.AppendLine($"maxGraphicsBufferSize: {SystemInfo.maxGraphicsBufferSize}");
            sb.AppendLine($"supportsGraphicsFence: {SystemInfo.supportsGraphicsFence}");
            sb.AppendLine($"renderingThreadingMode: {SystemInfo.renderingThreadingMode}");
            sb.AppendLine($"hasHiddenSurfaceRemovalOnGPU: {SystemInfo.hasHiddenSurfaceRemovalOnGPU}");
            sb.AppendLine($"hasDynamicUniformArrayIndexingInFragmentShaders: {SystemInfo.hasDynamicUniformArrayIndexingInFragmentShaders}");
            sb.AppendLine($"supportsShadows: {SystemInfo.supportsShadows}");
            sb.AppendLine($"supportsRawShadowDepthSampling: {SystemInfo.supportsRawShadowDepthSampling}");
            sb.AppendLine($"supportsMotionVectors: {SystemInfo.supportsMotionVectors}");
            sb.AppendLine($"supports3DTextures: {SystemInfo.supports3DTextures}");
            sb.AppendLine($"supports2DArrayTextures: {SystemInfo.supports2DArrayTextures}");
            sb.AppendLine($"supports3DRenderTextures: {SystemInfo.supports3DRenderTextures}");
            sb.AppendLine($"supportsCubemapArrayTextures: {SystemInfo.supportsCubemapArrayTextures}");
            sb.AppendLine($"copyTextureSupport: {SystemInfo.copyTextureSupport}");
            sb.AppendLine($"supportsComputeShaders: {SystemInfo.supportsComputeShaders}");
            sb.AppendLine($"renderingThreadingMode: {SystemInfo.renderingThreadingMode}");
            sb.AppendLine($"supportsGeometryShaders: {SystemInfo.supportsGeometryShaders}");
            sb.AppendLine($"supportsTessellationShaders: {SystemInfo.supportsTessellationShaders}");
            sb.AppendLine($"supportsInstancing: {SystemInfo.supportsInstancing}");
            sb.AppendLine($"supportsHardwareQuadTopology: {SystemInfo.supportsHardwareQuadTopology}");
            sb.AppendLine($"supports32bitsIndexBuffer: {SystemInfo.supports32bitsIndexBuffer}");
            sb.AppendLine($"supportsSparseTextures: {SystemInfo.supportsSparseTextures}");
            sb.AppendLine($"supportedRenderTargetCount: {SystemInfo.supportedRenderTargetCount}");
            sb.AppendLine($"supportsSeparatedRenderTargetsBlend: {SystemInfo.supportsSeparatedRenderTargetsBlend}");
            sb.AppendLine($"supportedRandomWriteTargetCount: {SystemInfo.supportedRandomWriteTargetCount}");
            sb.AppendLine($"supportsMultisampledTextures: {SystemInfo.supportsMultisampledTextures}");
            sb.AppendLine($"supportsMultisampleAutoResolve: {SystemInfo.supportsMultisampleAutoResolve}");
            sb.AppendLine($"supportsTextureWrapMirrorOnce: {SystemInfo.supportsTextureWrapMirrorOnce}");
            sb.AppendLine($"usesReversedZBuffer: {SystemInfo.usesReversedZBuffer}");
            sb.AppendLine($"npotSupport: {SystemInfo.npotSupport}");
            sb.AppendLine($"maxTextureSize: {SystemInfo.maxTextureSize}");
            sb.AppendLine($"maxCubemapSize: {SystemInfo.maxCubemapSize}");
            sb.AppendLine($"maxComputeBufferInputsVertex: {SystemInfo.maxComputeBufferInputsVertex}");
            sb.AppendLine($"maxComputeBufferInputsFragment: {SystemInfo.maxComputeBufferInputsFragment}");
            sb.AppendLine($"maxComputeBufferInputsGeometry: {SystemInfo.maxComputeBufferInputsGeometry}");
            sb.AppendLine($"maxComputeBufferInputsDomain: {SystemInfo.maxComputeBufferInputsDomain}");
            sb.AppendLine($"maxComputeBufferInputsHull: {SystemInfo.maxComputeBufferInputsHull}");
            sb.AppendLine($"maxComputeBufferInputsCompute: {SystemInfo.maxComputeBufferInputsCompute}");
            sb.AppendLine($"maxComputeWorkGroupSize: {SystemInfo.maxComputeWorkGroupSize}");
            sb.AppendLine($"maxComputeWorkGroupSizeX: {SystemInfo.maxComputeWorkGroupSizeX}");
            sb.AppendLine($"maxComputeWorkGroupSizeY: {SystemInfo.maxComputeWorkGroupSizeY}");
            sb.AppendLine($"maxComputeWorkGroupSizeZ: {SystemInfo.maxComputeWorkGroupSizeZ}");
            sb.AppendLine($"supportsAsyncCompute: {SystemInfo.supportsAsyncCompute}");
            sb.AppendLine($"supportsGraphicsFence: {SystemInfo.supportsGraphicsFence}");
            sb.AppendLine($"supportsAsyncGPUReadback: {SystemInfo.supportsAsyncGPUReadback}");
            sb.AppendLine($"supportsRayTracing: {SystemInfo.supportsRayTracing}");
            sb.AppendLine($"supportsSetConstantBuffer: {SystemInfo.supportsSetConstantBuffer}");
            sb.AppendLine($"minConstantBufferOffsetAlignment: {SystemInfo.constantBufferOffsetAlignment}");
            sb.AppendLine($"hasMipMaxLevel: {SystemInfo.hasMipMaxLevel}");
            sb.AppendLine($"supportsMipStreaming: {SystemInfo.supportsMipStreaming}");
            sb.AppendLine($"usesLoadStoreActions: {SystemInfo.usesLoadStoreActions}");

            sb.Append($"supportedTextureFormats: ");
            foreach (var v in Enum.GetValues(typeof(TextureFormat)))
            {
                try
                {
                    var format = (TextureFormat)v;
                    if (SystemInfo.SupportsTextureFormat(format))
                    {
                        sb.Append($"{format},");
                    }
                }
                catch (ArgumentException)
                {
                    continue;
                }
            }
            sb.AppendLine();
            sb.Append($"supportedRenderTextureFormats: ");
            foreach (var v in Enum.GetValues(typeof(RenderTextureFormat)))
            {
                try
                {
                    var format = (RenderTextureFormat)v;
                    if (SystemInfo.SupportsRenderTextureFormat(format))
                    {
                        sb.Append($"{format},");
                    }
                }
                catch (ArgumentException)
                {
                    continue;
                }
            }
            sb.AppendLine();
            Debug.Log(sb.ToString());
        }
        IGame IMainManager.Game => Game;

        [TranslateMsg("初始化任务名称")]
        public const string TASK_LOAD_SPONSORS = "获取赞助者列表……";
        public static MainManager Instance { get; private set; }
        public Game Game { get; private set; }
        public string BuiltinNamespace => builtinNamespace;
        public CoroutineManager CoroutineManager => coroutine;
        public ResourceManager ResourceManager => resource;
        public ModelManager ModelManager => model;
        public SoundManager SoundManager => sound;
        public MusicManager MusicManager => music;
        public LevelManager LevelManager => level;
        public LanguageManager LanguageManager => lang;
        public SaveManager SaveManager => save;
        public ModManager ModManager => mod;
        public CursorManager CursorManager => cursor;
        public ShakeManager ShakeManager => shake;
        public FileManager FileManager => file;
        public OptionsManager OptionsManager => options;
        public ResolutionManager ResolutionManager => resolution;
        public SceneLoadingManager SceneManager => sceneLoadingManager;
        public AlmanacManager AlmanacManager => almanacManager;
        public StoreManager StoreManager => storeManager;
        public InputManager InputManager => inputManager;
        public SponsorManager SponsorManager => sponsorManager;
        public ParticleManager ParticleManager => particleManager;
        public MainSceneController Scene => scene;
        ISceneController IMainManager.Scene => scene;
        IMusicManager IMainManager.Music => music;
        ILevelManager IMainManager.Level => level;
        IOptionsManager IMainManager.Options => options;
        IGlobalSave IMainManager.Saves => save;
        [SerializeField]
        private string builtinNamespace = "mvz2";
        [SerializeField]
        private PlatformMode platformMode = PlatformMode.Default;
        [SerializeField]
        private bool fastMode;
        [SerializeField]
        private CoroutineManager coroutine;
        [SerializeField]
        private ResourceManager resource;
        [SerializeField]
        private ModelManager model;
        [SerializeField]
        private SoundManager sound;
        [SerializeField]
        private MusicManager music;
        [SerializeField]
        private LevelManager level;
        [SerializeField]
        private LanguageManager lang;
        [SerializeField]
        private SaveManager save;
        [SerializeField]
        private ModManager mod;
        [SerializeField]
        private CursorManager cursor;
        [SerializeField]
        private ShakeManager shake;
        [SerializeField]
        private FileManager file;
        [SerializeField]
        private OptionsManager options;
        [SerializeField]
        private ResolutionManager resolution;
        [SerializeField]
        private SceneLoadingManager sceneLoadingManager;
        [SerializeField]
        private AlmanacManager almanacManager;
        [SerializeField]
        private StoreManager storeManager;
        [SerializeField]
        private InputManager inputManager;
        [SerializeField]
        private SponsorManager sponsorManager;
        [SerializeField]
        private ParticleManager particleManager;
        [SerializeField]
        private MainSceneController scene;
        public enum PlatformMode
        {
            Default,
            Mobile,
            Standalone
        }
    }
    public class DuplicateInstanceException : Exception
    {
        public DuplicateInstanceException(string instanceName) : base($"There's already an instance of {instanceName} in the game.")
        {

        }
    }
    public class TaskPipeline
    {
        public void AddTask(TaskProgress child)
        {
            tasks.Add(child);
        }
        public void RemoveTask(TaskProgress child)
        {
            tasks.Remove(child);
        }
        public void SetCurrentTask(int index)
        {
            currentTaskIndex = index;
        }
        public bool IsFinished()
        {
            return currentTaskIndex >= tasks.Count;
        }
        public string GetCurrentTaskName()
        {
            var task = GetCurrentTask();
            if (task == null)
                return string.Empty;
            return task.GetName();
        }
        public float GetProgress()
        {
            if (tasks.Count > 0)
            {
                return tasks.Sum(c => c.GetProgress()) / tasks.Count;
            }
            return 1;
        }
        private TaskProgress GetCurrentTask()
        {
            if (currentTaskIndex < 0 || currentTaskIndex >= tasks.Count)
                return null;
            return tasks[currentTaskIndex];
        }
        private int currentTaskIndex;
        private List<TaskProgress> tasks = new List<TaskProgress>();
        private float progress;
    }
    public class TaskProgress
    {
        public TaskProgress(string name)
        {
            this.name = name;
        }
        public void SetProgress(float progress)
        {
            this.progress = progress;
        }
        public float GetProgress()
        {
            return progress;
        }
        public string GetName()
        {
            return name;
        }
        private float progress;
        private string name;
    }
}
