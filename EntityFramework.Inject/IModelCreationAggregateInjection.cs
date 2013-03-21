using System.Collections.Generic;
using Method.Inject;

namespace EntityFramework.Inject
{
	public interface IModelCreationAggregateInjection : IMethodInjection
	{
		List<IModelCreationInjection> InnerInjections { get; }
	}
}