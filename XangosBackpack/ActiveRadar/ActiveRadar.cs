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
        public string ModVersion = "ActiveRadar v1.0.2";
        public Dictionary<int, radarData> storedInfo = new Dictionary<int, radarData> { };
        public int CurrentSeqNr = 500;

        private void LogFile(string FileName, string FileData)
        {
            if (!System.IO.File.Exists("Content\\Mods\\ActiveRadar\\" + FileName))
            {
                System.IO.File.Create("Content\\Mods\\ActiveRadar\\" + FileName);
            }
            string FileData2 = FileData + Environment.NewLine;
            System.IO.File.AppendAllText("Content\\Mods\\ActiveRadar\\" + FileName, FileData2);
        }

        public class radarData
        {
            public ChatInfo chatData;
            public GlobalStructureList structsList;
            public IdStructureBlockInfo vesselInfo;
            public PVector3 coords;
            public PlayerInfo PlayerInfo;
            public int stepCounter;
            public GlobalStructureInfo piloting;
        }

        public class SensorContacts
        {
            public GlobalStructureInfo GlobalStructureInfo;
            public double Distance;
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
            } while(Fail == true);
            return newSeqNr;
        }

        private static string Sanitize(String input)
        {
            string sanitizeMe = input.Replace(" ", "_");
            sanitizeMe = sanitizeMe.Replace("'", "");
            sanitizeMe = sanitizeMe.Replace('"', Convert.ToChar("*"));
            return sanitizeMe;
        }

        public void Game_Start(ModGameAPI gameAPI)
        {
            GameAPI = gameAPI;
            System.IO.File.WriteAllText("Content\\Mods\\ActiveRadar\\debug.txt", "");
            System.IO.File.WriteAllText("Content\\Mods\\ActiveRadar\\ERROR.txt", "");
        }
        public void Game_Event(CmdId cmdId, ushort seqNr, object data)
        {
            try
            {
                switch (cmdId)
                {

                    case CmdId.Event_ChatMessage:
                        ChatInfo chatInfo = (ChatInfo)data;
                        if (chatInfo.msg.StartsWith("/scan"))
                        {
                            CurrentSeqNr = SeqNrGenerator(CurrentSeqNr);
                            radarData StoreThisInfo = new radarData();
                            if (storedInfo.ContainsKey(CurrentSeqNr))
                            {

                                StoreThisInfo = storedInfo[CurrentSeqNr];
                                StoreThisInfo.chatData = chatInfo;
                                storedInfo[CurrentSeqNr] = StoreThisInfo;
                            }
                            else
                            {
                                StoreThisInfo.chatData = chatInfo;
                                storedInfo.Add(CurrentSeqNr, StoreThisInfo);
                            }
                            GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)CurrentSeqNr, new Id(chatInfo.playerId));
                        }else if(chatInfo.msg.StartsWith("!mods"))
                        {
                            radarData StoreThisInfo = new radarData();
                            StoreThisInfo.chatData = chatInfo;
                            storedInfo.Add(CurrentSeqNr, StoreThisInfo);
                            GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)CurrentSeqNr, new Id(chatInfo.playerId));
                        }
                        break;
                    case CmdId.Event_Player_Info:
                        if (storedInfo.ContainsKey(seqNr))
                        {
                            if (storedInfo[seqNr].chatData.msg == "/scan")
                            {
                                PlayerInfo playerInfo = (PlayerInfo)data;
                                if (storedInfo[seqNr].chatData.playerId == playerInfo.entityId)
                                {
                                    radarData StoreThisInfo = new radarData();
                                    StoreThisInfo = storedInfo[seqNr];
                                    StoreThisInfo.PlayerInfo = playerInfo;
                                    CurrentSeqNr = SeqNrGenerator(CurrentSeqNr);
                                    storedInfo[CurrentSeqNr] = StoreThisInfo;
                                    GameAPI.Game_Request(CmdId.Request_GlobalStructure_Update, (ushort)CurrentSeqNr, new Eleon.Modding.PString(playerInfo.playfield));
                                    try
                                    {
                                        storedInfo.Remove(seqNr);
                                    }
                                    catch { }
                                }
                            } else if (storedInfo[seqNr].chatData.msg == "/scan")
                            { PlayerInfo playerInfo = (PlayerInfo)data;
                                if (storedInfo[seqNr].chatData.playerId == playerInfo.entityId)
                                {
                                    GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new Eleon.Modding.PString("say cl:'" + playerInfo.clientId+ " " + ModVersion + "'"));
                                    try
                                    {
                                        storedInfo.Remove(seqNr);
                                    }
                                    catch { }
                                }
                            }
                        }
                        break;
                    case CmdId.Event_Playfield_Entity_List:
                        if (seqNr == 112)
                        {
                            PlayfieldEntityList pfEntsList = (PlayfieldEntityList)data;
                            LogFile("debug.txt", "Ents Received");
                            foreach (var entity in pfEntsList.entities)
                            {
                                LogFile("pfEntity", Convert.ToString(entity.id));
                            }
                        }
                        break;
                    case CmdId.Event_GlobalStructure_List:
                        if (storedInfo.ContainsKey(seqNr))
                        {
                            GlobalStructureList Structs = (GlobalStructureList)data;
                            if (Structs.globalStructures.Keys.Contains(storedInfo[seqNr].PlayerInfo.playfield))
                            {
                                bool isPiloting = false;
                                foreach (GlobalStructureInfo item in Structs.globalStructures[storedInfo[seqNr].PlayerInfo.playfield])
                                {
                                    if (item.pilotId == storedInfo[seqNr].PlayerInfo.entityId)
                                    {
                                        isPiloting = true;
                                        if (item.powered)
                                        {
                                            radarData StoreThisInfo = new radarData();
                                            StoreThisInfo = storedInfo[seqNr];
                                            StoreThisInfo.structsList = Structs;
                                            StoreThisInfo.piloting = item;
                                            CurrentSeqNr = SeqNrGenerator(CurrentSeqNr);
                                            storedInfo[CurrentSeqNr] = StoreThisInfo;
                                            GameAPI.Game_Request(CmdId.Request_Structure_BlockStatistics, (ushort)CurrentSeqNr, new Eleon.Modding.Id(item.id));
                                            try
                                            {
                                                storedInfo.Remove(seqNr);
                                            }
                                            catch { }
                                        }
                                        else
                                        {
                                            GameAPI.Game_Request(CmdId.Request_InGameMessage_SinglePlayer, (ushort)CmdId.Request_InGameMessage_SinglePlayer, new IdMsgPrio(storedInfo[seqNr].PlayerInfo.entityId, "Vessel is Powered Down", 1, 5));
                                            try
                                            {
                                                storedInfo.Remove(seqNr);
                                            }
                                            catch { }
                                        }
                                    }
                                }
                                if (isPiloting == false)
                                {
                                    GameAPI.Game_Request(CmdId.Request_InGameMessage_SinglePlayer, (ushort)CmdId.Request_InGameMessage_SinglePlayer, new IdMsgPrio(storedInfo[seqNr].PlayerInfo.entityId, "You must be in the Pilot seat of a vessel", 1, 5));
                                    try
                                    {
                                        storedInfo.Remove(seqNr);
                                    }
                                    catch { }
                                }
                            }
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
                                    PVector3 myShip = storedInfo[seqNr].piloting.pos;
                                    List<SensorContacts> SensorContactsList = new List<SensorContacts> { };
                                    List<int> docked = new List<int> { };
                                    List<double> sortableList = new List<double> { };
                                    foreach (GlobalStructureInfo item in storedInfo[seqNr].structsList.globalStructures[storedInfo[seqNr].PlayerInfo.playfield])
                                    {
                                        double distance = Math.Sqrt(((myShip.x - item.pos.x) * (myShip.x - item.pos.x)) + ((myShip.y - item.pos.y) * (myShip.y - item.pos.y)) + ((myShip.z - item.pos.z) * (myShip.z - item.pos.z)));
                                        sortableList.Add(distance);
                                        SensorContacts contactData = new SensorContacts();
                                        contactData.Distance = distance;
                                        contactData.GlobalStructureInfo = item;
                                        SensorContactsList.Add(contactData);
                                        try
                                        {
                                            docked.AddRange(item.dockedShips);
                                        }
                                        catch { }
                                    }
                                    List<SensorContacts> notDocked = new List<SensorContacts> { };
                                    foreach (SensorContacts item in SensorContactsList)
                                    {
                                        if (docked.Contains(item.GlobalStructureInfo.id))
                                        {
                                            //ships is docked = Ignore
                                        }
                                        else
                                        {
                                            notDocked.Add(item);
                                        }
                                    }
                                    sortableList.Sort();
                                    int BroadcastListCount = 0;
                                    foreach (double distance in sortableList)
                                    {
                                        if (BroadcastListCount < 10)
                                        {
                                            foreach (SensorContacts contact in SensorContactsList)
                                            {
                                                if (docked.Contains(contact.GlobalStructureInfo.id))
                                                {
                                                }
                                                else
                                                {
                                                    
                                                    if (contact.Distance == distance)
                                                    {
                                                        if (contact.Distance > 2500)
                                                        {
                                                            GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new PString("remoteex cl=" + storedInfo[seqNr].PlayerInfo.clientId + " marker add name=[UnknownContact] pos=" + Math.Round(contact.GlobalStructureInfo.pos.x) + "," + Math.Round(contact.GlobalStructureInfo.pos.y) + "," + Math.Round(contact.GlobalStructureInfo.pos.z) + " wd"));
                                                            BroadcastListCount = BroadcastListCount + 1;
                                                        }
                                                        else
                                                        {
                                                            string RadarContact = Sanitize(contact.GlobalStructureInfo.name);
                                                            GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new PString("remoteex cl=" + storedInfo[seqNr].PlayerInfo.clientId + " marker add name="+ RadarContact +" pos=" + Math.Round(contact.GlobalStructureInfo.pos.x) + "," + Math.Round(contact.GlobalStructureInfo.pos.y) + "," + Math.Round(contact.GlobalStructureInfo.pos.z) + " wd"));
                                                            BroadcastListCount = BroadcastListCount + 1;
                                                        }
                                                    }
                                                    else
                                                    {
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    //if Player piloting vessel without radar
                                    GameAPI.Game_Request(CmdId.Request_InGameMessage_SinglePlayer, (ushort)CmdId.Request_InGameMessage_SinglePlayer, new IdMsgPrio(storedInfo[seqNr].PlayerInfo.entityId, "No Radar Present on current vessel", 1, 5));
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
            //LogFile("debug.txt", "-------------------Server shut down-------------------");
        }
    }
}