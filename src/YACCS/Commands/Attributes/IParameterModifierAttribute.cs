using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands.Models;

namespace YACCS.Commands.Attributes
{

	public interface IParameterModifierAttribute
	{
		void ModifyParameter(IParameter parameter);
	}
}