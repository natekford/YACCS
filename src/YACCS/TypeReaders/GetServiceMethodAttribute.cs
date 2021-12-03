namespace YACCS.TypeReaders;

/// <summary>
/// Used in <see cref="TypeReaderUtils.ThrowIfUnregisteredServices(ITypeReader, IServiceProvider)"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class GetServiceMethodAttribute : Attribute
{
}