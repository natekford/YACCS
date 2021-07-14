using System;

using YACCS.Localization;

namespace YACCS.Results
{
	public abstract class LocalizedResult : Result, IFormattable
	{
		public override string Response => UnlocalizedResponse.Localized;
		protected NeedsLocalization UnlocalizedResponse { get; }

		protected LocalizedResult(bool isSuccess, NeedsLocalization response) : base(isSuccess, response)
		{
			UnlocalizedResponse = response;
		}

		public override string ToString()
			=> Response;

		public virtual string ToString(string ignored, IFormatProvider formatProvider)
			=> ToString();
	}
}