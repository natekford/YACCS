
using YACCS.Localization;

namespace YACCS.Results
{
	public abstract class LocalizedResult : Result
	{
		public override string Response => UnlocalizedResponse.Localized;
		protected NeedsLocalization UnlocalizedResponse { get; }

		protected LocalizedResult(bool isSuccess, NeedsLocalization response) : base(isSuccess, response)
		{
			UnlocalizedResponse = response;
		}
	}
}