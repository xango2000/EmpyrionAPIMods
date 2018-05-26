using System;
using System.Collections.Generic;
using System.Linq;
using Eleon.Modding;
using ProtoBuf;
using System.IO;


namespace VirtualBackpack
{
    public class VirtualBackpack : ModInterface
    {
        ModGameAPI GameAPI;
        public string ModVersion = "VirtualBackpack v2.0.0";
        public string ModPath = "Content\\Mods\\VirtualBackpack\\";
        public Dictionary<int, RequestData> SeqNrStorage = new Dictionary<int, RequestData> { };
        public int CurrentSeqNr = 500;
        public Dictionary<int, OnlinePlayers> LongtermStorage = new Dictionary<int, OnlinePlayers> { };
        //public object ModFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        ConfigYaml config;

        public bool Debug = true;

        //internal YamlSerializer.Root Config { get => config; set => config = value; }
        public void LogFile(string FileName, string FileData)
        {
            if (Debug == true)
            {
                FileInfo file = new FileInfo(ModPath + FileName);
                file.Directory.Create();
                string FileData2 = FileData + Environment.NewLine;
                File.AppendAllText(ModPath + FileName, FileData2);
            }
        }

        public class RequestData
        {
            public ChatInfo chatData;
            public GlobalStructureList structsList;
            public IdStructureBlockInfo vesselInfo;
            public PlayerInfo PlayerInfo;
            public GlobalStructureInfo piloting;
            public Id ID;
        }

        public class OnlinePlayers
        {
            public PlayerInfo PlayerInfo;
            public ItemStack[] Backpack;
            public ItemStack[] vBackpack;
            public ItemStack[] Toolbar;
        }

        public ItemStack[] BuildItemStack(string job)
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

        /*
        public ItemStack[] BuildItemStack(string fileName)
        {
            string[] bagLines = File.ReadAllLines(fileName);
            int itemStackSize = bagLines.Count();
            ItemStack[] itStack = new ItemStack[itemStackSize];
            for (int i = 0; i < itemStackSize; ++i)
            {
                //string[] bagLinesSplit = bagLines[i].Split(',');
                itStack[i] = new ItemStack(){
                slotIdx = Convert.ToByte(bagLinesSplit[0]),
                id = Convert.ToByte(bagLinesSplit[1]),
                count = Convert.ToByte(bagLinesSplit[2]),
                ammo = Convert.ToInt32(bagLinesSplit[3]),
                decay = Convert.ToInt32(bagLinesSplit[4])
                }
            }
            
            return itStack;
        }*/


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
                if (SeqNrStorage.ContainsKey(newSeqNr)) { Fail = true; }
            } while (Fail == true);
            return newSeqNr;
        }

        public void Game_Start(ModGameAPI gameAPI)
        {
            GameAPI = gameAPI;
            //config = ConfigYaml.Retrieve(ModPath + "Config.yaml");
            LogFile("Debug.txt", "Server Starting");
            ConfigYaml.Root config = ConfigYaml.Retrieve(ModPath + "Config.yaml");
            //KnownEntities.Contact test = KnownEntities.Retrieve("Content\\Mods\\ActiveRadar\\Players\\test.yaml");
            LogFile("Debug.txt", "Server Start OK");

        }
        public void Game_Event(CmdId cmdId, ushort seqNr, object data)
        {
            try
            {
                switch (cmdId)
                {
                    case CmdId.Event_Player_Connected:
                        Id PlayerConnected = (Id)data;
                        CurrentSeqNr = SeqNrGenerator(CurrentSeqNr);
                        if (LongtermStorage.ContainsKey(PlayerConnected.id))
                        {
                        }
                        else
                        {
                            RequestData StoreThisInfo = new RequestData()
                            {
                                //StoreThisInfo = storedRequest[seqNr];
                                ID = PlayerConnected
                            };
                            CurrentSeqNr = SeqNrGenerator(CurrentSeqNr);
                            SeqNrStorage[CurrentSeqNr] = StoreThisInfo;
                            GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)CurrentSeqNr, new Id(PlayerConnected.id));
                        }
                        break;
                    case CmdId.Event_Player_Disconnected:
                        Id pd = (Id)data;
                        break;
                    case CmdId.Event_Player_Info:
                        PlayerInfo PlayerInfoReceived = (PlayerInfo)data;
                        OnlinePlayers PlayerData = new OnlinePlayers { };
                        PlayerData.PlayerInfo = PlayerInfoReceived;
                        LongtermStorage[PlayerInfoReceived.clientId] = PlayerData;

                        if (SeqNrStorage.ContainsKey(seqNr))
                        {
                            try
                            {
                                if (SeqNrStorage[seqNr].chatData.msg == "/vb")
                                {
                                    if (SeqNrStorage[seqNr].chatData.playerId == PlayerInfoReceived.entityId)
                                    {
                                        ItemStack[] Backpack = new ItemStack[] { };
                                        if (File.Exists(ModPath + "VirtualBackpacks\\" + PlayerInfoReceived.steamId + "\\VirtualBackpack.csv"))
                                        {
                                            Backpack = BuildItemStack(ModPath + "VirtualBackpacks\\" + PlayerInfoReceived.steamId + "\\VirtualBackpack.csv");
                                        }
                                        else
                                        {

                                            FileInfo file = new FileInfo(ModPath + "VirtualBackpacks\\" + PlayerInfoReceived.steamId + "\\VirtualBackpack.csv");
                                            file.Directory.Create();

                                            //try { File.Create(ModPath + "Players\\" + PlayerInfoReceived.steamId); } catch { }
                                            File.AppendAllText(ModPath + "VirtualBackpacks\\" + PlayerInfoReceived.steamId + "\\VirtualBackpack.csv", "");
                                        }
                                        RequestData StoreThisInfo = new RequestData();
                                        StoreThisInfo = SeqNrStorage[seqNr];
                                        SeqNrStorage.Remove(seqNr);
                                        StoreThisInfo.PlayerInfo = PlayerInfoReceived;
                                        CurrentSeqNr = SeqNrGenerator(CurrentSeqNr);
                                        SeqNrStorage[CurrentSeqNr] = StoreThisInfo;
                                        GameAPI.Game_Request(CmdId.Request_Player_ItemExchange, (ushort)CurrentSeqNr, new ItemExchangeInfo(PlayerInfoReceived.entityId, "Virtual Backpack", "Extra Storage, Yay!!", "Close", Backpack));
                                        //GameAPI.Game_Request(CmdId.Request_Player_ItemExchange, (ushort)CurrentSeqNr, new ItemExchangeInfo(PlayerInfoReceived.entityId, "Virtual Backpack", "Extra Storage, Yay!!", "Close", SeqNrStorage[seqNr].PlayerInfo.bag));
                                    }
                                }
                            }
                            catch { }
                        }

                                    break;
                    case CmdId.Event_Player_ItemExchange:
                        ItemExchangeInfo exchangeInfo = (ItemExchangeInfo)data;
                        if (SeqNrStorage.ContainsKey(seqNr))
                        {
                            if (SeqNrStorage[seqNr].chatData.msg == "/vb")
                            {
                                if (SeqNrStorage[seqNr].chatData.playerId == exchangeInfo.id)
                                {
                                    File.WriteAllText(ModPath + "VirtualBackpacks\\" + SeqNrStorage[seqNr].PlayerInfo.steamId + "\\VirtualBackpack.csv", "");
                                    foreach (ItemStack Stack in exchangeInfo.items)
                                    {
                                        string FileData = Stack.slotIdx + "," + Stack.id + "," + Stack.count + "," + Stack.ammo + "," + Stack.decay + "\r\n";
                                        File.AppendAllText(ModPath + "VirtualBackpacks\\" + SeqNrStorage[seqNr].PlayerInfo.steamId + "\\VirtualBackpack.csv", FileData);
                                    }
                                    SeqNrStorage.Remove(seqNr);
                                }
                            }
                        }
                        break;
                    case CmdId.Event_Player_DisconnectedWaiting:
                        Id pdw = new Id();
                        LogFile("ERROR.txt", "Failed Login: " + pdw.id);
                        break;
                    case CmdId.Event_ChatMessage:
                        ChatInfo chatMessage = (ChatInfo)data;
                        LogFile("Debug.txt", "Chat Message Received");
                        LogFile("Debug.txt", chatMessage.msg.ToLower());

                        if (chatMessage.msg.ToLower().StartsWith("/backpack"))
                        {
                            CurrentSeqNr = SeqNrGenerator(CurrentSeqNr);
                        }
                        else if (chatMessage.msg.ToLower().StartsWith("/vb"))
                        {
                            RequestData StoreThisInfo = new RequestData();
                            StoreThisInfo.chatData = chatMessage;
                            CurrentSeqNr = SeqNrGenerator(CurrentSeqNr);
                            SeqNrStorage[CurrentSeqNr] = StoreThisInfo;
                            GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)CurrentSeqNr, new Id(chatMessage.playerId));
                        }
                        else if (chatMessage.msg.ToLower().StartsWith("/toolbar"))
                        {
                            CurrentSeqNr = SeqNrGenerator(CurrentSeqNr);
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
                //ConfigYaml.WriteYaml(ModPath + "Config.yaml", config);
            }
            catch (Exception ex)
            {
                LogFile("ERROR.txt", "Source: " + ex.Source);
                LogFile("ERROR.txt", "Message: " + ex.Message);
                LogFile("ERROR.txt", "TargetSite: " + ex.TargetSite);
                LogFile("ERROR.txt", "StackTrace: " + ex.StackTrace);
                LogFile("ERROR.txt", "Data: " + ex.Data);
                LogFile("ERROR.txt", "HelpLink: " + ex.HelpLink);
                LogFile("ERROR.txt", "InnerException: " + ex.InnerException);
                LogFile("ERROR.txt", "");
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