using System;
using System.Collections.Generic;
using System.Linq;
using Eleon.Modding;
using ProtoBuf;
//using System.IO;


namespace XangosBackpackModule
{

    public class XangosBackpack : ModInterface
    {
        //public string step;
        public IDictionary<int, ItemStack[]> vBackpackDictionary = new Dictionary<int, ItemStack[]>(){};
        public ItemStack[] EmptyExchange = new ItemStack[0];
        public Dictionary<int, string> ItemExchangeSwitch = new Dictionary<int, string>() { };
        //public List<int> PlysList;
        //public FactionInfoList FactionListDump;
        //public List<List<string>> CommandSetup;
        public Dictionary<string, ItemStack[]> MailDictionary = new Dictionary<string, ItemStack[]>() { };
        public Dictionary<string, Players> PlayerDictionary = new Dictionary<string, Players> { };
        //public Dictionary<string, PlayfieldStats> PFData = new Dictionary<string, PlayfieldStats> { };
        //public Dictionary<string, Permissions> PermissionsDictionary = new Dictionary<string, Permissions> { };
        public Dictionary<string, Int32> PlayfieldIDDict = new Dictionary<string, Int32> { };
        //private List<int> RegenAsteroids;


        ModGameAPI GameAPI;

        private void Messenger(String msgType, int Priority, int player, String msg, int Duration)
        {
            if (msgType == "ChatAsServer")
            {
                string command = "SAY '" + msg + "'";
                GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new Eleon.Modding.PString(command));
            }
            if (msgType == "Alert")
                GameAPI.Game_Request(CmdId.Request_InGameMessage_AllPlayers, (ushort)CmdId.Request_InGameMessage_AllPlayers, new IdMsgPrio(player, msg, Convert.ToByte(Priority), Duration));
        }

        private void Alert(int Priority, int player, String msg, int Duration)
        {
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
            if (!System.IO.File.Exists("Content\\Mods\\Xango\\"+ FileName))
            {
                System.IO.File.Create("Content\\Mods\\Xango\\" + FileName);
            }
            string FileData2 = FileData + Environment.NewLine;
            System.IO.File.AppendAllText("Content\\Mods\\Xango\\"+FileName, FileData2);
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

        [ProtoContract]
        class Person
        {
            [ProtoMember(1)]
            public string SteamID { get; set; }
            [ProtoMember(2)]
            public int Admin { get; set; }
            [ProtoMember(3)]
            public int EmpyrionID { get; set; }
            [ProtoMember(4)]
            public int FactionID { get; set; }
            [ProtoMember(5)]
            public string PlayerName { get; set; }
            [ProtoMember(6)]
            public string Playfield { get; set; }
            [ProtoMember(7)]
            public float x { get; set; }
            [ProtoMember(8)]
            public float y { get; set; }
            [ProtoMember(9)]
            public float z { get; set; }
            [ProtoMember(10)]
            public int ClientID { get; set; }
        }

        public struct Permissions
        {
            public string Admin;
            public String[] File;
            public String[] BlockedOnPlayfieldsFile;
            public string Math;
        }


        public class Permission
        {
            //WIP
            public static Permissions PermissionData(String File)
            {
                Permissions Parameters = new XangosBackpackModule.XangosBackpack.Permissions();
                string[] CommandFile = System.IO.File.ReadAllLines("Content\\Mods\\Xango\\Permissions\\" + File + ".txt");
                for (int i = 0; i < CommandFile.Count(); ++i)
                {
                    var FileLines = CommandFile[i].Split(new[] { ',' }, 2);
                    if (FileLines[0].StartsWith("Admin:"))
                    {
                        Parameters.Admin = FileLines[1];
                    }
                    else if (FileLines[0].StartsWith("File:"))
                    {
                        Parameters.File = System.IO.File.ReadAllLines("Content\\Mods\\Xango\\Permissions\\CommandFiles\\" + FileLines[1] + ".txt");
                    }
                    else if (FileLines[0].StartsWith("BlockedOnPlayfieldsFile:"))
                    {
                        Parameters.BlockedOnPlayfieldsFile = System.IO.File.ReadAllLines("Content\\Mods\\Xango\\Permissions\\CommandFiles\\" + FileLines[1] + ".txt");
                    }
                    else if (FileLines[0].StartsWith("Math:"))
                    {
                        Parameters.Math = FileLines[1];
                    }
                }
                return Parameters;
            }
        }
        /*
        private void LoadSetup()
        {
            string FileName = "";
            string[] SetupFile = System.IO.File.ReadAllLines("Content\\Mods\\Xango\\permissions\\"+ FileName + ".txt");
            for (int i = 0; i < SetupFile.Count(); ++i)
            {
                var Message = SetupFile[i].Split(new[] { ',' }, 2);
                //if line starts with Space, line is command setting
                if (Message[0].StartsWith("Admin:"))
                {

                }
                else if (Message[0].StartsWith("File:"))
                {

                }
                else if (Message[0].StartsWith("BlockedOnPlayfieldsFile:"))
                {

                }
                else if (Message[0].StartsWith("Math:"))
                {

                }

            }
        }
        */

        public void Game_Start(ModGameAPI gameAPI)
        {

            LogFile("chat.txt", "Mod Loaded");
            GameAPI = gameAPI;
            //GameAPI.Game_Request(CmdId.Request_Get_Factions, (ushort)CmdId.Request_Get_Factions, new Id(1));
            System.IO.DirectoryInfo d = new System.IO.DirectoryInfo("content\\Mods\\Xango\\Mail");//Assuming Test is your Folder
            System.IO.FileInfo[] Files = d.GetFiles("*.txt"); //Getting Text files
            foreach (System.IO.FileInfo file in Files)
            {
                string filename = file.Name.Substring(0, file.Name.Length - 4);
                LogFile("chat.txt", "Game Start filename=" + file.Name);
                ItemStack[] MailContents = buildItemStack("Content\\Mods\\Xango\\Mail\\" + filename + ".txt");
                MailDictionary[filename] = MailContents;
                //MailDictionary.Add(file.Name, MailContents);
            }
            /*
            PermissionsDictionary["Send"] = Permission.PermissionData("send");
            LogFile("chat.txt", "Send permissions loaded");
            PermissionsDictionary["Backpack"] = Permission.PermissionData("backpack");
            PermissionsDictionary["Find"] = Permission.PermissionData("find");
            PermissionsDictionary["Inbox"] = Permission.PermissionData("inbox");
            */
        }
        public void Game_Event(CmdId cmdId, ushort seqNr, object data)
        {
            try
            {
                switch (cmdId)
                {

                    case CmdId.Event_Player_Connected:
                        Id pc = (Id)data;
                        GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)CmdId.Request_Player_Info, new Eleon.Modding.Id(pc.id));
                        try { System.IO.Directory.CreateDirectory("Content\\Mods\\Xango\\players\\EID" + pc.id); } catch { };
                        try
                        { string[] bagLines = System.IO.File.ReadAllLines("Content\\Mods\\Xango\\players\\EID" + pc.id + "\\VirtualBackpack.txt");
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
                        catch { System.IO.File.Create("Content\\Mods\\Xango\\players\\EID" + Convert.ToString(pc.id) + "\\VirtualBackpack.txt"); }; //New player, create backpack... i think this will work
                        LogFile("chat.txt", "Player " + pc.id + " Connected");
                        if (System.IO.File.Exists("Content\\Mods\\Xango\\players\\EID" + pc.id + "\\Mail.txt")) { }
                        else { System.IO.File.Create("Content\\Mods\\Xango\\players\\EID" + pc.id + "\\Mail.txt"); }
                        break;
                    case CmdId.Event_Player_Disconnected:
                        Id pd = (Id)data;
                        LogFile("chat.txt", "Player " + pd.id + " DisConnected");
                        try { vBackpackDictionary.Remove(pd.id); } catch { }
                        break;
                    case CmdId.Event_Player_List:
                        /*
                        IdList PlayerIDs = (IdList)data;
                        //WIP add to /find
                        foreach (var player in PlayerIDs.list)
                        {
                            PlysList.Add(Convert.ToInt32(player));
                            GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)CmdId.Request_Player_Info, new Id(player));
                        }
                        */
                        break;
                    case CmdId.Event_Player_Info:
                        //LogFile("Chat.txt", "Start Event_Player_Info");
                        PlayerInfo PlayerInfoReceived = (PlayerInfo)data;

                        //WIP
                        PlayerDictionary[PlayerInfoReceived.playerName] = PlayerData.playerData(PlayerInfoReceived.steamId, PlayerInfoReceived.permission, PlayerInfoReceived.entityId, PlayerInfoReceived.factionId, PlayerInfoReceived.playerName, PlayerInfoReceived.playfield, Convert.ToInt32(PlayerInfoReceived.pos.x), Convert.ToInt32(PlayerInfoReceived.pos.y), Convert.ToInt32(PlayerInfoReceived.pos.z), PlayerInfoReceived.clientId);
                        if (PlayerInfoReceived.steamId == "76561198117632903") //"Godmode" for Xango while testing mod
                        {
                            GameAPI.Game_Request(CmdId.Request_Player_SetPlayerInfo, (ushort)CmdId.Request_Player_SetPlayerInfo, new Eleon.Modding.PlayerInfoSet()
                            {
                                entityId = PlayerInfoReceived.entityId,
                                oxygenMax = Convert.ToInt32(PlayerInfoReceived.oxygen),
                                radiation = 0,
                                food = +5000,
                                stamina = +5000,
                                bodyTemp = 30,
                                health = +5000
                            });
                        }
                        if (ItemExchangeSwitch.ContainsKey(PlayerInfoReceived.entityId))
                        {
                            if (ItemExchangeSwitch[PlayerInfoReceived.entityId].StartsWith("/backpack"))
                            {
                                if (seqNr == 1167)
                                {
                                    LogFile("chat.txt", seqNr + " Triggered");
                                    //BackpackChatDictionary.Remove(PlayerInfoReceived.entityId);
                                    if (vBackpackDictionary.ContainsKey(PlayerInfoReceived.entityId))
                                    {
                                        //LogFile("Chat.txt", "show backpack");
                                        GameAPI.Game_Request(CmdId.Request_Player_ItemExchange, (ushort)CmdId.Request_Player_ItemExchange, new ItemExchangeInfo(PlayerInfoReceived.entityId, "Virtual Backpack", "Extra Inventory Space, Yay!", "Save", vBackpackDictionary[PlayerInfoReceived.entityId]));
                                    }
                                    else
                                    {
                                        if (System.IO.File.Exists("Content\\Mods\\Xango\\players\\EID" + Convert.ToString(PlayerInfoReceived.entityId) + "\\VirtualBackpack.txt") == true)
                                        {
                                            //LogFile("Chat.txt", "Show Blank Backpack");
                                            GameAPI.Game_Request(CmdId.Request_Player_ItemExchange, (ushort)CmdId.Request_Player_ItemExchange, new ItemExchangeInfo(PlayerInfoReceived.entityId, "Virtual Backpack", "Extra Inventory Space, Yay!", "Save", EmptyExchange));
                                        }
                                        else
                                        {
                                            //LogFile("Chat.txt", "Build Blank Backpack");
                                            System.IO.File.Create("Content\\Mods\\Xango\\players\\EID" + Convert.ToString(PlayerInfoReceived.entityId) + "\\VirtualBackpack.txt");
                                            EmptyExchange = buildItemStack("Content\\Mods\\Xango\\players\\EID" + Convert.ToString(PlayerInfoReceived.entityId) + "\\VirtualBackpack.txt");
                                            //System.Threading.Thread.Sleep(5000);
                                            GameAPI.Game_Request(CmdId.Request_Player_ItemExchange, (ushort)CmdId.Request_Player_ItemExchange, new ItemExchangeInfo(PlayerInfoReceived.entityId, "Virtual Backpack", "Extra Inventory Space, Yay!", "Save", EmptyExchange));
                                            //GameAPI.Game_Request(CmdId.Request_Player_ItemExchange, (ushort)CmdId.Request_Player_ItemExchange, new ItemExchangeInfo(PlayerInfoReceived.entityId, "Virtual Backpack", "Extra Inventory Space, Yay!", "Save", vBackpackDictionary[PlayerInfoReceived.entityId]));
                                        }
                                    }
                                    //WhichItemExchange[PlayerInfoReceived.clientId] = "Backpack";
                                }
                            }
                            //LogFile("chat.txt", "IDDictionary starts here");
                            //if (IDDictionary.ContainsKey(PlayerInfoReceived.entityId) == true)

                            else if (ItemExchangeSwitch[PlayerInfoReceived.entityId].StartsWith("/find"))
                            {
                                if (seqNr == 1168)
                                {
                                    //GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new Eleon.Modding.PString("SAY p:" + PlayerInfoReceived.entityId + " '" + "Test Received" + "'"));
                                    List<string> Target = NameFragment(ItemExchangeSwitch[PlayerInfoReceived.entityId]);
                                    //GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new Eleon.Modding.PString("SAY p:" + PlayerInfoReceived.entityId + " 'Target0:" + Target[0] + "'"));

                                    if (Target.Count == 0) //Error
                                    {
                                        //WIP
                                        string nomsg = "No Players Found";
                                        GameAPI.Game_Request(CmdId.Request_ShowDialog_SinglePlayer, (ushort)CmdId.Request_ShowDialog_SinglePlayer, new Eleon.Modding.IdMsgPrio(PlayerInfoReceived.entityId, nomsg, 0, 10));
                                        ItemExchangeSwitch[PlayerInfoReceived.entityId] = "blank";
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
                                    ItemExchangeSwitch[PlayerInfoReceived.entityId] = "blank";
                                }
                            }
                            else if (ItemExchangeSwitch[PlayerInfoReceived.entityId].StartsWith("/send"))
                            {
                                if (seqNr == 1170)
                                {
                                    //GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new Eleon.Modding.PString("SAY p:" + PlayerInfoReceived.entityId + " 'Send Started'"));
                                    //GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new Eleon.Modding.PString("SAY p:" + PlayerInfoReceived.entityId + " '" + Convert.ToInt32(PlayerInfoReceived.permission) + "'"));
                                    //GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new Eleon.Modding.PString("SAY p:" + PlayerInfoReceived.entityId + " '" + Convert.ToInt32(PermissionsDictionary["Send"].Admin) + "'"));

                                    /*
                                    if (Convert.ToInt32(PermissionsDictionary["Send"].Admin) < Convert.ToInt32(PlayerInfoReceived.permission))
                                    {
                                        GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new Eleon.Modding.PString("SAY p:" + PlayerInfoReceived.entityId + " 'Pass 1'"));
                                        LogFile("chat.txt", "Permission Pass");
                                    }
                                    else
                                    {
                                        GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new Eleon.Modding.PString("SAY p:" + PlayerInfoReceived.entityId + " 'Fail 1'"));
                                        LogFile("chat.txt", "Permission Fail");
                                    }
                                    */

                                    List<string> Target = NameFragment(ItemExchangeSwitch[PlayerInfoReceived.entityId]);
                                    //GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new Eleon.Modding.PString("SAY p:" + PlayerInfoReceived.entityId + " 'Target.Count = " + Target.Count + "'"));
                                    if (Target.Count == 1)
                                    {
                                        //GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new Eleon.Modding.PString("SAY p:" + PlayerInfoReceived.entityId + " 'Target.Count = " + Target.Count + "'"));
                                        var SenderMessage = ItemExchangeSwitch[PlayerInfoReceived.entityId].Split(new[] { ' ' }, 3);
                                        if (SenderMessage.Count() == 3)
                                        {
                                            if (PlayerInfoReceived.entityId == PlayerDictionary[Target[0]].EmpyrionID)
                                            {
                                                GameAPI.Game_Request(CmdId.Request_ShowDialog_SinglePlayer, (ushort)CmdId.Request_ShowDialog_SinglePlayer, new Eleon.Modding.IdMsgPrio(PlayerInfoReceived.entityId, "Sending to yourself is not possible.", 0, 10));
                                            }
                                            else
                                            {
                                                GameAPI.Game_Request(CmdId.Request_Player_ItemExchange, (ushort)CmdId.Request_Player_ItemExchange, new ItemExchangeInfo(PlayerInfoReceived.entityId, "To: " + PlayerDictionary[Target[0]].PlayerName, SenderMessage[2], "Send", EmptyExchange));
                                            }
                                        }
                                        else if (SenderMessage.Count() == 2)
                                        {
                                            GameAPI.Game_Request(CmdId.Request_Player_ItemExchange, (ushort)CmdId.Request_Player_ItemExchange, new ItemExchangeInfo(PlayerInfoReceived.entityId, "To: " + PlayerDictionary[Target[0]].EmpyrionID, "[No Message]", "Send", EmptyExchange));
                                        }
                                        else if (SenderMessage.Count() == 1)
                                        {
                                            GameAPI.Game_Request(CmdId.Request_ShowDialog_SinglePlayer, (ushort)CmdId.Request_ShowDialog_SinglePlayer, new Eleon.Modding.IdMsgPrio(PlayerInfoReceived.entityId, "Usage: /send Xango2000 Message that player receives\r\nOR /send Xan Anything you want to say\r\nOR /send 12345\r\nSubject not required. Can use part of the player name or their empyrion ID", 0, 10));
                                            ItemExchangeSwitch[PlayerInfoReceived.entityId] = "blank";
                                        }
                                        //GameAPI.Game_Request(CmdId.Request_ShowDialog_SinglePlayer, (ushort)CmdId.Request_ShowDialog_SinglePlayer, new Eleon.Modding.IdMsgPrio(PlayerInfoReceived.entityId, nomsg, 0, 10));
                                    }
                                    else
                                    {
                                        GameAPI.Game_Request(CmdId.Request_ShowDialog_SinglePlayer, (ushort)CmdId.Request_ShowDialog_SinglePlayer, new Eleon.Modding.IdMsgPrio(PlayerInfoReceived.entityId, "Recipient Count Must Be Exactly One", 0, 10));
                                        ItemExchangeSwitch[PlayerInfoReceived.entityId] = "blank";
                                    }
                                    //LogFile("chat.txt", "SendDictionary in PlayerInfoReceived triggered");
                                    //var Message = ItemExchangeSwitch[PlayerInfoReceived.entityId].Split(new[] { ' ' }, 3);
                                    //GameAPI.Game_Request(CmdId.Request_Player_ItemExchange, (ushort)CmdId.Request_Player_ItemExchange, new ItemExchangeInfo(PlayerInfoReceived.entityId, "To: " + Message[1], Message[2], "Send", EmptyExchange));
                                }
                            }
                            else if (ItemExchangeSwitch[PlayerInfoReceived.entityId].StartsWith("/reward"))
                            {
                                if (seqNr == 1173)
                                {
                                    //WhichItemExchange[PlayerInfoReceived.clientId] = "MailReward";
                                }
                            }
                            else if (ItemExchangeSwitch[PlayerInfoReceived.entityId].StartsWith("/inbox"))
                            {
                                if (seqNr == 1169)
                                {
                                    //WIP
                                    //LogFile("chat.txt", "InboxDictionary in PlayerInfoReceived triggered");
                                    string[] UserMail = System.IO.File.ReadAllLines("Content\\Mods\\Xango\\players\\EID" + PlayerInfoReceived.entityId + "\\mail.txt"); //pull up user mail
                                                                                                                                                                        //LogFile("chat.txt", "UserMail 0 =" + UserMail[0]);
                                    var Message = UserMail[0].Split(new[] { ',' }, 4); //split first line of user mail: Timestamp, Sender, New?, Message
                                                                                       /*
                                                                                       LogFile("chat.txt", "Message 0 =" + Message[0]);
                                                                                       LogFile("chat.txt", "Message 1 =" + Message[1]);
                                                                                       LogFile("chat.txt", "Message 2 =" + Message[2]);
                                                                                       LogFile("chat.txt", "Message 3 =" + Message[3]);
                                                                                       LogFile("chat.txt", "MailDictionary lookup =" + "Content\\Mods\\Xango\\Mail\\" + Message[0] + ".txt");
                                                                                       LogFile("chat.txt", "MailDictionary lookup =" + Convert.ToString(MailDictionary[Message[0]]));
                                                                                       */
                                                                                       //ItemStack[] MailContents = buildItemStack("Content\\Mods\\Xango\\Mail\\" + Message[0] + ".txt");
                                    List<string> Recipient = NameFragment("blank " + Message[1]);
                                    GameAPI.Game_Request(CmdId.Request_Player_ItemExchange, (ushort)CmdId.Request_Player_ItemExchange, new ItemExchangeInfo(PlayerInfoReceived.entityId, "From: " + PlayerDictionary[Recipient[0]].PlayerName, Message[3], "Close", MailDictionary[Message[0]]));
                                }
                            }
                            else if (ItemExchangeSwitch[PlayerInfoReceived.entityId].StartsWith("/factory"))
                            {
                                if (seqNr == 1172)
                                {
                                    List<int> BPkeys = new List<int>(PlayerInfoReceived.bpResourcesInFactory.Keys);
                                    //LogFile("chat.txt", "BlueprintResources=" + BPkeys);
                                    ItemStack[] itStack = new ItemStack[PlayerInfoReceived.bpResourcesInFactory.Keys.Count];
                                    for (int i = 0; i < PlayerInfoReceived.bpResourcesInFactory.Keys.Count; ++i)
                                    {
                                        itStack[i] = new ItemStack(Convert.ToInt32(PlayerInfoReceived.bpResourcesInFactory), Convert.ToInt32(PlayerInfoReceived.bpResourcesInFactory[BPkeys[i]]));
                                    }
                                    GameAPI.Game_Request(CmdId.Request_Player_ItemExchange, (ushort)CmdId.Request_Player_ItemExchange, new ItemExchangeInfo(PlayerInfoReceived.entityId, "Factory", "Found this in your factory, thought you might want it back", "Close", itStack));
                                }
                            }
                            else if (ItemExchangeSwitch[PlayerInfoReceived.entityId].StartsWith("/resource"))
                            {
                                if (seqNr == 1174)
                                {
                                    //ServerSay(0, Convert.ToString(PlayerInfoReceived.permission) + " 2");
                                    if (PlayerInfoReceived.permission == 9)
                                    {
                                        //ServerSay(0, "9=9");
                                        GameAPI.Game_Request(CmdId.Request_GlobalStructure_Update, 2437, new Eleon.Modding.PString(PlayerInfoReceived.playfield));
                                        //GameAPI.Game_Request(CmdId.Request_GlobalStructure_List, (ushort)2437, new Eleon.Modding.PString(""));
                                    }

                                }
                                ItemExchangeSwitch[PlayerInfoReceived.entityId] = "blank";
                            }
                        }
                                System.IO.File.WriteAllText("Content\\Mods\\Xango\\players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", string.Empty);
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "playerName= " + Convert.ToString(PlayerInfoReceived.playerName));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "entityId= " + Convert.ToString(PlayerInfoReceived.entityId));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "permission= " + Convert.ToString(PlayerInfoReceived.permission));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "playfield= " + Convert.ToString(PlayerInfoReceived.playfield));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "startPlayfield= " + Convert.ToString(PlayerInfoReceived.startPlayfield));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "pos= " + Convert.ToString(PlayerInfoReceived.pos.x) + ",  " + Convert.ToString(PlayerInfoReceived.pos.y) + ",  " + Convert.ToString(PlayerInfoReceived.pos.z));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "rot= " + Convert.ToString(PlayerInfoReceived.rot.x) + ",  " + Convert.ToString(PlayerInfoReceived.rot.y) + ",  " + Convert.ToString(PlayerInfoReceived.rot.z));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "clientId= " + Convert.ToString(PlayerInfoReceived.clientId));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "SteamId= " + Convert.ToString(PlayerInfoReceived.steamId));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "SteamOwnerId= " + Convert.ToString(PlayerInfoReceived.steamOwnerId));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "origin= " + Convert.ToString(PlayerInfoReceived.origin));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "ping= " + Convert.ToString(PlayerInfoReceived.ping));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "factionGroup= " + Convert.ToString(PlayerInfoReceived.factionGroup));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "factionRole= " + Convert.ToString(PlayerInfoReceived.factionRole));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "factionId= " + Convert.ToString(PlayerInfoReceived.factionId));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "credits= " + Convert.ToString(PlayerInfoReceived.credits));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "bodyTemp= " + Convert.ToString(PlayerInfoReceived.bodyTemp));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "TempMax= " + Convert.ToString(PlayerInfoReceived.bodyTempMax));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "food= " + Convert.ToString(PlayerInfoReceived.food));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "foodMax= " + Convert.ToString(PlayerInfoReceived.foodMax));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "health= " + Convert.ToString(PlayerInfoReceived.health));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "healthmax= " + Convert.ToString(PlayerInfoReceived.healthMax));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "oxygen= " + Convert.ToString(PlayerInfoReceived.oxygen));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "oxygenMax= " + Convert.ToString(PlayerInfoReceived.oxygenMax));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "radiation= " + Convert.ToString(PlayerInfoReceived.radiation));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "radiationMax= " + Convert.ToString(PlayerInfoReceived.radiationMax));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "stamina= " + Convert.ToString(PlayerInfoReceived.stamina));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "staminaMax= " + Convert.ToString(PlayerInfoReceived.staminaMax));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "exp= " + Convert.ToString(PlayerInfoReceived.exp));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "upgrade= " + Convert.ToString(PlayerInfoReceived.upgrade));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "died= " + Convert.ToString(PlayerInfoReceived.died));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "kills= " + Convert.ToString(PlayerInfoReceived.kills));

                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "producedPrefabs= " + Convert.ToString(PlayerInfoReceived.producedPrefabs));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "bpInFactory= " + Convert.ToString(PlayerInfoReceived.bpInFactory));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "bpRemainingTime= " + Convert.ToString(PlayerInfoReceived.bpRemainingTime));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "bpResourcesInFactory= " + Convert.ToString(PlayerInfoReceived.bpResourcesInFactory));

                        break;
                    case CmdId.Event_Player_Inventory:
                        break;
                    case CmdId.Event_Player_ItemExchange:
                        ItemExchangeInfo exchangeInfo = (ItemExchangeInfo)data;
                        //LogFile("chat.txt", "ItemExchange ID= " + exchangeInfo.id);
                        //LogFile("chat.txt", "ItemExchange Title= " + exchangeInfo.title);
                        //LogFile("chat.txt", "ItemExchange desc= " + exchangeInfo.desc);
                        //LogFile("chat.txt", "ItemExchange items= " + exchangeInfo.items);
                        //LogFile("chat.txt", "ItemExchange buttonText= " + exchangeInfo.buttonText);

                        //ServerSay(exchangeInfo.id, "Start of ItemExchange");
                        if (ItemExchangeSwitch.ContainsKey(exchangeInfo.id)) //This is the new version, now do parse
                        {
                            var Message = ItemExchangeSwitch[exchangeInfo.id].Split(new[] { ' ' }, 3);
                            if (Message[0].StartsWith("/backpack"))
                            {
                                //LogFile("chat.txt", "ItemExchange Backpack");
                                vBackpackDictionary[exchangeInfo.id] = exchangeInfo.items;
                                System.IO.File.WriteAllText("Content\\Mods\\Xango\\players\\EID" + exchangeInfo.id + "\\VirtualBackpack.txt", string.Empty);
                                for (int i = 0; i <= exchangeInfo.items.Count(); i++)
                                {
                                    LogFile("players\\EID" + exchangeInfo.id + "\\VirtualBackpack.txt", Convert.ToString(exchangeInfo.items[i].slotIdx) + "," + Convert.ToString(exchangeInfo.items[i].id) + "," + Convert.ToString(exchangeInfo.items[i].count) + "," + Convert.ToString(exchangeInfo.items[i].ammo) + "," + Convert.ToString(exchangeInfo.items[i].decay));
                                }
                                //step = "itemExchange complete";
                                ItemExchangeSwitch[exchangeInfo.id] = "blank";
                            }
                            else if (Message[0].StartsWith("/send"))
                            {
                                List<string> Recipient = NameFragment(ItemExchangeSwitch[exchangeInfo.id]);
                                //ServerSay(exchangeInfo.id, "Recipient= " + PlayerDictionary[Recipient[0]].EmpyrionID);
                                if (Recipient.Count == 1)
                                {
                                    //ServerSay(exchangeInfo.id, "Recipient[0]= " + PlayerDictionary[Recipient[0]].EmpyrionID);
                                }
                                else
                                { break; }
                                ItemExchangeSwitch[exchangeInfo.id] = "blank";
                                //LogFile("chat.txt", "ItemExchange Send step 1");
                                string timestamp = Convert.ToString((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
                                //LogFile("chat.txt", "ItemExchange Send step 2");
                                using (System.IO.StreamWriter writer = new System.IO.StreamWriter("Content\\Mods\\Xango\\Mail\\" + Convert.ToString(timestamp) + ".txt"))
                                {
                                    //LogFile("chat.txt", "Number of items in ItemStack=" + exchangeInfo.items.Count());
                                    for (int i = 0; i < exchangeInfo.items.Count(); i++)
                                    {
                                        writer.Write(Convert.ToString(exchangeInfo.items[i].slotIdx) + ",");
                                        writer.Write(Convert.ToString(exchangeInfo.items[i].id) + ",");
                                        writer.Write(Convert.ToString(exchangeInfo.items[i].count) + ",");
                                        writer.Write(Convert.ToString(exchangeInfo.items[i].ammo) + ",");
                                        writer.WriteLine(Convert.ToString(exchangeInfo.items[i].decay));
                                        //LogFile("chat.txt", string.Format("End Looping {0}", i));
                                    }
                                    //LogFile("chat.txt", "still in StreamWriter?");
                                }
                                //LogFile("chat.txt", "Finished StreamWriter");
                                string MailEntry = Convert.ToString(timestamp) + "," + Convert.ToString(exchangeInfo.id) + "," + "New" + "," + Convert.ToString(Message[2]);
                                //LogFile("chat.txt", "Mailentry=" + MailEntry);
                                MailDictionary[timestamp] = exchangeInfo.items;
                                ServerSay(PlayerDictionary[Recipient[0]].EmpyrionID, "New Mail Received");
                                /*
                                using (System.IO.StreamWriter writer = new System.IO.StreamWriter("Content\\Mods\\Xango\\players\\EID" + PlayerDictionary[Recipient[0]].EmpyrionID + "\\Mail.txt"))
                                {
                                    writer.WriteLine(MailEntry);
                                }
                                */
                                System.IO.File.AppendAllText("Content\\Mods\\Xango\\players\\EID" + PlayerDictionary[Recipient[0]].EmpyrionID + "\\Mail.txt", MailEntry + Environment.NewLine);
                                //MailDictionary.Add(timestamp, exchangeInfo.items);
                                //LogFile("chat.txt", "Content\\Mods\\Xango\\EID" + PlayerDictionary[Recipient[0]].EmpyrionID + "\\Mail.txt");
                                //Alert(2, Convert.ToInt32(PlayerDictionary[Recipient[0]].EmpyrionID), "New Mail Received", 5);
                                //Messenger("Alert", 2, Convert.ToInt32(PlayerDictionary[Recipient[0]].EmpyrionID), "New Mail Received", 5);

                            }
                            else if (Message[0].StartsWith("/inbox"))
                            {
                                LogFile("chat.txt", "ItemExchange MailInbox?");

                                string[] UserMail = System.IO.File.ReadAllLines("Content\\Mods\\Xango\\players\\EID" + exchangeInfo.id + "\\mail.txt"); //pull up user mail
                                var MailMessage = UserMail[0].Split(new[] { ',' }, 4); //split first line of user mail
                                MailDictionary.Remove(MailMessage[0]);
                                MailMessage = UserMail.Skip(1).ToArray();
                                System.IO.File.WriteAllLines("Content\\Mods\\Xango\\players\\EID" + exchangeInfo.id + "\\mail.txt", MailMessage);


                                /*
                                 var lines = File.ReadAllLines("test.txt");
                                 File.WriteAllLines("test.txt", lines.Skip(1).ToArray());
                                */
                                ItemExchangeSwitch[exchangeInfo.id] = "blank";
                            }
                        }
                        LogFile("chat.txt", "ItemExchange Done");
                        break;
                    case CmdId.Event_Player_Credits:
                        break;
                    case CmdId.Event_Player_ChangedPlayfield:
                        //WIP
                        //trigger asteroid regen
                        break;
                    case CmdId.Event_Player_GetAndRemoveInventory:
                        break;
                    case CmdId.Event_Playfield_List:
                        break;
                    case CmdId.Event_Playfield_Stats:
                        //PlayfieldStats PFStats = (PlayfieldStats)data;
                        //PFData[PFStats.playfield] = PFStats;
                        break;
                    case CmdId.Event_Playfield_Loaded:
                        try
                        {
                            PlayfieldLoad LoadedPlayfield = (PlayfieldLoad)data;
                            //GameAPI.Game_Request(CmdId.Request_Playfield_Stats, (ushort)CmdId.Request_Playfield_Stats, new Eleon.Modding.PString(LoadedPlayfield.playfield));
                            //GameAPI.Game_Request(CmdId.Request_Playfield_Entity_List, (ushort)CmdId.Request_Playfield_Entity_List, new Eleon.Modding.IdPlayfield(LoadedPlayfield.processId,LoadedPlayfield.playfield));
                            //GameAPI.Game_Request(CmdId.Request_GlobalStructure_List, (ushort)2436, new Eleon.Modding.PString(""));
                            GameAPI.Game_Request(CmdId.Request_GlobalStructure_Update, 2436, new Eleon.Modding.PString(LoadedPlayfield.playfield));
                            LogFile("chat.txt", "Load Playfield ProcessID=" + LoadedPlayfield.processId + " Playfield=" + LoadedPlayfield.playfield);
                            try { PlayfieldIDDict[LoadedPlayfield.playfield] = LoadedPlayfield.processId; }
                            catch { LogFile("chat.txt", "Failed adding playfield to PlayfieldIDDict: " + LoadedPlayfield.playfield); }
                        }
                        catch { }
                        break;
                    case CmdId.Event_Playfield_Unloaded:
                        break;
                    case CmdId.Event_Playfield_Entity_List:
                        PlayfieldEntityList PFList = (PlayfieldEntityList)data;
                        break;
                    case CmdId.Event_Dedi_Stats:
                        break;
                    case CmdId.Event_GlobalStructure_List:
                        if (seqNr == 2436)
                        {
                            GlobalStructureList Structs = (GlobalStructureList)data;
                            LogFile("chat.txt", seqNr + " Triggered");
                            foreach (string playfieldName in Structs.globalStructures.Keys)
                            {
                                foreach (GlobalStructureInfo item in Structs.globalStructures[playfieldName])
                                {
                                    if (item.type == 7)
                                    {
                                        LogFile("Chat.txt", "Regenerate Type=" + item.type + " ID=" + item.id + " Called:" + item.name + " On:" + PlayfieldIDDict[playfieldName]);
                                        GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new Eleon.Modding.PString("remoteex pf=" + PlayfieldIDDict[playfieldName] + "' regenerate " + item.id + "'"));
                                    }
                                }
                            }
                        }
                        else if (seqNr == 2437)
                        {
                            GlobalStructureList Structs = (GlobalStructureList)data;
                            LogFile("chat.txt", seqNr + " Triggered");
                            foreach (string playfieldName in Structs.globalStructures.Keys)
                            {
                                //ServerSay(0, "Regenerating Asteroids 0");
                                foreach (GlobalStructureInfo item in Structs.globalStructures[playfieldName])
                                {
                                    //ServerSay(0, "Regenerating Asteroids 1");
                                    if (item.type == 7)
                                    {
                                        //ServerSay(0, "Regenerating Asteroids 2");
                                        LogFile("Chat.txt", "Regenerate Type=" + item.type + " ID=" + item.id + " Called:" + item.name + " On:" + PlayfieldIDDict[playfieldName]);
                                        GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new Eleon.Modding.PString("remoteex pf=" + PlayfieldIDDict[playfieldName] + "' regenerate " + item.id + "'"));
                                    }
                                }

                            }
                        }
                            break;
                    case CmdId.Event_Entity_PosAndRot:
                        break;
                    case CmdId.Event_Faction_Changed:
                        /*
                        FactionChangeInfo factionChange = (FactionChangeInfo)data;
                        GameAPI.Game_Request(CmdId.Request_Get_Factions, (ushort)CmdId.Request_Get_Factions, new Id(1));
                        */
                        break;
                    case CmdId.Event_Get_Factions:
                        /*
                        FactionInfoList factioninfo = (FactionInfoList)data;
                        FactionListDump = factioninfo;
                        //FactionInfoDict.Remove("FactionDump");
                        //FactionInfoDict.Add("FactionDump", factioninfo);
                        */
                        break;
                    case CmdId.Event_Statistics:
                        break;
                    case CmdId.Event_NewEntityId:
                        break;
                    case CmdId.Event_Player_DisconnectedWaiting:
                        Id pdw = new Id();
                        LogFile("ERROR.txt", "FAILED LOGIN:  Player=" + pdw.id);
                        break;
                    case CmdId.Event_ChatMessage:
                        ChatInfo ci = (ChatInfo)data;
                        LogFile("Chat.txt", ci.playerId + " SAYS: " + ci.msg);
                        //GameAPI.Game_Request(CmdId.Request_Player_List, (ushort)CmdId.Request_Player_List, null);
                        if (ci.msg.StartsWith("s! "))
                        {
                            ci.msg = ci.msg.Remove(0, 3);
                        }
                        //LogFile("Chat.txt", ci.playerId + " s!  strip check?:" + ci.msg);
                        var chatmsg = ci.msg.Split(' ');
                        chatmsg[0] = chatmsg[0].ToLower();
                        string cimsg = string.Join(" ", chatmsg);
                        //string cimsg = ciMsg[0].ToLower() + " " + ciMsg[1];
                        //LogFile("Chat.txt", ci.playerId + " SAYS: " + cimsg);
                        //string check = Message[0].ToLower();

                        //string.Compare(cimsg, "/backpack", StringComparison.OrdinalIgnoreCase) == 0;
                        //string joe = string.Join(" ", cimsg);

                        if (cimsg.StartsWith("/backpack"))
                        {
                            ItemExchangeSwitch[ci.playerId] = cimsg;
                            //BackpackChatDictionary[ci.playerId] = ci.msg;
                            GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)1167, new Eleon.Modding.Id(ci.playerId));
                            //step = "Request Playerinfo";
                        }
                        else if (cimsg.StartsWith("/find"))
                        {
                            //LogFile("chat.txt", "/find command received");
                            ItemExchangeSwitch[ci.playerId] = cimsg;
                            GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)1168, new Eleon.Modding.Id(ci.playerId));
                        }
                        else if (cimsg.StartsWith("/inbox"))
                        {
                            ItemExchangeSwitch[ci.playerId] = cimsg;
                            GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)1169, new Eleon.Modding.Id(ci.playerId));
                            //LogFile("chat.txt", "/inbox command received");
                            //step = "Request Playerinfo";
                        }
                        else if (cimsg.StartsWith("/send"))
                        {
                            //LogFile("chat.txt", "/send command received");
                            ItemExchangeSwitch[ci.playerId] = cimsg;
                            GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)1170, new Eleon.Modding.Id(ci.playerId));
                            //step = "Request Playerinfo";
                            //LogFile("chat.txt", "/send complete, requesting player info");
                        }
                        else if (cimsg.StartsWith("/reward"))
                        {
                            ItemExchangeSwitch[ci.playerId] = cimsg;
                            GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)1173, new Eleon.Modding.Id(ci.playerId));
                            //step = "Request Playerinfo";
                        }
                        else if (cimsg.StartsWith("/resource"))
                        {
                            ItemExchangeSwitch[ci.playerId] = cimsg;
                            GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)1174, new Eleon.Modding.Id(ci.playerId));
                            //LogFile("chat.txt", "/test");
                            //string msg = "Loading, please stand by...";
                            //GameAPI.Game_Request(CmdId.Request_ShowDialog_SinglePlayer, (ushort)CmdId.Request_ShowDialog_SinglePlayer, new Eleon.Modding.IdMsgPrio(ci.playerId, msg, 0, 10));
                            /*
                            GameAPI.Game_Request(CmdId.Request_Player_SetPlayerInfo, (ushort)1171, new Eleon.Modding.PlayerInfoSet()
                            {
                                entityId = ci.playerId,
                                oxygenMax = 375,
                                radiation = 0,
                                food = +5000,
                                stamina = +5000,
                                bodyTemp = 30,
                                health = +5000
                            });
                            */
                            //GameAPI.Game_Request(CmdId.Request_Playfield_Entity_List, (ushort)CmdId.Request_Playfield_Entity_List, new Eleon.Modding.IdPlayfield("Akua"));
                            //GameAPI.Game_Request(CmdId.Request_Playfield_Entity_List, (ushort)CmdId.Request_Playfield_Entity_List, new Eleon.Modding.IdPlayfield(ci.playerId, "Akua"));
                            /*
                            foreach (string thing in PlayerDictionary.Keys)
                            {
                                GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new Eleon.Modding.PString("SAY p:" + ci.playerId + " '" + thing + "'"));
                                GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_ConsoleCommand, new Eleon.Modding.PString("SAY p:" + ci.playerId + " '" + PlayerDictionary[thing].EmpyrionID + "'"));
                                try { GameAPI.Game_Request(CmdId.Request_Playfield_List, (ushort)CmdId.Request_Playfield_List, "blank"); }catch { };
                            }

*/
                        }

                        else if (cimsg.StartsWith("/factory"))
                        {
                            //LogFile("chat.txt", "/factory");
                            ItemExchangeSwitch[ci.playerId] = cimsg;
                            GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)1172, new Eleon.Modding.Id(ci.playerId));
                        }
                        else if (cimsg.StartsWith("/admin"))
                        {
                            //ItemExchangeSwitch[ci.playerId] = cimsg;
                            //GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)1175, new Eleon.Modding.Id(ci.playerId));
                        };

                            break;
                    case CmdId.Event_Structure_BlockStatistics:
                        break;
                    case CmdId.Event_AlliancesAll:
                        break;
                    case CmdId.Event_AlliancesFaction:
                        break;
                    case CmdId.Event_BannedPlayers:
                        break;
                    case CmdId.Event_TraderNPCItemSold:
                        break;
                    case CmdId.Event_Ok:
                        /*
                        LogFile("EventOK.txt", "Event OK: " + step);
                        break;
                        */
                        break;
                    case CmdId.Event_Error:
                        ErrorInfo err = (ErrorInfo)data;
                        ErrorType err2 = (ErrorType)data;
                        //try { Messenger("Alert", 0, 0, "Error Info " + Convert.ToString(err), 1000); } catch { };
                        //try { Messenger("Alert", 0, 0, "Error Type " + Convert.ToString(err2), 1000); } catch { };
                        
                        LogFile("ERROR.txt", "ERROR: " + Convert.ToString(err2) + ": " + Convert.ToString(err));
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

