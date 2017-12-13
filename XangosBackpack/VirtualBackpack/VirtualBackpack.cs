using System;
using System.Collections.Generic;
using System.Linq;
using Eleon.Modding;
using ProtoBuf;


namespace VirtualBackpack
{
    public class VirtualBackpack : ModInterface
    {
        private IDictionary<int, ItemStack[]> vBackpackDictionary = new Dictionary<int, ItemStack[]>(){};
        private ItemStack[] EmptyExchange = new ItemStack[0];
        private Dictionary<int, string> BackpackChatDictionary = new Dictionary<int, string>() { };
        private Dictionary<int, string> ItemExchangeSwitch = new Dictionary<int, string>() { };
        private Dictionary<string, Players> PlayerDictionary = new Dictionary<string, Players> { };
        #pragma warning disable 0649
        public static List<Int32> LogonList = new List<Int32>();
        #pragma warning restore 0649

        ModGameAPI GameAPI;
        /*
        private void Messenger(String msgType, int Priority, int player, String msg, int Duration)
        {
            if (msgType == "ChatAsServer")
            {
                string command = "SAY '" + msg + "'";
                GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_InGameMessage_AllPlayers, new Eleon.Modding.PString(command));
            }
            if (msgType == "Alert")
                GameAPI.Game_Request(CmdId.Request_InGameMessage_AllPlayers, (ushort)CmdId.Request_InGameMessage_AllPlayers, new IdMsgPrio(player, msg, Convert.ToByte(Priority), Duration));
        }*/


        private void LogFile(String FileName, String FileData)
        {
            if (!System.IO.File.Exists("Content\\Mods\\VirtualBackpack\\"+ FileName))
            {
                System.IO.File.Create("Content\\Mods\\VirtualBackpack\\" + FileName);
            }
            string FileData2 = FileData + Environment.NewLine;
            System.IO.File.AppendAllText("Content\\Mods\\VirtualBackpack\\" + FileName, FileData2);
        }

        public ItemStack[] buildItemStack(string filename)
        {
            string[] bagLines = System.IO.File.ReadAllLines(filename);
            int itemStackSize = bagLines.Count();
            ItemStack[] itStack = new ItemStack[itemStackSize];
            for (int i = 0; i < itemStackSize; ++i)
            {
                string[] bagLinesSplit = bagLines[i].Split(',');
                itStack[i] = new ItemStack(Convert.ToInt32(bagLinesSplit[1]), Convert.ToInt32(bagLinesSplit[2]));
                itStack[i].slotIdx = Convert.ToByte(bagLinesSplit[0]);
                itStack[i].ammo = Convert.ToInt32(bagLinesSplit[3]);
                itStack[i].decay = Convert.ToInt32(bagLinesSplit[4]);
            }
            return itStack;
        }

        public struct Players
        {
            public string SteamID;
            public int Admin;
            public int EmpyrionID;
            public int FactionID;
            public string PlayerName;
            public string Playfield;
            public float x;
            public float y;
            public float z;
            public int ClientID;
        }
        public class PlayerData
        {
            public static Players playerData(string steamID, int admin, int empyrionID, int factionID, string playername, string playfield, float coordX, float coordY, float coordZ, int clientID)
            {
                Players NewPlayer = new Players();
                NewPlayer.SteamID = steamID;
                NewPlayer.Admin = admin;
                NewPlayer.EmpyrionID = empyrionID;
                NewPlayer.FactionID = factionID;
                NewPlayer.PlayerName = playername;
                NewPlayer.Playfield = playfield;
                NewPlayer.x = coordX;
                NewPlayer.y = coordY;
                NewPlayer.z = coordZ;
                NewPlayer.ClientID = clientID;
                return NewPlayer;
            }
        }

        public void Game_Start(ModGameAPI gameAPI)
        {
            LogFile("log.txt", "Mod Loaded");
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
                        LogonList.Add(pc.id);
                        GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)1179, new Id(pc.id));
                        break;
                    case CmdId.Event_Player_Disconnected:
                        Id pd = (Id)data;
                        LogFile("log.txt", "Player " + pd.id + " DisConnected");
                        vBackpackDictionary.Remove(pd.id);
                        break;
                    case CmdId.Event_Player_Info:
                        PlayerInfo PlayerInfoReceived = (PlayerInfo)data;
                        PlayerDictionary[PlayerInfoReceived.steamId] = PlayerData.playerData(PlayerInfoReceived.steamId, PlayerInfoReceived.permission, PlayerInfoReceived.entityId, PlayerInfoReceived.factionId, PlayerInfoReceived.playerName, PlayerInfoReceived.playfield, Convert.ToInt32(PlayerInfoReceived.pos.x), Convert.ToInt32(PlayerInfoReceived.pos.y), Convert.ToInt32(PlayerInfoReceived.pos.z), PlayerInfoReceived.clientId);
                        if (seqNr == 1179)
                        {
                            if (LogonList.Contains(PlayerInfoReceived.entityId))
                            {
                                LogonList.Remove(PlayerInfoReceived.entityId);
                                if (vBackpackDictionary.ContainsKey(PlayerInfoReceived.entityId))
                                { }
                                else
                                {
                                    try { System.IO.Directory.CreateDirectory("Content\\Mods\\VirtualBackpack\\players"); } catch { };
                                    try { System.IO.Directory.CreateDirectory("Content\\Mods\\VirtualBackpack\\players\\SID" + PlayerInfoReceived.steamId); } catch { };
                                    try { System.IO.Directory.CreateDirectory("Content\\Mods\\VirtualBackpack\\players\\EID" + PlayerInfoReceived.entityId); } catch { };
                                    try
                                    {
                                        if (System.IO.File.Exists("Content\\Mods\\VirtualBackpack\\players\\SID" + PlayerInfoReceived.steamId + "\\VirtualBackpack.txt"))
                                        {
                                            ItemStack[] itStack = buildItemStack("Content\\Mods\\VirtualBackpack\\players\\SID" + PlayerInfoReceived.steamId + "\\VirtualBackpack.txt");
                                            vBackpackDictionary.Add(PlayerInfoReceived.entityId, itStack);
                                        }
                                        else
                                        {
                                            try { System.IO.File.Create("Content\\Mods\\VirtualBackpack\\players\\SID" + PlayerInfoReceived.steamId + "\\VirtualBackpack.txt"); } catch { };
                                            ItemStack[] itStack = buildItemStack("Content\\Mods\\VirtualBackpack\\players\\EID" + PlayerInfoReceived.entityId + "\\VirtualBackpack.txt");
                                            vBackpackDictionary.Add(PlayerInfoReceived.entityId, itStack);
                                        }
                                    }
                                    catch { System.IO.File.Create("Content\\Mods\\VirtualBackpack\\players\\SID" + PlayerInfoReceived.steamId + "\\VirtualBackpack.txt"); }; //New player, create backpack... i think this will work
                                    LogFile("log.txt", "Player " + PlayerInfoReceived.entityId + " Connected");
                                }
                            }
                        }
                        if (seqNr == 1178)
                        {
                            if (ItemExchangeSwitch.ContainsKey(PlayerInfoReceived.entityId))
                            {
                                if (ItemExchangeSwitch[PlayerInfoReceived.entityId].StartsWith("/xbp"))
                                {

                                    //LogFile("log.txt", seqNr + " Triggered");
                                    if (vBackpackDictionary.ContainsKey(PlayerInfoReceived.entityId))
                                    {
                                        GameAPI.Game_Request(CmdId.Request_Player_ItemExchange, (ushort)CmdId.Request_Player_ItemExchange, new ItemExchangeInfo(PlayerInfoReceived.entityId, "Virtual Backpack", "Extra Inventory Space, Yay!", "Save", vBackpackDictionary[PlayerInfoReceived.entityId]));
                                    }
                                    else
                                    {
                                        if (System.IO.File.Exists("Content\\Mods\\VirtualBackpack\\players\\EID" + Convert.ToString(PlayerInfoReceived.entityId) + "\\VirtualBackpack.txt") == true)
                                        {
                                            GameAPI.Game_Request(CmdId.Request_Player_ItemExchange, (ushort)CmdId.Request_Player_ItemExchange, new ItemExchangeInfo(PlayerInfoReceived.entityId, "Virtual Backpack", "Extra Inventory Space, Yay!", "Save", EmptyExchange));
                                        }
                                        else
                                        {
                                            System.IO.File.Create("Content\\Mods\\VirtualBackpack\\players\\EID" + Convert.ToString(PlayerInfoReceived.entityId) + "\\VirtualBackpack.txt");
                                            EmptyExchange = buildItemStack("Content\\Mods\\VirtualBackpack\\players\\EID" + Convert.ToString(PlayerInfoReceived.entityId) + "\\VirtualBackpack.txt");
                                            GameAPI.Game_Request(CmdId.Request_Player_ItemExchange, (ushort)CmdId.Request_Player_ItemExchange, new ItemExchangeInfo(PlayerInfoReceived.entityId, "Virtual Backpack", "Extra Inventory Space, Yay!", "Save", EmptyExchange));
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case CmdId.Event_Player_ItemExchange:
                        ItemExchangeInfo exchangeInfo = (ItemExchangeInfo)data;
                        if (ItemExchangeSwitch.ContainsKey(exchangeInfo.id)) 
                        {
                            var Message = ItemExchangeSwitch[exchangeInfo.id].Split(new[] { ' ' }, 3);

                            if (Message[0].StartsWith("/xbp"))
                            {
                                ItemExchangeSwitch[exchangeInfo.id] = "blank";
                                string speakerSteam = "";
                                foreach (string SteamID in PlayerDictionary.Keys)
                                {
                                    if (PlayerDictionary[SteamID].EmpyrionID == exchangeInfo.id)
                                    {
                                        speakerSteam = SteamID;
                                    }
                                }


                                vBackpackDictionary[exchangeInfo.id] = exchangeInfo.items;
                                System.IO.File.WriteAllText("Content\\Mods\\VirtualBackpack\\players\\EID" + exchangeInfo.id + "\\VirtualBackpack.txt", string.Empty); //Old Way
                                System.IO.File.WriteAllText("Content\\Mods\\VirtualBackpack\\players\\SID" + speakerSteam + "\\VirtualBackpack.txt", string.Empty);
                                for (int i = 0; i <= exchangeInfo.items.Count(); i++)
                                {
                                    LogFile("players\\EID" + exchangeInfo.id + "\\VirtualBackpack.txt", Convert.ToString(exchangeInfo.items[i].slotIdx) + "," + Convert.ToString(exchangeInfo.items[i].id) + "," + Convert.ToString(exchangeInfo.items[i].count) + "," + Convert.ToString(exchangeInfo.items[i].ammo) + "," + Convert.ToString(exchangeInfo.items[i].decay));
                                    LogFile("players\\SID" + speakerSteam + "\\VirtualBackpack.txt", Convert.ToString(exchangeInfo.items[i].slotIdx) + "," + Convert.ToString(exchangeInfo.items[i].id) + "," + Convert.ToString(exchangeInfo.items[i].count) + "," + Convert.ToString(exchangeInfo.items[i].ammo) + "," + Convert.ToString(exchangeInfo.items[i].decay));

                                }
                            }
                        }
                        break;
                    case CmdId.Event_Player_DisconnectedWaiting:
                        Id pdw = new Id();
                        LogFile("ERROR.txt", "Failed Login: " + pdw.id);
                        break;
                    case CmdId.Event_ChatMessage:
                        ChatInfo ci = (ChatInfo)data;
                        if (ci.msg.StartsWith("!MODS"))
                        {
                            string command = "SAY p:" + ci.playerId + " '" + "!MODS: Wipe-Safe Virtual Backpack v 1.0 by Xango2000" + "'";
                            GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new Eleon.Modding.PString(command));
                            LogFile("log.txt", "System.IO.Directory.GetCurrentDirectory()");
                        }
                        if (ci.msg.StartsWith("s! "))
                        {
                            ci.msg = ci.msg.Remove(0, 3);
                        }
                        var chatmsg = ci.msg.Split(' ');
                        chatmsg[0] = chatmsg[0].ToLower();
                        string cimsg = string.Join(" ", chatmsg);
                        if (cimsg.StartsWith("/xbp"))
                        {
                            ItemExchangeSwitch[ci.playerId] = cimsg;
                            GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)1178, new Eleon.Modding.Id(ci.playerId));
                            LogFile("log.txt", "Type:" + ci.type + " RecipientID:" + ci.recipientEntityId + " FactionRecipientID:" + ci.recipientFactionId + " " + ci.playerId + " SAYS: " + ci.msg);
                        }
                        break;
                    case CmdId.Event_Error:
                        ErrorInfo err = (ErrorInfo)data;
                        ErrorType err2 = (ErrorType)data;
                        LogFile("ERROR.txt", Convert.ToString(err2) + ": " + Convert.ToString(err.errorType) + " " + Convert.ToString(err));
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LogFile("ERROR.txt", "Message: " + ex.Message);
                LogFile("ERROR.txt", "Data: " + ex.Data);
                LogFile("ERROR.txt", "HelpLink: " + ex.HelpLink);
                LogFile("ERROR.txt", "InnerException: " + ex.InnerException);
                LogFile("ERROR.txt", "Source: " + ex.Source);
                LogFile("ERROR.txt", "StackTrace: " + ex.StackTrace);
                LogFile("ERROR.txt", "TargetSite: " + ex.TargetSite);
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