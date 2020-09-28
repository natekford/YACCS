using System.Threading.Tasks;

namespace YACCS.Commands.Interactivity
{
	public interface ICriterion<in TContext, in TInput> where TContext : IContext
	{
		Task<bool> JudgeAsync(TContext context, TInput input);
	}
}