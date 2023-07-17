using Application.Domain.Enums.Wallet;
using Application.Domain.Models;
using Application.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Domain.Constants;
using Application.Domain.Enums.User;
using Attribute = Application.Domain.Models.Attribute;

namespace Application.Persistence
{
    public static class SeedHelper
    {
        public static async Task Seed(IServiceProvider services)
        {
            var serviceScope = services.CreateScope();
            var sp = serviceScope.ServiceProvider;
            var dbContext = sp.GetService<DataContext>()!;

            await dbContext.Database.MigrateAsync();
            await Seed(dbContext);
        }

        public static async Task Seed(DataContext dbContext)
        {
            //var reportWithoutAttrs = await dbContext.ProjectReportMembers
            //    .Where(x => x.ProjectReportMemberAttributes.Count < 2).ToListAsync();

            //var projectMembers = await dbContext.ProjectMembers
            //    .Include(x => x.ProjectMemberAttributes)
            //    .ThenInclude(x => x.Attribute)
            //    .Where(x => reportWithoutAttrs.Select(x => x.ProjectMemberId).Contains(x.ProjectMemberId))
            //        .ToListAsync();

            //reportWithoutAttrs.ForEach(x =>
            //{
            //    var pm = projectMembers.First(p => p.ProjectMemberId == x.ProjectMemberId);
            //    var attSoft = new ProjectReportMemberAttribute()
            //    {
            //        ProjectReportMemberId = x.ProjectReportMemberId,
            //        Attribute = new Attribute
            //        {
            //            AttributeGroupId = AttributeGroupNameValues.SoftSkill,
            //            Value = pm.ProjectMemberAttributes
            //                .First(pma => pma.Attribute.AttributeGroupId == AttributeGroupNameValues.SoftSkill).Attribute
            //                .Value
            //        }
            //    };

            //    var attHard = new ProjectReportMemberAttribute()
            //    {
            //        ProjectReportMemberId = x.ProjectReportMemberId,
            //        Attribute = new Attribute
            //        {
            //            AttributeGroupId = AttributeGroupNameValues.HardSkill,
            //            Value = pm.ProjectMemberAttributes
            //                .First(pma => pma.Attribute.AttributeGroupId == AttributeGroupNameValues.HardSkill).Attribute
            //                .Value
            //        }
            //    };

            //    dbContext.ProjectReportMemberAttributes.Add(attSoft);
            //    dbContext.ProjectReportMemberAttributes.Add(attHard);
            //});


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
            if (!attGroups.Any())
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
                    AttributeGroupName = "Creativity Skill",
                    AttributeGroupDescription = "[P2] - Creativity skill of member in a project",
                  },
                   new AttributeGroup() {
                    AttributeGroupId = AttributeGroupNameValues.CommunicationSkill,
                    AttributeGroupName = "Communication Skill",
                    AttributeGroupDescription = "[P2] - Communication skill of member in a project",
                  },


                   new AttributeGroup() {
                    AttributeGroupId = AttributeGroupNameValues.IsGraduated,
                    AttributeGroupName = "Communication Skill",
                    AttributeGroupDescription = "[P2] - Member is graduated in a project",
                  },
                   new AttributeGroup() {
                    AttributeGroupId = AttributeGroupNameValues.HadEnglishCertificate,
                    AttributeGroupName = "Communication Skill",
                    AttributeGroupDescription = "[P2] - Member had english certificate in a project",
                  },
                   new AttributeGroup() {
                    AttributeGroupId = AttributeGroupNameValues.YearsOfExperience,
                    AttributeGroupName = "Communication Skill",
                    AttributeGroupDescription = "[P2] - Member's Years Of Experience in a project",
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
                MinWorkHour = 0,
                MaxWorkHour = 0,
                LevelColor = "#cddc39",
            };

            var defaultLevel2 = new Level()
            {
                LevelName = "Leaf",
                BasePoint = 20,
                BasePointPerHour = 2,
                XPNeeded = 200,
                MinWorkHour = 75,
                MaxWorkHour = 125,
                LevelColor = "#8bc34a",
            };

            if (!levels.Any())
            {
                dbContext.Levels.Add(defaultLevel);
                dbContext.Levels.Add(defaultLevel2);
            }

            //var admin = await dbContext.Users.FirstOrDefaultAsync(x => x.EmailAddress == "admin@gmail.com");
            //if (admin == null)
            //{
            //    dbContext.Add(new User()
            //    {
            //        LoginType = LoginType.Password,
            //        EmailAddress = "admin@gmail.com",
            //        Password =
            //            "$CNB$V1$10000$kD9fK0zhvxmvZfNazMRm4aS9GYiLKetEIDIWsgSVTSOCfpGwCmNIYve6apDATJCvx3bObaG5CVfBw4Ejv8yuVw==",
            //        RoleId = "Admin",
            //        Member = new Member()
            //        {
            //            EmailAddress = "admin@gmail.com",
            //            FullName = "Admin",
            //            MemberLevels = new List<MemberLevel>()
            //            {
            //                new()
            //                {
            //                    LevelId = defaultLevel.LevelId
            //                }
            //            }
            //        }
            //    });
            //}

            // var projectWithWallet = await dbContext.Projects.Include(p => p.ProjectWallets).ThenInclude(pw => pw.Wallet).ToListAsync();
            // projectWithWallet.ForEach(p =>
            // {
            //   var mainWallets = p.ProjectWallets.Where(p => p.Wallet.WalletTag == "bonus").ToList();
            //   if (mainWallets.Count() > 0) mainWallets.RemoveAt(0);

            //   mainWallets.ForEach(w =>
            //   {
            //     dbContext.ProjectWallets.Remove(w);
            //     dbContext.Wallets.Remove(w.Wallet);
            //   });
            // });
            await dbContext.SaveChangesAsync();
        }
    }
}
