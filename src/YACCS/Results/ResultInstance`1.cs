using System.Threading.Tasks;

using MorseCode.ITask;

namespace YACCS.Results
{
	public class ResultInstance<T> where T : Result
	{
		public Task<T> GenericTask { get; }
		public ITask<T> ITask { get; }
		public T Sync { get; }
		public Task<IResult> Task { get; }

		public ResultInstance(T instance)
		{
			Sync = instance;
			Task = instance.AsTask();
			GenericTask = System.Threading.Tasks.Task.FromResult(instance);
			ITask = GenericTask.AsITask();
		}
	}
}