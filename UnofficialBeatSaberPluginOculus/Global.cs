using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UnofficialLeaderBoardPlugin
{
    public static class Global
    {
        public static string playerId;
        public static string playerName;
        public static string playerImage;
        public static bool steam;
        public static void Log(string data)
        {

            File.AppendAllText(@"LeaderBoardPluginLog.txt", data + Environment.NewLine);
        }
    }
  
}
