using System.Threading.Tasks;

using MorseCode.ITask;

namespace YACCS.Results
{
	public class TypeReaderResultInstance<T> where T : TypeReaderResult
	{
		public Task<T> GenericTask { get; }
		public ITask<T> ITask { get; }
		public T Sync { get; }
		public Task<ITypeReaderResult> Task { get; }

		public TypeReaderResultInstance(T instance)
		{
			Sync = instance;
			Task = instance.AsTask();
			GenericTask = System.Threading.Tasks.Task.FromResult(instance);
			ITask = GenericTask.AsITask();
		}
	}
}