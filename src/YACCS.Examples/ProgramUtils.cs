using System.Threading;

namespace YACCS.Examples
{
	public static class ProgramUtils
	{
		public static int ReleaseIfZero(this SemaphoreSlim semaphore)
			=> semaphore.CurrentCount == 0 ? semaphore.Release() : semaphore.CurrentCount;
	}
}