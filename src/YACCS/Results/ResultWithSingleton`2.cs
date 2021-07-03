namespace YACCS.Results
{
	public abstract class ResultWithSingleton<T, TBase> : Result
		where T : TBase, new()
		where TBase : IResult
	{
		public static ResultInstance<T, TBase> Instance { get; }
			= new(new());

		protected ResultWithSingleton(bool isSuccess, string response) : base(isSuccess, response)
		{
		}
	}
}