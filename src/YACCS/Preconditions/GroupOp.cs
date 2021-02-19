namespace YACCS.Preconditions
{
	public enum GroupOp
	{
		And = 1,
		Or = 2,
		// Don't include NOT since getting the "correct" failure message would be a pain
		// Not = 3,
	}
}