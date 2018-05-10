using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using static CitizenFX.Core.UI.Screen;

namespace FivemPlayerlist
{
    public class FivemPlayerlist : BaseScript
    {
        private int maxClients = -1;
        private bool ScaleSetup = false;
        private int currentPage = 0;
        Scaleform scale;
        public FivemPlayerlist()
        {
            TriggerServerEvent("fs:getMaxPlayers");
            Tick += ShowScoreboard;
            Tick += DisplayController;
            Tick += BackupTimer;
            EventHandlers.Add("fs:setMaxPlayers", new Action<int>(SetMaxPlayers));
        }
        private int maxPages = (int)Math.Ceiling((double)new PlayerList().Count() / 16.0);


        private async Task BackupTimer()
        {
            var timer = GetGameTimer();
            var oldPage = currentPage;
            while (GetGameTimer() - timer < 8000 && currentPage > 0 && currentPage == oldPage)
            {
                await Delay(0);
            }
            if (oldPage == currentPage)
            {
                currentPage = 0;
            }
        }

        private void UpdateMaxPages()
        {
            maxPages = (int)Math.Ceiling((double)new PlayerList().Count() / 16.0);
        }

        private async Task DisplayController()
        {
            if (Game.IsControlJustPressed(0, Control.MultiplayerInfo))
            {

                UpdateMaxPages();
                if (ScaleSetup)
                {
                    currentPage++;
                    if (currentPage > maxPages)
                    {
                        currentPage = 0;
                    }
                    await LoadScale();
                    var timer = GetGameTimer();
                    bool nextPage = false;
                    while (GetGameTimer() - timer < 8000)
                    {
                        await Delay(1);
                        if (Game.IsControlJustPressed(0, Control.MultiplayerInfo))
                        {
                            nextPage = true;
                            break;
                        }
                    }
                    if (nextPage)
                    {
                        UpdateMaxPages();
                        if (currentPage < maxPages)
                        {
                            currentPage++;
                            await LoadScale();
                        }
                        else
                        {
                            currentPage = 0;
                        }
                    }
                    else
                    {
                        currentPage = 0;
                    }
                }
            }

        }

        private void SetMaxPlayers(int count)
        {
            maxClients = count;
        }

        private async Task ShowScoreboard()
        {
            if (maxClients != -1)
            {
                if (!ScaleSetup)
                {
                    await LoadScale();
                    ScaleSetup = true;
                }
                if (currentPage > 0)
                {
                    float safezone = GetSafeZoneSize();
                    float change = (safezone - 0.89f) / 0.11f;
                    float x = 50f;
                    x -= change * 78f;
                    float y = 50f;
                    y -= change * 50f;

                    var width = 400f;
                    var height = 490f;
                    if (scale != null)
                    {
                        if (scale.IsLoaded)
                        {
                            scale.Render2DScreenSpace(new System.Drawing.PointF(x, y), new System.Drawing.PointF(width, height));
                        }
                    }
                }
            }
        }

        private async Task LoadScale()
        {
            if (scale != null)
            {
                for (var i = 0; i < maxClients * 2; i++)
                {
                    scale.CallFunction("SET_DATA_SLOT_EMPTY", i);
                }
                scale.Dispose();
            }
            scale = null;
            while (!HasScaleformMovieLoaded(RequestScaleformMovie("MP_MM_CARD_FREEMODE")))
            {
                await Delay(0);
            }
            scale = new Scaleform("MP_MM_CARD_FREEMODE");
            var titleIcon = "2";
            var titleLeftText = "FiveM";
            var titleRightText = $"Players {NetworkGetNumConnectedPlayers()}/{maxClients}";
            scale.CallFunction("SET_TITLE", titleLeftText, titleRightText, titleIcon);
            await UpdateScale();
            scale.CallFunction("DISPLAY_VIEW");
        }

        struct PlayerRow
        {
            public int serverId;
            public string name;
            public string rightText;
            public int color;
            public string iconOverlayText;
            public string jobPointsText;
            public string crewLabelText;
            public enum DisplayType
            {
                NUMBER_ONLY = 0,
                ICON = 1,
                NONE = 2
            };
            public DisplayType jobPointsDisplayType;
            public enum RightIconType
            {
                NONE = 0,
                INACTIVE_HEADSET = 48,
                MUTED_HEADSET = 49,
                ACTIVE_HEADSET = 47,
                RANK_FREEMODE = 65,
                KICK = 64,
                LOBBY_DRIVER = 79,
                LOBBY_CODRIVER = 80,
                SPECTATOR = 66,
                BOUNTY = 115,
                DEAD = 116,
                DPAD_GANG_CEO = 121,
                DPAD_GANG_BIKER = 122,
                DPAD_DOWN_TARGET = 123
            };
            public int rightIcon;
            public string textureString;
            public char friendType;
        }

        private async Task<string> GetHeadshotImage(int ped)
        {
            var headshotHandle = RegisterPedheadshot(ped);
            while (!IsPedheadshotReady(headshotHandle))
            {
                await Delay(0);
            }
            return GetPedheadshotTxdString(headshotHandle) ?? "";
        }

        private async Task UpdateScale()
        {
            List<PlayerRow> rows = new List<PlayerRow>();

            for (var x = 0; x < 150; x++) // cleaning up in case of a reload, this frees up all ped headshot handles :)
            {
                UnregisterPedheadshot(x);
            }

            var amount = 0;
            foreach (Player p in new PlayerList())
            {
                if (IsRowSupposedToShow(amount))
                {
                    PlayerRow row = new PlayerRow()
                    {
                        color = 111,
                        crewLabelText = "",
                        friendType = ' ',
                        iconOverlayText = "",
                        jobPointsDisplayType = PlayerRow.DisplayType.NONE,
                        jobPointsText = "",
                        name = p.Name.Replace("<", "").Replace(">", "").Replace("^", "").Replace("~", "").Trim(),
                        rightIcon = (int)PlayerRow.RightIconType.RANK_FREEMODE,
                        rightText = $"{GetPlayerServerId(p.Handle)}",
                        serverId = GetPlayerServerId(p.Handle),
                    };
                    row.textureString = await GetHeadshotImage(GetPlayerPed(p.Handle));
                    rows.Add(row);
                }
                amount++;
            }

            rows.Sort((row1, row2) => row1.serverId.CompareTo(row2.serverId));
            for (var i = 0; i < maxClients * 2; i++)
            {
                scale.CallFunction("SET_DATA_SLOT_EMPTY", i);
            }
            var index = 0;
            foreach (PlayerRow row in rows)
            {
                if (row.crewLabelText != "")
                {
                    scale.CallFunction("SET_DATA_SLOT", index, row.rightText, row.name, row.color, row.rightIcon, row.iconOverlayText, row.jobPointsText,
                        $"..+{row.crewLabelText}", (int)row.jobPointsDisplayType, row.textureString, row.textureString, row.friendType);
                }
                else
                {
                    scale.CallFunction("SET_DATA_SLOT", index, row.rightText, row.name, row.color, row.rightIcon, row.iconOverlayText, row.jobPointsText,
                        "", (int)row.jobPointsDisplayType, row.textureString, row.textureString, row.friendType);
                }
                index++;
            }
        }

        private bool IsRowSupposedToShow(int row)
        {
            if (currentPage > 0)
            {
                var max = currentPage * 16;
                var min = (currentPage * 16) - 16;
                if (row >= min && row < max)
                {
                    return true;
                }
                return false;
            }
            return false;
        }
    }
}
