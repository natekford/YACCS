using YACCS.Localization;

namespace YACCS.Results
{
	public abstract class ResultWithSingleton<T, TBase> : Result
		where T : TBase, new()
		where TBase : IResult
	{
		public static ResultInstance<T, TBase> Instance { get; }
			= new(new());

		public override string Response => UnlocalizedResponse.Localized;
		protected NeedsLocalization UnlocalizedResponse { get; }

		protected ResultWithSingleton(bool isSuccess, NeedsLocalization response) : base(isSuccess, response)
		{
			UnlocalizedResponse = response;
		}
	}
}