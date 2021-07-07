using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace YACCS.Localization
{
	public static class Keys
	{
		public const string ATTRIBUTES = "Attributes";
		public const string ID = "Id";
		public const string LENGTH = "Length";
		public const string NAMES = "Names";
		public const string PARAMETERS = "Parameters";
		public const string PRECONDITIONS = "Preconditions";
		public const string PRIORITY = "Priority";
		public const string REMAINDER = "Remainder";
		public const string SUMMARY = "Summary";

		public static ImmutableArray<string> AllKeys { get; }
			= typeof(Keys).GetFields(BindingFlags.Static | BindingFlags.Public)
			.Where(x => x.FieldType == typeof(string))
			.Select(x => (string)x.GetValue(null))
			.ToImmutableArray();
	}
}