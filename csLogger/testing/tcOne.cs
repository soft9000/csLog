using System;
using System.IO;
using ClassicLog;

namespace MyLogTest
{
	/// <summary>
	/// Regression test cases for ClassicLog.
	/// </summary>
	class tcOne
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			SimpleLog log = new SimpleLog();

			// STEP: Delete any bones -
			#region Clean Up
			File.Delete(log.FileName);
			if(File.Exists(log.FileName) == true)
			{
				System.Console.Out.WriteLine("Error: Test 1.0 fails.");
				return;
			}
			#endregion

			// TEST SET ONE - Simple Log Testing
			#region Basic Tests
			{
				string TestPattern01 = "This is a test for " + log.sProcessId;
				// STEP: Write a test pattern to the file
					if(SimpleLog.Log(TestPattern01) == null)
					{
						System.Console.Out.WriteLine("Error: Test 1.0.1 fails.");
						return;
					}
					// STEP: Be sure the file was written
					if(File.Exists(log.FileName) == false)
					{
						System.Console.Out.WriteLine("Error: Test 1.1 fails.");
						return;
					}
					// STEP: Verify that our pattern was put in the first line
					System.IO.StreamReader sr = new System.IO.StreamReader(File.OpenRead(log.FileName));
					string sLine = sr.ReadLine();
					if(sLine.IndexOf(TestPattern01) == -1)
					{
						System.Console.WriteLine("Error: Test 1.2 fails");
						return;
					}
				}
			#endregion

			// TEST SET TWO - FileSemaphore Testing
			#region Semaphore Tests
		{
			const string TestFileObj = "C:\\TEST.TXT";

			FileSemaphore sem = new FileSemaphore(TestFileObj, 3, 100);

			// STEP: Lock a file
			if(FileSemaphore.Lock(ref sem) == false)
			{
				System.Console.WriteLine("Error: Test 2.0 fails");
				return;
			}

			// STEP: Should be locked
			if(sem.IsLocked() == false)
			{
				System.Console.WriteLine("Error: Test 2.0.1 fails");
				return;
			}

			// STEP: UnLock same
			if(FileSemaphore.UnLock(ref sem) == false)
			{
				System.Console.WriteLine("Error: Test 2.1 fails");
				return;
			}

			// STEP: Should not be locked
			if(sem.IsLocked() == true)
			{
				System.Console.WriteLine("Error: Test 2.1.1 fails");
				return;
			}

			// STEP: Now lets use our REAL pid so we can test the LockWait -
			FileSemaphore semMine = new FileSemaphore(TestFileObj, 15);
			if(FileSemaphore.Lock(ref sem) == false)
			{
				System.Console.WriteLine("Error: Test 3.0 fails");
				return;
			}
			// Should be locked
			if(sem.IsLocked() == false)
			{
				System.Console.WriteLine("Error: Test 3.0.1 fails");
				return;
			}
			#endregion

			// TEST SET THREE - LockWait Testing
			#region WaitLock
			double dtPreCheck = DateTime.Now.TimeOfDay.TotalSeconds;
			if(FileSemaphore.LockWait(ref semMine) == false)
			{
				System.Console.WriteLine("Error: Test 3.1 fails");
				return;
			}
			// Should be locked
			if(semMine.IsLocked() == false)
			{
				System.Console.WriteLine("Error: Test 3.2 fails");
				return;
			}
			double dtPostCheck = DateTime.Now.TimeOfDay.TotalSeconds;

			if(FileSemaphore.UnLock(ref semMine) == false)
			{
				System.Console.WriteLine("Error: Test 3.3 fails");
				return;
			}
			// Should not be locked
			if(semMine.IsLocked() == true)
			{
				System.Console.WriteLine("Error: Test 3.4 fails");
				return;
			}

			double dRes = dtPostCheck - dtPreCheck;
			if(dRes < 14.0)
				System.Console.WriteLine("WARNING: Test 3.5 fails");

			#endregion
		}

		System.Console.WriteLine("Success: All testing passes.");

		} // Main()

	} // Class

} // Namespace
