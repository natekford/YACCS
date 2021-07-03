using System.Threading.Tasks;

using MorseCode.ITask;

namespace YACCS.Results
{
	public class ResultInstance<T, TBase> where T : TBase
	{
		public Task<T> GenericTask { get; }
		public ITask<T> ITask { get; }
		public T Sync { get; }
		public Task<TBase> Task { get; }

		public ResultInstance(T instance)
		{
			Sync = instance;
			Task = System.Threading.Tasks.Task.FromResult<TBase>(instance);
			GenericTask = System.Threading.Tasks.Task.FromResult(instance);
			ITask = GenericTask.AsITask();
		}
	}
}