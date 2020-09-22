namespace YACCS.Results
{
	public class ValueResult : Result
	{
		public object? Value { get; }

		public ValueResult(object? value) : base(true, "")
		{
			Value = value;
		}
	}
}