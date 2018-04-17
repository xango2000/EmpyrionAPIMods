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
        public Int32 Recording = 0;
        public Dictionary<int, string> chatCommands = new Dictionary<int, string> { };
        public Dictionary<int, PVector3> currentCoords = new Dictionary<int, PVector3> { };
        public Dictionary<int, PlayerInfo> PlayerData = new Dictionary<int, PlayerInfo> { };

        private void LogFile(string FileName, string FileData)
        {
            System.IO.File.AppendAllText("Content\\Mods\\RacingMod\\" + FileName + ".txt", FileData + Environment.NewLine);
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
                        if (chatInfo.msg.StartsWith("/swp"))
                        {
                            GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)1110, new Id(chatInfo.playerId));
                            if (chatCommands.ContainsKey(chatInfo.playerId))
                            {
                                chatCommands[chatInfo.playerId] = chatInfo.msg;
                            }
                            else
                            {
                                chatCommands.Add(chatInfo.playerId, chatInfo.msg);
                            }
                        }
                        break;
                    case CmdId.Event_Player_Info:
                        if (seqNr == 1110)
                        {
                            PlayerInfo playerInfo = (PlayerInfo)data;
                            LogFile("debug", "Player Info Received");
                            if (chatCommands.ContainsKey(playerInfo.entityId))
                            {
                                if (chatCommands[playerInfo.entityId] == "/swp start")
                                {
                                    //System.IO.File.WriteAllText("Content\\Mods\\RacingMod\\" + playerInfo.entityId + ".txt","");
                                    Recorder = true;
                                    Recording = playerInfo.entityId;
                                }else if (chatCommands[playerInfo.entityId] == "/swp stop")
                                {
                                    Recorder = false;
                                }
                            }
                        }
                        break;
                    case CmdId.Event_Playfield_Entity_List:
                        break;
                    case CmdId.Event_Entity_PosAndRot:
                        IdPositionRotation waypoint = (IdPositionRotation)data;
                        LogFile(Convert.ToString(waypoint.id), waypoint.pos.x + "," + waypoint.pos.y + "," + waypoint.pos.z + "  Distance = " + Math.Sqrt(((currentCoords[waypoint.id].x - waypoint.pos.x) * (currentCoords[waypoint.id].x - waypoint.pos.x)) + ((currentCoords[waypoint.id].y - waypoint.pos.y) * (currentCoords[waypoint.id].y - waypoint.pos.y)) + ((currentCoords[waypoint.id].z - waypoint.pos.z) * (currentCoords[waypoint.id].z - waypoint.pos.z))));
                        if (currentCoords.ContainsKey(waypoint.id))
                        {
                            if (Math.Sqrt(((currentCoords[waypoint.id].x - waypoint.pos.x)* (currentCoords[waypoint.id].x - waypoint.pos.x)) + ((currentCoords[waypoint.id].y - waypoint.pos.y)* (currentCoords[waypoint.id].y - waypoint.pos.y)) + ((currentCoords[waypoint.id].z - waypoint.pos.z)* (currentCoords[waypoint.id].z - waypoint.pos.z))) > 10)
                            {
                                LogFile(Convert.ToString(waypoint.id), waypoint.pos.x + "," + waypoint.pos.y + "," + waypoint.pos.z + "Distance = "+ Math.Sqrt(((currentCoords[waypoint.id].x - waypoint.pos.x) * (currentCoords[waypoint.id].x - waypoint.pos.x)) + ((currentCoords[waypoint.id].y - waypoint.pos.y) * (currentCoords[waypoint.id].y - waypoint.pos.y)) + ((currentCoords[waypoint.id].z - waypoint.pos.z) * (currentCoords[waypoint.id].z - waypoint.pos.z))));
                                currentCoords[waypoint.id] = waypoint.pos;
                            }
                        }
                        else
                        {
                            currentCoords.Add(waypoint.id, waypoint.pos);
                        }
                        break;
                    case CmdId.Event_GameEvent:
                        GameEventData eventData = (GameEventData)data;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
            /*
                LogFile("debug", ex.Message);
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
            if (Recorder == true)
            {
                GameAPI.Game_Request(CmdId.Request_Entity_PosAndRot, 11111, new Eleon.Modding.Id(Recording));
            }
        }
        public void Game_Exit()
        {
            LogFile("debug", "Server Was shut down");
        }
    }
}