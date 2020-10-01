using System.Diagnostics;
using System.Threading.Tasks;

using MorseCode.ITask;

namespace YACCS.Results
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class TypeReaderResult : Result, ITypeReaderResult
	{
		public static TypeReaderResultInstance<TypeReaderResult> Failure { get; }
			= FromError().AsTypeReaderResultInstance();

		public object? Value { get; }
		private string DebuggerDisplay => $"IsSuccess = {IsSuccess}, Response = {Response}, Value = {Value}";

		public TypeReaderResult(bool isSuccess, string response, object? value)
			: base(isSuccess, response)
		{
			Value = value;
		}

		public static TypeReaderResult FromError()
			=> new TypeReaderResult(false, "", null);

		public static TypeReaderResult FromSuccess(object value)
			=> new TypeReaderResult(true, "", value);

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
}