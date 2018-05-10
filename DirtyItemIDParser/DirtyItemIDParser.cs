using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eleon.Modding;
using ProtoBuf;
using System.Collections;

namespace DirtyItemIDParser
{
    public class MyEmpyrionMod : ModInterface
    {
        //public object ModFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        private void LogFile(string FileName, string FileData)
        {
            if (!System.IO.File.Exists("Content\\Mods\\DirtyItemIDParser\\" + FileName))
            {
                System.IO.File.Create("Content\\Mods\\DirtyItemIDParser\\" + FileName);
            }
            string FileData2 = FileData + Environment.NewLine;
            System.IO.File.AppendAllText("Content\\Mods\\DirtyItemIDParser\\" + FileName, FileData2);
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
            System.IO.File.WriteAllText("Content\\Mods\\DirtyItemIDParser\\blocks.csv", "");
            System.IO.File.WriteAllText("Content\\Mods\\DirtyItemIDParser\\items.csv", "");
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
