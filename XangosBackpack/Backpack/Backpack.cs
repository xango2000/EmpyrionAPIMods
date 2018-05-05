using System;
using System.Collections.Generic;
using System.Linq;
using Eleon.Modding;
using ProtoBuf;


namespace VirtualBackpack
{
    public class VirtualBackpack : ModInterface
    {
        ModGameAPI GameAPI;
        public string ModVersion = "VirtualBackpack v2.0.0";
        public Dictionary<int, requestData> storedRequest = new Dictionary<int, requestData> { };
        public int CurrentSeqNr = 500;
        public Dictionary<int, onlinePlayers> LongtermStorage = new Dictionary<int, onlinePlayers> { };
        //public object ModFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        private void LogFile(string FileName, string FileData)
        {
            if (!System.IO.File.Exists("Content\\Mods\\VirtualBackpack\\" + FileName))
            {
                System.IO.File.Create("Content\\Mods\\VirtualBackpack\\" + FileName);
            }
            string FileData2 = FileData + Environment.NewLine;
            System.IO.File.AppendAllText("Content\\Mods\\VirtualBackpack\\" + FileName, FileData2);
        }

        public class requestData
        {
            public ChatInfo chatData;
            public GlobalStructureList structsList;
            public IdStructureBlockInfo vesselInfo;
            public PlayerInfo PlayerInfo;
            public GlobalStructureInfo piloting;
            public Id ID;
        }

        public class onlinePlayers
        {
            public PlayerInfo PlayerInfo;
            public ItemStack[] Backpack;
            public ItemStack[] vBackpack;
            public ItemStack[] Toolbar;
        }

        public ItemStack[] buildItemStack(string fileName)
        {
            string[] bagLines = System.IO.File.ReadAllLines(fileName);
            int itemStackSize = bagLines.Count();
            ItemStack[] itStack = new ItemStack[itemStackSize];
            for (int i = 0; i < itemStackSize; ++i)
            {
                string[] bagLinesSplit = bagLines[i].Split(',');
                itStack[i] = new ItemStack();
                itStack[i].slotIdx = Convert.ToByte(bagLinesSplit[0]);
                itStack[i].id = Convert.ToByte(bagLinesSplit[1]);
                itStack[i].count = Convert.ToByte(bagLinesSplit[2]);
                itStack[i].ammo = Convert.ToInt32(bagLinesSplit[3]);
                itStack[i].decay = Convert.ToInt32(bagLinesSplit[4]);
            }
            return itStack;
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
                if (storedRequest.ContainsKey(newSeqNr)) { Fail = true; }
            } while (Fail == true);
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
                    case CmdId.Event_Player_Connected:
                        Id PlayerConnected = (Id)data;
                        CurrentSeqNr = SeqNrGenerator(CurrentSeqNr);
                        if (LongtermStorage.ContainsKey(PlayerConnected.id))
                        {
                        }
                        else
                        {
                            requestData StoreThisInfo = new requestData();
                            //StoreThisInfo = storedRequest[seqNr];
                            StoreThisInfo.ID = PlayerConnected;
                            CurrentSeqNr = SeqNrGenerator(CurrentSeqNr);
                            storedRequest[CurrentSeqNr] = StoreThisInfo;
                            GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)CurrentSeqNr, new Id(PlayerConnected.id));
                        }
                        break;
                    case CmdId.Event_Player_Disconnected:
                        Id pd = (Id)data;
                        break;
                    case CmdId.Event_Player_Info:
                        PlayerInfo PlayerInfoReceived = (PlayerInfo)data;
                        onlinePlayers PlayerData = new onlinePlayers { };
                        PlayerData.PlayerInfo = PlayerInfoReceived;
                        LongtermStorage.Add(PlayerInfoReceived.clientId, PlayerData);

                        if (storedRequest.ContainsKey(seqNr))
                        {
                            PlayerInfo playerInfo = (PlayerInfo)data;
                            if (storedRequest[seqNr].chatData.msg == "/vb")
                            {
                                if (storedRequest[seqNr].chatData.playerId == playerInfo.entityId)
                                {
                                    onlinePlayers StoreThisInfo = new onlinePlayers();
                                    StoreThisInfo = LongtermStorage[seqNr];
                                    StoreThisInfo.PlayerInfo = playerInfo;
                                    CurrentSeqNr = SeqNrGenerator(CurrentSeqNr);
                                    LongtermStorage[CurrentSeqNr] = StoreThisInfo;
                                }
                            }
                        }

                                    break;
                    case CmdId.Event_Player_ItemExchange:
                        ItemExchangeInfo exchangeInfo = (ItemExchangeInfo)data;
                        break;
                    case CmdId.Event_Player_DisconnectedWaiting:
                        Id pdw = new Id();
                        LogFile("ERROR.txt", "Failed Login: " + pdw.id);
                        break;
                    case CmdId.Event_ChatMessage:
                        ChatInfo chatMessage = (ChatInfo)data;
                        if (chatMessage.msg.ToLower().StartsWith("/backpack"))
                        {
                            CurrentSeqNr = SeqNrGenerator(CurrentSeqNr);
                        }
                        else if (chatMessage.msg.ToLower().StartsWith("/vb"))
                        {
                            CurrentSeqNr = SeqNrGenerator(CurrentSeqNr);
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
        }
    }
}