using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading;
using Eleon.Modding;
using ProtoBuf;


namespace XangosEmpyrionAPITool
{

    public class MyEmpyrionMod : ModInterface
    {
        public int triggerPlayer = 0;
        public bool noExchangeData = false;
//        public int i;
        public string chatCommand;
        public int chatSpeaker;
        public string step;

        //public List<int> logonlist = new List<int>();
        public List<int> PlysList = new List<int>();
        public List<int> kickList = new List<int>();
        

        public IDictionary<int, ItemStack[]> exchangeDictionary = new Dictionary<int, ItemStack[]>(){}; //bag5
        public IDictionary<int, ItemStack[]> bagDictionary = new Dictionary<int, ItemStack[]>() { };
        public IDictionary<int, ItemStack[]> toolbarDictionary = new Dictionary<int, ItemStack[]>() { };
        //public IDictionary<int, ItemStack[]> builtDictionary = new Dictionary<int, ItemStack[]>() { };
        //public IDictionary<int, int> LogonStep = new Dictionary<int, int>() { };
        public IDictionary<int, String> ChatDictionary = new Dictionary<int, String>() { };
        public Dictionary<string, FactionInfo> FactionInfoDict = new Dictionary<string, FactionInfo>(){};
        public Dictionary<string, DediStats> DediStatsDict = new Dictionary<string, DediStats>() { };
        public Dictionary<int, string> banDict = new Dictionary<int, string>() { };


        private FactionInfoList FactionListDump;
        //private ItemStack[] reLoadSpiritBag = new ItemStack[35];
        public ItemStack[] EmptyExchange = new ItemStack[0];
        public ItemStack[] bagdata;
/*        private struct FactionsData
        {
            public byte origin;
            public int factionId;
            public string name;
            public string abbrev;
        };
        */

        ModGameAPI GameAPI;

        private void Messenger(String msgType, int Priority, int player, String msg, int Duration)
        {
            if (msgType == "Server")
            {
                string command = "SAY '" + msg + "'";
                GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_InGameMessage_AllPlayers, new Eleon.Modding.PString(command));
            }
            if (msgType == "Alert")
                GameAPI.Game_Request(CmdId.Request_InGameMessage_AllPlayers, (ushort)CmdId.Request_InGameMessage_AllPlayers, new IdMsgPrio(player, msg, Convert.ToByte(Priority), Duration));
        }


        private void LogFile(String FileName, String FileData)
        {
            //string FileName2 = "Content\\Mods\\XangosEmpyrionAPITool\\" + FileName;
            string FileData2 = FileData + Environment.NewLine;
            System.IO.File.AppendAllText("Content\\Mods\\XangosEmpyrionAPITool\\"+FileName, FileData2);
        }

        private void buildItemStack(int playerId, string job)
        {
            string[] bagLines = System.IO.File.ReadAllLines("Content\\Mods\\XangosEmpyrionAPITool\\players\\spiritbag.txt");
            int itemStackSize = bagLines.Count();
            ItemStack[] itStack = new ItemStack[itemStackSize];
            for (int i = 0; i < itemStackSize; ++i)
            {
                itStack[i] = new ItemStack(Convert.ToInt32(bagLines[i][1]), Convert.ToInt32(bagLines[i][2]));
                itStack[i].slotIdx = Convert.ToByte(bagLines[i][0]);
                itStack[i].ammo = Convert.ToInt32(bagLines[i][3]);
                itStack[i].decay = Convert.ToInt32(bagLines[i][4]);

            }
        }
        private void spiritbag(int playerId, string job)
        {
            Dictionary<string, int> SpiritDictionary = new Dictionary<string, int>();

            string[] bagLines = System.IO.File.ReadAllLines("Content\\Mods\\XangosEmpyrionAPITool\\players\\EID" + Convert.ToString(playerId) + "spiritbag.txt");
//            reLoadSpiritBag.RemoveRange(0, 27);
            for (int i = 1; i <= 5; i++)
            {
                string[] line = bagLines[i].Split(Convert.ToChar(","));
                if (line[i] == "270"){ SpiritDictionary.Add("heal", 1); }
                if (line[i] == "1132") { SpiritDictionary.Add("smelt", 1); }
                if (line[i] == "721") { SpiritDictionary.Add("o2", 1); }
                if (line[i] == "2056") { SpiritDictionary.Add("drill", 1); }
                if (line[i] == "2067") { SpiritDictionary.Add("drill2", 1); }
                if (line[i] == "535") { SpiritDictionary.Add("toolbar2", 1); }
                if (line[i] == "2428") { SpiritDictionary.Add("stamina", 1); }

                if (line[i] == "2389") { SpiritDictionary.Add("heal1available", Int32.Parse(line[2])); }
                if (line[i] == "2355") { SpiritDictionary.Add("heal2available", Int32.Parse(line[2])); }
                if (line[i] == "2356") { SpiritDictionary.Add("heal3available", Int32.Parse(line[2])); }
                if (line[i] == "2128") { SpiritDictionary.Add("o2available", Int32.Parse(line[2])); }
                if (line[i] == "2111") { SpiritDictionary.Add("drillavailable", Int32.Parse(line[2])); }
                if (line[i] == "2105") { SpiritDictionary.Add("drill2available", Int32.Parse(line[2])); }
                if (line[i] == "2428") { SpiritDictionary.Add("staminaavailable", Int32.Parse(line[2])); }

            };
            if (job == "heal")
            {
                if (SpiritDictionary.ContainsKey("heal"))
                {
                    if(SpiritDictionary.ContainsKey("heal1available"))
                    {
                        if (SpiritDictionary["heal1available"] > 1)
                        {
                            SpiritDictionary.Remove("heal1available");
                            GameAPI.Game_Request(CmdId.Request_Player_SetPlayerInfo, (ushort)CmdId.Request_Player_SetPlayerInfo, new PlayerInfoSet()
                            {
                                health = +10,
                            });
                        }
                    }
                }
            }



        }

        public void Game_Start(ModGameAPI gameAPI)
        {
            LogFile("log.txt", "Mod Loaded");
            GameAPI = gameAPI;
            //GameAPI.Console_Write("XangosEmpyrionAPITool Loaded");
            GameAPI.Game_Request(CmdId.Request_Get_Factions, (ushort)CmdId.Request_Get_Factions, new Id(1));
            
        }
        public void Game_Event(CmdId cmdId, ushort seqNr, object data)
        {
            try
            {

                if (data is ItemStack[])
                {
                    bagdata = (ItemStack[])data;
                }
                //int i = 0;
                //int triggerPlayer = 0;
                switch (cmdId)
                {

                    case CmdId.Event_Player_Connected:
                        Id pc = (Id)data;

                        try { string[] bagLines = System.IO.File.ReadAllLines("Content\\Mods\\XangosEmpyrionAPITool\\players\\EID" + pc.id + "\\spiritbag.txt"); 
                        LogFile("chat.txt", "Player " + pc.id + " Connected 1");
                        int itemStackSize = bagLines.Count();
                        ItemStack[] itStack = new ItemStack[itemStackSize];
                        LogFile("chat.txt", "Player " + pc.id + " Connected 2");
                        //List<int> bagLines1 = new List<int> { };
                        for (int i = 0; i < itemStackSize; ++i)
                        {
                            string[] bagLinesSplit = bagLines[i].Split(',');//.ToList();
                            itStack[i] = new ItemStack(Convert.ToInt32(bagLinesSplit[1]), Convert.ToInt32(bagLinesSplit[2]));
                            itStack[i].slotIdx = Convert.ToByte(bagLinesSplit[0]);
                            itStack[i].ammo = Convert.ToInt32(bagLinesSplit[3]);
                            itStack[i].decay = Convert.ToInt32(bagLinesSplit[4]);
                        }


                            exchangeDictionary.Add(pc.id, itStack);
                        }
                        catch { };
                        LogFile("chat.txt", "Player " + pc.id + " Connected 3");
                        System.IO.Directory.CreateDirectory("Content\\Mods\\XangosEmpyrionAPITool\\players\\EID" + pc.id);
                        GameAPI.Console_Write("Player " + pc.id + " is now Online");

                        break;
                    case CmdId.Event_Player_Disconnected:
                        Id pd = (Id)data;
                        LogFile("chat.txt", "Player " + pd.id + " DisConnected");
                        GameAPI.Console_Write("Player " + pd.id + " is now Offline");
                        break;
                    case CmdId.Event_Player_List:
                        IdList PlayerIDs = (IdList)data;
                        foreach (var player in PlayerIDs.list)
                        {
                            //Messenger("Server", 0, chatSpeaker, Convert.ToString(player), 0);
                            PlysList.Add(Convert.ToInt32(player));
                            GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)CmdId.Request_Player_Info, new Id(player));
                        }
                        break;
                    case CmdId.Event_Player_Info:
                        PlayerInfo PlayerInfoReceived = (PlayerInfo)data;

                        if (banDict.ContainsKey(PlayerInfoReceived.entityId))
                        {
                            foreach (KeyValuePair<int, string> banThis in banDict)
                            {
                                Messenger("Server", 0, chatSpeaker, "Banning " + PlayerInfoReceived.steamId + " for " + banDict[PlayerInfoReceived.entityId]  , 0);
                                GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new PString("ban " + PlayerInfoReceived.steamId + " " + banDict[PlayerInfoReceived.entityId] ));
                                banDict.Remove(Convert.ToInt32(PlayerInfoReceived.entityId));

                            }
                        }

                        if (kickList.Contains(Convert.ToInt32(PlayerInfoReceived.entityId)))
                        {
                            Messenger("Server", 0, chatSpeaker, "Kicking " + PlayerInfoReceived.steamId, 0);
                            GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new PString("kick " + PlayerInfoReceived.steamId +" 'You Have Been Kicked'"));
                            kickList.Remove(Convert.ToInt32(PlayerInfoReceived.entityId));
                        }


                        if (PlysList.Contains(Convert.ToInt32(PlayerInfoReceived.entityId)))
                        {
                            PlysList.Remove(Convert.ToInt32(PlayerInfoReceived.entityId));
                            var factionabbr = Convert.ToString(PlayerInfoReceived.factionId);
                            if (PlayerInfoReceived.factionId == PlayerInfoReceived.entityId)
                            {
                                Messenger("Server", 0, chatSpeaker, PlayerInfoReceived.entityId + "  [   ]  " + PlayerInfoReceived.playerName + "  PF=" + PlayerInfoReceived.playfield, 0);
                                LogFile("plys.txt", PlayerInfoReceived.entityId + "  [   ]  " + PlayerInfoReceived.playerName + "  PF=" + PlayerInfoReceived.playfield);
                            }
                            else
                            {
                                foreach (FactionInfo faction in FactionListDump.factions)
                                {
                                    if (faction.factionId == PlayerInfoReceived.factionId)
                                    {
                                        factionabbr = faction.abbrev;
                                    }
                                    Messenger("Server", 0, chatSpeaker, PlayerInfoReceived.entityId + "  [" + factionabbr + "]  " + PlayerInfoReceived.playerName + "  PF=" + PlayerInfoReceived.playfield, 0);
                                    LogFile("plys.txt", PlayerInfoReceived.entityId + "  [" + PlayerInfoReceived.factionId + "]  " + PlayerInfoReceived.playerName + "  PF=" + PlayerInfoReceived.playfield);
                                    break;
                                }
                            }
                        }


                        if (chatSpeaker == PlayerInfoReceived.entityId)
                        {
                            if (chatCommand.StartsWith("/spirit"))
                            {
                                if (chatCommand.Contains("heal"))
                                {
                                    spiritbag(chatSpeaker, "heal");
                                };
                                if (chatCommand.Contains("o2"))
                                {
                                    spiritbag(chatSpeaker, "o2");
                                };
                                if (chatCommand.Contains("stamina"))
                                {
                                    spiritbag(chatSpeaker, "stamina");
                                };
                                if (chatCommand.Contains("trade"))
                                {
                                    LogFile("Chat.txt", "If /spirit trade");

                                    if (!exchangeDictionary.ContainsKey(chatSpeaker))
                                    {
                                        if (System.IO.File.Exists("Content\\Mods\\XangosEmpyrionAPITool\\players\\EID" + Convert.ToString(chatSpeaker) + "\\spiritbag.txt"))
                                        {
                                            LogFile("Chat.txt", "Build Blank spiritbag");
                                            buildItemStack(PlayerInfoReceived.entityId, "blank");
                                        }
                                        else
                                        {
                                            LogFile("Chat.txt", "Show Blank spiritbag");
                                            GameAPI.Game_Request(CmdId.Request_Player_ItemExchange, (ushort)CmdId.Request_Player_ItemExchange, new ItemExchangeInfo(chatSpeaker, "Spectral Spirit", "Spirit performs chores for it's companion", "Save", EmptyExchange));
                                            step = "Request ItemExchange";
                                        }
                                    }
                                    else
                                    {
                                        LogFile("Chat.txt", "show spiritbag");
                                        GameAPI.Game_Request(CmdId.Request_Player_ItemExchange, (ushort)CmdId.Request_Player_ItemExchange, new ItemExchangeInfo(chatSpeaker, "Spectral Spirit", "Spirit performs chores for it's companion", "Save", exchangeDictionary[PlayerInfoReceived.entityId]));
                                        step = "Request ItemExchange";
                                    }

                                }
                            }
                            else if (chatCommand.StartsWith("/admin"))
                            {
                                LogFile("plys.txt", "/admin received");
                                if (PlayerInfoReceived.permission == 0)
                                {

                                }
                                else
                                {
                                    if (chatCommand.Contains("plys"))
                                    {
                                        LogFile("plys.txt", "Plys List Requested");
                                        chatCommand = "";
                                        GameAPI.Game_Request(CmdId.Request_Get_Factions, (ushort)CmdId.Request_Get_Factions, new Id(1));
                                        GameAPI.Game_Request(CmdId.Request_Player_List, (ushort)CmdId.Request_Player_List, null);
                                    }
                                    else if (chatCommand.Contains("kick"))
                                    {
                                        string[] parameters = chatCommand.Split(Convert.ToChar(" "));
                                        chatCommand = "";
                                        try
                                        {
                                            kickList.Add(Convert.ToInt32(parameters[2]));
                                            //Messenger("Server", 0, chatSpeaker, "KickTest 3" + parameters[2], 0);
                                            GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)CmdId.Request_Player_Info, new Id(Convert.ToInt32(parameters[2])));
                                        }
                                        catch { };
                                    }
                                    else if (chatCommand.Contains("ban"))
                                    {
                                        try
                                        {
                                            string[] parameters = chatCommand.Split(Convert.ToChar(" "));
                                            chatCommand = "";
                                            banDict.Add(Convert.ToInt32(parameters[2]), Convert.ToString(parameters[3]));
                                            GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)CmdId.Request_Player_Info, new Id(Convert.ToInt32(parameters[2])));
                                        }
                                        catch { };
                                    }
                                }
                            };
                        }
                        LogFile("players\\EID" + PlayerInfoReceived.entityId+ "\\PlayerInfo.txt", "playerName= " + Convert.ToString(PlayerInfoReceived.playerName));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "entityId= " + Convert.ToString(PlayerInfoReceived.entityId));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "permission= " + Convert.ToString(PlayerInfoReceived.permission));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "playfield= " + Convert.ToString(PlayerInfoReceived.playfield));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "startPlayfield= " + Convert.ToString(PlayerInfoReceived.startPlayfield));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "pos= " + Convert.ToString(PlayerInfoReceived.pos.x) + ",  " + Convert.ToString(PlayerInfoReceived.pos.y) + ",  " + Convert.ToString(PlayerInfoReceived.pos.z));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "rot= " + Convert.ToString(PlayerInfoReceived.rot.x) + ",  " + Convert.ToString(PlayerInfoReceived.rot.y) + ",  " + Convert.ToString(PlayerInfoReceived.rot.z));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "clientId= " + Convert.ToString(PlayerInfoReceived.clientId));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "SteamId= " + Convert.ToString(PlayerInfoReceived.steamId));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "SteamOwnerId= " + Convert.ToString(PlayerInfoReceived.steamOwnerId));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "origin= " + Convert.ToString(PlayerInfoReceived.origin));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "ping= " + Convert.ToString(PlayerInfoReceived.ping));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "factionGroup= " + Convert.ToString(PlayerInfoReceived.factionGroup));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "factionRole= " + Convert.ToString(PlayerInfoReceived.factionRole));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "factionId= " + Convert.ToString(PlayerInfoReceived.factionId ));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "credits= " + Convert.ToString(PlayerInfoReceived.credits));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "bodyTemp= " + Convert.ToString(PlayerInfoReceived.bodyTemp));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "TempMax= " + Convert.ToString(PlayerInfoReceived.bodyTempMax));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "food= " + Convert.ToString(PlayerInfoReceived.food));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "foodMax= " + Convert.ToString(PlayerInfoReceived.foodMax));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "health= " + Convert.ToString(PlayerInfoReceived.health));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "healthmax= " + Convert.ToString(PlayerInfoReceived.healthMax));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "oxygen= " + Convert.ToString(PlayerInfoReceived.oxygen));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "oxygenMax= " + Convert.ToString(PlayerInfoReceived.oxygenMax));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "radiation= " + Convert.ToString(PlayerInfoReceived.radiation));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "radiationMax= " + Convert.ToString(PlayerInfoReceived.radiationMax));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "stamina= " + Convert.ToString(PlayerInfoReceived.stamina));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "staminaMax= " + Convert.ToString(PlayerInfoReceived.staminaMax));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "exp= " + Convert.ToString(PlayerInfoReceived.exp));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "upgrade= " + Convert.ToString(PlayerInfoReceived.upgrade));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "died= " + Convert.ToString(PlayerInfoReceived.died));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "kills= " + Convert.ToString(PlayerInfoReceived.kills));

                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "producedPrefabs= " + Convert.ToString(PlayerInfoReceived.producedPrefabs));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "bpInFactory= " + Convert.ToString(PlayerInfoReceived.bpInFactory));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "bpRemainingTime= " + Convert.ToString(PlayerInfoReceived.bpRemainingTime));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "bpResourcesInFactory= " + Convert.ToString(PlayerInfoReceived.bpResourcesInFactory));

                        try
                        {

                            int bagdatacounter = 0;
                            //for (int bagdatacounter = 0; bagdatacounter <= 35; bagdatacounter++)
                            while (bagdatacounter < PlayerInfoReceived.bag.Count())
                            {
                                LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\BagData.txt", Convert.ToString(PlayerInfoReceived.bag[bagdatacounter].id + ",  " + Convert.ToString(PlayerInfoReceived.bag[bagdatacounter].count) + ",  " + Convert.ToString(PlayerInfoReceived.bag[bagdatacounter].slotIdx) + ",  " + Convert.ToString(PlayerInfoReceived.bag[bagdatacounter].ammo) + ",  " + Convert.ToString(PlayerInfoReceived.bag[bagdatacounter].decay)));
                                bagdatacounter++;
                            }
                        }
                        catch { };

                        try
                        {
                            for (int i = 0; i <= PlayerInfoReceived.toolbar.Count(); i++)
                            {
                                LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\ToolbarData.txt", Convert.ToString(PlayerInfoReceived.toolbar[i].id + ",  " + Convert.ToString(PlayerInfoReceived.toolbar[i].count) + ",  " + Convert.ToString(PlayerInfoReceived.toolbar[i].slotIdx) + ",  " + Convert.ToString(PlayerInfoReceived.toolbar[i].ammo) + ",  " + Convert.ToString(PlayerInfoReceived.toolbar[i].decay)));
                            }
                        }
                        catch { };

                        break;
                    case CmdId.Event_Player_Inventory:
                        
                    case CmdId.Event_Player_ItemExchange:
                        ItemExchangeInfo exchangeInfo = (ItemExchangeInfo)data;
                        //LogFile("ExchangeData.txt", Convert.ToString(exchangeInfo.id) + ",  " + Convert.ToString(exchangeInfo.items));

                        //Messenger("Server", 1, 0, "ExchangeInfo.id " + Convert.ToString(exchangeInfo.id), 1000);
                        exchangeDictionary[exchangeInfo.id] = exchangeInfo.items;
                        //LogFile("ExchangeData.txt", "exchangeDictionary saved");
                        System.IO.File.Delete("Content\\Mods\\XangosEmpyrionAPITool\\players\\EID" +exchangeInfo.id + "\\spiritbag.txt");
                        for (int i = 0; i <= exchangeInfo.items.Count(); i++)
                        {
                            LogFile("ExchangeData.txt", Convert.ToString(exchangeInfo.items[i].id));
                            LogFile("players\\EID" + exchangeInfo.id + "\\spiritbag.txt", Convert.ToString(exchangeInfo.items[i].slotIdx) + "," + Convert.ToString(exchangeInfo.items[i].id) + "," + Convert.ToString(exchangeInfo.items[i].count) + "," + Convert.ToString(exchangeInfo.items[i].ammo) + "," + Convert.ToString(exchangeInfo.items[i].decay));
                        }
                        step = "itemExchange complete";
                        //LogFile("ExchangeData.txt", step);


                        break;
                    case CmdId.Event_Player_Credits:
                    case CmdId.Event_Player_ChangedPlayfield:
                    case CmdId.Event_Player_GetAndRemoveInventory:
                        Inventory playerInventory = (Inventory)data;
                        break;
                    case CmdId.Event_Playfield_List:
                    case CmdId.Event_Playfield_Stats:
                    case CmdId.Event_Playfield_Loaded:
                    case CmdId.Event_Playfield_Unloaded:
                    case CmdId.Event_Playfield_Entity_List:
                    case CmdId.Event_Dedi_Stats:
                        DediStats dedistats = (DediStats)data;
                        DediStatsDict.Add("DediStats", dedistats);
                        break;
                    case CmdId.Event_GlobalStructure_List:
                        /*
                        GlobalStructureList structureList = (GlobalStructureList)data;
                        for (int i = 0; i < structureList.globalStructures.Count(); i++ )
                        {
                            if (structureList.globalStructures["playfield"] == "factionId")
                            {
                                "do this stuff";
                            }
                        }
                        break;
                        */
                    case CmdId.Event_Entity_PosAndRot:
                    case CmdId.Event_Faction_Changed:
                        FactionChangeInfo factionChange = (FactionChangeInfo)data;
                        GameAPI.Game_Request(CmdId.Request_Get_Factions, (ushort)CmdId.Request_Get_Factions, new Id(1));
                        break;
                    case CmdId.Event_Get_Factions:
                        FactionInfoList factioninfo = (FactionInfoList)data;
                        FactionListDump = factioninfo;
                        //FactionInfoDict.Remove("FactionDump");
                        //FactionInfoDict.Add("FactionDump", factioninfo);
                        break;
                    case CmdId.Event_Statistics:
                    case CmdId.Event_NewEntityId:
                    case CmdId.Event_Player_DisconnectedWaiting:
                        Id pdw = new Id();
                        GameAPI.Console_Write("Player " + pdw.id + " Failed Login Attempt");
                        break;
                    case CmdId.Event_ChatMessage:
                        ChatInfo ci = (ChatInfo)data;
                        LogFile("Chat.txt", ci.playerId + " SAYS: " + ci.msg);
                        //GameAPI.Game_Request(CmdId.Request_Player_List, (ushort)CmdId.Request_Player_List, null);
                        if (ci.msg.StartsWith("s! "))
                        {
                            ci.msg = ci.msg.Substring(3, ci.msg.Length);
                        }

                        if (ci.msg.StartsWith("/spirit"))
                        {
                            chatCommand = ci.msg;
                            chatSpeaker = ci.playerId;
                            GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)CmdId.Request_Player_Info, new Eleon.Modding.Id(ci.playerId));
                            step = "Request Playerinfo";
                        }
                        else if (ci.msg.StartsWith("/admin"))
                        {
                            chatCommand = ci.msg;
                            chatSpeaker = ci.playerId;
                            //GameAPI.Game_Request(CmdId.Request_Get_Factions, (ushort)CmdId.Request_Get_Factions, new Id(1));
                            GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)CmdId.Request_Player_Info, new Eleon.Modding.Id(ci.playerId));
                            step = "Request Playerinfo";
                        };

                        //Messenger("Alert", 2, 0, "Data Sent", 1000);
                        break;
                    case CmdId.Event_Structure_BlockStatistics:
                        IdStructureBlockInfo BlockStatistics = (IdStructureBlockInfo)data;
                        break;
                    case CmdId.Event_AlliancesAll:
                    case CmdId.Event_AlliancesFaction:
                    case CmdId.Event_BannedPlayers:
                    case CmdId.Event_TraderNPCItemSold:
                    case CmdId.Event_Ok:
                        LogFile("CurrentProject.txt", "Event OK: " + step);


                        break;
                    case CmdId.Event_Error:
                        ErrorInfo err = (ErrorInfo)data;
                        ErrorType err2 = (ErrorType)data;
                        try { Messenger("Alert", 0, 0, "Error Info " + Convert.ToString(err), 1000); } catch { };
                        try { Messenger("Alert", 0, 0, "Error Type " + Convert.ToString(err2), 1000); } catch { };
                        //Messenger("Alert", 0, 0, "Error", 1000);

                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                GameAPI.Console_Write(ex.Message);
            }

        }
        public void Game_Update()
        {
        }
        public void Game_Exit()
        {
        }

    }
}

