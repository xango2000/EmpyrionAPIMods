using System;
using System.Collections.Generic;
using System.Linq;
using Eleon.Modding;
using ProtoBuf;


namespace XangosBackpackModule
{

    public class XangosBackpack : ModInterface
    {
        //public int triggerPlayer = 0;
        //public bool noExchangeData = false;
        //public int i;
        //public string chatCommand;
        //public int chatSpeaker;
        public string step;
        //public List<int> PlysList = new List<int>();
        public IDictionary<int, ItemStack[]> vBackpackDictionary = new Dictionary<int, ItemStack[]>(){};
        //public IDictionary<int, String> ChatDictionary = new Dictionary<int, String>() { };
        //public Dictionary<string, FactionInfo> FactionInfoDict = new Dictionary<string, FactionInfo>(){};
        //private FactionInfoList FactionListDump;
        public ItemStack[] EmptyExchange = new ItemStack[0];
        //public ItemStack[] bagdata;
        public Dictionary<int, string> BackpackChatDictionary = new Dictionary<int, string>() { };
        //public Dictionary<int, string> IDDictionary = new Dictionary<int, string>() { };


        ModGameAPI GameAPI;

        private void Messenger(String msgType, int Priority, int player, String msg, int Duration)
        {
            if (msgType == "ChatAsServer")
            {
                string command = "SAY '" + msg + "'";
                GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_InGameMessage_AllPlayers, new Eleon.Modding.PString(command));
            }
            if (msgType == "Alert")
                GameAPI.Game_Request(CmdId.Request_InGameMessage_AllPlayers, (ushort)CmdId.Request_InGameMessage_AllPlayers, new IdMsgPrio(player, msg, Convert.ToByte(Priority), Duration));
        }


        private void LogFile(String FileName, String FileData)
        {
            if (!System.IO.File.Exists("Content\\Mods\\xango\\"+ FileName))
            {
                System.IO.File.Create("Content\\Mods\\xango\\" + FileName);
            }
            //string FileName2 = "Content\\Mods\\xango\\" + FileName;
            string FileData2 = FileData + Environment.NewLine;
            System.IO.File.AppendAllText("Content\\Mods\\Xango\\"+FileName, FileData2);
        }

        private void buildItemStack(int playerId, string job)
        {
            string[] bagLines = System.IO.File.ReadAllLines("Content\\Mods\\Xango\\players\\EID" + Convert.ToString(playerId) + "\\VirtualBackpack.txt");
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

        public void Game_Start(ModGameAPI gameAPI)
        {

            LogFile("chat.txt", "Mod Loaded");
            GameAPI = gameAPI;
            //GameAPI.Game_Request(CmdId.Request_Get_Factions, (ushort)CmdId.Request_Get_Factions, new Id(1));
           
        }
        public void Game_Event(CmdId cmdId, ushort seqNr, object data)
        {
            try
            {
                /*
                if (data is ItemStack[])
                {
                    bagdata = (ItemStack[])data;
                }
                */
                //int i = 0;
                //int triggerPlayer = 0;
                switch (cmdId)
                {

                    case CmdId.Event_Player_Connected:
                        Id pc = (Id)data;

                        try { string[] bagLines = System.IO.File.ReadAllLines("Content\\Mods\\Xango\\players\\EID" + pc.id + "\\VirtualBackpack.txt");
                            /*
                            int itemStackSize = 0;
                            if (bagLines.Count() == 0)
                            {
                                itemStackSize = bagLines.Count();
                            }
                            else
                            {
                                itemStackSize = bagLines.Count()-1;
                            }
                            */
                            int itemStackSize = bagLines.Count();
                            ItemStack[] itStack = new ItemStack[itemStackSize];
                            for (int i = 0; i < itemStackSize; ++i)
                            {
                                string[] bagLinesSplit = bagLines[i].Split(',');
                                itStack[i] = new ItemStack(Convert.ToInt32(bagLinesSplit[1]), Convert.ToInt32(bagLinesSplit[2])); //1=ItemNumber, 2=StackSize
                                itStack[i].slotIdx = Convert.ToByte(bagLinesSplit[0]);//0=SlotNumber
                                itStack[i].ammo = Convert.ToInt32(bagLinesSplit[3]);//3=Ammo
                                itStack[i].decay = Convert.ToInt32(bagLinesSplit[4]);//4=Decay
                            }
                            vBackpackDictionary.Add(pc.id, itStack);
                        }
                        catch { };
                        LogFile("chat.txt", "Player " + pc.id + " Connected");
                        try { System.IO.Directory.CreateDirectory("Content\\Mods\\Xango\\players\\EID" + pc.id); }
                        catch { };
                        break;
                    case CmdId.Event_Player_Disconnected:
                        Id pd = (Id)data;
                        LogFile("chat.txt", "Player " + pd.id + " DisConnected");
                        vBackpackDictionary.Remove(pd.id);
                        break;
                    case CmdId.Event_Player_List:
                        /*
                        IdList PlayerIDs = (IdList)data;
                        foreach (var player in PlayerIDs.list)
                        {
                            //PlysList.Add(Convert.ToInt32(player));
                            GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)CmdId.Request_Player_Info, new Id(player));
                        }
                        break;
                        */
                    case CmdId.Event_Player_Info:
                        PlayerInfo PlayerInfoReceived = (PlayerInfo)data;
                        /*
                        if (PlysList.Contains(Convert.ToInt32(PlayerInfoReceived.entityId)))
                        {
                            PlysList.Remove(Convert.ToInt32(PlayerInfoReceived.entityId));
                            var factionabbr = Convert.ToString(PlayerInfoReceived.factionId);
                            if (PlayerInfoReceived.factionId == PlayerInfoReceived.entityId)
                            {
                                Messenger("ChatAsServer", 0, chatSpeaker, PlayerInfoReceived.entityId + "  [   ]  " + PlayerInfoReceived.playerName + "  PF=" + PlayerInfoReceived.playfield, 0);
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
                                    Messenger("ChatAsServer", 0, chatSpeaker, PlayerInfoReceived.entityId + "  [" + factionabbr + "]  " + PlayerInfoReceived.playerName + "  PF=" + PlayerInfoReceived.playfield, 0);
                                    LogFile("plys.txt", PlayerInfoReceived.entityId + "  [" + PlayerInfoReceived.factionId + "]  " + PlayerInfoReceived.playerName + "  PF=" + PlayerInfoReceived.playfield);
                                    break;
                                }
                            }
                        }
                        */
                        /*
                        BackpackChatDictionary[PlayerInfoReceived.entityId].StartsWith("/backpack");
                        if (chatSpeaker == PlayerInfoReceived.entityId)
                        {
                            if (chatCommand.StartsWith("/backpack"))
                            {
                            */
                        if (BackpackChatDictionary.ContainsKey(PlayerInfoReceived.entityId))
                        {
                            BackpackChatDictionary.Remove(PlayerInfoReceived.entityId);
                            if (vBackpackDictionary.ContainsKey(PlayerInfoReceived.entityId))
                            {
                                BackpackChatDictionary.Remove(PlayerInfoReceived.entityId);
                                LogFile("Chat.txt", "show backpack");
                                GameAPI.Game_Request(CmdId.Request_Player_ItemExchange, (ushort)CmdId.Request_Player_ItemExchange, new ItemExchangeInfo(PlayerInfoReceived.entityId, "Virtual Backpack", "Extra Inventory Space, Yay!", "Save", vBackpackDictionary[PlayerInfoReceived.entityId]));
                                step = "Request ItemExchange";
                            }
                            else
                            {
                                if (System.IO.File.Exists("Content\\Mods\\Xango\\players\\EID" + Convert.ToString(PlayerInfoReceived.entityId) + "\\VirtualBackpack.txt"))
                                {
                                    LogFile("Chat.txt", "Build Blank Backpack");
                                    System.IO.File.Create("Content\\Mods\\Xango\\players\\EID" + Convert.ToString(PlayerInfoReceived.entityId) + "\\VirtualBackpack.txt");
                                    buildItemStack(PlayerInfoReceived.entityId, "blank");
                                }
                                else
                                {
                                    LogFile("Chat.txt", "Show Blank Backpack");
                                    GameAPI.Game_Request(CmdId.Request_Player_ItemExchange, (ushort)CmdId.Request_Player_ItemExchange, new ItemExchangeInfo(PlayerInfoReceived.entityId, "Virtual Backpack", "Extra Inventory Space, Yay!", "Save", EmptyExchange));
                                    step = "Request ItemExchange";

                                }
                            }
                        }
                        /*
                            }
                            else if (chatCommand.StartsWith("/id"))
                            {
                                LogFile("plys.txt", "/id");
                                        LogFile("plys.txt", "Plys List Requested");
                                        chatCommand = "";
                                        GameAPI.Game_Request(CmdId.Request_Get_Factions, (ushort)CmdId.Request_Get_Factions, new Id(1));
                                        GameAPI.Game_Request(CmdId.Request_Player_List, (ushort)CmdId.Request_Player_List, null);
                            };
                        }
                        */
                        /*
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
                        */
                        /*
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
                        */
                        break;
                    case CmdId.Event_Player_Inventory:
                        
                    case CmdId.Event_Player_ItemExchange:
                        ItemExchangeInfo exchangeInfo = (ItemExchangeInfo)data;
                        vBackpackDictionary[exchangeInfo.id] = exchangeInfo.items;
                        //Rewrite: Cant delete and write this quickly
                        System.IO.File.WriteAllText("Content\\Mods\\Xango\\players\\EID" + exchangeInfo.id + "\\VirtualBackpack.txt", string.Empty);
                        //System.IO.File.Delete("Content\\Mods\\Xango\\players\\EID" +exchangeInfo.id + "\\VirtualBackpack.txt");
                        for (int i = 0; i <= exchangeInfo.items.Count(); i++)
                        {
                            //LogFile("ExchangeData.txt", Convert.ToString(exchangeInfo.items[i].id));
                            LogFile("players\\EID" + exchangeInfo.id + "\\VirtualBackpack.txt", Convert.ToString(exchangeInfo.items[i].slotIdx) + "," + Convert.ToString(exchangeInfo.items[i].id) + "," + Convert.ToString(exchangeInfo.items[i].count) + "," + Convert.ToString(exchangeInfo.items[i].ammo) + "," + Convert.ToString(exchangeInfo.items[i].decay));
                        }
                        step = "itemExchange complete";


                        break;
                    case CmdId.Event_Player_Credits:
                    case CmdId.Event_Player_ChangedPlayfield:
                    case CmdId.Event_Player_GetAndRemoveInventory:
                    case CmdId.Event_Playfield_List:
                    case CmdId.Event_Playfield_Stats:
                    case CmdId.Event_Playfield_Loaded:
                    case CmdId.Event_Playfield_Unloaded:
                    case CmdId.Event_Playfield_Entity_List:
                    case CmdId.Event_Dedi_Stats:
                    case CmdId.Event_GlobalStructure_List:
                    case CmdId.Event_Entity_PosAndRot:
                    case CmdId.Event_Faction_Changed:
                        FactionChangeInfo factionChange = (FactionChangeInfo)data;
                        GameAPI.Game_Request(CmdId.Request_Get_Factions, (ushort)CmdId.Request_Get_Factions, new Id(1));
                        break;
                    case CmdId.Event_Get_Factions:
                        FactionInfoList factioninfo = (FactionInfoList)data;
                        //FactionListDump = factioninfo;
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
                            ci.msg = ci.msg.Remove(0,3);
                        }
                        //LogFile("Chat.txt", ci.playerId + " s!  strip check?:" + ci.msg);
                        ci.msg = ci.msg.ToLower();

                        if (ci.msg.StartsWith("/backpack"))
                        {
                            BackpackChatDictionary[ci.playerId] = ci.msg;
                            //chatCommand = ci.msg;
                            //chatSpeaker = ci.playerId;
                            GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)CmdId.Request_Player_Info, new Eleon.Modding.Id(ci.playerId));
                            step = "Request Playerinfo";
                        }
                        /*
                        else if (ci.msg.StartsWith("/id"))
                        {
                            IDDictionary[ci.playerId] = ci.msg;
                            //chatCommand = ci.msg;
                            //chatSpeaker = ci.playerId;
                            GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)CmdId.Request_Player_Info, new Eleon.Modding.Id(ci.playerId));
                            step = "Request Playerinfo";
                        };
                        */
                        break;
                    case CmdId.Event_Structure_BlockStatistics:
                    case CmdId.Event_AlliancesAll:
                    case CmdId.Event_AlliancesFaction:
                    case CmdId.Event_BannedPlayers:
                    case CmdId.Event_TraderNPCItemSold:
                    case CmdId.Event_Ok:
                        /*
                        LogFile("CurrentProject.txt", "Event OK: " + step);
                        break;
                        */
                    case CmdId.Event_Error:
                        ErrorInfo err = (ErrorInfo)data;
                        ErrorType err2 = (ErrorType)data;
                        //try { Messenger("Alert", 0, 0, "Error Info " + Convert.ToString(err), 1000); } catch { };
                        //try { Messenger("Alert", 0, 0, "Error Type " + Convert.ToString(err2), 1000); } catch { };
                        
                        LogFile("ERROR.txt", Convert.ToString(err2) + ": " + Convert.ToString(err));
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

