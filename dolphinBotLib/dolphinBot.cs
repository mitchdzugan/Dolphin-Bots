using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace dolphinBotLib
{
    public class GCPadStatus
    {
        public bool A, B, X, Y, Z, Start, L, R, Dpad_Up, Dpad_Left, Dpad_Right, Dpad_Down;
        public int Joy_Horiz, Joy_Vert, C_Horiz, C_Vert, L_Analog, R_Analog, Frame, ID;
        public GCPadStatus(int f, int i)
        {
            Frame = f;
            ID = i;
            A = false;
            B = false;
            X = false;
            Y = false;
            Z = false;
            Start = false;
            L = false;
            R = false;
            Dpad_Down = false;
            Dpad_Left = false;
            Dpad_Up = false;
            Dpad_Right = false;
            Joy_Horiz = 128;
            Joy_Vert = 128;
            C_Horiz = 128;
            C_Vert = 128;
            L_Analog = 0;
            R_Analog = 0;
        }
        public GCPadStatus(string packet)
        {
            string[] packetArr = packet.Split(new char[2] { ';', '\n' });
            Frame = Convert.ToInt32(packetArr[0]);
            ID = Convert.ToInt32(packetArr[1]);
            Joy_Horiz = Convert.ToInt32(packetArr[3]);
            Joy_Vert = Convert.ToInt32(packetArr[4]);
            C_Horiz = Convert.ToInt32(packetArr[5]);
            C_Vert = Convert.ToInt32(packetArr[6]);
            L_Analog = Convert.ToInt32(packetArr[7]);
            R_Analog = Convert.ToInt32(packetArr[8]);

            int button = Convert.ToInt32(packetArr[2]);

            Dpad_Left = (button & 1) != 0;
            Dpad_Right = (button & 2) != 0;
            Dpad_Down = (button & 4) != 0;
            Dpad_Up = (button & 8) != 0;
            Z = (button & 16) != 0;
            R = (button & 32) != 0;
            L = (button & 64) != 0;
            A = (button & 256) != 0;
            B = (button & 512) != 0;
            X = (button & 1024) != 0;
            Y = (button & 2048) != 0;
            Start = (button & 4096) != 0;
        }

        public override string ToString()
        {
            int button = 0;
            if (Dpad_Left) { button += 1; }
            if (Dpad_Right) { button += 2; }
            if (Dpad_Down) { button += 4; }
            if (Dpad_Up) { button += 8; }
            if (Z) { button += 16; }
            if (R) { button += 32; }
            if (L) { button += 64; }
            if (A) { button += 256; }
            if (B) { button += 512; }
            if (X) { button += 1024; }
            if (Y) { button += 2048; }
            if (Start) { button += 4096; }
            return Frame.ToString() + ";" + ID.ToString() + ";" +
                button.ToString() + ";" + Joy_Horiz.ToString() + ";" +
                Joy_Vert.ToString() + ";" + C_Horiz.ToString() + ";" +
                C_Vert.ToString() + ";" + L_Analog.ToString() + ";" +
                R_Analog.ToString() + ";0;0\n";
        }

        public void clearPad()
        {
            A = false;
            B = false;
            X = false;
            Y = false;
            Z = false;
            Start = false;
            L = false;
            R = false;
            Dpad_Down = false;
            Dpad_Left = false;
            Dpad_Up = false;
            Dpad_Right = false;
            Joy_Horiz = 128;
            Joy_Vert = 128;
            C_Horiz = 128;
            C_Vert = 128;
            L_Analog = 0;
            R_Analog = 0;
        }

        public static bool samePad(GCPadStatus a, GCPadStatus b)
        {
            string[] asplit = a.ToString().Split(new char[1] { ';' });
            string[] bsplit = b.ToString().Split(new char[1] { ';' });
            for (int i = 1; i < asplit.Length; i++)
            {
                if (asplit[i] != bsplit[i])
                    return false;
            }
            return true;
        }
    }
    public class dolphinBot
    {
        public delegate void GCPadCallback(GCPadStatus currentStatus, GCPadStatus previousStatus, StreamWriter writer);

        public static void DefaultGCPadCallback(GCPadStatus cs, GCPadStatus ps, StreamWriter w)
        { }

        private GCPadCallback[] GCPadCallbacks = new GCPadCallback[4];

        public dolphinBot(GCPadCallback[] callbacks)
        {
            for (int i = 0; i < 4; i++)
                GCPadCallbacks[i] = callbacks[i];
        }

        public void run()
        {
            TcpClient tcpclnt = new TcpClient();
            tcpclnt.Connect("127.0.0.1", 27015);
            StreamReader read = new StreamReader(tcpclnt.GetStream());
            StreamWriter write = new StreamWriter(tcpclnt.GetStream());

            GCPadStatus[] PreviousPads = new GCPadStatus[4];
            bool[] firsts = new bool[4] { true, true, true, true };

            GCPadStatus CurrentPad;
            string packet;

            while (true)
            {
                packet = read.ReadLine();
                CurrentPad = new GCPadStatus(packet);
                if (!firsts[1] && CurrentPad.ID == 1 && CurrentPad.Frame == PreviousPads[1].Frame && !GCPadStatus.samePad(CurrentPad, PreviousPads[1]))
                {
                    Console.WriteLine("Hmmm");
                    Console.WriteLine(CurrentPad);
                    Console.WriteLine(PreviousPads[1]);
                    Console.WriteLine("");
                }
                if (!firsts[1] && CurrentPad.ID == 1 && CurrentPad.Frame != PreviousPads[1].Frame && !GCPadStatus.samePad(CurrentPad, PreviousPads[1]))
                {
                    Console.WriteLine(CurrentPad);
                }
                for (int i=0; i<4; i++)
                {
                    if (!firsts[i] && CurrentPad.ID == i && CurrentPad.Frame != PreviousPads[i].Frame)
                    {
                        GCPadCallbacks[i](CurrentPad, PreviousPads[i], write);
                        PreviousPads[i] = CurrentPad;
                    }
                    else if (firsts[i] && CurrentPad.ID == i)
                    {
                        firsts[i] = false;
                        PreviousPads[i] = CurrentPad;
                    }
                }
            }

            tcpclnt.Close();
        }
    }
}
