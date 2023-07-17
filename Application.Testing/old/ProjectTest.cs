// using System.Diagnostics;
// using System.Text.Json;
// using Application.Controllers;
// using Application.DTOs;
// using Application.DTOs.Project;
// using Application.Enums.Project;
// using Application.Enums.ProjectMember;
// using Application.Helpers;
// using Application.Models;
// using Application.QueryParams.Project;
// using Application.Repositories;
// using Application.Services;
// using Microsoft.EntityFrameworkCore;

// [assembly: CollectionBehavior(DisableTestParallelization = true)]
// namespace UnitTest
// {
//   public class ProjectTest : IAsyncLifetime
//   {
//     ProjectService _projectService = null!;
//     UnitOfWork _unitOfWork = null!;

//     List<Member> testMembers = new List<Member>() {
//       new Member() {
//         FullName = "User1",
//         EmailAddress = "user1@gmail.com",
//         PhoneNumber = "123456789",
//       },
//       new Member() {
//         FullName = "User2",
//         EmailAddress = "user2@gmail.com",
//         PhoneNumber = "123456789",
//       },
//     };

//     List<Project> testProjects = new List<Project>() {
//       new Project()
//       {
//         ProjectName = "Project 1",
//         ProjectShortName = "P1",
//         ProjectLongDescription = "Description",
//         ProjectShortDescription = "Description",
//         ProjectType = ProjectType.Application,
//         ProjectStatus = ProjectStatus.Created,
//         Budget = 10
//       },
//       new Project()
//       {
//         ProjectName = "Project 1",
//         ProjectShortName = "P1",
//         ProjectLongDescription = "Description",
//         ProjectShortDescription = "Description",
//         ProjectType = ProjectType.Application,
//         ProjectStatus = ProjectStatus.Started,
//         Budget = 10
//       }
//     };

//     public async Task<Guid> CreateProject(String managerEmail)
//     {
//       var dto = new ProjectCreateDTO()
//       {
//         ProjectName = "New Name For You!",
//         ProjectDescription = "New Description For You!",
//         ProjectShortName = "NNFY",
//         ProjectManagerEmail = managerEmail,
//         Budget = 10
//       };

//       var result = await _projectService.CreateProject(dto);
//       return result;
//     }

//     public async Task InitializeAsync()
//     {
//       var helper = new TestHelper();
//       await helper.InitDefaultData();

//       _unitOfWork = helper.GetUnitOfWork();
//       var _redisQueueService = helper.GetRedisQueueService();
//       var _mapper = helper.GetMapper();

//       // Insert Values
//       _unitOfWork.ProjectRepository.Add(testProjects);
//       _unitOfWork.MemberRepository.Add(testMembers);

//       await _unitOfWork.SaveAsync();

//       _projectService = new ProjectService(_unitOfWork, null, _mapper, _redisQueueService, null);
//     }

//     public Task DisposeAsync()
//     {
//       return Task.CompletedTask;
//     }

//     [Fact]
//     public async Task Project_GetAll_ShouldReturnAll()
//     {
//       var projects = await _projectService.GetAll(new ProjectQueryParams());
//       Assert.True(projects.Count() == testProjects.Count());
//     }

//     [Fact]
//     public async Task Project_GetAllWithPaging_ShouldReturn1()
//     {
//       var projects = await _projectService.GetAll(new ProjectQueryParams() { PageNumber = 1, PageSize = 1 });
//       Assert.True(projects.Count() == 1);
//     }

//     [Fact]
//     public async Task Project_GetByID_ShouldReturnProject()
//     {
//       var project = await _projectService.GetProjectById(testProjects[0].ProjectId, "admin@gmail.com", true);

//       Assert.NotNull(project);
//     }

//     [Fact]
//     public async Task Project_GetByID_NotFound_ThrowException()
//     {
//       var act = () => _projectService.GetProjectById(Guid.NewGuid(), "admin@gmail.com", true);
//       await Assert.ThrowsAsync<NotFoundException>(act);
//     }

//     [Fact]
//     public async Task Project_Update_Success()
//     {
//       var dto = new ProjectAdminUpdateDTO()
//       {
//         ProjectId = testProjects[0].ProjectId,
//         ProjectName = "New Name For You!",
//         ProjectDescription = "New Description For You and it's very long so it can be cut at anytime but i don't know when goddamnit AAAAAA!",
//         ProjectShortName = "NNFY",
//         Budget = 10
//       };

//       var result = await _projectService.UpdateProjectAsAdmin(dto);
//       Assert.True(result);
//       Assert.Equal(testProjects[0].ProjectName, dto.ProjectName);
//       Assert.Equal(testProjects[0].ProjectShortDescription, StringHelper.SubstringWithDots(dto.ProjectDescription, 0, 100));
//       Assert.Equal(testProjects[0].ProjectLongDescription, dto.ProjectDescription);
//       Assert.Equal(testProjects[0].ProjectShortName, dto.ProjectShortName);
//       Assert.Equal(testProjects[0].Budget, dto.Budget);
//     }

//     [Fact]
//     public async Task Project_UpdatePM_AsPM_Success()
//     {
//       var resultId = await CreateProject(testMembers[0].EmailAddress);

//       var dto = new ProjectPMUpdateDTO()
//       {
//         ProjectId = resultId,
//         ProjectDescription = "New Description",
//         ProjectShortName = "NNFY",
//       };

//       var result = await _projectService.UpdateProjectAsPM(dto, testMembers[0].EmailAddress);
//       Assert.True(result);
//     }

//     [Fact]
//     public async Task Project_UpdatePM_NotAsPM_ThrowException()
//     {
//       var resultId = await CreateProject(testMembers[0].EmailAddress);

//       var dto = new ProjectPMUpdateDTO()
//       {
//         ProjectId = resultId,
//         ProjectDescription = "New Description",
//         ProjectShortName = "NNFY",
//       };

//       var act = () => _projectService.UpdateProjectAsPM(dto, testMembers[1].EmailAddress);
//       await Assert.ThrowsAsync<BadRequestException>(act);
//     }


//     [Fact]
//     public async Task Project_Update_NotFound_ThrowException()
//     {
//       var dto = new ProjectAdminUpdateDTO()
//       {
//         ProjectId = Guid.NewGuid(),
//         ProjectName = "Bad Project",
//       };

//       var act = () => _projectService.UpdateProjectAsAdmin(dto);
//       await Assert.ThrowsAsync<NotFoundException>(act);
//     }

//     [Fact]
//     public async Task Project_Create_Success_CheckAllShape()
//     {
//       var resultId = await CreateProject(testMembers[0].EmailAddress);

//       var createdProject = await _unitOfWork.ProjectRepository.GetQuery()
//         .Include(p => p.ProjectMember)
//           .ThenInclude(p => p.ProjectMemberAttribute)
//             .ThenInclude(p => p.Attribute)
//         .Include(p => p.ProjectWallets)
//           .ThenInclude(pw => pw.Wallet)
//         .Where(p => p.ProjectId == resultId)
//           .FirstOrDefaultAsync();

//       Assert.NotNull(createdProject);

//       // Assert Wallets
//       var wallets = createdProject!.ProjectWallets.Select(p => p.Wallet).ToList();
//       Assert.True(wallets.Count() == 2);
//       Assert.NotNull(wallets.FirstOrDefault(w => w.WalletTag == "main"));
//       Assert.NotNull(wallets.FirstOrDefault(w => w.WalletTag == "bonus"));

//       // Assert Members
//       var projectMember = createdProject!.ProjectMember.FirstOrDefault();
//       Assert.NotNull(projectMember);
//       Assert.Equal(projectMember!.MemberId, testMembers[0].MemberId);
//       Assert.Equal(projectMember!.Role, ProjectMemberRole.Manager);

//       // Asserts Attribute
//       var attributes = projectMember.ProjectMemberAttribute.Select(s => s.Attribute).ToList();
//       Assert.True(attributes.Count() == 11);

//       var nowPrjs = await _projectService.GetAll(new ProjectQueryParams());
//       Assert.Equal(nowPrjs.Count(), 3);

//       // Asserts Get APIs
//       var walletsFromAPI = await _projectService.GetProjectWalletById(createdProject.ProjectId, testMembers[0].EmailAddress);
//       Assert.Equal(walletsFromAPI.Count(), 2);

//     }

//     [Fact]
//     public async Task Project_Create_GetSelf()
//     {
//       var dto = new ProjectCreateDTO()
//       {
//         ProjectName = "New Name For You!",
//         ProjectDescription = "New Description For You!",
//         ProjectShortName = "NNFY",
//         ProjectManagerEmail = testMembers[0].EmailAddress,
//         Budget = 10
//       };

//       var result = await _projectService.CreateProject(dto);

//       var projects = await _projectService.GetAllSelf(new ProjectSelfQueryParams(), testMembers[0].EmailAddress);
//       Assert.Equal(projects.Count(), 1);
//     }

//     [Fact]
//     public async Task Project_UpdateStatus_Success()
//     {
//       var toStart = new ProjectStatusUpdateDTO()
//       {
//         ProjectId = testProjects[0].ProjectId,
//         ProjectStatus = ProjectStatus.Started
//       };

//       var result = await _projectService.UpdateProjectStatus(toStart, testMembers[0].EmailAddress, true);
//       var project = await _unitOfWork.ProjectRepository.GetByID(testProjects[0].ProjectId);
//       Assert.Equal(testProjects[0].ProjectStatus, ProjectStatus.Started);

//       var toEnd = new ProjectStatusUpdateDTO()
//       {
//         ProjectId = testProjects[0].ProjectId,
//         ProjectStatus = ProjectStatus.Ended
//       };

//       var result2 = await _projectService.UpdateProjectStatus(toEnd, testMembers[0].EmailAddress, true);
//       var project2 = await _unitOfWork.ProjectRepository.GetByID(testProjects[0].ProjectId);
//       Assert.Equal(testProjects[0].ProjectStatus, ProjectStatus.Ended);
//     }

//     [Fact]
//     public async Task Project_UpdateStatus_WrongState_ThrowException()
//     {
//       var toStart = new ProjectStatusUpdateDTO()
//       {
//         ProjectId = testProjects[0].ProjectId,
//         ProjectStatus = ProjectStatus.Started
//       };

//       var result = await _projectService.UpdateProjectStatus(toStart, testMembers[0].EmailAddress, true);
//       var project = await _unitOfWork.ProjectRepository.GetByID(testProjects[0].ProjectId);
//       Assert.Equal(testProjects[0].ProjectStatus, ProjectStatus.Started);

//       var act = () => _projectService.UpdateProjectStatus(toStart, testMembers[0].EmailAddress, true);
//       await Assert.ThrowsAsync<BadRequestException>(act);
//     }

//     [Fact]
//     public async Task Project_GetAll_ShouldReturnAll2()
//     {
//       var projects = await _projectService.GetAll(new ProjectQueryParams());
//       Assert.True(projects.Count() == testProjects.Count());
//     }
//   }
// }