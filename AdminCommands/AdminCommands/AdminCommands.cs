using System;
using System.Collections.Generic;
using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using Eleon.Modding;
using ProtoBuf;
//using Tracker;

namespace AdminCommands
{
    public class AdminCommands : ModInterface
    {
        ModGameAPI GameAPI;
        public Dictionary<string, Players> PlayerDictionary = new Dictionary<string, Players> { };
        ushort SeqNrCounter = 1500;
        Dictionary<ushort, TrackData> SeqNrDict = new Dictionary<ushort, TrackData> { };
        ushort NewSeqNr = 1500;
        List<Int32> isOnline = new List<Int32> { };

        private void Alert(int Priority, int player, String msg, float Duration)
        {
            //Priority 0 = Red
            //Priority 1 = Yellow
            //Priority 2 = Blue
            GameAPI.Game_Request(CmdId.Request_InGameMessage_AllPlayers, (ushort)CmdId.Request_InGameMessage_AllPlayers, new IdMsgPrio(player, msg, Convert.ToByte(Priority), Duration));
        }

        private void ServerSay(int player, String msg)
        {
            if (player == 0)
            {
                string command = "SAY '" + msg + "'";
                GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new Eleon.Modding.PString(command));
            }
            else
            {
                string command = "SAY p:" + player + " '" + msg + "'";
                GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new Eleon.Modding.PString(command));
            }
        }

        private void ConsoleCommand(String command)
        {
            GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new Eleon.Modding.PString(command));
        }

        private void LogFile(String FileName, String FileData)
        {
            if (!System.IO.File.Exists("Content\\Mods\\AdminCommands\\" + FileName))
            {
                System.IO.File.Create("Content\\Mods\\AdminCommands\\" + FileName);
            }
            string FileData2 = FileData + Environment.NewLine;
            System.IO.File.AppendAllText("Content\\Mods\\AdminCommands\\" + FileName, FileData2);
        }

        private List<string> NameFragment(string findThis)
        {
            List<string> ExactMatches = new List<string> { };
            List<string> NearMatches = new List<string> { };
                foreach (string SteamID in PlayerDictionary.Keys)
                {
                    if (Convert.ToString(PlayerDictionary[SteamID].EmpyrionID) == findThis)
                    {
                        ExactMatches.Add(SteamID);
                    }
                    else if (PlayerDictionary[SteamID].PlayerName == findThis)
                    {
                        ExactMatches.Add(SteamID);
                    }
                    else if (PlayerDictionary[SteamID].PlayerName.Contains(findThis))
                    {
                        NearMatches.Add(SteamID);
                    }
                    else if (PlayerDictionary[SteamID].PlayerName.ToLower().Contains(findThis.ToLower()))
                    {
                        NearMatches.Add(SteamID);
                    }

                }
                if (ExactMatches.Count == 1)
                {
                    return ExactMatches;
                }
                else //if (ExactMatches.Count > 0)
                {
                    foreach (string item in NearMatches)
                    {
                        ExactMatches.Add(item);
                    }
                    return ExactMatches;
                }
            //}
            //else
            //{
                //return ExactMatches;
            //}
        }

        public ItemStack[] buildItemStack(string job)
        {
            string[] bagLines = System.IO.File.ReadAllLines(job);
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

        public ushort GetNewSeqNr()
        {
            if (SeqNrCounter == (ushort)35000)
            {
                SeqNrCounter = (ushort)1500;
            }
            else
            {
                SeqNrCounter = (ushort)(SeqNrCounter+1);
            }
            return SeqNrCounter;
        }
            public struct TrackData
            {
                public ushort seqnr;
                public object anything;
                public Eleon.Modding.CmdId requestID;
            }
        public class Request
        {
            public static TrackData SeqNrTracker(Eleon.Modding.CmdId RequestID, ushort seqNr, object Anything)
            {
                TrackData NewData = new TrackData();
                NewData.seqnr = seqNr;
                NewData.anything = Anything;
                NewData.requestID = RequestID;
                return NewData;
            }
        }

        public void Game_Start(ModGameAPI gameAPI)
        {
            GameAPI = gameAPI;
            LogFile("log.txt", "Admin Commands Loaded");
        }

        public void Game_Event(CmdId cmdId, ushort seqNr, object data)
        {
            LogFile("log.txt", "Game Event Running. SeqNr is " + NewSeqNr + " " + SeqNrCounter);
            try
            {
                switch (cmdId)
                {
                    case CmdId.Event_ChatMessage:
                        LogFile("log.txt", "Chat received");
                        ChatInfo chatdata = (ChatInfo)data;
                        if (chatdata.msg.ToLower().StartsWith("!mods"))
                        {
                            string command = "SAY '" + "AdminCommands v0.1 by Xango2000" + "'";
                            //string command = "SAY p:" + chatdata.playerId + " '" + "!MODS: Wipe-Safe Virtual Backpack v 1.0 by Xango2000" + "'";
                            GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new Eleon.Modding.PString(command));
                            LogFile("log.txt", "System.IO.Directory.GetCurrentDirectory()");
                        }
                        if (chatdata.msg.StartsWith("s! "))
                        {
                            chatdata.msg = chatdata.msg.Remove(0, 3);
                        }
                        string[] chatmsg = chatdata.msg.Split(' ');
                        chatmsg[0] = chatmsg[0].ToLower();
                        string cdmsg = string.Join(" ", chatmsg);
                        LogFile("log.txt", "cdmsg=" + cdmsg);
                        if (cdmsg.StartsWith("/find"))
                        {
                            LogFile("log.txt", "/find in chat message confirmed");
                            NewSeqNr = GetNewSeqNr();
                            GameAPI.Game_Request(CmdId.Request_Player_Info, NewSeqNr, new Id(chatdata.playerId));
                            SeqNrDict[NewSeqNr] = Request.SeqNrTracker(CmdId.Request_Player_Info, NewSeqNr, chatdata);
                        }
                        else if (cdmsg.StartsWith("/kick"))
                        {
                            NewSeqNr = GetNewSeqNr();
                            GameAPI.Game_Request(CmdId.Request_Player_Info, NewSeqNr, new Id(chatdata.playerId));
                            SeqNrDict[NewSeqNr] = Request.SeqNrTracker(CmdId.Request_Player_Info, NewSeqNr, chatdata);
                        }
                        else if (cdmsg.StartsWith("/ban"))
                        {
                            NewSeqNr = GetNewSeqNr();
                            GameAPI.Game_Request(CmdId.Request_Player_Info, NewSeqNr, new Id(chatdata.playerId));
                            SeqNrDict[NewSeqNr] = Request.SeqNrTracker(CmdId.Request_Player_Info, NewSeqNr, chatdata);
                        }
                        else if (cdmsg.StartsWith("/say"))
                        {
                            NewSeqNr = GetNewSeqNr();
                            GameAPI.Game_Request(CmdId.Request_Player_Info, NewSeqNr, new Id(chatdata.playerId));
                            SeqNrDict[NewSeqNr] = Request.SeqNrTracker(CmdId.Request_Player_Info, NewSeqNr, chatdata);
                        }
                        else if (cdmsg.StartsWith("/alert"))
                        {
                            NewSeqNr = GetNewSeqNr();
                            GameAPI.Game_Request(CmdId.Request_Player_Info, NewSeqNr, new Id(chatdata.playerId));
                            SeqNrDict[NewSeqNr] = Request.SeqNrTracker(CmdId.Request_Player_Info, NewSeqNr, chatdata);
                        }
                        else if (cdmsg.StartsWith("/tt"))
                        {
                            NewSeqNr = GetNewSeqNr();
                            GameAPI.Game_Request(CmdId.Request_Player_Info, NewSeqNr, new Id(chatdata.playerId));
                            SeqNrDict[NewSeqNr] = Request.SeqNrTracker(CmdId.Request_Player_Info, NewSeqNr, chatdata);
                        }
                        else if (cdmsg.StartsWith("/summon"))
                        {
                            NewSeqNr = GetNewSeqNr();
                            GameAPI.Game_Request(CmdId.Request_Player_Info, NewSeqNr, new Id(chatdata.playerId));
                            SeqNrDict[NewSeqNr] = Request.SeqNrTracker(CmdId.Request_Player_Info, NewSeqNr, chatdata);
                        }
                        else if (cdmsg.StartsWith("/flip"))
                        {
                            NewSeqNr = GetNewSeqNr();
                            GameAPI.Game_Request(CmdId.Request_Player_Info, NewSeqNr, new Id(chatdata.playerId));
                            SeqNrDict[NewSeqNr] = Request.SeqNrTracker(CmdId.Request_Player_Info, NewSeqNr, chatdata);
                        }
                        break;
                    case CmdId.Event_Faction_Changed://Automatic
                        break;
                    case CmdId.Event_GameEvent://Automatic
                        break;
                    case CmdId.Event_PdaStateChange://Automatic
                        break;
                    case CmdId.Event_Player_ChangedPlayfield://Automatic
                        IdPlayfield PlayerChangePF = (IdPlayfield)data;
                        NewSeqNr = GetNewSeqNr();
                        GameAPI.Game_Request(CmdId.Request_Player_Info, NewSeqNr, new Id(PlayerChangePF.id));
                        SeqNrDict[NewSeqNr] = Request.SeqNrTracker(CmdId.Request_Player_Info, NewSeqNr, PlayerChangePF);
                        break;
                    case CmdId.Event_Player_Connected://Automatic
                        Id PlayerConnected = (Id)data;
                        //ushort NewSeqNr = GetNewSeqNr();
                        NewSeqNr = GetNewSeqNr();
                        GameAPI.Game_Request(CmdId.Request_Player_Info, NewSeqNr, new Id(PlayerConnected.id));
                        SeqNrDict[NewSeqNr] = Request.SeqNrTracker(CmdId.Request_Player_Info, NewSeqNr, PlayerConnected);
                        isOnline.Add(PlayerConnected.id);
                        break;
                    case CmdId.Event_Player_Disconnected://Automatic
                        Id PlayerDisconnected = (Id)data;
                        isOnline.Remove(PlayerDisconnected.id);
                        break;
                    case CmdId.Event_Player_DisconnectedWaiting://Automatic
                        break;
                    case CmdId.Event_Playfield_Loaded://Automatic
                        break;
                    case CmdId.Event_Playfield_Unloaded://Automatic
                        break;
                    case CmdId.Event_Statistics://Automatic
                        break;
                    case CmdId.Event_TraderNPCItemSold://Automatic
                        break;
                    case CmdId.Event_AlliancesAll: //Triggered
                        break;
                    case CmdId.Event_AlliancesFaction://Triggered
                        break;
                    case CmdId.Event_BannedPlayers://Triggered
                        break;
                    case CmdId.Event_ConsoleCommand://Triggered
                        break;
                    case CmdId.Event_Dedi_Stats://Triggered
                        break;
                    case CmdId.Event_Entity_PosAndRot://Triggered
                        break;
                    case CmdId.Event_Get_Factions://Triggered
                        break;
                    case CmdId.Event_GlobalStructure_List://Triggered
                        break;
                    case CmdId.Event_NewEntityId://Triggered
                        break;
                    case CmdId.Event_Player_Credits://Triggered
                        break;
                    case CmdId.Event_Player_GetAndRemoveInventory://Triggered
                        break;
                    case CmdId.Event_Player_Info://Triggered
                        LogFile("log.txt", "Player Info Triggered");
                        GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new Eleon.Modding.PString("say '" + "player info triggered" + "'"));
                        PlayerInfo PlayerInfoReceived = (PlayerInfo)data;
                        PlayerDictionary[PlayerInfoReceived.steamId] = PlayerData.playerData(PlayerInfoReceived.steamId, PlayerInfoReceived.permission, PlayerInfoReceived.entityId, PlayerInfoReceived.factionId, PlayerInfoReceived.playerName, PlayerInfoReceived.playfield, Convert.ToInt32(PlayerInfoReceived.pos.x), Convert.ToInt32(PlayerInfoReceived.pos.y), Convert.ToInt32(PlayerInfoReceived.pos.z), PlayerInfoReceived.clientId);

                        if (SeqNrDict[seqNr].anything is Eleon.Modding.Id)
                        {
                            Id IdObject = (Id)SeqNrDict[seqNr].anything;
                            SeqNrDict.Remove(seqNr);
                            //IdObject.id
                        }
                        else if (SeqNrDict[seqNr].anything is Eleon.Modding.ChatInfo)
                        {
                            GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new Eleon.Modding.PString("say '" + "correct type received" + "'"));
                            ChatInfo chatData = (ChatInfo)SeqNrDict[seqNr].anything;
                            if (chatData.msg.StartsWith("/find"))
                            {
                                string[] findThis = chatData.msg.Split(new[] { ' ' }, 2);
                                List<string> Target = NameFragment(findThis[1]);
                                if (Target.Count == 0) //Error
                                {
                                    string nomsg = "No Players Found";
                                    GameAPI.Game_Request(CmdId.Request_ShowDialog_SinglePlayer, (ushort)CmdId.Request_ShowDialog_SinglePlayer, new Eleon.Modding.IdMsgPrio(PlayerInfoReceived.entityId, nomsg, 0, 10));
                                }
                                else //Actual player's Empyrion ID
                                {
                                    foreach (string SID in PlayerDictionary.Keys)
                                    {
                                        if (Target.Contains(SID))
                                        {
                                            Players SpecificTarget = PlayerDictionary[SID];
                                            string msg = "[" + SpecificTarget.FactionID + "]" + SpecificTarget.PlayerName + " @" + SpecificTarget.Playfield + " " + SpecificTarget.x + "," + SpecificTarget.y + "," + SpecificTarget.z + " #" + SpecificTarget.EmpyrionID;
                                            GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new Eleon.Modding.PString("SAY p:" + PlayerInfoReceived.entityId + " '" + msg + "'"));
                                        }
                                        else
                                        {
                                            //GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new Eleon.Modding.PString("SAY p:" + PlayerInfoReceived.entityId + " 'Target.Contains=False'"));
                                        };
                                    }
                                }
                                SeqNrDict.Remove(seqNr);
                            }
                            else if (chatData.msg.StartsWith("/kick"))
                            {
                                GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new Eleon.Modding.PString("say '" + "kick received" + "'"));
                                string[] findThis = chatData.msg.Split(new[] { ' ' }, 3);
                                GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new Eleon.Modding.PString("say '" + "split" + "'"));
                                List<string> Target = NameFragment(findThis[1]);
                                if (Target.Count == 0) //Error
                                {
                                    string nomsg = "No Players Found";
                                    GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new Eleon.Modding.PString("say '" + nomsg + "'"));
                                    GameAPI.Game_Request(CmdId.Request_ShowDialog_SinglePlayer, (ushort)CmdId.Request_ShowDialog_SinglePlayer, new Eleon.Modding.IdMsgPrio(PlayerInfoReceived.entityId, nomsg, 0, 5));
                                }
                                else if (Target.Count == 1)
                                {
                                    GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new Eleon.Modding.PString("say '" + "attempting to kick" + "'"));
                                    GameAPI.Game_Request(CmdId.Request_ShowDialog_SinglePlayer, (ushort)CmdId.Request_ShowDialog_SinglePlayer, new Eleon.Modding.IdMsgPrio(PlayerInfoReceived.entityId, "kicking: " + Target[0], 0, 5));
                                    GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new Eleon.Modding.PString("kick " + Target[0] + " '" + findThis[2] + "'"));
                                }
                                SeqNrDict.Remove(seqNr);
                            }
                            else
                            {
                                GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new Eleon.Modding.PString("say '" + "Nothing Happened" + "'"));
                                GameAPI.Game_Request(CmdId.Request_ShowDialog_SinglePlayer, (ushort)CmdId.Request_ShowDialog_SinglePlayer, new Eleon.Modding.IdMsgPrio(PlayerInfoReceived.entityId, "Nothing Happened", 0, 10));
                            }
                            SeqNrDict.Remove(seqNr);
                        }
                        break;
                    case CmdId.Event_Player_Inventory://Triggered
                        break;
                    case CmdId.Event_Player_ItemExchange://Triggered
                        break;
                    case CmdId.Event_Player_List://Triggered
                        break;
                    case CmdId.Event_Playfield_Entity_List://Triggered
                        break;
                    case CmdId.Event_Playfield_List://Triggered
                        break;
                    case CmdId.Event_Playfield_Stats://Triggered
                        break;
                    case CmdId.Event_Structure_BlockStatistics://Triggered
                        break;
                    case CmdId.Event_Ok://Other?
                        break;
                    case CmdId.Event_Error://Other?
                        ErrorInfo err = (ErrorInfo)data;
                        ErrorType err2 = (ErrorType)data;
                        LogFile("ERROR.txt", "Event_ERROR: " + Convert.ToString(err2) + ": " + Convert.ToString(err));

                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LogFile("ERROR.txt", "Exception: " + ex.Message);
                LogFile("ERROR.txt", "Exception: " + ex.Data);
                LogFile("ERROR.txt", "Exception: " + ex.HelpLink);
                LogFile("ERROR.txt", "Exception: " + ex.InnerException);
                LogFile("ERROR.txt", "Exception: " + ex.Source);
                LogFile("ERROR.txt", "Exception: " + ex.StackTrace);
                LogFile("ERROR.txt", "Exception: " + ex.TargetSite);
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