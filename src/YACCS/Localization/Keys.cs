using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace YACCS.Localization
{
	public static class Keys
	{
		public static ImmutableArray<NeedsLocalization> AllKeys { get; }
			= typeof(Keys).GetProperties(BindingFlags.Static | BindingFlags.Public)
			.Where(x => x.PropertyType == typeof(NeedsLocalization))
			.Select(x => (NeedsLocalization)x.GetValue(null))
			.ToImmutableArray();
		public static NeedsLocalization Attributes { get; } = "Attributes";
		public static NeedsLocalization Id { get; } = "Id";
		public static NeedsLocalization Length { get; } = "Length";
		public static NeedsLocalization Names { get; } = "Names";
		public static NeedsLocalization Parameters { get; } = "Parameters";
		public static NeedsLocalization Preconditions { get; } = "Preconditions";
		public static NeedsLocalization Priority { get; } = "Priority";
		public static NeedsLocalization Remainder { get; } = "Remainder";
		public static NeedsLocalization Summary { get; } = "Summary";
	}
}