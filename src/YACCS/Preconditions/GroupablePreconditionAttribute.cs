namespace YACCS.Preconditions
{
	/// <summary>
	/// The base class for a groupable precondition attribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
	public abstract class GroupablePreconditionAttribute : Attribute, IGroupablePrecondition
	{
		/// <inheritdoc />
		public string[] Groups { get; set; } = Array.Empty<string>();
		/// <inheritdoc />
		public BoolOp Op { get; set; } = BoolOp.And;

		IReadOnlyList<string> IGroupablePrecondition.Groups => Groups;
	}
}