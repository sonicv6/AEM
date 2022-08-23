﻿using System.Linq.Expressions;
using Anarchy.Network.Discord.SDK;
using Anarchy.UI;
using UnityEngine;

namespace Anarchy.Configuration
{
    /// <summary>
    /// Specific settings that apply to Video/Graphics
    /// </summary>
    public static class VideoSettings
    {
        public static BoolSetting BladeTrails = new BoolSetting("BladeTrails", true);
        public static BoolSetting Blur = new BoolSetting("Blur", false);
        public static BoolSetting CameraTilt = new BoolSetting("CameraTilt", true);
        public static BoolSetting InfiniteTrail = new BoolSetting("InfiniteBladeTrail", false);
        public static BoolSetting Mipmap = new BoolSetting("Mipmap", false);
        public static BoolSetting OcclusionCulling = new BoolSetting("OcclusionCulling", true);
        public static BoolSetting VSync = new BoolSetting("VSync", false);
        public static BoolSetting UseShadows = new BoolSetting("UseShadows", true);
        public static BoolSetting WindEffect = new BoolSetting("WindEffect", true);
        public static BoolSetting ShadowsUI = new BoolSetting("ShadowsUI", true);
        public static BoolSetting DisableFog = new BoolSetting("DisableFog", false);
        public static BoolSetting Fullscreen = new BoolSetting("Fullscreen", true);

        public static IntSetting AntiAliasing = new IntSetting("AntiAliasing", 0);
        public static IntSetting AnisotropicFiltering = new IntSetting("AnisoLevel", 0);
        public static IntSetting BlendWeight = new IntSetting("BlendWeight", 0);
        public static IntSetting ShadowCascades = new IntSetting("ShadowCascades", 2);
        public static IntSetting ShadowProjection = new IntSetting("ShadowProjection", 0);
        public static IntSetting TextureQuality = new IntSetting("TextureQuality", 1);
        public static IntSetting TrailType = new IntSetting("BladeTrailType", 1);
        public static IntSetting MaxFPS = new IntSetting("MaxFPS", 0);
        public static IntSetting ResolutionIndex = new IntSetting("ResolutionIndex", Screen.resolutions.Length - 1);

        public static FloatSetting LODBias = new FloatSetting("LODBias", 2f);
        public static FloatSetting DrawDistance = new FloatSetting("DrawDistance", 1000f);
        public static FloatSetting ShadowDistance = new FloatSetting("ShadowDistance", 100f);
        public static FloatSetting Quality = new FloatSetting("Quality", 0f);
        public static FloatSetting TrailFPS = new FloatSetting("BladeTrailFPS", 60f);

        public static bool MCFogOverride;

        public static void Apply()
        {
            if (IN_GAME_MAIN_CAMERA.MainCamera != null && IN_GAME_MAIN_CAMERA.MainCamera.GetComponent<TiltShift>() != null)
            {
                IN_GAME_MAIN_CAMERA.MainCamera.GetComponent<TiltShift>().enabled = Blur.Value;
            }
            if (IN_GAME_MAIN_CAMERA.BaseCamera != null)
            {
                IN_GAME_MAIN_CAMERA.BaseCamera.useOcclusionCulling = OcclusionCulling;
                IN_GAME_MAIN_CAMERA.BaseCamera.farClipPlane = Mathf.RoundToInt(DrawDistance.Value);
            }
            ChangeQuality.setCurrentQuality();
            QualitySettings.masterTextureLimit = TextureQuality.Value;
            QualitySettings.anisotropicFiltering = (AnisotropicFiltering)AnisotropicFiltering.Value;
            QualitySettings.vSyncCount = VSync ? 1 : 0;
            int aa = 0;
            switch (AntiAliasing.Value)
            {
                case 0:
                    break;

                case 1:
                    aa = 2;
                    break;

                case 2:
                    aa = 4;
                    break;

                case 3:
                    aa = 8;
                    break;

                default:
                    aa = 0;
                    break;
            }
            QualitySettings.antiAliasing = aa;
            QualitySettings.lodBias = LODBias;
            QualitySettings.blendWeights = (BlendWeights)(BlendWeight.Value <= 1 ? (BlendWeight.Value + 1) : 4);
            if (UseShadows.Value)
            {
                QualitySettings.shadowDistance = ShadowDistance.Value;
                QualitySettings.shadowCascades = (int)System.Math.Pow(2, ShadowCascades.Value);
                QualitySettings.shadowProjection = (UnityEngine.ShadowProjection)ShadowProjection.Value;
            }
            else
            {
                QualitySettings.shadowDistance = 0f;
                QualitySettings.shadowCascades = 1;
                QualitySettings.shadowProjection = UnityEngine.ShadowProjection.CloseFit;
            }

            if (FengGameManagerMKII.Level != null && FengGameManagerMKII.Level.HasFog && !MCFogOverride)
            {
                RenderSettings.fog = !DisableFog.Value;
            }
            else if (!MCFogOverride)
            {
                RenderSettings.fog = false;
            }
            Application.targetFrameRate = (MaxFPS <= 30) ? -1 : MaxFPS;
            Settings.Apply();
        }

        public static string[] Resolutions
        {
            get
            {
                string[] resolutions = new string[Screen.resolutions.Length];
                for (int r = 0; r < Screen.resolutions.Length; r++)
                {
                    resolutions[r] = $"{Screen.resolutions[r].width}x{Screen.resolutions[r].height}";
                }

                return resolutions;
            }
        }
    }
}
