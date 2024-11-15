using System.Collections.Frozen;
using System.Globalization;

using YACCS.TypeReaders;

namespace YACCS.Sample.TypeReaders;

[TypeReaderTargetTypes(typeof(CultureInfo))]
public class CultureInfoTypeReader : TryParseTypeReader<CultureInfo>
{
	public static FrozenDictionary<string, CultureInfo> Cultures { get; }
		= CultureInfo.GetCultures(CultureTypes.AllCultures)
			.ToFrozenDictionary(x => x.Name);

	public CultureInfoTypeReader() : base(Cultures.TryGetValue)
	{
	}
}