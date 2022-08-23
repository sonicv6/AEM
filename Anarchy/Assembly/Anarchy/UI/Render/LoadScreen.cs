﻿using Anarchy.IO;
using System;
using System.Collections;
using Anarchy.Configuration;
using UnityEngine;
using UnityEngine.UI;

namespace Anarchy.UI
{
    public class LoadScreen : UIBase
    {
        private const float Fade = 0.02f;
        private const float MaxAlpha = 1f;
        private const float MinAlpha = 0.2f;
        private const float UpdateTime = 0.25f;

        private bool colorInc = false;
        private int dots = 0;
        private Color fadeColor = new Color(1f, 1f, 1f, 1f);
        public Text Info = null;
        public Image Logo = null;
        public Text Loading = null;
        private bool textUpdate = true;
        private float timeToUpdate = 0f;

        private void Awake()
        {
            Loading.text = "Loading";
            StartCoroutine(Load());
        }

        private void FixedUpdate()
        {
            LogoFadeAnimation();
        }

        private IEnumerator Load()
        {
            string profile = "";
            Info.text = "Loading configuration...";
            using (ConfigFile file = new ConfigFile(Application.dataPath + "/Configuration/Settings.ini", ' ', false))
            {
                file.Load();
                file.AutoSave = false;
                Resolution res;
                if (VideoSettings.ResolutionIndex >= Screen.resolutions.Length)
                {
                    VideoSettings.ResolutionIndex.Value = VideoSettings.ResolutionIndex.DefaultValue;
                    VideoSettings.ResolutionIndex.Save();
                }
                res = Screen.resolutions[VideoSettings.ResolutionIndex];
                Screen.SetResolution(res.width, res.height, VideoSettings.Fullscreen);
                profile = file.GetString("profile");
                QualitySettings.SetQualityLevel(file.GetInt("graphics"), true);
                Localization.Language.SetLanguage(file.GetString("language"));

                Configuration.Settings.Load();
                Configuration.VideoSettings.Apply();
            }
            yield return new WaitForSeconds(0.5f);

            Info.text = "Loading RCAssets...";
            yield return StartCoroutine(RC.RCManager.DownloadAssets());

            Optimization.Caching.Pool.Create();
            yield return new WaitForSeconds(0.5f);

            Info.text = $"Loading profile({profile})..";
            User.LoadProfile(profile);
            Localization.Language.UpdateFormats();
            Localization.Locale loc = new Localization.Locale("GUI", true);

            GUI.LabelEnabled = loc["enabled"];
            GUI.LabelDisabled = loc["disabled"];
            yield return new WaitForSeconds(0.5f);

            Info.text = "Loading visuals..";
            Style.Load();
            Style.ResetScreenParameters();
            UIManager.UpdateGUIScaling();
            Optimization.Labels.Font = Style.Font;
            yield return new WaitForSeconds(0.5f);
            Info.text = "Enjoy!";
            Optimization.Labels.VERSION = string.Format(UIMainReferences.VersionShow, AnarchyManager.AnarchyVersion.ToString());
            textUpdate = false;
            Loading.text = "Loading complete";
            yield return new WaitForSeconds(2f);

            Destroy(gameObject);
            AnarchyManager.Background.Enable();
        }

        private void LogoFadeAnimation()
        {
            float delta = colorInc ? Fade : -Fade;
            fadeColor.a += delta;
            Logo.color = fadeColor;
            if (fadeColor.a <= MinAlpha)
            {
                colorInc = true;
            }
            else if (fadeColor.a >= MaxAlpha)
            {
                colorInc = false;
            }
        }

        private void Update()
        {
            if (textUpdate)
            {
                timeToUpdate += Time.deltaTime;
                if (timeToUpdate >= UpdateTime)
                {
                    timeToUpdate = 0;
                    dots = dots == 3 ? 0 : dots + 1;
                    string newText = "Loading";
                    for (int i = 0; i < dots; i++)
                    {
                        newText += '.';
                    }
                    Loading.text = newText;
                }
            }
        }
    }
}
