using dolphinBotLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SheikTechChaseBot
{
    class SheikTechChase
    {
        private const int Nothing = 0;
        private const int Grabbed = 1;

        public static int[] joyExtremes = new int[] { 0, 128, 255 };
        public static int dthrow_frame_start;

        public static Random rand;
        public static int currentState;
        public static int joyXonGrab;
        public static int joyYonGrab;

        public static void MyGCPadCallback(GCPadStatus CurrentPad, GCPadStatus PreviousPad, StreamWriter write)
        {
            /* Use A + DPAD_DOWN to clear all inputs from stream. Resets the bot in case
             * it somehow gets into a whack state */
            if (CurrentPad.A && CurrentPad.Dpad_Down && (!PreviousPad.A || !PreviousPad.Dpad_Down))
            {
                Console.WriteLine("CLEAR");
                write.WriteLine("CLEAR");
                write.Flush();
            }

            /* Use B + DPAD_DOWN to send START to controller 2 so that you cam get
             * through end game screens */
            if (CurrentPad.B && CurrentPad.Dpad_Down && (!PreviousPad.B || !PreviousPad.Dpad_Down))
            {
                Console.WriteLine("PAUSE");
                GCPadStatus startPress = new GCPadStatus(CurrentPad.Frame + 5, 1);
                startPress.Start = true;
                write.WriteLine(startPress.ToString());
                write.Flush();
            }
            else
            {
                switch (currentState)
                {
                    case Nothing:
                        /* Look for grab with Z or R/L + A */
                        if ((CurrentPad.Z && !PreviousPad.Z) || (CurrentPad.A && (CurrentPad.L || CurrentPad.R) && !PreviousPad.A))
                        {
                            currentState = Grabbed;
                            joyXonGrab = CurrentPad.Joy_Horiz;
                            joyYonGrab = CurrentPad.Joy_Vert;
                        }
                        break;
                    case Grabbed:
                        /* If a not dthrow reset back to neutral */
                        if (CurrentPad.C_Horiz < 64 || 
                            CurrentPad.C_Horiz > 191 || CurrentPad.C_Vert > 191 ||
                            (CurrentPad.Joy_Horiz < 64 && !(joyXonGrab < 64)) ||
                            (CurrentPad.Joy_Horiz > 191 && !(joyXonGrab > 191)) ||
                            (CurrentPad.Joy_Vert > 191 && !(joyYonGrab > 191)))
                        {
                            currentState = Nothing;
                        }
                        /* If dthrow is found, begin random tech reactions */
                        else if ((CurrentPad.Joy_Vert < 64 && !(joyYonGrab < 64)) || CurrentPad.C_Vert < 64)
                        {
                            currentState = Nothing;
                            Console.WriteLine("DTHROW INC");

                            write.WriteLine("CLEAR");
                            write.Flush();

                            dthrow_frame_start = CurrentPad.Frame;
                            GCPadStatus ps = new GCPadStatus(0, 1);

                            /* DI The Throw */
                            ps.Joy_Horiz = joyExtremes[rand.Next(3)];
                            if (ps.Joy_Horiz == 0)
                                Console.WriteLine("DI LEFT");
                            if (ps.Joy_Horiz == 128)
                                Console.WriteLine("NOOOOOOOOO DEEEEEEEE EYYYYYYEEEE");
                            if (ps.Joy_Horiz == 255)
                                Console.WriteLine("DI RIGHT");
                            for (int i = dthrow_frame_start + 15; i < dthrow_frame_start + 46; i++)
                            {
                                ps.Frame = i;
                                write.WriteLine(ps.ToString());
                                write.Flush();
                            }

                            /* Decide whether to tech or not */
                            if (rand.Next(4) != 0)
                            {
                                ps = new GCPadStatus(-1, 1);
                                ps.L = true;

                                /* Choose tech direction */
                                int dir = rand.Next(3);
                                ps.Joy_Horiz = joyExtremes[dir];
                                if (ps.Joy_Horiz == 0)
                                    Console.WriteLine("TECH LEFT");
                                if (ps.Joy_Horiz == 128)
                                    Console.WriteLine("TECH IN PLACE");
                                if (ps.Joy_Horiz == 255)
                                    Console.WriteLine("TECH RIGHT");
                                for (int i = dthrow_frame_start + 50; i < dthrow_frame_start + 71; i++)
                                {
                                    ps.Frame = i;
                                    write.WriteLine(ps.ToString());
                                    write.Flush();
                                }

                                /* SPAM SHINE */
                                ps.clearPad();
                                ps.Joy_Horiz = 128;
                                ps.Joy_Vert = 0;
                                for (int i = dthrow_frame_start + 71; i < dthrow_frame_start + 110; i++)
                                {
                                    ps.Frame = i;
                                    ps.B = i % 2 == 0;
                                    write.WriteLine(ps.ToString());
                                    write.Flush();
                                }
                            }
                            else
                            {
                                Console.WriteLine("NO TECH");
                            }
                            Console.WriteLine("");
                        }
                        else if ((CurrentPad.Joy_Vert > 64 && CurrentPad.Joy_Vert < 191)) { joyYonGrab = 128; }
                        else if ((CurrentPad.Joy_Horiz > 64 && CurrentPad.Joy_Horiz < 191)) { joyXonGrab = 128; }
                        break;
                }
            }
        }

        static void Main(string[] args)
        {
            rand = new Random();
            currentState = Nothing;
            (new dolphinBot(new dolphinBot.GCPadCallback[4] { 
                MyGCPadCallback, dolphinBot.DefaultGCPadCallback, dolphinBot.DefaultGCPadCallback, dolphinBot.DefaultGCPadCallback
            })).run();
        }
    }
}
