using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eleon.Modding;
using ProtoBuf;
using System.Collections;

namespace ActiveRadar
{
    public class MyEmpyrionMod : ModInterface
    {
        ModGameAPI GameAPI;

        public Dictionary<int, radarData> storedInfo = new Dictionary<int, radarData> { };
        public radarData StoreThisInfo = new radarData();
        public int CurrentSeqNr = 500;

        private void LogFile(string FileName, string FileData)
        {
            string FileData2 = FileData + Environment.NewLine;
            System.IO.File.AppendAllText("Content\\Mods\\ActiveRadar\\" + FileName + ".txt", FileData2);
        }

        public class radarData
        {
            public ChatInfo chatData;
            //public int vesselID;
            public GlobalStructureList structsList;
            public IdStructureBlockInfo vesselInfo;
            public PVector3 coords;
            public PlayerInfo PlayerInfo;
            public int stepCounter;
            public GlobalStructureInfo piloting;
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
                if (storedInfo.ContainsKey(newSeqNr)) { Fail = true; }
                LogFile("debug", Convert.ToString(newSeqNr));
            } while(Fail == true);
            return newSeqNr;
        }


        public void Game_Start(ModGameAPI gameAPI)
        {
            GameAPI = gameAPI;
        }
        public void Game_Event(CmdId cmdId, ushort seqNr, object data)
        {
            try
            {
                switch (cmdId)
                {

                    case CmdId.Event_ChatMessage:
                        ChatInfo chatInfo = (ChatInfo)data;
                        LogFile("debug", "Chat Received");
                        if (chatInfo.msg.StartsWith("scan"))
                        {
                            LogFile("debug", "if chat startswith 'scan'");
                            CurrentSeqNr = SeqNrGenerator(CurrentSeqNr);
                            if (storedInfo.ContainsKey(CurrentSeqNr))
                            {
                                StoreThisInfo = storedInfo[CurrentSeqNr];
                                StoreThisInfo.chatData = chatInfo;
                                storedInfo[CurrentSeqNr] = StoreThisInfo;
                                LogFile("debug", "ChatInfo Stored Again?");
                            }
                            else
                            {
                                try
                                {
                                    StoreThisInfo = storedInfo[CurrentSeqNr];
                                }
                                catch { }
                                StoreThisInfo.chatData = chatInfo;
                                storedInfo.Add(CurrentSeqNr, StoreThisInfo);
                                LogFile("debug", "ChatInfo Stored");
                            }
                            LogFile("debug", "Dictionary Entry Added");
                            GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)CurrentSeqNr, new Id(chatInfo.playerId));
                            LogFile("debug", "Request " + CurrentSeqNr + " Sent");
                        }
                        break;
                    case CmdId.Event_Player_Info:
                        LogFile("debug", "Player Info Received");
                        if (storedInfo.ContainsKey(seqNr))
                        {
                            if (storedInfo[seqNr].chatData.msg == "scan")
                            {
                                PlayerInfo playerInfo = (PlayerInfo)data;
                                if (storedInfo[seqNr].chatData.playerId == playerInfo.entityId)
                                {
                                    StoreThisInfo = storedInfo[seqNr];
                                    StoreThisInfo.PlayerInfo = playerInfo;
                                    CurrentSeqNr = SeqNrGenerator(CurrentSeqNr);
                                    storedInfo[CurrentSeqNr] = StoreThisInfo;
                                    GameAPI.Game_Request(CmdId.Request_GlobalStructure_Update, (ushort)CurrentSeqNr, new Eleon.Modding.PString(playerInfo.playfield));
                                    LogFile("debug", "Request "+ CurrentSeqNr + " Sent");
                                    storedInfo.Remove(seqNr);
                                }
                            }
                        }
                        break;
                    case CmdId.Event_Playfield_Entity_List:
                        /*
                        if (seqNr == 112)
                        {
                            PlayfieldEntityList pfEntsList = (PlayfieldEntityList)data;
                            LogFile("debug", "Ents Received");
                            foreach (var entity in pfEntsList.entities)
                            {
                                LogFile("pfEntity", Convert.ToString(entity.id));
                            }
                        }
                        */
                        break;
                    case CmdId.Event_GlobalStructure_List:
                        if (storedInfo.ContainsKey(seqNr))
                        {
                            GlobalStructureList Structs = (GlobalStructureList)data;
                            if (Structs.globalStructures.Keys.Contains(storedInfo[seqNr].PlayerInfo.playfield))
                            {
                                foreach (GlobalStructureInfo item in Structs.globalStructures[storedInfo[seqNr].PlayerInfo.playfield])
                                {
                                    if (item.pilotId == storedInfo[seqNr].PlayerInfo.entityId)
                                    {
                                        if (item.powered)
                                        {
                                            StoreThisInfo = storedInfo[seqNr];
                                            StoreThisInfo.structsList = Structs;
                                            StoreThisInfo.piloting = item;
                                            CurrentSeqNr = SeqNrGenerator(CurrentSeqNr);
                                            storedInfo[CurrentSeqNr] = StoreThisInfo;
                                            LogFile("debug", "StructsInfo Stored");
                                            GameAPI.Game_Request(CmdId.Request_Structure_BlockStatistics, (ushort)CurrentSeqNr, new Eleon.Modding.Id(item.id));
                                            LogFile("debug", "Request " + CurrentSeqNr + " Sent");
                                            storedInfo.Remove(seqNr);
                                        }
                                        else
                                        {
                                            GameAPI.Game_Request(CmdId.Request_InGameMessage_SinglePlayer, (ushort)CmdId.Request_InGameMessage_SinglePlayer, new IdMsgPrio(storedInfo[seqNr].PlayerInfo.entityId, "Vessel is Powered Down", 1, 5));
                                            LogFile("debug", "Request " + CmdId.Request_InGameMessage_SinglePlayer + " Sent");
                                            storedInfo.Remove(seqNr);
                                        }
                                    }
                                }
                            }
                            /*
                                LogFile("debug", "Ents Received");
                            foreach (string playfieldName in Structs.globalStructures.Keys)
                            {

                                foreach (GlobalStructureInfo item in Structs.globalStructures[playfieldName])
                                {
                                    if (item.pilotId == 0)
                                    { }
                                    else
                                    {
                                        GameAPI.Game_Request(CmdId.Request_Structure_BlockStatistics, 113, new Eleon.Modding.Id(item.id));
                                    }
                                    LogFile("pfEntity", Convert.ToString(item.id) + " " +Convert.ToString(item.name) + "  Type=" + Convert.ToString(item.type) + "  CoreType=" + Convert.ToString(item.coreType) + "  Pilot=" + Convert.ToString(item.pilotId) + " pos=" + item.pos.x + "," + item.pos.y + "," + item.pos.y);
                                    try
                                    {
                                        foreach (int vessel in item.dockedShips)
                                        {
                                            LogFile("pfEntity", "     " + vessel);

                                        }
                                    }
                                    catch { }
                                }
                            }*/
                        }
                        break;
                    case CmdId.Event_Structure_BlockStatistics:
                        if (storedInfo.ContainsKey(seqNr))
                        {
                            IdStructureBlockInfo Entity = (IdStructureBlockInfo)data;
                            if (storedInfo[seqNr].piloting.id == Entity.id)
                            {
                                if (Entity.blockStatistics.ContainsKey(289))
                                {
                                    //if Player piloting vessel with radar
                                    GameAPI.Game_Request(CmdId.Request_InGameMessage_SinglePlayer, (ushort)CmdId.Request_InGameMessage_SinglePlayer, new IdMsgPrio(storedInfo[seqNr].PlayerInfo.entityId, "Radar Present on current vessel", 1, 5));
                                    LogFile("debug", "Request " + CmdId.Request_InGameMessage_SinglePlayer + " Sent");
                                    List<GlobalStructureInfo> structs = storedInfo[seqNr].structsList.globalStructures[storedInfo[seqNr].PlayerInfo.playfield];
                                }
                                else
                                {
                                    //if Player piloting vessel without radar
                                    GameAPI.Game_Request(CmdId.Request_InGameMessage_SinglePlayer, (ushort)CmdId.Request_InGameMessage_SinglePlayer, new IdMsgPrio(storedInfo[seqNr].PlayerInfo.entityId, "No Radar Present on current vessel", 1, 5));
                                    LogFile("debug", "Request " + CmdId.Request_InGameMessage_SinglePlayer + " Sent");

                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LogFile("debug", ex.Message);
                /*
                LogFile("debug", ex.HelpLink);
                LogFile("debug", Convert.ToString(ex.HResult));
                LogFile("debug", Convert.ToString(ex.InnerException));
                LogFile("debug", ex.Source);
                LogFile("debug", ex.StackTrace);
                LogFile("debug", Convert.ToString(ex.TargetSite));
                LogFile("debug", Convert.ToString(ex.Data));
                */
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