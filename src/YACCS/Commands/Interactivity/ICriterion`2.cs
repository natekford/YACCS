using System.Threading.Tasks;

using YACCS.Results;

namespace YACCS.Commands.Interactivity
{
	public interface ICriterion<in TContext, in TInput> where TContext : IContext
	{
		Task<IResult> JudgeAsync(TContext context, TInput input);
	}
}