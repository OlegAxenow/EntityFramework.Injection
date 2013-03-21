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
			Database.SetInitializer(new TestDatabaseInitializer<BasicDbContext>());
			Database.SetInitializer(new TestDatabaseInitializer<LocalizedDbContext>());
			Database.SetInitializer(new TestDatabaseInitializer<DbContext>());
			Database.SetInitializer(new TestDatabaseInitializer<TestDbContext>());
			Database.SetInitializer(new TestDatabaseInitializer<TestDbContext1>());
			Database.SetInitializer(new TestDatabaseInitializer<TestDbContext2>());
			Database.SetInitializer(new TestDatabaseInitializer<TestDbContext3>());
			Database.SetInitializer(new TestDatabaseInitializer<TestConcurrencyDbContext>());
			Database.SetInitializer(new TestDatabaseInitializer<BasicDbContext_generated>());

			AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Empty));

			using (var context = new DbContext("EntityFrameworkInject"))
			{
				context.Database.ExecuteSqlCommand(@"IF OBJECT_ID('ActionTypes') IS NOT NULL DROP TABLE [dbo].[ActionTypes]");
				context.Database.ExecuteSqlCommand(@"CREATE TABLE [dbo].[ActionTypes] (
    [Id] INT IDENTITY (1, 1) NOT NULL, [Name] NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_dbo.ActionTypes] PRIMARY KEY CLUSTERED ([Id]))");

				context.Database.ExecuteSqlCommand(@"IF OBJECT_ID('Categories') IS NOT NULL DROP TABLE [dbo].[Categories]");
				context.Database.ExecuteSqlCommand(@"CREATE TABLE [dbo].[Categories] (
    [CategoryID] INT IDENTITY (1, 1) NOT NULL,
    [CategoryName_1]     NVARCHAR (100) NULL,
    [CategoryName_2]     NVARCHAR (100) NULL,
    [CategoryName_3]     NVARCHAR (100) NULL,
    [CategoryName_4]     NVARCHAR (100) NULL,
    [CategoryName_5]     NVARCHAR (100) NULL,
    [CategoryComputed_1] NVARCHAR (100) NULL,
    [CategoryComputed_2] NVARCHAR (100) NULL,
    [CategoryComputed_3] NVARCHAR (100) NULL,
    [CategoryComputed_4] NVARCHAR (100) NULL,
    [CategoryComputed_5] NVARCHAR (100) NULL,
    [NotLocalizedName]        NVARCHAR (100) NULL,
    CONSTRAINT [PK_dbo.Categories] PRIMARY KEY CLUSTERED ([CategoryID]))");
			}
		}
	}
}