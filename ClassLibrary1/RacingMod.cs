using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eleon.Modding;
using ProtoBuf;


namespace RacingMod
{
    public class MyEmpyrionMod : ModInterface
    {
        ModGameAPI GameAPI;
        public Boolean Recorder = false;
        public Boolean RaceStarted = false;
        public Int32 Recording = 0;
        public ulong LastTicker = 0;
        //public Dictionary<int, string> chatCommands = new Dictionary<int, string> { };
        public Dictionary<int, PVector3> currentCoords = new Dictionary<int, PVector3> { };
        //public Dictionary<int, PlayerInfo> PlayerData = new Dictionary<int, PlayerInfo> { };

        public List<int> racers = new List<int> { };
        public List<int> racecars = new List<int> { };
        public int CurrentSeqNr = 500;
        public Dictionary<int, StorableData> tempStoredData = new Dictionary<int, StorableData> { };
        public StorableData StoreThisInfo = new StorableData();
        public Dictionary<int, RacerData> RacerDataHolder = new Dictionary<int, RacerData>{};
        public RacerData RacerDataBuilder = new RacerData();
        public Dictionary<int, Racetrack> RacetrackHolder = new Dictionary<int, Racetrack> { };

        public class StorableData
        {
            public ChatInfo chatData;
            //public int vesselID;
            public GlobalStructureInfo GlobalStructureInfo;
            public IdStructureBlockInfo IdStructureBlockInfo;
            public PVector3 coords;
            public PlayerInfo PlayerInfo;
            public bool racer;
        }

        public class RacerData
        {
            public GlobalStructureInfo GlobalStructureInfo;
            public IdPositionRotation IdPositionRotation;
            public int LastWaypoint;
            public int Lap;
        }

        public class Racetrack
        {
            public PVector3 StartingLine;
            public PVector3 Finishline;
            public List<PVector3> Waypoints;
        }

        public int SeqNrGenerator(int LastSeqNr)
        {
            bool Fail = false;
            int newSeqNr = 500;
            do
            {
                if (LastSeqNr > 65530)
                {
                    LastSeqNr = 500;
                }
                newSeqNr = LastSeqNr + 1;
                if (tempStoredData.ContainsKey(newSeqNr)) { Fail = true; }
                LogFile("debug", Convert.ToString(newSeqNr));
            } while (Fail == true);
            return newSeqNr;
        }



        private void LogFile(string FileName, string FileData)
        {
            if (!System.IO.File.Exists("Content\\Mods\\RacingMod\\" + FileName + ".txt"))
            {
                System.IO.File.Create("Content\\Mods\\RacingMod\\" + FileName + ".txt");
            }
            System.IO.File.AppendAllText("Content\\Mods\\RacingMod\\" + FileName + ".txt", FileData + Environment.NewLine);
            GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new Eleon.Modding.PString("say '" + FileData +  "'"));
        }

        public void Game_Start(ModGameAPI gameAPI)
        {
            GameAPI = gameAPI;
            System.IO.File.WriteAllText("Content\\Mods\\RacingMod\\debug.txt", "");
            System.IO.File.WriteAllText("Content\\Mods\\RacingMod\\racers.txt", "");
        }
        public void Game_Event(CmdId cmdId, ushort seqNr, object data)
        {
            try
            {
                switch (cmdId)
                {

                    case CmdId.Event_ChatMessage:
                        ChatInfo chatInfo = (ChatInfo)data;
                        //LogFile("debug", "Chat Received");
                        if (chatInfo.msg.StartsWith("/racing"))
                        {
                            LogFile("debug", "/racing");
                            CurrentSeqNr = SeqNrGenerator(CurrentSeqNr);
                            StoreThisInfo.chatData = chatInfo;
                            tempStoredData[CurrentSeqNr] = StoreThisInfo;
                            racers.Add(chatInfo.playerId);
                            GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)CurrentSeqNr, new Id(chatInfo.playerId));
                            LogFile("Racers", Convert.ToString(chatInfo.playerId));
                        }
                        else if (chatInfo.msg.StartsWith("/plot"))
                        {
                            CurrentSeqNr = SeqNrGenerator(CurrentSeqNr);
                            StoreThisInfo.chatData = chatInfo;
                            tempStoredData[CurrentSeqNr] = StoreThisInfo;
                            GameAPI.Game_Request(CmdId.Request_Entity_PosAndRot, (ushort)CurrentSeqNr, new Id(chatInfo.playerId));
                        }
                        else if (chatInfo.msg.StartsWith("/race start"))
                        {
                            LogFile("debug", "/race start");
                            CurrentSeqNr = SeqNrGenerator(CurrentSeqNr);
                            StoreThisInfo.chatData = chatInfo;
                            tempStoredData[CurrentSeqNr] = StoreThisInfo;
                            GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)CurrentSeqNr, new Id(chatInfo.playerId));
                            RaceStarted = true;
                        }
                        else if (chatInfo.msg.StartsWith("/race stop"))
                        {
                            LogFile("debug", "/race reset");
                            CurrentSeqNr = SeqNrGenerator(CurrentSeqNr);
                            StoreThisInfo.chatData = chatInfo;
                            tempStoredData[CurrentSeqNr] = StoreThisInfo;
                            GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)CurrentSeqNr, new Id(chatInfo.playerId));
                            RaceStarted = false;
                        }
                        else if (chatInfo.msg.StartsWith("/record start"))
                        {
                            LogFile("debug", "/record start");
                            CurrentSeqNr = SeqNrGenerator(CurrentSeqNr);
                            StoreThisInfo.chatData = chatInfo;
                            tempStoredData[CurrentSeqNr] = StoreThisInfo;
                            GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)CurrentSeqNr, new Id(chatInfo.playerId));
                        }
                        else if (chatInfo.msg.StartsWith("/record stop"))
                        {
                            LogFile("debug", "/record stop");
                            CurrentSeqNr = SeqNrGenerator(CurrentSeqNr);
                            //StoreThisInfo = tempStoredData[CurrentSeqNr];
                            StoreThisInfo.chatData = chatInfo;
                            tempStoredData[CurrentSeqNr] = StoreThisInfo;
                            GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)CurrentSeqNr, new Id(chatInfo.playerId));
                        }
                        else if (chatInfo.msg.StartsWith("test"))
                        {
                            LogFile("debug", "test");
                            GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)500, new PString(""));
                        }
                        break;
                    case CmdId.Event_Player_Info:
                        //LogFile("debug", "Player Info Received");
                        if (tempStoredData.ContainsKey(seqNr))
                        {
                            PlayerInfo playerInfo = (PlayerInfo)data;
                            if (tempStoredData[seqNr].chatData.playerId == playerInfo.entityId)
                            {
                                if (tempStoredData[seqNr].chatData.msg == "/racing")
                                {
                                    LogFile("debug", "/racing");
                                    CurrentSeqNr = SeqNrGenerator(CurrentSeqNr);
                                    StoreThisInfo = tempStoredData[seqNr];
                                    StoreThisInfo.PlayerInfo = playerInfo;
                                    tempStoredData[CurrentSeqNr] = StoreThisInfo;
                                    GameAPI.Game_Request(CmdId.Request_GlobalStructure_Update, (ushort)CurrentSeqNr , new PString(playerInfo.playfield));
                                }
                                else if (tempStoredData[seqNr].chatData.msg == "/race start")
                                {
                                    LogFile("debug", "/race start");
                                    CurrentSeqNr = SeqNrGenerator(CurrentSeqNr);
                                    StoreThisInfo = tempStoredData[seqNr];
                                    StoreThisInfo.PlayerInfo = playerInfo;
                                    tempStoredData[CurrentSeqNr] = StoreThisInfo;
                                }
                                else if (tempStoredData[seqNr].chatData.msg == "/race stop")
                                {
                                    LogFile("debug", "/race stop");
                                    CurrentSeqNr = SeqNrGenerator(seqNr);
                                    StoreThisInfo = tempStoredData[CurrentSeqNr];
                                    StoreThisInfo.PlayerInfo = playerInfo;
                                    tempStoredData[CurrentSeqNr] = StoreThisInfo;
                                }
                                if (tempStoredData[seqNr].chatData.msg == "/record start")
                                {
                                    LogFile("debug", "/record start");
                                    CurrentSeqNr = SeqNrGenerator(CurrentSeqNr);
                                    StoreThisInfo = tempStoredData[seqNr];
                                    StoreThisInfo.PlayerInfo = playerInfo;
                                    tempStoredData[CurrentSeqNr] = StoreThisInfo;
                                    Recorder = true;
                                    Recording = playerInfo.entityId;
                                    GameAPI.Game_Request(CmdId.Request_InGameMessage_SinglePlayer, (ushort)CmdId.Request_InGameMessage_SinglePlayer, new IdMsgPrio(tempStoredData[seqNr].chatData.playerId, "Recording Started", 1, 5));
                                }
                                else if (tempStoredData[seqNr].chatData.msg == "/record stop")
                                {
                                    LogFile("debug", "/record stop");
                                    CurrentSeqNr = SeqNrGenerator(CurrentSeqNr);
                                    StoreThisInfo = tempStoredData[seqNr];
                                    StoreThisInfo.PlayerInfo = playerInfo;
                                    tempStoredData[CurrentSeqNr] = StoreThisInfo;
                                    Recorder = false;
                                    Recording = playerInfo.entityId;
                                    GameAPI.Game_Request(CmdId.Request_InGameMessage_SinglePlayer, (ushort)CmdId.Request_InGameMessage_SinglePlayer, new IdMsgPrio(tempStoredData[seqNr].chatData.playerId, "Recording Stopped", 1, 5));
                                }
                            }
                        }
                        break;
                    case CmdId.Event_Playfield_Entity_List:
                        //DONT USE THIS!!! Bug: doesnt return and real entities.
                        break;
                    case CmdId.Event_GlobalStructure_List:
                        if (tempStoredData.ContainsKey(seqNr))
                        {
                            GlobalStructureList Structs = (GlobalStructureList)data;
                            if (Structs.globalStructures.Keys.Contains(tempStoredData[seqNr].PlayerInfo.playfield))
                            {
                                bool isPiloting = false;
                                foreach (GlobalStructureInfo item in Structs.globalStructures[tempStoredData[seqNr].PlayerInfo.playfield])
                                {
                                    if (item.pilotId == tempStoredData[seqNr].PlayerInfo.entityId)
                                    {
                                        isPiloting = true;
                                        CurrentSeqNr = SeqNrGenerator(CurrentSeqNr);
                                        StoreThisInfo = tempStoredData[seqNr];
                                        StoreThisInfo.GlobalStructureInfo = item;
                                        tempStoredData[CurrentSeqNr] = StoreThisInfo;

                                        RacerDataBuilder.GlobalStructureInfo = item;
                                        RacerDataHolder.Add(item.pilotId, RacerDataBuilder);
                                        GameAPI.Game_Request(CmdId.Request_InGameMessage_SinglePlayer, (ushort)CmdId.Request_InGameMessage_SinglePlayer, new IdMsgPrio(tempStoredData[seqNr].PlayerInfo.entityId, "Racer Registered: " + item.pilotId + " is Piloting "+ item.name , 2, 5));
                                        tempStoredData.Remove(seqNr);
                                    }
                                }
                                if (isPiloting == false)
                                {
                                    GameAPI.Game_Request(CmdId.Request_InGameMessage_SinglePlayer, (ushort)CmdId.Request_InGameMessage_SinglePlayer, new IdMsgPrio(tempStoredData[seqNr].PlayerInfo.entityId, "You must be in the Pilot seat of a vessel so I can record your vessel ID", 0, 5));
                                    tempStoredData.Remove(seqNr);
                                }

                            }
                        }
                                        break;
                    case CmdId.Event_Entity_PosAndRot:
                        LogFile("debug", "Entity PosRot received");
                        IdPositionRotation IDPosRot = (IdPositionRotation)data;
                        if (tempStoredData.ContainsKey(seqNr))
                        {
                            LogFile("debug", "TempStoredData contains seqNr");
                            if (tempStoredData[seqNr].chatData.playerId == IDPosRot.id)
                            {
                                LogFile("debug", "PlayerID Match");
                                Racetrack StoreThisWaypoint = new Racetrack { };
                                List<PVector3> WaypointList = new List<PVector3> { };
                                if (tempStoredData[seqNr].chatData.msg == "/plot start")
                                {
                                    CurrentSeqNr = SeqNrGenerator(CurrentSeqNr);
                                    //StoreThisWaypoint = RacetrackHolder[tempStoredData[seqNr].chatData.playerId];
                                    StoreThisWaypoint.StartingLine = IDPosRot.pos;
                                    RacetrackHolder[tempStoredData[seqNr].chatData.playerId] = StoreThisWaypoint;
                                    LogFile("Debug", "/plot start");
                                    GameAPI.Game_Request(CmdId.Request_InGameMessage_SinglePlayer, (ushort)CmdId.Request_InGameMessage_SinglePlayer, new IdMsgPrio(tempStoredData[seqNr].PlayerInfo.entityId, "Startingline Plotted", 0, 5));
                                    tempStoredData.Remove(seqNr);
                                }
                                else if (tempStoredData[seqNr].chatData.msg == "/plot finish")
                                {
                                    CurrentSeqNr = SeqNrGenerator(CurrentSeqNr);
                                    StoreThisWaypoint = RacetrackHolder[tempStoredData[seqNr].chatData.playerId];
                                    StoreThisWaypoint.Finishline = IDPosRot.pos;
                                    RacetrackHolder[tempStoredData[seqNr].chatData.playerId] = StoreThisWaypoint;
                                    LogFile("Debug", "/plot finish");
                                    GameAPI.Game_Request(CmdId.Request_InGameMessage_SinglePlayer, (ushort)CmdId.Request_InGameMessage_SinglePlayer, new IdMsgPrio(tempStoredData[seqNr].PlayerInfo.entityId, "Finishline Plotted", 0, 5));
                                    tempStoredData.Remove(seqNr);
                                }
                                else if(tempStoredData[seqNr].chatData.msg == "/plot")
                                {
                                    CurrentSeqNr = SeqNrGenerator(CurrentSeqNr);
                                    StoreThisWaypoint = RacetrackHolder[tempStoredData[seqNr].chatData.playerId];
                                    LogFile("Debug", "/plot waypoint 1");
                                    WaypointList = StoreThisWaypoint.Waypoints;
                                    LogFile("Debug", "/plot waypoint 1.1");
                                    WaypointList.Add(IDPosRot.pos);
                                    LogFile("Debug", "/plot waypoint 1.2");
                                    StoreThisWaypoint.Waypoints = WaypointList;
                                    LogFile("Debug", "/plot waypoint 2");
                                    RacetrackHolder[tempStoredData[seqNr].chatData.playerId] = StoreThisWaypoint;
                                    LogFile("Debug", "/plot waypoint 3");
                                    GameAPI.Game_Request(CmdId.Request_InGameMessage_SinglePlayer, (ushort)CmdId.Request_InGameMessage_SinglePlayer, new IdMsgPrio(tempStoredData[seqNr].PlayerInfo.entityId, "Waypoint Plotted", 0, 5));
                                    tempStoredData.Remove(seqNr);
                                }
                            }
                        }




                        // need to split this: racer and recorder
                        if (racers.Contains(IDPosRot.id))
                        {

                        }
                        else if (Recording == IDPosRot.id)
                        {
                            LogFile(Convert.ToString(IDPosRot.id), IDPosRot.pos.x + "," + IDPosRot.pos.y + "," + IDPosRot.pos.z + "  Distance = " + Math.Sqrt(((currentCoords[IDPosRot.id].x - IDPosRot.pos.x) * (currentCoords[IDPosRot.id].x - IDPosRot.pos.x)) + ((currentCoords[IDPosRot.id].y - IDPosRot.pos.y) * (currentCoords[IDPosRot.id].y - IDPosRot.pos.y)) + ((currentCoords[IDPosRot.id].z - IDPosRot.pos.z) * (currentCoords[IDPosRot.id].z - IDPosRot.pos.z))));
                            if (currentCoords.ContainsKey(IDPosRot.id))
                            {
                                if (Math.Sqrt(((currentCoords[IDPosRot.id].x - IDPosRot.pos.x) * (currentCoords[IDPosRot.id].x - IDPosRot.pos.x)) + ((currentCoords[IDPosRot.id].y - IDPosRot.pos.y) * (currentCoords[IDPosRot.id].y - IDPosRot.pos.y)) + ((currentCoords[IDPosRot.id].z - IDPosRot.pos.z) * (currentCoords[IDPosRot.id].z - IDPosRot.pos.z))) > 10)
                                {
                                    LogFile(Convert.ToString(IDPosRot.id), IDPosRot.pos.x + "," + IDPosRot.pos.y + "," + IDPosRot.pos.z + "Distance = " + Math.Sqrt(((currentCoords[IDPosRot.id].x - IDPosRot.pos.x) * (currentCoords[IDPosRot.id].x - IDPosRot.pos.x)) + ((currentCoords[IDPosRot.id].y - IDPosRot.pos.y) * (currentCoords[IDPosRot.id].y - IDPosRot.pos.y)) + ((currentCoords[IDPosRot.id].z - IDPosRot.pos.z) * (currentCoords[IDPosRot.id].z - IDPosRot.pos.z))));
                                    currentCoords[IDPosRot.id] = IDPosRot.pos;
                                }
                            }
                            else
                            {
                                currentCoords.Add(IDPosRot.id, IDPosRot.pos);
                            }

                        }
                        break;
                        /*
                    case CmdId.Event_Statistics:
                        StatisticsParam StatsParam = (StatisticsParam)data;
                        //LogFile ("Trigger Tests", Convert.ToString(eventData.EventType));
                        string SendData = (
                        "Int1:" + StatsParam.int1 + "\r\n" + 
                        "Int2:" + StatsParam.int2 + "\r\n" +
                        "Int3:" + StatsParam.int3 + "\r\n" +
                        "Int4:" + StatsParam.int4 + "\r\n" +
                        "Type:" + Convert.ToString(StatsParam.type));
                        LogFile("Trigger Tests", SendData);
                        GameAPI.Game_Request(CmdId.Request_InGameMessage_SinglePlayer, (ushort)CmdId.Request_InGameMessage_SinglePlayer, new IdMsgPrio(3026, SendData, 1, 5));
                        break;
                    case CmdId.Event_TraderNPCItemSold:
                        TraderNPCItemSoldInfo TraderSale = (TraderNPCItemSoldInfo)data;
                        string SendTradeData = (
                        "Player=" + TraderSale.playerEntityId + "\r\n" +
                        "Structure=" + TraderSale.structEntityId + "\r\n" +
                         "BoughtItemCount=" + TraderSale.boughtItemCount + "\r\n" +
                         "BoughtItemID=" + TraderSale.boughtItemId + "\r\n" +
                         "BoughtItemPrice=" + TraderSale.boughtItemPrice + "\r\n" +
                         "TraderEntityID=" + TraderSale.traderEntityId + "\r\n" +
                         "TraderType=" + TraderSale.traderType);
                        LogFile("Trigger Tests", SendTradeData);
                        GameAPI.Game_Request(CmdId.Request_InGameMessage_SinglePlayer, (ushort)CmdId.Request_InGameMessage_SinglePlayer, new IdMsgPrio(3026, SendTradeData, 1, 5));
                        break;
                        */
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
            ulong ticker = GameAPI.Game_GetTickTime();
            //LogFile("debug", Convert.ToString(ticker));
            if(ticker > (LastTicker + 100))
            {
                //LogFile("debug", Convert.ToString(ticker));
                if (RaceStarted == true)
                {
                    foreach (int racer in racers)
                    {
                        GameAPI.Game_Request(CmdId.Request_Entity_PosAndRot, (ushort)CmdId.Request_Entity_PosAndRot, new Eleon.Modding.Id(racer));
                        LogFile("debug", "GameUpdate Racer");
                        LastTicker = ticker;
                    }
                }
                if (Recorder == true)
                {
                    GameAPI.Game_Request(CmdId.Request_Entity_PosAndRot, (ushort)CmdId.Request_Entity_PosAndRot, new Eleon.Modding.Id(Recording));
                    LogFile("debug", "GameUpdate Recording");
                    LastTicker = ticker;
                }
            }
        }
        public void Game_Exit()
        {
            LogFile("debug", "-------------------Server shut down-------------------");
        }
    }
}