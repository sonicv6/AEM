﻿using System.IO;
using RC;
using UnityEngine;
using static Anarchy.UI.GUI;

namespace Anarchy.UI
{
    public class CustomPanel : GUIPanel
    {
        private const int CommandsPage = 3;
        private const int UnityMapPage = 2;
        private const int CustomLogicPage = 1;
        private const int CustomMapsPage = 0;

        public static readonly string LogicsPath = Application.dataPath + "/../Logics/";
        public static readonly string MapsPath = Application.dataPath + "/../Maps/";
        public static readonly string UnityMapPath = Application.dataPath + "/CustomMap/";
        public static readonly string CommandListsPath = Application.dataPath + "/CommandLists/";

        private string[] wagons = {"No canopy"};
        private string[] allNames;
        private GUIStyle areaStyle;
        private string filter;
        private float filterUpdate = 0f;
        private string[] gameTypes;
        private SmartRect left;
        private string[] selections;
        private SmartRect rect;
        private SmartRect right;
        private Vector2 scroll;
        private Rect scrollArea;
        private Rect scrollAreaView;
        private SmartRect scrollRect;
        private string selectedMap;
        private Vector3 pos;
        private string prefab = string.Empty;
        private string x = "0";
        private string y = "0";
        private string z = "0";

        public CustomPanel() : base(nameof(CustomPanel), GUILayers.CustomPanel)
        {
        }

        protected override void DrawMainPart()
        {
            left.Reset();
            right.Reset();
            rect.Reset();
            float offset = new AutoScaleFloat(120f);
            rect.MoveOffsetX(offset);
            rect.width -= offset;
            pageSelection = SelectionGrid(rect, pageSelection, selections, selections.Length, true);
            rect.ResetX();
            LabelCenter(right, locale["picker" + pageSelection.ToString()], true);
            right.BeginHorizontal(2);
            if (Button(right, locale["update"], false))
            {
                switch (pageSelection)
                {
                    case CustomMapsPage:
                        allNames = LoadFiles(MapsPath);
                        break;
                    case CustomLogicPage:
                        allNames = LoadFiles(LogicsPath);
                        break;
                    case UnityMapPage:
                        allNames = LoadFiles(UnityMapPath);
                        break;
                    case CommandsPage:
                        allNames = LoadFiles(CommandListsPath);
                        break;
                }
            }
            right.MoveX();
            if (Button(right, locale["random"], true))
            {
                int pickId = Random.Range(0, allNames.Length);
                if (filter.Length > 0)
                {
                    PickByName(allNames[pickId], true);
                }
                else
                {
                    Pick(pickId, allNames[pickId], true);
                }
            }
            right.ResetX();
            filter = TextField(right, filter, locale["filter"], Style.LabelOffset, true);

            scrollArea.y = right.y;
            right.MoveToEndY(WindowPosition, Style.Height + Style.VerticalMargin);
            scrollArea.height = right.y - scrollArea.y;
            scrollRect.Reset();
            scrollAreaView.height = (Style.Height * allNames.Length) + ((Style.VerticalMargin * 2) * allNames.Length);

            scroll = BeginScrollView(scrollArea, scroll, scrollAreaView);
            for (int i = 0; i < allNames.Length; i++)
            {
                if (Button(scrollRect, allNames[i], (i != allNames.Length - 1)))
                {
                    if (filter.Length == 0)
                    {
                        Pick(i, allNames[i], false);
                    }
                    else
                    {
                        PickByName(allNames[i], false);
                    }
                }
            }
            EndScrollView();
        }

        private string GetSendString(bool rnd, string name)
        {
            return
                User.FormatColors($"<color=#$maincolor$>Next " +
                $"{(pageSelection == CustomLogicPage ? "logic" : "map")}" +
                $"{(rnd ? " (Random)" : "")}: " +
                $"<color=#$subcolor$><b>{name}</b></color></color>");
        }

        private string Load(int id, string path)
        {
            string[] files = System.IO.Directory.GetFiles(path);
            if (id == -1)
            {
                id = Random.Range(0, files.Length);
            }
            return System.IO.File.ReadAllText(files[id]);
        }

        private string LoadByName(string name, string path)
        {
            if (System.IO.File.Exists(path + name + ".txt"))
            {
                return System.IO.File.ReadAllText(path + name + ".txt");
            }
            return string.Empty;
        }

        private string[] LoadFiles(string path)
        {
            string[] files = System.IO.Directory.GetFiles(path);
            if (files.Length == 0)
            {
                return new string[0];
            }
            for (int i = 0; i < files.Length; i++)
            {
                var file = new System.IO.FileInfo(files[i]);
                string name = file.Name;
                if (file.Extension.Length > 0)
                {
                    name = name.Replace(file.Extension, "");
                }
                files[i] = name;
            }
            return files;
        }

        [GUIPage(CommandsPage)]
        private void CommandListsPage()
        {
        }

        [GUIPage(UnityMapPage)]
        private void UnityPage()
        {
            prefab = TextField(left, prefab, "Prefab", Style.LabelOffset, true);
            Label(left, "Position", true);
            x = TextField(left, x, "X", Style.LabelOffset, true);
            y = TextField(left, y, "Y", Style.LabelOffset, true);
            z = TextField(left, z, "Z", Style.LabelOffset, true);
            Vector3 p = new Vector3(float.Parse(x), float.Parse(y), float.Parse(z));

            if (Button(left, "Spawn Map", true))
            {
                FengGameManagerMKII.FGM.BasePV.RPC("EMCustomMapRPC", PhotonTargets.AllBuffered, selectedMap, prefab, p, new Quaternion());
            }
        }

        [GUIPage(CustomLogicPage)]
        private void LogicPage()
        {
            LabelCenter(left, locale["logicScript"], true);
            left.height = (WindowPosition.y + WindowPosition.height - Style.WindowBottomOffset - Style.Height - Style.VerticalMargin) - left.y;
            CustomLevel.currentScriptLogic = UnityEngine.GUI.TextArea(left.ToRect(), CustomLevel.currentScriptLogic, areaStyle);

            rect.MoveToEndY(WindowPosition, Style.Height);
            rect.width = 144f;
            rect.height = Style.Height;
            if (Button(rect, locale["btnClear"], false))
            {
                CustomLevel.currentScriptLogic = "";
            }

            rect.MoveToEndX(WindowPosition, 144f);
            if (Button(rect, locale["btnClose"]))
            {
                Disable();
                return;
            }
        }

        [GUIPage(CustomLogicPage, GUIPageType.EnableMethod)]
        private void LogicPageEnable()
        {
            Rect pos = WindowPosition;
            pos.y += (Style.Height + Style.VerticalMargin);
            SmartRect[] rects = Helper.GetSmartRects(pos, 2);
            left = rects[0];
            right = rects[1];
            allNames = LoadFiles(LogicsPath);
        }

        [GUIPage(CustomMapsPage)]
        private void MapPage()
        {
            Label(rect, locale["gameMode"], false);
            rect.MoveOffsetX(Style.LabelOffset);
            SelectionGrid(rect, RCManager.GameType, gameTypes, gameTypes.Length, true);
            rect.ResetX();
            rect.width = right.width;
            TextField(rect, RCManager.SpawnCapCustom, locale["spawnCap"], Style.BigLabelOffset, true);

            LabelCenter(left, locale["mapScript"], true);
            left.height = (WindowPosition.y + WindowPosition.height - Style.WindowBottomOffset - Style.Height - Style.VerticalMargin) - left.y;
            CustomLevel.currentScript = UnityEngine.GUI.TextArea(left.ToRect(), CustomLevel.currentScript, areaStyle);

            rect.MoveToEndY(WindowPosition, Style.Height);
            rect.width = 144f;
            rect.height = Style.Height;
            if (Button(rect, locale["btnClear"], false))
            {
                CustomLevel.currentScript = "";
            }

            rect.MoveToEndX(WindowPosition, 144f);
            if (Button(rect, locale["btnClose"]))
            {
                Disable();
                return;
            }
        }

        [GUIPage(CustomMapsPage, GUIPageType.EnableMethod)]
        private void MapPageEnable()
        {
            Rect pos = WindowPosition;
            float k = ((Style.Height + Style.VerticalMargin) * 3f);
            pos.y += k;
            SmartRect[] rects = Helper.GetSmartRects(pos, 2);
            left = rects[0];
            right = rects[1];
            scrollArea = new Rect(right.x, right.y, right.width, WindowPosition.height - (4 * (Style.Height + Style.VerticalMargin)) - (Style.WindowTopOffset + Style.WindowBottomOffset) - 10f);
            scrollRect = new SmartRect(0f, 0f, right.width, right.height);
            scrollAreaView = new Rect(0f, 0f, rect.width, 1000f);
            allNames = LoadFiles(MapsPath);
        }

        protected override void OnPanelDisable()
        {
            gameTypes = null;
            selections = null;
            rect = null;
            areaStyle = null;
            allNames = null;
            filter = null;
        }

        protected override void OnPanelEnable()
        {
            gameTypes = locale.GetArray("gameTypes");
            rect = Helper.GetSmartRects(WindowPosition, 1)[0];
            selections = locale.GetArray("selection");
            areaStyle = new GUIStyle(Style.TextField);
            areaStyle.alignment = TextAnchor.UpperLeft;
            scroll = Optimization.Caching.Vectors.v2zero;
            filter = "";
        }

        private void Pick(int id, string name, bool rnd)
        {
            switch (pageSelection)
            {
                case CustomLogicPage:
                    CustomLevel.currentScriptLogic = Load(id, LogicsPath);
                    break;
                case CustomMapsPage:
                    CustomLevel.currentScript = Load(id, MapsPath);
                    break;
                case UnityMapPage:
                    selectedMap = name;
                    break;
                case CommandsPage:
                    FengGameManagerMKII.FGM.StartCoroutine(FengGameManagerMKII.FGM.ExecuteCommandList(Path.Combine(Application.dataPath, "CommandLists/" + name + ".txt")));
                    break;
            }
        }

        private void PickByName(string name, bool rnd)
        {
            switch (pageSelection)
            {
                case CustomLogicPage:
                    CustomLevel.currentScriptLogic = LoadByName(name, LogicsPath);
                    if (CustomLevel.currentScriptLogic == string.Empty)
                    {
                        return;
                    }
                    break;
                case CustomMapsPage:
                    CustomLevel.currentScript = LoadByName(name, MapsPath);
                    if (CustomLevel.currentScript == string.Empty)
                    {
                        return;
                    }
                    break;
                case UnityMapPage:
                    selectedMap = name;
                    break;
                case CommandsPage:
                    FengGameManagerMKII.FGM.StartCoroutine(FengGameManagerMKII.FGM.ExecuteCommandList(Path.Combine(Application.dataPath, "CommandLists/" + name + ".txt")));
                    break;
            }
        }

        public override void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Disable();
                return;
            }
            filterUpdate += Time.unscaledDeltaTime;
            if (filterUpdate >= 1f)
            {
                string[] loaded;
                switch (pageSelection)
                {
                    case CustomMapsPage:
                        loaded = LoadFiles(MapsPath);
                        break;
                    case CustomLogicPage:
                        loaded = LoadFiles(LogicsPath);
                        break;
                    case UnityMapPage:
                        loaded = LoadFiles(UnityMapPath);
                        break;
                    case CommandsPage:
                        loaded = LoadFiles(CommandListsPath);
                        break;
                    default:
                        loaded = new string[1];
                        break;
                }
                if (filter != string.Empty)
                {
                    var list = new System.Collections.Generic.List<string>();
                    string flt = filter.ToLower();
                    foreach (string str in loaded)
                    {
                        if (str.ToLower().Contains(flt))
                        {
                            list.Add(str);
                        }
                    }
                    allNames = list.ToArray();
                }
                else
                {
                    allNames = loaded;
                }
                filterUpdate = 0f;
            }
        }

        protected override void OnAnyPageEnabled()
        {
            filter = string.Empty;
        }
    }
}