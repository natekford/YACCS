using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Help;
using YACCS.Help.Models;
using YACCS.TypeReaders;

namespace YACCS.NamedArguments;

internal static class NamedArgumentsUtils
{
	internal static async ValueTask<string> CombineSummariesAsync(
		this INamedArgumentParameters parameters,
		IContext context)
	{
		var helpFormatter = context.Services.GetRequiredService<IHelpFormatter>();

		var helpBuilder = helpFormatter.GetBuilder(context);
		var helpParameters = parameters.Parameters.Values
			.Select(x => new HelpParameter(x))
			.ToArray();
		await helpBuilder.AppendParametersAsync(helpParameters).ConfigureAwait(false);
		return helpBuilder.ToString();
	}

	internal static FrozenDictionary<string, IImmutableParameter> CreateParamDict(
		this Type type,
		Func<IImmutableParameter, string> keySelector)
	{
		var (properties, fields) = type.GetWritableMembers();
		return properties
			.Select(x => new Parameter(x))
			.Concat(fields.Select(x => new Parameter(x)))
			.Select(x => x.ToImmutable())
			.ToParamDict(keySelector);
	}

	internal static FrozenDictionary<string, IImmutableParameter> ToParamDict(
		this IEnumerable<IImmutableParameter> parameters,
		Func<IImmutableParameter, string> keySelector)
		=> parameters.ToFrozenDictionary(keySelector, StringComparer.OrdinalIgnoreCase);
}