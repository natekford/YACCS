using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands.Models;

namespace YACCS.Commands.Attributes
{
	public interface ICommandGeneratorAttribute
	{
		ValueTask<IEnumerable<IImmutableCommand>> GenerateCommandsAsync(
			IServiceProvider services,
			IImmutableCommand source);
	}
}