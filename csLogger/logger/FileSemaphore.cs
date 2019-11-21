using System;
using System.IO;
using System.Text;
using System.Net;
using System.Diagnostics;

namespace ClassicLog
{
	/// <summary>
	/// <p>This FileSemaphore implementation operates by placing a file with a ".sem" suffix
	/// next to the file that you need to manage. The sem file is used to mark when that  
	/// resource has been locked. </p>
	/// 
	/// <p>Note that the .sem file contains the process id of the lock owner. </p>
	/// 
	/// <p>LockWait: If any process locks a resource for longer than it should, then the lock 
	/// will be removed whenever someone else needs to use the file. ONLY LockWait leverages
	/// the timeout mechanism.</p>
	/// 
	/// <p>FileSemaphore exists independantly of any operating system IPC mechanisms.</p>
	/// 
	/// </summary>
	public class FileSemaphore
	{
		const string sSemExt = ".sem";
		private string sFQSemFile = "";
		private double dSec = 15.0;
		int iSemCode = Process.GetCurrentProcess().Id;

		/// <summary>
		/// Constructor to override the file, seconds, AND the uniqie process identifier. Usefull
		/// if we want to manage a legion of thise things from a central repository across many
		/// operating systems (e.g. Assign a process ID from a SOAP service, etc.)
		/// </summary>
		/// <param name="sFullyQualifiedFileToManage">The fully qualified file resource to manage.</param>
		/// <param name="iMaxLockSec">The number of seconds to wait before assuming that a lock is no longer valid.</param>
		/// <param name="iUniqieId">A *very* special process identifier. Also handy for testing!</param>
		public FileSemaphore(string sFullyQualifiedFileToManage, int iMaxLockSec, int iUniqieId) 
		{
			iSemCode = iUniqieId;
			dSec = iMaxLockSec * 1.0;
			sFQSemFile = sFullyQualifiedFileToManage + sSemExt;
		}


		/// <summary>
		/// Constructor to override the number of seconds honored by WaitFor()
		/// </summary>
		/// <param name="sFullyQualifiedFileToManage">The fully qualified file resource to manage.</param>
		/// <param name="iMaxLockSec">The number of seconds to wait before assuming that a lock is no longer valid.</param>
		public FileSemaphore(string sFullyQualifiedFileToManage, int iMaxLockSec) 
		{
			dSec = iMaxLockSec * 1.0;
			sFQSemFile = sFullyQualifiedFileToManage + sSemExt;
		}

		/// <summary>
		/// Constructor to accept the default latency. Ideal for short, anatomic file operations (such as simple textual logging.)
		/// </summary>
		/// <param name="sFullyQualifiedFileToManage">The fully qualified file resource to manage.</param>
		public FileSemaphore(string sFullyQualifiedFileToManage) 
		{
			dSec = 15.0;	// Not a bad default for simple logging operations -
			sFQSemFile = sFullyQualifiedFileToManage + sSemExt;
		}

		public FileSemaphore() 
		{
			dSec = 15.0;	// Not a bad default for simple logging operations -
			sFQSemFile = Directory.GetCurrentDirectory() + "\\" + "default" + sSemExt;
		}

		/// <summary>
		/// Defeats the protocol by removing the file and replacing it with one that contains the
		/// new process identifier -
		/// </summary>
		/// <param name="sem">A FileSempahore</param>
		/// <returns>True if successfull.</returns>
		private static bool ForceLock(ref FileSemaphore sem)
		{
			if(sem.RemoveLock() == false)
				return false;
			return Lock(ref sem);
		}

		/// <summary>
		/// Use this function to ABSOLOUELY remove the semaphore file.
		/// </summary>
		/// <returns>True when the semaphore file absolutely no longer exists. Else you have 
		/// a real problem at the file system level (read only, access, locked open, etc).</returns>
		internal bool RemoveLock()
		{
			if(IsLocked())
			{
				try 
				{
					File.Delete(sFQSemFile);
					if(File.Exists(sFQSemFile) == true)
						return false;
				}
				catch(Exception)
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// If the semaphore file exists, then it is locked.
		/// </summary>
		/// <returns>True if senaphore file exists, false of it does not.</returns>
		public bool IsLocked()
		{
			return File.Exists(sFQSemFile);
		}

		/// <summary>
		///  Returns 0 if the semaphore file is not found, else the integer that has been saved therein -
		/// </summary>
		/// <returns>The sempahore found in the file. (implies process ownership herein)</returns>
		protected int GetSemaphore()
		{
			int iResult = 0;
			if(IsLocked() == false)
				return iResult;
			try
			{
				System.IO.StreamReader ss = new System.IO.StreamReader(File.OpenRead(sFQSemFile));
				string sLine = ss.ReadLine();
				ss.Close();
				iResult = int.Parse(sLine);
			} 
			catch (Exception)
			{
			}
			return iResult;
		}

		/// <summary>
		/// Creates and sets the contents of the semaphore file to the specified integer content.
		/// </summary>
		/// <param name="iSem"></param>
		/// <returns>The result of the file update operaiton.</returns>
		protected bool SetSemaphore(int iSem)
		{
			bool bResult = false;
			try
			{
				System.IO.FileStream fs = File.OpenWrite(sFQSemFile);
				System.IO.StreamWriter sr = new System.IO.StreamWriter(fs);
				sr.WriteLine(iSem);
				sr.Flush();
				sr.Close();
				bResult = true;
			} 
			catch (Exception)
			{
			}
			return bResult;
		}

		/// <summary>
		/// Lock a semaphore file. You can only UnLock a semaphore that either does not exists, or that your process 
		/// has previously Locked.
		/// </summary>
		/// <param name="sem">A semaphore file (context)</param>
		/// <returns>True if the semaphore file was released.</returns>
		public static bool UnLock(ref FileSemaphore sem)
		{
			// STEP: If the semaphore file is not there, then the resource is unlocked.
			if(sem.IsLocked() == false)
				return true;

			// STEP: The general public can ONLY remove a semaphore file IF it is theirs
			if(sem.iSemCode == sem.GetSemaphore())
			{
				return sem.RemoveLock();
			}

			// Otherwise the user CANNOT unlock it!
			return false;
		}

		/// <summary>
		/// You can only Lock a file if the semaphore file does not exist.
		/// Use LockWait if you want to honor the timeout mechanism - otherwise
		/// it is COMPLETELY ignored -
		/// </summary>
		/// <param name="sem">A semaphore file (context)</param>
		/// <returns>Returns true if the semaphore was updated to your process ID.</returns>
		public static bool Lock(ref FileSemaphore sem)
		{
			// STEP: If it is locked, see if we have locked it -
			if(sem.IsLocked() == true)
				{
				int iResult = sem.GetSemaphore();
				// See if it is already locked - by us
				if(iResult == sem.iSemCode)
					return true;
				// See if it was just removed -
				if(iResult != 0)
					return false;
				}
			// STEP: Lock it -
			return sem.SetSemaphore(sem.iSemCode);
		}

		/// <summary>
		/// The ONLY place where the lock timeous is honored.
		/// </summary>
		/// <param name="sem">A semaphore file (context)</param>
		/// <returns>True when you are able to obtain a lock.</returns>
		public static bool LockWait(ref FileSemaphore sem)
		{
			if(sem.IsLocked())
			{
				DateTime dtMax = DateTime.Now;

				// DESIRED: No matter who has locked it (even us!), ForceLock the 
				// file if it has been around longer that it should have been -
				// - OBSERVED -
				// This is an odd fish - Sometimes the test case reports that
				// the file was written (or created) when we actually did so,
				// other times it just seems to take the value for the last 
				// time the inode was created / written to... This type of 
				// recovery is not on the critical path the moment, so we will 
				// move on -
				// TODO: Resolve or report the above issue.
				/*
				DateTime dtActual = File.GetLastWriteTime(sem.sFQSemFile);
				dtActual = dtActual.AddSeconds(sem.dSec);
				if(dtActual < dtMax)
					return ForceLock(ref sem);
				*/

				// Set the most recent time to the future -
				dtMax = dtMax.AddSeconds(sem.dSec);

				// Prime the semaphore that we need to start timing -
				int iHasIt = sem.GetSemaphore();

				// STEP: If we have locked it, then just leave it -
				if(iHasIt == sem.iSemCode)
					return true;

				// STEP: Block while there is a lock file -OR- until a 
				// process has locked the recource for the specified 
				// recource timeout limit  -
				while(sem.IsLocked() == true)
				{
					int iPeek = sem.GetSemaphore();
					if(iPeek == 0)
						continue;
					if(iPeek == iHasIt)
					{
						if(DateTime.Now > dtMax)
							return ForceLock(ref sem);	// Time to FORCE the issue -
					}
					else
					{
						dtMax = DateTime.Now;
						dtMax.AddSeconds(sem.dSec);
					}
				}
			}

			// STEP: The file is no longer there - Lock it -
			return Lock(ref sem);
		}
	}
}
