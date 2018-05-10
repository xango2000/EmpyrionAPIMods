using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eleon.Modding;

namespace HelpCommand
{
    public class HelpCommand : ModInterface
    {
        ModGameAPI GameAPI;
        public string ModVersion = "HelpCommand v0.0.3";
        public int CurrentSeqNr = 500;
        public Dictionary<int, RequestData> storedRequest = new Dictionary<int, RequestData> { };
        public Dictionary<int, OnlinePlayers> LongtermStorage = new Dictionary<int, OnlinePlayers> { };
        public List<string> ModFolders = new List<string> { };
        public Dictionary<string, string> Nicknames = new Dictionary<string, string> { };
        public string ThisModFolder = "content\\mods\\helpcommand";

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

        private void LogFile(string FileName, string FileData)
        {
            if (!System.IO.File.Exists(ThisModFolder +"\\" + FileName))
            {
                System.IO.File.Create(ThisModFolder + "\\" + FileName);
            }
            string FileData2 = FileData + Environment.NewLine;
            System.IO.File.AppendAllText(ThisModFolder + "\\" + FileName, FileData2);
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

        public void ShowDialog(string filename, ChatInfo player)
        {
            string[] FileData = System.IO.File.ReadAllLines(filename);
            string concatFileData = "";
            foreach (string line in FileData)
            {
                concatFileData = concatFileData + line + "\r\n";
            }
            CurrentSeqNr = SeqNrGenerator(CurrentSeqNr);
            GameAPI.Game_Request(CmdId.Request_ShowDialog_SinglePlayer, (ushort)CurrentSeqNr, new IdMsgPrio(player.playerId, concatFileData, 1, 20));

        }


        public void Game_Start(ModGameAPI gameAPI)
        {
            System.IO.File.WriteAllText(ThisModFolder + "\\Mods.txt", "");
            System.IO.File.WriteAllText(ThisModFolder + "\\debug.txt", "");

            GameAPI = gameAPI;
            String[] Directories = System.IO.Directory.GetDirectories("Content\\Mods\\");
            foreach (string Mod in Directories)
            {
                ModFolders.Add(Mod);
                if (System.IO.File.Exists(Mod + "\\help.txt"))
                {
                    string[] FileData = System.IO.File.ReadAllLines(Mod + "\\help.txt");
                    LogFile("Mods.txt", FileData[0]);
                    try
                    {
                        string line2 = FileData[1];
                        if (line2.StartsWith("Nicknames:"))
                        {
                            string[] nicknameline = line2.Split(':');
                            string[] nicknames = nicknameline[1].Split(',');
                            foreach (string nickname in nicknames)
                            {
                                string nicknameTrim = nickname.Trim().ToLower();
                                if (Nicknames.ContainsKey(nicknameTrim))
                                { }
                                else
                                {
                                    Nicknames.Add(nicknameTrim, Mod);
                                }
                            }
                        }
                    }
                    catch { }

                }
                string folderstring = Mod.Replace("\\", "\\\\");
                string[] filenames = System.IO.Directory.GetFiles(folderstring);
                foreach (string filename in filenames)
                {
                    //LogFile("Debug.txt", filename);
                }

            }
        }
        public void Game_Update()
        {

        }
        public void Game_Event(CmdId cmdId, ushort seqNr, object data)
        {
            {
                try
                {
                    switch (cmdId)
                    {

                        case CmdId.Event_ChatMessage:
                            ChatInfo chatMessage = (ChatInfo)data;
                            if (chatMessage.msg.ToLower() == "/help")
                            {
                                ShowDialog(ThisModFolder + "\\Help.txt", chatMessage);
                            }
                            else if (chatMessage.msg.ToLower() == "/mods")
                            {
                                ShowDialog(ThisModFolder+ "\\Mods.txt", chatMessage);
                            }
                            else if (chatMessage.msg.ToLower().StartsWith("/help"))
                            {
                                //mod help
                                string[] splitChat = chatMessage.msg.ToLower().Split(' ');
                                if (Nicknames.ContainsKey(splitChat[1]))
                                {
                                    ShowDialog(Nicknames[splitChat[1]] + "\\Help.txt", chatMessage);
                                }
                            }
                            else if (chatMessage.msg.ToLower().StartsWith("/"))
                            {
                                if (chatMessage.msg.Contains(' '))
                                {
                                    string[] splitChat = chatMessage.msg.ToLower().Split(' ');
                                    String[] Directories = System.IO.Directory.GetDirectories(ThisModFolder);
                                    Directories = Directories.Select(s => s.ToLowerInvariant()).ToArray();
                                    //LogFile("Debug.txt", ThisModFolder + "\\" + splitChat[0].Remove(0, 1) + "\\" + splitChat[1] + ".txt");
                                    //LogFile("Debug.txt", "Content\\Mods\\HelpCommand\\" + splitChat[0].Remove(0, 1));
                                    foreach( string directory in Directories)
                                    {
                                        //LogFile("Debug.txt", directory);
                                    }
                                    if (Directories.Contains(ThisModFolder + "\\" + splitChat[0].Remove(0, 1)))
                                    {
                                        //LogFile("Debug.txt", "test 1");
                                        if (splitChat.Count() == 3)
                                        {
                                        }
                                        else if (splitChat.Count() == 2)
                                        {
                                            //LogFile("Debug.txt", "test 2");
                                            if (System.IO.File.Exists(ThisModFolder + "\\" + splitChat[0].Remove(0, 1) + "\\" + splitChat[1] + ".txt"))
                                            {
                                                //LogFile("Debug.txt", "test 3");
                                                ShowDialog(ThisModFolder + "\\" + splitChat[0].Remove(0, 1) + "\\" + splitChat[1] + ".txt", chatMessage);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (System.IO.File.Exists(ThisModFolder + "\\" + chatMessage.msg.Remove(0, 1) + "\\Main.txt"))
                                    {
                                        ShowDialog(ThisModFolder + "\\" + chatMessage.msg.Remove(0, 1) + "\\Main.txt", chatMessage);
                                    }

                                }
                            }
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
        }
        public void Game_Exit()
        {

        }

    }
}
