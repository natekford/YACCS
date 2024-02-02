using System.Collections.Frozen;

using YACCS.TypeReaders;

namespace YACCS.Examples.TypeReaders;

[TypeReaderTargetTypes(typeof(TimeZoneInfo))]
public class TimeZoneInfoTypeReader : TryParseTypeReader<TimeZoneInfo>
{
	public static FrozenDictionary<string, TimeZoneInfo> TimeZones { get; }
		= TimeZoneInfo.GetSystemTimeZones()
			.ToFrozenDictionary(x => x.StandardName, StringComparer.OrdinalIgnoreCase);

	public TimeZoneInfoTypeReader() : base(TimeZones.TryGetValue)
	{
	}
}