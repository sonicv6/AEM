using System;
using Anarchy.Configuration;
using Anarchy.UI.Animation;
using System.Diagnostics;
using System.Linq;
using Anarchy;
using Anarchy.Commands.Chat;
using Anarchy.UI;
using UnityEngine;
using static Anarchy.UI.GUI;
using Object = UnityEngine.Object;

namespace Anarchy.UI
{
    public class PlayersPanel : GUIPanel

    {
        private SmartRect left;
        private Rect pagePosition;
        private SmartRect rect;
        private SmartRect right;
        private Vector2 playerScroll = Vector2.zero;
        private SmartRect playersRect;
        private Rect playersView;
        private Rect playersArea;
        private PhotonPlayer selectedPlayer;
        private string tpCoords = string.Empty;
        
        public PlayersPanel() : base(nameof(PlayersPanel), GUILayers.SettingsPanel)
        {
        }

        protected override void DrawMainPart()
        {
            //Player selector
            left.Reset();
            LabelCenter(left, "Select Player");
            left.MoveY();
            playersArea = new Rect(left.x, 0f, left.width, Style.Height * 10 + Style.VerticalMargin * (10 - 1));
            playersView = new Rect(0f, 0f, left.width, Style.Height * PhotonNetwork.playerList.Length + (Style.VerticalMargin * (PhotonNetwork.playerList.Length - 1)));
            playersArea.y = left.y;
            playersRect.Reset();
            playerScroll = BeginScrollView(playersArea, playerScroll, playersView);
            {
                for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
                {
                    if (Button(playersRect, PhotonNetwork.playerList[i].UIName.ToHTMLFormat(), true))
                    {
                        selectedPlayer = PhotonNetwork.playerList[i];
                    }
                }
            }
            EndScrollView();

            //Controls
            right.Reset();
            LabelCenter(right, selectedPlayer.UIName.ToHTMLFormat(), true);
            selectedPlayer.RCIgnored = ToggleButton(right, selectedPlayer.RCIgnored, "Ignored", true);
            selectedPlayer.Wagoneer = ToggleButton(right, selectedPlayer.Wagoneer, "Wagoneer", true);
            if (Button(right, "Kick", true))
            {
                if (!selectedPlayer.IsLocal)
                {
                    Network.Antis.Kick(selectedPlayer, false, string.Empty);
                }
            }
            if (Button(right, "Ban", true))
            {
                if (!selectedPlayer.IsLocal)
                {
                    Network.Antis.Kick(selectedPlayer, true, string.Empty);
                }
            }

            right.MoveY();
            tpCoords = TextField(right, tpCoords, "Coords", Style.LabelOffset, true);
            if (Button(right, "TP", true))
            {
                string[] tpCoordsSplit = tpCoords.Split(' ');
                selectedPlayer.GameObject.GetComponent<HERO>().BasePV.RPC("moveToRPC", selectedPlayer, new object[]
                {
                    float.Parse(tpCoordsSplit[0]),
                    float.Parse(tpCoordsSplit[1]),
                    float.Parse(tpCoordsSplit[2])
                });
            }
            SmartRect rect = new SmartRect(0, 0, 144, Style.Height);
            rect.MoveToEndY(WindowPosition, Style.Height);
            rect.MoveToEndX(WindowPosition, 144);
            if (Button(rect, "Close"))
            {
                Disable();
            }
        }

        protected override void OnPanelDisable()
        {
            UIManager.HUDScaleGUI.Value = (float)System.Math.Round(UIManager.HUDScaleGUI.Value, 2);
            head = null;
            left = null;
            rect = null;
            right = null;
            if (Application.loadedLevelName == "menu")
            {
                AnarchyManager.MainMenu.EnableImmediate();
            }
        }

        protected override void OnPanelEnable()
        {
            selectedPlayer = PhotonNetwork.playerList[0];
            rect = Helper.GetSmartRects(WindowPosition, 1)[0];
            head = "Players";
            pagePosition = new Rect(WindowPosition.x, WindowPosition.y + ((rect.height + Style.VerticalMargin) * 2f), WindowPosition.width, WindowPosition.height - (rect.y + rect.height + Style.VerticalMargin) - Style.WindowBottomOffset - Style.WindowTopOffset);
            SmartRect[] rects = Helper.GetSmartRects(pagePosition, 2);
            left = rects[0];
            right = rects[1];
            playersRect = new SmartRect(0f, 0f, left.width, Style.Height, 0f, Style.VerticalMargin);
        }
        public override void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Disable();
                return;
            }
        }
    }
}
