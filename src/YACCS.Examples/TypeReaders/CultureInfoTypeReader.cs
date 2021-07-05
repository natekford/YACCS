using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using YACCS.TypeReaders;

namespace YACCS.Examples.TypeReaders
{
	[TypeReaderTargetTypes(typeof(CultureInfo))]
	public class CultureInfoTypeReader : TryParseTypeReader<CultureInfo>
	{
		private static readonly Dictionary<string, CultureInfo> _Cultures =
			CultureInfo.GetCultures(CultureTypes.AllCultures).ToDictionary(x => x.Name);

		public CultureInfoTypeReader() : base(_Cultures.TryGetValue)
		{
		}
	}
}