using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Application.Domain;
using Application.Domain.Constants;
using Application.Domain.Enums.Wallet;
using Application.Domain.Models;
using Application.Helpers;
using Application.Persistence;
using Application.Persistence.Repositories;
using Application.Services;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace UnitTest
{
  public class TestHelper
  {
    private readonly DataContext dbContext;

    public TestHelper()
    {
      var builder = new DbContextOptionsBuilder<DataContext>();
      builder.UseInMemoryDatabase(databaseName: "UnicareInMemory");

      var dbContextOptions = builder.Options;
      dbContext = new DataContext(dbContextOptions);
      // Delete existing db before creating a new one
      dbContext.Database.EnsureDeleted();
      dbContext.Database.EnsureCreated();
    }

    public async Task InitDefaultData()
    {
      var roleCounts = await dbContext.Roles.CountAsync();
      if (roleCounts == 0)
      {
        dbContext.Roles.Add(new Role() { RoleId = "ADMIN", RoleName = "Administrator" });
        dbContext.Roles.Add(new Role() { RoleId = "USER", RoleName = "User" });
      }

      var systemWallet = await dbContext.Wallets.Where(w => w.IsSystem).FirstOrDefaultAsync();
      if (systemWallet == null)
      {
        dbContext.Wallets.AddRange(new Wallet()
        {
          WalletToken = WalletToken.Point,
          WalletType = WalletType.Cold,
          WalletStatus = WalletStatus.Available,
          Amount = 100000000,
          IsSystem = true,
          ExpiredDate = DateTimeHelper.Now().AddYears(1000)
        },
        new Wallet()
        {
          WalletToken = WalletToken.XP,
          WalletType = WalletType.Cold,
          WalletStatus = WalletStatus.Available,
          Amount = 100000000,
          IsSystem = true,
          ExpiredDate = DateTimeHelper.Now().AddYears(1000)
        });
      }

      // Attribute Groups
      var attGroups = await dbContext.AttributeGroups.ToListAsync();
      if (attGroups.Count() == 0)
      {

        var defaultAtts = new List<AttributeGroup>() {
          // P1
          new AttributeGroup() {
            AttributeGroupId = AttributeGroupNameValues.BasePoint,
            AttributeGroupName = "Base Point",
            AttributeGroupDescription = "[P1] - Base point of Level",
          },
          new AttributeGroup() {
            AttributeGroupId = AttributeGroupNameValues.PointPerHour,
            AttributeGroupName = "Point per Hour",
            AttributeGroupDescription = "[P1] - Point Per Hours Of Level",
          },
          // P2
          new AttributeGroup() {
            AttributeGroupId = AttributeGroupNameValues.SoftSkill,
            AttributeGroupName = "Soft Skill",
            AttributeGroupDescription = "[P2] [Calculated] - Soft Skill of member in a project",
          },
          new AttributeGroup() {
            AttributeGroupId = AttributeGroupNameValues.HardSkill,
            AttributeGroupName = "Hard Skill",
            AttributeGroupDescription = "[P2] [Calculated] - Hard Skill of member in a project",
          },

          new AttributeGroup() {
            AttributeGroupId = AttributeGroupNameValues.LeadershipSkill,
            AttributeGroupName = "Leadership Skill",
            AttributeGroupDescription = "[P2] - Leadership skill of member in a project",
          },
          new AttributeGroup() {
            AttributeGroupId = AttributeGroupNameValues.ProblemSolvingSkill,
            AttributeGroupName = "Problem Solving Skill",
            AttributeGroupDescription = "[P2] - Problem solving Skill of member in a project",
          },
          new AttributeGroup() {
            AttributeGroupId = AttributeGroupNameValues.PositiveAttitude,
            AttributeGroupName = "Positive Attitude",
            AttributeGroupDescription = "[P2] - Possitive attitude of member in a project",
          },
          new AttributeGroup() {
            AttributeGroupId = AttributeGroupNameValues.TeamworkSkill,
            AttributeGroupName = "Teamwork Skill",
            AttributeGroupDescription = "[P2] - Team work skill of member in a project",
          },
          new AttributeGroup() {
            AttributeGroupId = AttributeGroupNameValues.CreativitySkill,
            AttributeGroupName = "Teamwork Skill",
            AttributeGroupDescription = "[P2] - Creativity skill of member in a project",
          },
           new AttributeGroup() {
            AttributeGroupId = AttributeGroupNameValues.CommunicationSkill,
            AttributeGroupName = "Teamwork Skill",
            AttributeGroupDescription = "[P2] - Communication skill of member in a project",
          },
        };

        dbContext.AttributeGroups.AddRange(defaultAtts);
      }

      // Level
      var levels = await dbContext.Levels.ToListAsync();
      var defaultLevel = new Level()
      {
        LevelName = "Seed",
        BasePoint = 10,
        BasePointPerHour = 1,
        XPNeeded = 0,
        MinWorkHour = 1,
        MaxWorkHour = 1,
        LevelColor = "#ffffff"
      };

      if (levels.Count == 0)
      {
        dbContext.Levels.Add(defaultLevel);
      }

      await dbContext.SaveChangesAsync();
    }

    public static IConfiguration GetConfiguration()
    {
      var config = new ConfigurationBuilder()
         .AddJsonFile("appsettings.test.json")
          .AddEnvironmentVariables()
          .Build();
      return config;
    }

    public UnitOfWork GetUnitOfWork()
    {
      return new UnitOfWork(dbContext);
    }

    public RedisQueueService GetRedisQueueService()
    {
      var _redisQueueService = new RedisQueueService(GetConfiguration());
      // _redisQueueService.Setup(_ => _.AddToQueue("", "")).Returns(Task.CompletedTask);
      return _redisQueueService;
    }

    public IMapper GetMapper()
    {
      MapperConfiguration mapperConfig = new MapperConfiguration(
      cfg =>
      {
        cfg.AddProfile(new MappingProfile());
      });

      return new Mapper(mapperConfig);
    }
  }
}