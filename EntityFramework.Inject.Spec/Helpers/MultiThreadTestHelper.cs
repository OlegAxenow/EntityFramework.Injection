/*
 Based on CrossThreadTestRunner by Peter Provost
 * http://web.archive.org/web/20100410142636/http://www.peterprovost.org/blog/post/NUnit-and-Multithreaded-Tests-CrossThreadTestRunner.aspx
 * http://www.peterprovost.org/blog/2004/11/03/Using-CrossThreadTestRunner/
 */

using System;
using System.Reflection;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFramework.Inject.Spec.Helpers
{
	/// <summary>
	/// Run tests in separate threads and allow to gracefully process assertions.</summary>
	public class MultiThreadTestHelper
	{
		private readonly ThreadStart _userDelegate;
		private Exception _lastException;

		public MultiThreadTestHelper(ThreadStart userDelegate)
		{
			_userDelegate = userDelegate;
		}

		/// <summary>
		/// Runs tests in <paramref name="times"/> threads.</summary>
		/// <param name="times">Concurrent thread count.</param>
		/// <param name="millisecondsAverageDelay">Average delay before starting next thread.</param>
		/// <param name="millisecondsTimeout">Timeout for all threads.</param>
		public void Run(int times = 1, int millisecondsAverageDelay = 10, int millisecondsTimeout = 10000)
		{
			if (times < 0 || times > 100) throw new ArgumentOutOfRangeException("times");
			var tasks = new Task[times];
			var random = new Random();
			for (int i = 0; i < times; i++)
			{
				tasks[i] = Task.Factory.StartNew(MultiThreadedWorker);
				if (i < times - 1)
					Thread.Sleep(Convert.ToInt32((0.5 + random.NextDouble()) * millisecondsAverageDelay));
			}

			Task.WaitAll(tasks, millisecondsTimeout);

			if (_lastException != null) ThrowExceptionPreservingStack(_lastException);
		}

		[ReflectionPermission(SecurityAction.Demand)]
		private void ThrowExceptionPreservingStack(Exception exception)
		{
			FieldInfo remoteStackTraceString = typeof(Exception).GetField("_remoteStackTraceString", 
				BindingFlags.Instance | BindingFlags.NonPublic);
			if (remoteStackTraceString != null)
				remoteStackTraceString.SetValue(exception, exception.StackTrace + Environment.NewLine);
			throw exception;
		}

		private void MultiThreadedWorker()
		{
			try
			{
				_userDelegate.Invoke();
			}
			catch (Exception e)
			{
				_lastException = e;
			}
		}
	}
}