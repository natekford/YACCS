using YACCS.TypeReaders;

namespace YACCS.Commands.Models
{
	public interface IParameter : IEntityBase, IQueryableParameter
	{
		object? DefaultValue { get; set; }
		bool HasDefaultValue { get; set; }
		ITypeReader? TypeReader { get; set; }

		IImmutableParameter ToImmutable();
	}
}