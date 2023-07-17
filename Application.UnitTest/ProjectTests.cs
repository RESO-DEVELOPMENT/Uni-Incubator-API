using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Domain.Models;
using Application.Persistence;
using Application.Persistence.Repositories;
using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Application.UnitTest
{
    public class ProjectTests
    {
        private async Task<DataContext> GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var databaseContext = new DataContext(options);
            databaseContext.Database.EnsureCreated();

            await SeedHelper.Seed(databaseContext);
            var testProject = new Faker<Project>();

            for (int i = 0; i < 10; i++)
            {
                databaseContext.Projects.Add(testProject.Generate());
            }

            var testMember = new Faker<Member>();

            for (int i = 0; i < 10; i++)
            {
                databaseContext.Projects.Add(testProject.Generate());
            }


            return databaseContext;
        }

        [Fact]
        public async void Repo_GetAll()
        {
            var dbContext = await GetDatabaseContext();
            var projectRepo = new ProjectRepository(dbContext);

            var result = projectRepo.GetAll();

            result.Should().NotBeNull();
        }
    }
}
