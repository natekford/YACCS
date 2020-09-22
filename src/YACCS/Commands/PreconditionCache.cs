using System.Collections.Generic;

using YACCS.Results;

namespace YACCS.Commands
{
	public class PreconditionCache
	{
		public Dictionary<object, IResult> ParameterPreconditions { get; }
			= new Dictionary<object, IResult>();
		public Dictionary<object, IResult> Preconditions { get; }
			= new Dictionary<object, IResult>();
		public Dictionary<object, ITypeReaderResult> TypeReaders { get; }
			= new Dictionary<object, ITypeReaderResult>();
	}
}