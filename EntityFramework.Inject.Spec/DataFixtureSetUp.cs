using System;
using System.Data.Entity;
using System.IO;
using EntityFramework.Inject.Spec.Entities;
using EntityFramework.Inject.Spec.Helpers;
using EntityFramework.Inject.Spec.Samples;
using NUnit.Framework;
using SampleLibrary.Entities;

namespace EntityFramework.Inject.Spec
{
	[SetUpFixture]
	public class DataFixtureSetUp
	{
		[SetUp]
		public void SetUp()
		{
			Database.SetInitializer<BasicDbContext>(null);
			Database.SetInitializer<LocalizedDbContext>(null);
			Database.SetInitializer<TestDbContext>(null);
			Database.SetInitializer<TestDbContext1>(null);
			Database.SetInitializer<TestDbContext2>(null);
			Database.SetInitializer<TestDbContext3>(null);
			Database.SetInitializer<TestConcurrencyDbContext>(null);
			Database.SetInitializer<BasicDbContext_generated>(null);

			Database.SetInitializer(new CreateDatabaseIfNotExists<DbContext>());

			AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Empty));

			using (var context = new DbContext("EntityFrameworkInject"))
			{
				context.Database.ExecuteSqlCommand(@"IF OBJECT_ID('ActionTypes') IS NOT NULL DROP TABLE [dbo].[ActionTypes]");
				context.Database.ExecuteSqlCommand(@"CREATE TABLE [dbo].[ActionTypes] (
    [Id] INT IDENTITY (1, 1) NOT NULL, [Name] NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_dbo.ActionTypes] PRIMARY KEY CLUSTERED ([Id]))");

				context.Database.ExecuteSqlCommand(@"IF OBJECT_ID('Categories') IS NOT NULL DROP TABLE [dbo].[Categories]");
				context.Database.ExecuteSqlCommand(@"CREATE TABLE [dbo].[Categories] (
    [Id] INT IDENTITY (1, 1) NOT NULL,
    [CategoryName_1]     NVARCHAR (100) NULL,
    [CategoryName_2]     NVARCHAR (100) NULL,
    [CategoryName_3]     NVARCHAR (100) NULL,
    [CategoryName_4]     NVARCHAR (100) NULL,
    [CategoryName_5]     NVARCHAR (100) NULL,
    [CategoryComputed_1] AS 'value1',
    [CategoryComputed_2] AS 'value2',
    [CategoryComputed_3] AS 'value3',
    [CategoryComputed_4] AS 'value4',
    [CategoryComputed_5] AS 'value5',
    [NotLocalizedName]        NVARCHAR (100) NULL,
    CONSTRAINT [PK_dbo.Categories] PRIMARY KEY CLUSTERED ([Id]))");

				context.Database.ExecuteSqlCommand(@"SET IDENTITY_INSERT Categories ON
INSERT Categories(Id, CategoryName_1, CategoryName_2, CategoryName_3) VALUES(1, 'Beverages', '1', '11')
INSERT Categories(Id, CategoryName_1, CategoryName_2, CategoryName_3) VALUES(2, 'Condiments', '2', '22')
INSERT Categories(Id, CategoryName_1, CategoryName_2, CategoryName_3) VALUES(3, 'Confections', '3', '33')
INSERT Categories(Id, CategoryName_1, CategoryName_2, CategoryName_3) VALUES(4, 'Dairy Products', '4', '44')
INSERT Categories(Id, CategoryName_1, CategoryName_2, CategoryName_3) VALUES(5, 'Grains/Cereals', '5', '55')
INSERT Categories(Id, CategoryName_1, CategoryName_2, CategoryName_3) VALUES(6, 'Meat/Poultry', '6', '66')
INSERT Categories(Id, CategoryName_1, CategoryName_2, CategoryName_3) VALUES(7, 'Produce', '7', '77')
INSERT Categories(Id, CategoryName_1, CategoryName_2, CategoryName_3) VALUES(8, 'Seafood', '8', '88')
SET IDENTITY_INSERT Categories OFF");
			}
		}
	}
}