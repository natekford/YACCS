using System.Collections.Immutable;

using YACCS.TypeReaders;

namespace YACCS.Examples.TypeReaders
{
	[TypeReaderTargetTypes(typeof(TimeZoneInfo))]
	public class TimeZoneInfoTypeReader : TryParseTypeReader<TimeZoneInfo>
	{
		public static ImmutableDictionary<string, TimeZoneInfo> TimeZones { get; }
			= TimeZoneInfo.GetSystemTimeZones()
				.ToImmutableDictionary(x => x.StandardName, StringComparer.OrdinalIgnoreCase);

		public TimeZoneInfoTypeReader() : base(TimeZones.TryGetValue)
		{
		}
	}
}