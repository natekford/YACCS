namespace YACCS.Preconditions;

/// <summary>
/// The base class for a groupable precondition attribute.
/// </summary>
[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public abstract class GroupablePrecondition : Attribute, IGroupablePrecondition
{
	/// <inheritdoc />
	public virtual string[] Groups { get; set; } = [];
	/// <inheritdoc />
	public virtual Op Op { get; set; } = Op.And;
	IReadOnlyList<string> IGroupablePrecondition.Groups => Groups;
}