using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using Eleon.Modding;
using ProtoBuf;
using System.Collections;
using YamlDotNet.Serialization;



namespace ActiveRadar
{
    public class MyEmpyrionMod : ModInterface
    {
        ModGameAPI GameAPI;
        public string ModVersion = "ActiveRadar v0.0.2";
        public Dictionary<int, RadarData> storedInfo = new Dictionary<int, RadarData> { };
        public int CurrentSeqNr = 500;
        //public object ModFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        private void LogFile(string FileName, string FileData)
        {
            if (!System.IO.File.Exists("Content\\Mods\\ActiveRadar\\" + FileName))
            {
                System.IO.File.Create("Content\\Mods\\ActiveRadar\\" + FileName);
            }
            string FileData2 = FileData + Environment.NewLine;
            System.IO.File.AppendAllText("Content\\Mods\\ActiveRadar\\" + FileName, FileData2);
        }
        //System.IO.File.AppendAllText("Content\\Mods\\ActiveRadar\\" + FileName, FileData2);

        public class RadarData
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

        //#################################################################################################### Testing Data
        public static Contact Retrieve(String filePath)
        {
            System.IO.File.AppendAllText("Content\\Mods\\ActiveRadar\\debug.txt", "1");
            var input = System.IO.File.OpenText(filePath);
            System.IO.File.AppendAllText("Content\\Mods\\ActiveRadar\\debug.txt", "2");
            var deserializer = new Deserializer();
            System.IO.File.AppendAllText("Content\\Mods\\ActiveRadar\\debug.txt", "3");
            var Contacts = deserializer.Deserialize<Contact>(input);
            System.IO.File.AppendAllText("Content\\Mods\\ActiveRadar\\debug.txt", "4");
            return Contacts;
        }
        public class Contact
        {
            public List<Ident> Scanned { get; set; }
        }
        public class Ident
        {
            public int ID { get; set; }
            public int Power { get; set; }

        }
        //#################################################################################################### End Test


        public void Game_Start(ModGameAPI gameAPI)
        {
            GameAPI = gameAPI;
            System.IO.File.WriteAllText("Content\\Mods\\ActiveRadar\\debug.txt", "");
            System.IO.File.WriteAllText("Content\\Mods\\ActiveRadar\\ERROR.txt", "");
            //System.IO.File.WriteAllText("Content\\Mods\\ActiveRadar\\pfEntity.txt", "");

            LogFile("debug.txt", "Test Started");
            if (System.IO.File.Exists("Content\\Mods\\ActiveRadar\\Players\\test.yaml"))
            {
                LogFile("Debug.txt", "File Exists");
                Contact test = Retrieve("Content\\Mods\\ActiveRadar\\Players\\test.yaml");
                LogFile("Debug.txt", "Done reading");

                foreach (Ident item in test.Scanned)
                {
                    LogFile("Debug.txt", Convert.ToString(item.ID));
                }
            }

        }
        public void Game_Event(CmdId cmdId, ushort seqNr, object data)
        {
            try
            {
                switch (cmdId)
                {

                    case CmdId.Event_ChatMessage:
                        ChatInfo chatInfo = (ChatInfo)data;
                        if (chatInfo.msg.ToLower().StartsWith("/scan"))
                        {

                            CurrentSeqNr = SeqNrGenerator(CurrentSeqNr);
                            RadarData StoreThisInfo = new RadarData();
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
                        } else if (chatInfo.msg.ToLower().StartsWith("!mods"))
                        {
                            RadarData StoreThisInfo = new RadarData
                            {
                                chatData = chatInfo
                            };
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
                                    RadarData StoreThisInfo = new RadarData();
                                    StoreThisInfo = storedInfo[seqNr];
                                    StoreThisInfo.PlayerInfo = playerInfo;
                                    CurrentSeqNr = SeqNrGenerator(CurrentSeqNr);
                                    storedInfo[CurrentSeqNr] = StoreThisInfo;
                                    GameAPI.Game_Request(CmdId.Request_GlobalStructure_Update, (ushort)CurrentSeqNr, new Eleon.Modding.PString(playerInfo.playfield));
                                    CurrentSeqNr = SeqNrGenerator(CurrentSeqNr);
                                    GameAPI.Game_Request(CmdId.Request_Playfield_Stats, (ushort)CurrentSeqNr, new Eleon.Modding.PString(playerInfo.playfield));
                                    try { GameAPI.Game_Request(CmdId.Request_Playfield_Entity_List, (ushort)CurrentSeqNr, new Eleon.Modding.PString(playerInfo.playfield));}catch { }
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
                        PlayfieldEntityList pfEntsList = (PlayfieldEntityList)data;
                        //LogFile("debug.txt", "Ents Received");
                        foreach (var entity in pfEntsList.entities)
                        {
                            //LogFile("pfEntity.txt", "Marker add name=" + Convert.ToString(entity.id) + "T=" + Convert.ToString(entity.type) + " pos=" + Math.Round(entity.pos.x) + "," + Math.Round(entity.pos.y) + "," + Math.Round(entity.pos.z) + " expire=15");
                            //GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new PString("remoteex cl=1 marker add name=[Blank] pos=" + Math.Round(entity.pos.x) + "," + Math.Round(entity.pos.y - 1) + "," + Math.Round(entity.pos.z) + " wd"));
                        }
                        break;
                    case CmdId.Event_GlobalStructure_List:
                        if (storedInfo.ContainsKey(seqNr))
                        {
                            GlobalStructureList Structs = (GlobalStructureList)data;
                            //start test code
                            /*
                            foreach (GlobalStructureInfo info in Structs.globalStructures[storedInfo[seqNr].PlayerInfo.playfield])
                            {
                                foreach (int ship in info.dockedShips)
                                {
                                    LogFile("docked.txt", Convert.ToString(ship));
                                }
                            }*/
                            //end test code

                            if (Structs.globalStructures.Keys.Contains(storedInfo[seqNr].PlayerInfo.playfield))
                            {
                                bool isPiloting = false;
                                foreach (GlobalStructureInfo item in Structs.globalStructures[storedInfo[seqNr].PlayerInfo.playfield])
                                {
                                    /*
                                    if (item.id == 110007)
                                    {
                                        GameAPI.Game_Request(CmdId.Request_InGameMessage_SinglePlayer, (ushort)CmdId.Request_InGameMessage_SinglePlayer, new IdMsgPrio(storedInfo[seqNr].PlayerInfo.entityId, "BINGO!!!", 1, 5));
                                    }
                                    */
                                    if (item.pilotId == storedInfo[seqNr].PlayerInfo.entityId)
                                    {
                                        isPiloting = true;
                                        if (item.powered)
                                        {
                                            RadarData StoreThisInfo = new RadarData();
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
                                        SensorContacts contactData = new SensorContacts
                                        {
                                            Distance = distance,
                                            GlobalStructureInfo = item
                                        };
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
                                        if (BroadcastListCount < 100)
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
                                                            GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new PString("remoteex cl=" + storedInfo[seqNr].PlayerInfo.clientId + " marker add name=[UnknownContact] pos=" + Math.Round(contact.GlobalStructureInfo.pos.x) + "," + Math.Round(contact.GlobalStructureInfo.pos.y-1) + "," + Math.Round(contact.GlobalStructureInfo.pos.z) + " expire=15"));
                                                            BroadcastListCount = BroadcastListCount + 1;
                                                        }
                                                        else
                                                        {
                                                            string RadarContact = Sanitize(contact.GlobalStructureInfo.name);
                                                            GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new PString("remoteex cl=" + storedInfo[seqNr].PlayerInfo.clientId + " marker add name="+ RadarContact +" pos=" + Math.Round(contact.GlobalStructureInfo.pos.x) + "," + Math.Round(contact.GlobalStructureInfo.pos.y-1) + "," + Math.Round(contact.GlobalStructureInfo.pos.z) + " expire=15"));
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
/*                    case CmdId.Event_Playfield_Stats:
                        PlayfieldStats pfStats = (PlayfieldStats)data;
                        LogFile("pfStats.txt", "playfield=" + pfStats.playfield);
                        LogFile("pfStats.txt", "Chunks=" + pfStats.chunks);
                        LogFile("pfStats.txt", "devices=" + pfStats.devices);
                        LogFile("pfStats.txt", "fps=" + pfStats.fps);
                        LogFile("pfStats.txt", "mem=" + pfStats.mem);
                        LogFile("pfStats.txt", "mobs=" + pfStats.mobs);
                        LogFile("pfStats.txt", "players=" + pfStats.players);
                        LogFile("pfStats.txt", "processID=" + pfStats.processId);
                        LogFile("pfStats.txt", "structs=" + pfStats.structs);
                        LogFile("pfStats.txt", "uptime=" + pfStats.uptime);
                        break;
                    case CmdId.Event_Ok:
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
        }
        public void Game_Exit()
        {
            //LogFile("debug.txt", "-------------------Server shut down-------------------");
        }
    }
}