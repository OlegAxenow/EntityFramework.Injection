using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using Method.Inject;

namespace EntityFramework.Inject.Spec.Entities
{
	public class TestDbContext : DbContext
	{
	    public object StringParameter { get; private set; }
	    public object ObjectParameter { get; private set; }
	    public InjectionSet ClassParameter { get; private set; }
	    public IEnumerable<int> InterfaceParameter { get; private set; }

	    public TestDbContext(string nameOrConnectionString ) : base(nameOrConnectionString)
	    {
	    }

	    public TestDbContext(string nameOrConnectionString, object stringParameter, object objectParameter, InjectionSet classParameter, IEnumerable<int> interfaceParameter) : base(nameOrConnectionString)
	    {
	        StringParameter = stringParameter;
	        ObjectParameter = objectParameter;
	        ClassParameter = classParameter;
	        InterfaceParameter = interfaceParameter;
	    }
	}
}