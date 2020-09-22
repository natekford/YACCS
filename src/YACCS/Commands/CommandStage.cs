namespace YACCS.Commands
{
	public enum CommandStage
	{
		QuoteMismatch = -1,
		NotFound = 0,
		BadArgCount = 1,
		CorrectArgCount = 2,
		FailedPrecondition = 3,
		FailedTypeReader = 4,
		FailedParameterPrecondition = 5,
		CanExecute = 6,
	}
}
