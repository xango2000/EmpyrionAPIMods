using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eleon.Modding;
using ProtoBuf;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Xunit.Abstractions;


namespace JumpGate
{
    public class JumpGates : ModInterface

    {
        public Dictionary<int, string> ChatSwitch = new Dictionary<int, string>() { };
        public Dictionary<string, Players> PlayerDictionary = new Dictionary<string, Players> { };

        ModGameAPI GameAPI;
       

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
            if (!System.IO.File.Exists("Content\\Mods\\JumpGate\\" + FileName))
            {
                System.IO.File.Create("Content\\Mods\\JumpGate\\" + FileName);
            }
            string FileData2 = FileData + Environment.NewLine;
            System.IO.File.AppendAllText("Content\\Mods\\JumpGate\\" + FileName, FileData2);
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

        private double Distance(PVector3 player, PVector3 poi)
        {
            //Int32 q = 1000000;
            //double distance = Math.Sqrt(( Math.Pow((player.x - poi.x + q + q),2)) + (Math.Pow((player.y - poi.y + q + q),2)) + (Math.Pow((player.z - poi.z + q + q),2)));
            double distance = Math.Sqrt((Math.Pow((player.x - poi.x), 2)) + (Math.Pow((player.y - poi.y), 2)) + (Math.Pow((player.z - poi.z), 2)));
            return distance;
        }


        public void Game_Start(ModGameAPI gameAPI)
        {
        }
        public void Game_Event(CmdId cmdId, ushort seqNr, object data)
        {
            try
            {
                switch (cmdId)
                {
                    case CmdId.Event_AlliancesAll:
                        break;
                    case CmdId.Event_AlliancesFaction:
                        break;
                    case CmdId.Event_BannedPlayers:
                        break;
                    case CmdId.Event_ChatMessage:
                        ChatInfo ci = (ChatInfo)data;
                        LogFile("Chat.txt", "Type:" + ci.type + " RecipientID:" + ci.recipientEntityId + " FactionRecipientID:" + ci.recipientFactionId + ci.playerId + " SAYS: " + ci.msg);
                        if (ci.msg.StartsWith("s! "))
                        {
                            ci.msg = ci.msg.Remove(0, 3);
                        }
                        var chatmsg = ci.msg.Split(' ');
                        chatmsg[0] = chatmsg[0].ToLower();
                        string cimsg = string.Join(" ", chatmsg);
                        if (cimsg.StartsWith("/bp"))
                        {
                            ChatSwitch[ci.playerId] = cimsg;
                            GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)1167, new Eleon.Modding.Id(ci.playerId));
                        }

                        break;
                    case CmdId.Event_ConsoleCommand:
                        break;
                    case CmdId.Event_Dedi_Stats:
                        break;
                    case CmdId.Event_Entity_PosAndRot:
                        break;
                    case CmdId.Event_Error:
                        break;
                    case CmdId.Event_Faction_Changed:
                        break;
                    case CmdId.Event_GameEvent:
                        break;
                    case CmdId.Event_Get_Factions:
                        break;
                    case CmdId.Event_GlobalStructure_List:
                        break;
                    case CmdId.Event_NewEntityId:
                        break;
                    case CmdId.Event_Ok:
                        break;
                    case CmdId.Event_PdaStateChange:
                        break;
                    case CmdId.Event_Player_ChangedPlayfield:
                        break;
                    case CmdId.Event_Player_Connected:
                        break;
                    case CmdId.Event_Player_Credits:
                        break;
                    case CmdId.Event_Player_Disconnected:
                        break;
                    case CmdId.Event_Player_DisconnectedWaiting:
                        break;
                    case CmdId.Event_Player_GetAndRemoveInventory:
                        break;
                    case CmdId.Event_Player_Info:
                        PlayerInfo PlayerInfoReceived = (PlayerInfo)data;
                        PlayerDictionary[PlayerInfoReceived.playerName] = PlayerData.playerData(PlayerInfoReceived.steamId, PlayerInfoReceived.permission, PlayerInfoReceived.entityId, PlayerInfoReceived.factionId, PlayerInfoReceived.playerName, PlayerInfoReceived.playfield, Convert.ToInt32(PlayerInfoReceived.pos.x), Convert.ToInt32(PlayerInfoReceived.pos.y), Convert.ToInt32(PlayerInfoReceived.pos.z), PlayerInfoReceived.clientId);

                        if (seqNr == 1176)
                        {
                            if (ChatSwitch.ContainsKey(PlayerInfoReceived.entityId))
                            {
                                if (ChatSwitch[PlayerInfoReceived.entityId].StartsWith("/jg"))
                                {
                                    PVector3 poiCoords = new PVector3(poiCoords.x = 0, poiCoords.y = 0, poiCoords.x = 0);
                                    double distance = Distance(PlayerInfoReceived.pos, poiCoords) ;
                                    ServerSay(0, Convert.ToString(distance));

                                    JumpGate.JumpGateSettings.JumpGates jumpdata = JumpGateSettings.Settings();
                                    string Destination = jumpdata.ToPlayfield;
                                    PVector3 DestinationCoords = new PVector3(jumpdata.Destination[0].Position["x"], jumpdata.Destination[1].Position["y"], jumpdata.Destination[2].Position["z"]) ;
                                    PVector3 Facing = new PVector3(jumpdata.Destination[0].Rotation["x"], jumpdata.Destination[1].Rotation["y"], jumpdata.Destination[2].Rotation["z"]);
                                    GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)1177, new Eleon.Modding.IdPlayfieldPositionRotation(PlayerInfoReceived.entityId, Destination, DestinationCoords, Facing));
                                    ServerSay(0, Convert.ToString(jumpdata.Destination[0].Rotation["x"] + ", " + jumpdata.Destination[1].Rotation["y"] + ", " + jumpdata.Destination[2].Rotation["z"]));
                                }
                            }
                        }

                                    break;
                    case CmdId.Event_Player_Inventory:
                        break;
                    case CmdId.Event_Player_ItemExchange:
                        break;
                    case CmdId.Event_Player_List:
                        break;
                    case CmdId.Event_Playfield_Entity_List:
                        break;
                    case CmdId.Event_Playfield_List:
                        break;
                    case CmdId.Event_Playfield_Loaded:
                        break;
                    case CmdId.Event_Playfield_Stats:
                        break;
                    case CmdId.Event_Playfield_Unloaded:
                        break;
                    case CmdId.Event_Statistics:
                        StatisticsParam StatsParam = (StatisticsParam)data;
                        LogFile("test.txt", "Int1:" + StatsParam.int1);
                        LogFile("test.txt", "Int2:" + StatsParam.int2);
                        LogFile("test.txt", "Int3:" + StatsParam.int3);
                        LogFile("test.txt", "Int4:" + StatsParam.int4);
                        LogFile("test.txt", "Type:" + Convert.ToString(StatsParam.type));
                        if(Convert.ToString(StatsParam.type) == "CoreRemoved")
                        {
                            Int32 playerID = StatsParam.int2;
                            Int32 entityID = StatsParam.int1;

                        }

                        break;
                    case CmdId.Event_Structure_BlockStatistics:
                        break;
                    case CmdId.Event_TraderNPCItemSold:
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