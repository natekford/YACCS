using System.Collections.Immutable;
using System.Globalization;

using YACCS.TypeReaders;

namespace YACCS.Examples.TypeReaders
{
	[TypeReaderTargetTypes(typeof(CultureInfo))]
	public class CultureInfoTypeReader : TryParseTypeReader<CultureInfo>
	{
		public static ImmutableDictionary<string, CultureInfo> Cultures { get; }
			= CultureInfo.GetCultures(CultureTypes.AllCultures)
				.ToImmutableDictionary(x => x.Name);

		public CultureInfoTypeReader() : base(Cultures.TryGetValue)
		{
		}
	}
}