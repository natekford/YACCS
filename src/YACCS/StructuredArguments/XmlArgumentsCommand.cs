using System.Collections.Generic;
using System.Xml;

using YACCS.Commands.Attributes;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;

namespace YACCS.StructuredArguments
{
	public class XmlArgumentsCommand : StructuredArgumentsCommand<XmlDocument>
	{
		public override int MaxLength => 1;
		public override int MinLength => 1;
		public override IReadOnlyList<IImmutableParameter> Parameters { get; }

		public XmlArgumentsCommand(IImmutableCommand source) : base(source)
		{
			Parameters = Source.CreateStructuredParameter<XmlDocument>(nameof(XmlDocument), x =>
			{
				x
				.AddAttribute(new LengthAttribute(1));
			});
		}

		protected override object? GetValue(XmlDocument structured, IImmutableParameter parameter)
			=> structured[parameter.ParameterName];
	}
}