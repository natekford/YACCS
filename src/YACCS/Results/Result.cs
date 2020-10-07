using System.Diagnostics;
using System.Threading.Tasks;

using MorseCode.ITask;

namespace YACCS.Results
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class Result : IResult
	{
		public bool IsSuccess { get; }
		public string Response { get; }
		private string DebuggerDisplay => $"IsSuccess = {IsSuccess}, Response = {Response}";

		public Result(bool isSuccess, string response)
		{
			IsSuccess = isSuccess;
			Response = response;
		}

		public static IResult FromError(string response)
			=> new Result(false, response);

		public static IResult FromSuccess()
			=> new Result(true, "");

		public class ResultInstance<T, TBase> where T : IResult, TBase
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
}