using System;
using System.IO;
using System.Text;
using System.Net;
using System.Diagnostics;

namespace ClassicLog
{
	/// <summary>
	/// SimpleLog is just that - A simple way to manage a log file on your machine.
	/// </summary>
	public class SimpleLog
	{
		public readonly  string sProcessId = Process.GetCurrentProcess().Id.ToString(); 
		public readonly  string sHost = Dns.GetHostName();
		public readonly  string sAssembly = System.Reflection.Assembly.GetExecutingAssembly().FullName;
		protected string sLogFile = "";

		/// <summary>
		/// Create the "classic time" pattern prefix
		/// </summary>
		/// <returns>A default log entry prefix.</returns>
		static public string ClassicTime()
		{
			System.Text.StringBuilder  tw = new System.Text.StringBuilder();
			tw.AppendFormat("{0} {1}" , System.DateTime.Now.ToShortDateString(), System.DateTime.Now.ToShortTimeString());
			return tw.ToString();

		}

		/// <summary>
		/// Place the log file in the default folder.
		/// </summary>
		public SimpleLog()
		{
			Home();
		}

		/// <summary>
		/// Place the log file wherever you want it.
		/// </summary>
		/// <param name="sFQFileName">A fully qualified file name for your LOG file.</param>
		/// <returns>True if either the file exists, or the file name was valid.</returns>
		public bool Home(string sFQFileName)
		{
			// If it exists as a file, then assume that they know what they are talking about
			if(File.Exists(sLogFile) == true)
			{
				sLogFile = sFQFileName;
				return true;
			}

			string sHold = sLogFile;
			sLogFile = sFQFileName;

			// STEP: Verify that the file name is valid -
			try
			{
                var zTest = sLogFile + "~";
				System.IO.FileStream fs = File.OpenWrite(zTest);
				fs.Close();
				// Valid - Remove the creation test file
                File.Delete(zTest);
				return true;
			}
			catch (Exception)
			{
				// Not valid - Keep whatever we HAD -
				sLogFile = sHold;
			}

			return false;
		}

		/// <summary>
		/// Place a host-named log file into the current directory (the default)
		/// </summary>
		/// <returns>True if either the file exists, or the file name was valid.</returns>
		public bool Home()
		{
			string sFQFileName =  Directory.GetCurrentDirectory() + "\\" + sHost + ".log";
			return Home(sFQFileName);
		}

		/// <summary>
		///  Returns a NULL string on error, else a copy of what was written to the log file.
		/// </summary>
		/// <param name="sPrefix">What you want to place BEFORE the log entry (e.g. ClassicTime())</param>
		/// <param name="log">The governing instance (log file, etc)</param>
		/// <param name="sMessage">The message to write to the log.</param>
		/// <returns>The pattern written. String is null on error.</returns>
		static public string Log(string sPrefix, ref SimpleLog log, string sMessage)
		{	
			string sFinal = sPrefix + " [" + log.sHost + "." + log.sProcessId + "]" + ": " + sMessage;
			try 
			{
				System.IO.StreamWriter sw = File.AppendText(log.sLogFile);
				sw.WriteLine(sFinal);
				sw.Flush();
				sw.Close();
			}
			catch (Exception)
			{
				sFinal = null;
				return sFinal;
			}
			return sFinal;
		}

		/// <summary>
		/// The preferred entry point.
		/// </summary>
		/// <param name="sMessage">The message to log.</param>
		/// <returns>The pattern written. String is null on error.</returns>
		static public string Log(string sMessage)
		{	
			SimpleLog sl = new SimpleLog();
			return Log(ClassicTime(), ref sl, sMessage);
		}

		/// <summary>
		/// An easy way to access or mutate the file name.
		/// </summary>
		public string FileName
		{
			set 
			{
				Home(value);
			}
			get 
			{
				return sLogFile;
			}
		}
	}
}
