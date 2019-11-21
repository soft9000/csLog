/* Needed to create a CLI for a class I am teaching.
 Lots of real-world lessons to be earned here:
 (1) Creating Assemblies / DLLs
 (2) Multiple Mains using / testing / sharing same
 (3) Mixed version of .Net frameworks - working fine
 (4) File & machine IP location / matching / use
 (5) Basic time & date usage
 (6) Basic file appending / logging example
 (7) File serialization /  semaphore usage to prevent overwriting
 (*) Probably a few more - yet not a bad set to be on the look-out, for?
 */

using System;
using System.Collections.Generic;
using System.Text;

using ClassicLog;

namespace clog
{
    class ConMain
    {
        /**
         * A simple user interface - just take what we have on 
         * the command line, then add the same to the end of a 
         * machine-named log file.
         **/
        static void Main(string[] args)
        {
            StringBuilder sb = new StringBuilder();
            for (int ss = 0; ss < args.Length; ss++)
            {
                if (ss != 0)
                    sb.Append(" ");
                sb.Append(args[ss]);
            }
            Console.WriteLine(
                ClassicLog.SimpleLog.Log(
                    sb.ToString()));
        }
    }
}
