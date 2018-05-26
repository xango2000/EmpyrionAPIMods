using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eleon.Modding;
using ProtoBuf;
using System.Collections;

namespace DirtyItemIDParser
{
    public class MyEmpyrionMod : ModInterface
    {
        //ModGameAPI GameAPI;
        public string ModVersion = "ActiveRadar v0.0.3";
        public string ModPath = "Content\\Mods\\DirtyItemIDParser\\";

        private void LogFile(string FileName, string FileData)
        {
            if (!System.IO.File.Exists(ModPath + FileName))
            {
                System.IO.File.Create(ModPath + FileName);
            }
            string FileData2 = FileData + Environment.NewLine;
            System.IO.File.AppendAllText(ModPath + FileName, FileData2);
        }

        public void FileCFG()
        {
            if (System.IO.File.Exists("Content\\Configuration\\Config.ecf"))
            {
                string[] filedata = System.IO.File.ReadAllLines("Content\\Configuration\\Config.ecf");
                foreach (string line in filedata)
                {
                    //LogFile("debug.txt", "new line=" + line);

                    if (line.StartsWith("{ Block"))
                    {
                        string[] piece = line.Split(' ');
                        string itemID = piece[3];
                        string itemName = piece[5];
                        LogFile("blocks.csv", itemID + itemName);
                    }
                    else if (line.StartsWith("{ Item"))
                    {
                        string[] piece = line.Split(' ');
                        string itemID = piece[3];
                        string itemName = piece[5];
                        LogFile("items.csv", itemID + itemName);
                    }
                }
            }
            else
            {
                string[] filedata = System.IO.File.ReadAllLines("Content\\Configuration\\Config_Example.ecf");
                foreach (string line in filedata)
                {
                    if (line.StartsWith("{ Block"))
                    {
                        string[] piece = line.Split(' ');
                        string itemID = piece[3];
                        string itemName = piece[5];
                        LogFile("blocks.csv", itemID + itemName);
                    }
                    else if (line.StartsWith("{ Item"))
                    {
                        string[] piece = line.Split(' ');
                        string itemID = piece[3];
                        string itemName = piece[5];
                        LogFile("items.csv", itemID + itemName);
                    }
                }

            }
        }


        public void Game_Start(ModGameAPI dediAPI)
        {
            //GameAPI = gameAPI;
            System.IO.File.WriteAllText(ModPath + "blocks.csv", "");
            System.IO.File.WriteAllText(ModPath + "items.csv", "");
            FileCFG();
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
