using dolphinBotLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WestballzSim
{
    class Westballz
    {
        public static Random rand;
        public static int nextSequenceStartFrame;
        public static bool currentlyPressuring;

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

            /* Use X + DPAD_DOWN to send START to controller 3 so that you cam get
             * through end game screens */
            if (CurrentPad.X && CurrentPad.Dpad_Down && (!PreviousPad.X || !PreviousPad.Dpad_Down))
            {
                Console.WriteLine("PAUSE");
                GCPadStatus startPress = new GCPadStatus(CurrentPad.Frame + 10, 2);
                startPress.Start = true;
                write.WriteLine(startPress.ToString());
                write.Flush();
            }

            /* Use L/R + DPAD_DOWN to begin (or cancel if already started) shield
             * pressure sequence */
            if (CurrentPad.Dpad_Down && !PreviousPad.Dpad_Down && (CurrentPad.L || CurrentPad.R))
            {
                currentlyPressuring = !currentlyPressuring;
                if (currentlyPressuring)
                {
                    Console.WriteLine("Begin Shield Pressure Sequence");
                    nextSequenceStartFrame = CurrentPad.Frame + 10;
                }
                else
                {
                    Console.WriteLine("End Shield Pressure Sequence");
                    write.WriteLine("CLEAR");
                    write.Flush();
                }
            }
            else if (CurrentPad.Frame == nextSequenceStartFrame - 5 && currentlyPressuring)
            {
                int f = nextSequenceStartFrame;
                GCPadStatus ps = new GCPadStatus(f, 1);
                /* Randomly select between two pressure options:
                 *      Westballz double shine
                 *      Shine super late dair
                 * These combine to form a mix up of whether you should counter
                 * after the grounded shine (in shine nair) or after what comes
                 * after the grounded shine (Westballs double shine) */

                /* Both start with shine -> jump */
                ps.Joy_Vert = 0;
                ps.B = true;
                write.WriteLine(ps.ToString());
                write.Flush();

                ps.Frame = f + 7;
                ps.clearPad();
                ps.Y = true;
                write.WriteLine(ps.ToString());
                write.Flush();
                if (rand.Next(2) == 0)
                {
                    /* Westballz double shine */
                    Console.WriteLine("Westballz double shine");
                    ps.Frame = f + 13;
                    ps.clearPad();
                    ps.Joy_Vert = 0;
                    ps.B = true;
                    write.WriteLine(ps.ToString());
                    write.Flush();

                    ps.Frame = f + 20;
                    ps.clearPad();
                    ps.Y = true;
                    write.WriteLine(ps.ToString());
                    write.Flush();

                    ps.Frame = f + 21;
                    ps.clearPad();
                    ps.Joy_Vert = 0;
                    ps.Joy_Horiz = 200;
                    ps.R = true;
                    write.WriteLine(ps.ToString());
                    write.Flush();

                    nextSequenceStartFrame = f + 33;
                }
                else
                {
                    /* Late Dair */
                    ps.clearPad();
                    ps.Joy_Horiz = 180;
                    for (int i = f + 8; i < f + 18; i++)
                    {
                        ps.Frame = i;
                        write.WriteLine(ps.ToString());
                        write.Flush();
                    }

                    Console.WriteLine("Dair Shine");
                    ps.Frame = f + 26;
                    ps.clearPad();
                    ps.C_Vert = 0;
                    write.WriteLine(ps.ToString());
                    write.Flush();

                    ps.Frame = f + 27;
                    ps.clearPad();
                    ps.Joy_Vert = 0;
                    write.WriteLine(ps.ToString());
                    write.Flush();

                    ps.Frame = f + 30;
                    ps.clearPad();
                    ps.Z = true;
                    write.WriteLine(ps.ToString());
                    write.Flush();

                    nextSequenceStartFrame = f + 46;
                }
            }
        }

        static void Main(string[] args)
        {
            rand = new Random();
            currentlyPressuring = false;
            nextSequenceStartFrame = 0;
            (new dolphinBot(new dolphinBot.GCPadCallback[4] { 
                MyGCPadCallback, dolphinBot.DefaultGCPadCallback, dolphinBot.DefaultGCPadCallback, dolphinBot.DefaultGCPadCallback
            })).run();
        }
    }
}
