using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eleon.Modding;
using ProtoBuf;
using System.Collections;
using YamlDotNet.Serialization;

namespace EntityOwnership
{
    public class MyEmpyrionMod : ModInterface
    {
        public string ModVersion = "EntityOwnership v0.0.1";
        public string ModPath = "Content\\Mods\\EntityOwnership\\";
        public string SaveGameFolder = "";
        public string SaveGameName = "";
        public string GameFolderPath = "";


        private void LogFile(string FileName, string FileData)
        {
            if (!System.IO.File.Exists(ModPath + FileName))
            {
                System.IO.File.Create(ModPath + FileName);
            }
            string FileData2 = FileData + Environment.NewLine;
            System.IO.File.AppendAllText(ModPath + FileName, FileData2);
        }

        public void Game_Start(ModGameAPI dediAPI)
        {
            System.IO.File.WriteAllText(ModPath+ "debug.txt", "");
            System.IO.File.WriteAllText(ModPath + "Entities.csv", "playfield,id,name,type,faction,blocks,devices,pos,rot,core,powered,docked,touched_time,touched_ticks,touched_name,touched_id,saved_time,saved_ticks,add_info\r\n");

            if (System.IO.File.Exists("dedicated.yaml"))
            {
                //LogFile("Debug.txt", "Dedicated.yaml exists");
                string[] DedicatedText = System.IO.File.ReadAllLines("Dedicated.yaml");
                //LogFile("Debug.txt", "Dedicated.yaml read");
                foreach (string line in DedicatedText)
                {
                    if (line.StartsWith("    GameName:"))
                    {
                        //LogFile("Debug.txt", "SaveGameName=" + line);
                        SaveGameName = line.Trim().Split(':')[1];
                        SaveGameName = SaveGameName.Trim().Split('#')[0];
                        SaveGameName = SaveGameName.Trim();
                    }
                    else if (line.StartsWith("    SaveDirectory:"))
                    {
                        //LogFile("Debug.txt", "SaveGamePath=" + line);
                        GameFolderPath = line;
                        GameFolderPath = GameFolderPath.Trim().Split(':')[1];
                        GameFolderPath = GameFolderPath.Trim().Split('#')[0];
                        GameFolderPath = GameFolderPath.Trim();
                    }
                    else if (line.StartsWith("    # SaveDirectory:"))
                    {
                        SaveGameFolder = "Saves";
                    }
                }
                //LogFile("Debug.txt", "SaveGamePath=" + SaveGameFolder);
                //LogFile("Debug.txt", "SaveGameName=" + SaveGameName);
                /*

                YamlReaders.YamlData DedicatedYaml = YamlReaders.Retrieve("dedicated.yaml");


                GameFolderPath = DedicatedYaml.ServerConfig.SaveDirectory + "\\Games\\" + DedicatedYaml.GameConfig.GameName;
                LogFile("Debug.txt", "Dedicated.yaml Read");

                if (System.IO.Directory.Exists(DedicatedYaml.ServerConfig.SaveDirectory))
                {
                    string GameFolderPath = DedicatedYaml.ServerConfig.SaveDirectory + "\\Games\\" + DedicatedYaml.GameConfig.GameName;
                }
                else
                {
                    string GameFolderPath = "Saves\\Games\\" + DedicatedYaml.GameConfig.GameName;
                }*/
                //LogFile("Debug.txt", SaveGameFolder + "\\Games\\" + SaveGameName + "\\Shared");

                String[] EntityFilenames = System.IO.Directory.GetFiles(SaveGameFolder + "\\Games\\"+ SaveGameName + "\\Shared");
                foreach (string fileName in EntityFilenames)
                {
                    //LogFile("debug.txt", fileName);
                    string FileName = fileName.Replace("\\", "\\\\");
                    string[] ThisFile = System.IO.File.ReadAllLines(FileName);
                    System.IO.File.AppendAllText(ModPath + "\\Entities.csv", ThisFile[1] + "\r\n");

                }
            }
        }
        public void Game_Event(CmdId cmdId, ushort seqNr, object data)
        {
        }
        public void Game_Exit()
        {
        }
        public void Game_Update()
        {
        }
    }
}
