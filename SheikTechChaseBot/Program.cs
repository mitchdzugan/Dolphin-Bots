using dolphinBotLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SheikTechChaseBot
{
    class Program
    {
        public static void MyGCPadCallback(GCPadStatus currentStatus, GCPadStatus previousStatus, StreamWriter writer)
        {

        }

        static void Main(string[] args)
        {
            dolphinBot bot = new dolphinBot(new dolphinBot.GCPadCallback[4] { 
                MyGCPadCallback, dolphinBot.DefaultGCPadCallback, dolphinBot.DefaultGCPadCallback, dolphinBot.DefaultGCPadCallback
            });
        }
    }
}
