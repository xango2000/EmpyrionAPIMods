using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eleon.Modding;
using ProtoBuf;

namespace XangosAPIToolModule.Commons
{
    public class common
    {
        public Dictionary<string, Players> PlayerDictionary = new Dictionary<string, Players> { };
        ModGameAPI GameAPI;
        //GameAPI = gameAPI

        private void Messenger(String msgType, int Priority, int player, String msg, int Duration)
        {
            if (msgType == "ChatAsServer")
            {
                string command = "SAY '" + msg + "'";
                GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new Eleon.Modding.PString(command));
            }
            if (msgType == "Alert")
                GameAPI.Game_Request(CmdId.Request_InGameMessage_SinglePlayer, (ushort)CmdId.Request_InGameMessage_SinglePlayer, new IdMsgPrio(player, msg, Convert.ToByte(Priority), Duration));
        }

        private void AlertAll(int Priority, int player, String msg, float Duration)
        {
            //Priority 0 = ?
            //Priority 1 = ?
            //Priority 2 = ?
            GameAPI.Game_Request(CmdId.Request_InGameMessage_AllPlayers, (ushort)CmdId.Request_InGameMessage_AllPlayers, new IdMsgPrio(0, msg, Convert.ToByte(Priority), Duration));
        }
        private void AlertPlayer(int Priority, int player, String msg, float Duration)
        {
            GameAPI.Game_Request(CmdId.Request_InGameMessage_SinglePlayer, (ushort)CmdId.Request_InGameMessage_SinglePlayer, new IdMsgPrio(player, msg, Convert.ToByte(Priority), Duration));
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

        private List<string> NameFragment(string ChatMessage)
        {
            //GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new Eleon.Modding.PString("SAY '-" + ChatMessage + "-'"));

            //List<string> EmpyrionID = new List<string> { };
            List<string> ExactMatches = new List<string> { };
            List<string> NearMatches = new List<string> { };
            if (ChatMessage.Contains(" "))
            {
                var Message = ChatMessage.Split(' ');
                string message1 = Message[1];
                foreach (string SteamID in PlayerDictionary.Keys)
                {
                    if (Convert.ToString(PlayerDictionary[SteamID].EmpyrionID) == Message[1])
                    {
                        ExactMatches.Add(SteamID);
                    }
                    else if (PlayerDictionary[SteamID].PlayerName == Message[1])
                    {
                        ExactMatches.Add(SteamID);
                    }
                    else if (PlayerDictionary[SteamID].PlayerName.Contains(Message[1]))
                    {
                        NearMatches.Add(SteamID);
                    }
                    else if (PlayerDictionary[SteamID].PlayerName.ToLower().Contains(Message[1].ToLower()))
                    {
                        NearMatches.Add(SteamID);
                    }

                }
                //GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new Eleon.Modding.PString("SAY 'ExactMatches.Count:" + ExactMatches.Count + "'"));
                //GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new Eleon.Modding.PString("SAY 'NearMatches.Count:" + NearMatches.Count + "'"));
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
            }
            else
            {
                return ExactMatches;
            }
        }


        private void LogFile(String FileName, String FileData)
        {
            if (!System.IO.File.Exists("Content\\Mods\\Xango\\" + FileName))
            {
                System.IO.File.Create("Content\\Mods\\Xango\\" + FileName);
            }
            string FileData2 = FileData + Environment.NewLine;
            System.IO.File.AppendAllText("Content\\Mods\\Xango\\" + FileName, FileData2);
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



        //[ProtoContract]
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
    }
}
