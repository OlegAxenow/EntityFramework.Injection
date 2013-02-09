using System.Data.Entity.Migrations;
using SampleLibrary.Entities;

namespace EntityFramework.Inject.Spec.Migrations
{
	internal sealed class Configuration : DbMigrationsConfiguration<BasicDbContext>
	{
		public Configuration()
		{
			AutomaticMigrationsEnabled = false;
		}

		protected override void Seed(BasicDbContext context)
		{
			/*          //  This method will be called after migrating to the latest version.
            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );*/
		}
	}
}