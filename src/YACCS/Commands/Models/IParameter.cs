using YACCS.TypeReaders;

namespace YACCS.Commands.Models
{
	public interface IParameter : IEntityBase, IQueryableParameter
	{
		object? DefaultValue { get; set; }
		bool HasDefaultValue { get; set; }
		ITypeReader? OverriddenTypeReader { get; set; }

		IImmutableParameter ToParameter();
	}
}