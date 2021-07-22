using System.Xml;

using YACCS.Commands.Models;

namespace YACCS.StructuredArguments
{
	public class XmlArgumentsCommand : StructuredArgumentsCommand<XmlDocument>
	{
		public XmlArgumentsCommand(IImmutableCommand source) : base(source)
		{
		}

		protected override object? GetValue(XmlDocument structured, IImmutableParameter parameter)
			=> structured[parameter.ParameterName];
	}
}