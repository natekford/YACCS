namespace YACCS.Commands
{
	public enum CommandStage
	{
		//QuoteMismatch = -1,
		NotFound = 0,
		BadContext = 1,
		BadArgCount = 2,
		//CorrectArgCount = 3,
		FailedPrecondition = 4,
		FailedTypeReader = 5,
		FailedParameterPrecondition = 6,
		FailedOptionalArgs = 7,
		CanExecute = 8,
	}
}