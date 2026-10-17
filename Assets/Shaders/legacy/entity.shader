Shader "MinecraftVSZombies2/Legacy/Entity"
{
    Properties
    {
        _MainTex("Diffuse", 2D) = "white" {}

        [Header(Color)]
        [Space(10)]
        _Color("Tint", Color) = (1,1,1,1)
        _ColorOffset("Color Offset", Color) = (0,0,0,0)
        [Toggle]
        _GrayscaleMaterial("Grayscale", Int) = 0
        _GrayscaleFactor("Grayscale Factor", Vector) = (0.598,1.174,0.224,0)
        [Enum(UnityEngine.Rendering.CompareFunction)]
        _ZTest ("Z Test", Int) = 4

        [Header(Burn)]
        _BurnNoise("Noise Tex", 2D) = "white"{}
        _BurnValue("Value", Range(0, 1)) = 0
        _BurnEdgeColor("Edge Color", Color) = (0,0.5,0.5,1)
        _BurnEdgeThreshold("Edge Threshold", Float) = 0.2
        _BurnFireColor("Fire Color", Color) = (0,1,1,1)
        
        [Header(Circle Tiled)]
        [Toggle(CIRCLE_TILED)]
        _CircleTiled("Circle Tiled", Int) = 0
        _CircleFill("Fill", Range(0, 1)) = 1
        [KeywordEnum(Right, Down, Left, Up)]
        _CircleStart("Start", Range(0, 3)) = 0
        [Toggle]
        _CircleClockwise("Clockwise", Int) = 0
        
        [Header(HSV)]
        _HSVOffset("HSV Offset", Vector) = (0,0,0,0)
        
        _LevelMapST ("Level Map ST", Vector) = (14, 10.2, 0, 0)

        [Header(Lighting)]
        [Toggle(LIT)]
        _LIT("Lit", Int) = 1
        [HideInInspector] _LightDisabled("Light Disabled", Int) = 0
        [Toggle] _BackgroundLit("Lit by Background", Int) = 0
        [Toggle] _SpotLit("Lit by Spot", Int) = 1
        _LightMapSpot("Light Map Spot", 2D) = "black" {}
        
        [Header(DepthTest)]
        [Toggle(DEPTH_TEST)]
        _DepthTestEnabled("Depth Test Enabled", Int) = 1
        _DepthMap("Depth Map Spot", 2D) = "black" {}
        _DepthOffset("Depth Offset", Float) = 100

        [Header(Terrain)]
        _TerrainTexture("Terrain Texture", 2D) = "black" {}
        _TerrainMask("Mask", Int) = 0
        _TerrainFlagRequired("Required Flag", Int) = 0
        _TerrainFlagRequiredNot("Required Not Flag", Int) = 0
        [Toggle]
        _TerrainFlagReverse("Reverse", Int) = 0

        // Legacy properties. They're here so that materials using this shader can gracefully fallback to the legacy sprite shader.
        
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
        [HideInInspector] _AlphaTex("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags 
        {
            "Queue" = "Transparent" 
            "RenderType" = "Transparent" 
            "PreviewType" = "Plane"
        }

        Blend One OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest [_ZTest]

        Pass
        {
            Name "Main"

            HLSLPROGRAM
            #pragma vertex EntityVert
            #pragma fragment EntityFrag
            #pragma shader_feature _ CIRCLE_TILED
            #pragma shader_feature _ LIT
            #pragma shader_feature _ DEPTH_TEST

            #include "../hlsl/entity.hlsl"
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}
