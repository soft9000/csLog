using System;

namespace ClassicLog
{
	/// <summary>
	/// This log file management type has been designed to allow multiple processes to share log files.
	/// Depending upon the constructor, you can optomise logging for either a single or multiple
	/// machines / operating systems.
	/// </summary>
	public class SharedSimpleLog
	{
		protected FileSemaphore fSem = new FileSemaphore();
		protected SimpleLog log = new SimpleLog();

		/// <summary>
		/// Use this constructor to allow processes on the SAME MACHINE to share a MACHINE NAMED logging file.
		/// The file will be placed in the CURRENT DIRECTORY.
		/// </summary>
		public SharedSimpleLog()
		{
			fSem = new FileSemaphore(log.FileName);
		}

		/// <summary>
		/// Use this constructor to share a log file ACROSS MANY machines AND operating systems. 
		/// It allows you to place a file AND it's locking mechanism wherever you have access 
		/// (across a LAN, etc.)
		/// </summary>
		/// <param name="sFQFileName">A fully qualified file log name.</param>
		public SharedSimpleLog(string sFQFileName)
		{
			log.FileName = sFQFileName;
			fSem = new FileSemaphore(log.FileName);
		}

		/// <summary>
		/// <para>Write to the log file in such a way so as to avoid collisions in updating same. </para>
		/// <para>Locks do NOT timeout!</para>
		/// </summary>
		/// <param name="sMessage">The message to write.</param>
		/// <returns>True if the log file was written.</returns>
		public bool Log(string sMessage)
		{
			bool br = false;
			if(FileSemaphore.Lock(ref fSem) == true)
				{
				if(SimpleLog.Log(sMessage) != null)
					br = true;
				FileSemaphore.UnLock(ref fSem);
				}
			return br;
		}

		/// <summary>
		/// Should not be used. Here just in case things go VERY wrong.
		/// </summary>
		/// <returns></returns>
		public bool RemoveLocks()
		{
			return fSem.RemoveLock();
		}

		/// <summary>
		/// <para>Write to the log file in such a way so as to avoid collisions in updating same.</para>
		/// <para>NOTE: THIS PROCESS USES LockWait. IF YOU ARE USING IT ACROSS MACHINES, THERE IS A SLIGHT CHANCE
		/// THAT YOUR PROCESS ID WILL NOT BE UNIQUE. CAVEAT USER.</para>
		/// </summary>
		/// <param name="sMessage">The message to write.</param>
		/// <returns>True if the log file was written.</returns>
		public bool LogWait(string sMessage)
		{
			bool br = false;
			if(FileSemaphore.LockWait(ref fSem) == true)
			{
				if(SimpleLog.Log(sMessage) != null)
					br = true;
				FileSemaphore.UnLock(ref fSem);
			}
			return br;
		}
	}
}
