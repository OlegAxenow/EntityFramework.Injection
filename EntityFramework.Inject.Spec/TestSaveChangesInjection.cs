using System.Collections.Generic;
using System.Data.Entity;
using Method.Inject;

namespace EntityFramework.Inject.Spec
{
	public class TestSaveChangesInjection : MethodInjection, ISaveChangesInjection
	{
		public TestSaveChangesInjection()
		{
			BeforeList = new List<DbContext>();
			AfterList = new List<DbContext>();
		}

		public List<DbContext> BeforeList { get; private set; }
		
		public List<DbContext> AfterList { get; private set; }

		public void OnBeforeSaveChanges(DbContext context)
		{
			BeforeList.Add(context);
		}

		public void OnAfterSaveChanges(DbContext context)
		{
			AfterList.Add(context);
		}
	}
}