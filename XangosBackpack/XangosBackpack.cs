using System;
using System.Collections.Generic;
using System.Linq;
using Eleon.Modding;
using ProtoBuf;

namespace XangosBackpackModule
{
    public class XangosBackpack : ModInterface
    {
        public string step;
        public IDictionary<int, ItemStack[]> vBackpackDictionary = new Dictionary<int, ItemStack[]>(){};
        public ItemStack[] EmptyExchange = new ItemStack[0];
        public Dictionary<int, string> BackpackChatDictionary = new Dictionary<int, string>() { };

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
        }
        public void Game_Event(CmdId cmdId, ushort seqNr, object data)
        {
            try
            {
                switch (cmdId)
                {
                    case CmdId.Event_Player_Connected:
                        Id pc = (Id)data;
                        try { string[] bagLines = System.IO.File.ReadAllLines("Content\\Mods\\Xango\\players\\EID" + pc.id + "\\VirtualBackpack.txt");
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
                    case CmdId.Event_Player_Info:
                        PlayerInfo PlayerInfoReceived = (PlayerInfo)data;
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
                        break;
                    case CmdId.Event_Player_Inventory:
                    case CmdId.Event_Player_ItemExchange:
                        ItemExchangeInfo exchangeInfo = (ItemExchangeInfo)data;
                        vBackpackDictionary[exchangeInfo.id] = exchangeInfo.items;
                        System.IO.File.WriteAllText("Content\\Mods\\Xango\\players\\EID" + exchangeInfo.id + "\\VirtualBackpack.txt", string.Empty);
                        for (int i = 0; i <= exchangeInfo.items.Count(); i++)
                        {
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
                        if (ci.msg.StartsWith("s! "))
                        {
                            ci.msg = ci.msg.Remove(0,3);
                        }
                        ci.msg = ci.msg.ToLower();
                        if (ci.msg.StartsWith("/backpack"))
                        {
                            BackpackChatDictionary[ci.playerId] = ci.msg;
                            GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)CmdId.Request_Player_Info, new Eleon.Modding.Id(ci.playerId));
                            step = "Request Playerinfo";
                        }
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