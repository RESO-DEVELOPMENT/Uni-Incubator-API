using Application.Domain;
using Application.Domain.Enums.Project;
using Application.Domain.Enums.ProjectMember;
using Application.Domain.Enums.ProjectMemberRequest;
using Application.Domain.Enums.User;
using Application.Domain.Models;
using Application.DTOs;
using Application.DTOs.Member;
using Application.DTOs.Project;
using Application.DTOs.ProjectMember;
using Application.DTOs.ProjectMemberRequest;
using Application.DTOs.ProjectSponsor;
using Application.DTOs.SalaryCycle;
using Application.DTOs.User;
using Application.Helpers;
using Application.Persistence.Repositories;
using Application.QueryParams.ProjectMemberRequest;
using MailKit.Search;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Application.Domain.Enums.MemberVoucher;
using Application.Domain.Enums.ProjectReport;
using Application.Domain.Enums.SalaryCycle;
using Application.Domain.Enums.Supplier;
using Application.Domain.Enums.Voucher;
using Application.Domain.Enums.Wallet;
using Application.DTOs.Level;
using Application.DTOs.MemberVoucher;
using Application.DTOs.ProjectReport;
using Application.DTOs.Transaction;
using Application.DTOs.Voucher;
using Application.DTOs.Wallet;
using Application.Persistence;
using Application.QueryParams.MemberVoucher;
using Azure;
using Xunit.Abstractions;
using Xunit.Priority;
using Google.Api.Gax.ResourceNames;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using StackExchange.Redis;
using Application.DTOs.MemberExport;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace Application.Testing
{
    //[Collection("WebApp Collection")]
    [Collection("Serial")]
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public class Tests
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program>
            _factory;

        private readonly ITestOutputHelper _outputHelper;

        private readonly UnitOfWork _unitOfWork;
        private static string _adminToken = null!;

        private static string _memberToken = null!;
        private static Guid _memberId;
        private static Guid _member1Id;

        private static string _member1Token = null!;
        private static string _member2Token = null!;

        private static Guid _projectId;
        private static Guid _sponsorId;
        private static Guid _salaryCycleId;
        private static Guid _projectSponsorId;
        private static Guid _supplierId;
        private static Guid _voucherId;
        private static int _levelId;

        public Tests(
            CustomWebApplicationFactory<Program> factory,
            ITestOutputHelper outputHelper)
        {
            _factory = factory;
            _outputHelper = outputHelper;

            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            _unitOfWork = (UnitOfWork)_factory.Services.GetService(typeof(UnitOfWork))!;
        }

        #region Pretest
        [Trait("Category", "System")]
        [Fact, Priority(-10)]
        public async Task System_PreTest()
        {
            await _unitOfWork.EnsureCreated();
            await SeedHelper.Seed(_unitOfWork._dataContext);
            // var role = await _unitOfWork.RoleRepository.GetByID("ADMIN");
            var level = await _unitOfWork.LevelRepository.GetFirstLevel();
            _unitOfWork.UserRepository.Add(new User()
            {
                LoginType = LoginType.Password,
                EmailAddress = "admin@gmail.com",
                Password = PasswordHasher.Hash("Asd123@@"),
                RoleId = "ADMIN",
                Member = new Member()
                {
                    EmailAddress = "admin@gmail.com",
                    FullName = "Admin",
                    MemberLevels = new List<MemberLevel>()
                    {
                        new()
                        {
                            LevelId = level.LevelId
                        }
                    }
                }
            });

            var supplier = new Supplier()
            {
                Name = "Test Supplier",
                Description = "Test Description",
                Status = SupplierStatus.Available,
            };

            _unitOfWork.SupplierRepository.Add(supplier);
            await _unitOfWork.SaveAsync();

            _supplierId = supplier.SupplierId;

            var response = await _client.Login("admin@gmail.com", "Asd123@@");
            var resJson = await Helper.DeserializeTo<ResponseDTO<UserLoginResultDTO>>(response);
            _adminToken = resJson.Message.Token;

            UserCreateDTO dto = new UserCreateDTO()
            {
                EmailAddress = "member@gmail.com",
                Password = "Asd123@@",
                FullName = "Member",
                PhoneNumber = "123456789",
                SendEmail = false,
                IsAdmin = false
            };

            UserCreateDTO dto1 = new UserCreateDTO()
            {
                EmailAddress = "member1@gmail.com",
                Password = "Asd123@@",
                FullName = "Member 1",
                PhoneNumber = "123456789",
                SendEmail = false,
                IsAdmin = false
            };

            UserCreateDTO dto2 = new UserCreateDTO()
            {
                EmailAddress = "member2@gmail.com",
                Password = "Asd123@@",
                FullName = "Member 2",
                PhoneNumber = "123456789",
                SendEmail = false,
                IsAdmin = false
            };


            await _client.CallWithToken("/v1/users", HttpMethod.Post, dto, _adminToken);
            await _client.CallWithToken("/v1/users", HttpMethod.Post, dto1, _adminToken);
            await _client.CallWithToken("/v1/users", HttpMethod.Post, dto2, _adminToken);

            var mRes = await _client.Login("member@gmail.com", "Asd123@@");
            var mJson = await Helper.DeserializeTo<ResponseDTO<UserLoginResultDTO>>(mRes);
            _memberToken = mJson.Message.Token;

            var m1Res = await _client.Login("member1@gmail.com", "Asd123@@");
            var m1Json = await Helper.DeserializeTo<ResponseDTO<UserLoginResultDTO>>(m1Res);
            _member1Token = m1Json.Message.Token;

            var m2Res = await _client.Login("member2@gmail.com", "Asd123@@");
            var m2Json = await Helper.DeserializeTo<ResponseDTO<UserLoginResultDTO>>(m2Res);
            _member2Token = m2Json.Message.Token;

            var mMeRes = await _client.CallWithToken("v1/members/me", HttpMethod.Get, null, _memberToken);
            var mMeJson = await Helper.DeserializeTo<ResponseDTO<MemberDetailedWithRoleDTO>>(mMeRes);
            _memberId = mMeJson.Message.MemberId;

            ProjectCreateDTO projectDTO = new ProjectCreateDTO()
            {
                ProjectName = "Test Project",
                ProjectShortName = "TPP",
                ProjectDescription = "Test Project Description",
                ProjectManagerEmail = "member@gmail.com",
                Budget = 100,
            };

            var projectRes = await _client.CallWithToken("v1/projects", HttpMethod.Post, projectDTO, _adminToken);
            var projectJson = await Helper.DeserializeTo<ResponseDTO<Guid>>(projectRes);

            _projectId = projectJson.Message;

            var memberInDb = await _unitOfWork.MemberRepository.GetQuery()
                .Include(x => x.MemberWallets.Where(x => x.Wallet.WalletStatus == WalletStatus.Available &&
                                                         x.Wallet.WalletType == WalletType.Cold &&
                                                         x.Wallet.WalletToken == WalletToken.Point))
                .ThenInclude(x => x.Wallet)
                .FirstAsync(x => x.MemberId == _memberId);

            memberInDb.MemberWallets.First().Wallet.Amount = 1000;

            await _unitOfWork.SaveAsync();

            var member1InDb = await _unitOfWork.MemberRepository.GetQuery()
                .FirstAsync(x => x.EmailAddress == "member1@gmail.com");

            _member1Id = member1InDb.MemberId;
        }
        #endregion

        #region Authentication

        [Trait("Category", "Authentication")]
        [Theory]
        [InlineData(null, null)]
        [InlineData(null, "")]
        [InlineData(null, "ValidPassword")]
        [InlineData("", null)]
        [InlineData("", "null")]
        [InlineData("", "ValidPassword")]
        [InlineData("ValidEmail@gmail.com", null)]
        [InlineData("ValidEmail@gmail.com", "")]
        //[InlineData("ValidEmail@gmail.com", "password")]
        public async Task Login_Fail(string? email, string? password)
        {
            var response = await _client.Login(email, password);
            var resJson = await response.Content.ReadAsStringAsync();

            _outputHelper.WriteLine(resJson);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Authentication")]
        [Fact, Priority(1)]
        public async Task Login_Success()
        {
            var response = await _client.Login("admin@gmail.com", "Asd123@@");
            var resJson = await Helper.DeserializeTo<ResponseDTO<UserLoginResultDTO>>(response);
            _adminToken = resJson.Message.Token;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("admin@gmail.com", resJson.Message.User.EmailAddress);

            var mRes = await _client.Login("member@gmail.com", "Asd123@@");
            var mJson = await Helper.DeserializeTo<ResponseDTO<UserLoginResultDTO>>(mRes);
            _memberToken = mJson.Message.Token;

            var m1Res = await _client.Login("member1@gmail.com", "Asd123@@");
            var m1Json = await Helper.DeserializeTo<ResponseDTO<UserLoginResultDTO>>(m1Res);
            _member1Token = m1Json.Message.Token;

            var m2Res = await _client.Login("member2@gmail.com", "Asd123@@");
            var m2Json = await Helper.DeserializeTo<ResponseDTO<UserLoginResultDTO>>(m2Res);
            _member2Token = m2Json.Message.Token;
        }

        #endregion

        #region Member
        [Trait("Category", "Member_Create")]
        [Theory, Priority(2)]
        [InlineData("", "invalid", "", "")]
        [InlineData("", "88888888", "", "")]
        [InlineData("validEmail@gmail.com", "invalid", "", "")]
        [InlineData("validEmail@gmail.com", "88888888", "", "")]
        [InlineData("validEmail@gmail.com", "88888888", "Valid Name", "")]
        public async Task Member_CreateFail(String? email, String? password, String? Fullname, String? PhoneNumber)
        {
            // Arrange
            UserCreateDTO dto = new UserCreateDTO()
            {
                EmailAddress = email,
                Password = password,
                FullName = Fullname,
                PhoneNumber = PhoneNumber,
                SendEmail = false,
                IsAdmin = false
            };

            var response = await _client.CallWithToken("/v1/users", HttpMethod.Post, dto, _adminToken);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Member_Create")]
        [Theory, Priority(2)]
        [InlineData("validEmail1@gmail.com", "88888888", "Valid Name", "0909909909")]
        [InlineData("validEmail2@gmail.com", "1234567890", "Valid Name", "0909909909")]
        public async Task Member_CreateSuccess(String? email, String? password, String? Fullname, String PhoneNumber)
        {
            // Arrange
            UserCreateDTO dto = new UserCreateDTO()
            {
                EmailAddress = email,
                Password = password,
                FullName = Fullname,
                PhoneNumber = PhoneNumber,
                SendEmail = false,
                IsAdmin = false
            };

            var response = await _client.CallWithToken("/v1/users", HttpMethod.Post, dto, _adminToken);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Trait("Category", "Member_UpdateSelf")]
        [Theory, Priority(5)]
        [InlineData(null, "1", null)]
        [InlineData(null, "1", "")]
        [InlineData(null, "1", "facebooklink")]

        [InlineData("Validname", "1", "")]
        [InlineData("Validname", "1", "null")]
        [InlineData("Validname", "1", "facebooklink")]
        public async Task Member_UpdateSelf_Failed(string? fullName, string? phoneNumber, string? facebookUrl)
        {
            MemberUpdateDTO dto = new MemberUpdateDTO()
            {
                FullName = fullName,
                PhoneNumber = phoneNumber,
                FacebookUrl = facebookUrl
            };

            var response = await _client.CallWithToken("/v1/members/me", HttpMethod.Patch, dto, _memberToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Member_UpdateSelf")]
        [Theory, Priority(6)]

        [InlineData(null, null, null)]
        [InlineData(null, null, "")]
        [InlineData(null, null, "facebooklink")]

        [InlineData(null, "0909090909", null)]
        [InlineData(null, "0909090909", "")]
        [InlineData(null, "0909090909", "facebooklink")]

        [InlineData("Valid Fullname", "0909090909", null)]
        [InlineData("Valid Fullname", "0909090909", "")]
        [InlineData("Valid Fullname", "0909090909", "facebooklink")]
        public async Task Member_UpdateSelf_Success(string? fullName, string? phoneNumber, string? facebookUrl)
        {
            MemberUpdateDTO dto = new MemberUpdateDTO()
            {
                FullName = fullName,
                PhoneNumber = phoneNumber,
                FacebookUrl = facebookUrl
            };

            var response = await _client.CallWithToken("/v1/members/me", HttpMethod.Patch, dto, _memberToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Trait("Category", "Member_UpdateStatus")]
        [Theory, Priority(7)]
        [InlineData(false, null)]
        [InlineData(false, "Available")]
        [InlineData(false, "Invalid")]
        [InlineData(true, null)]
        [InlineData(true, "Invalid")]

        public async Task Member_UpdateStatus_Failed(bool useMember, String? status)
        {
            object dto = new
            {
                MemberId = useMember ? _memberId.ToString() : null,
                Status = status
            };
            var response = await _client.CallWithToken($"/v1/members/status", HttpMethod.Put, dto, _adminToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Member_UpdateStatus")]
        [Theory, Priority(8)]
        [InlineData("Available")]
        public async Task Member_UpdateStatus_Success(String status)
        {
            object dto = new
            {
                MemberId = _memberId,
                Status = status
            };

            var response = await _client.CallWithToken("/v1/members/status", HttpMethod.Put, dto, _adminToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Trait("Category", "Member_GetAll")]
        public async Task Member_GetAll_Success()
        {


            var response = await _client.CallWithToken("/v1/members", HttpMethod.Get, null, _adminToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Trait("Category", "Member_Search")]
        [Theory, Priority(9)]
        [InlineData(null, null, "NotAnOrder")]
        [InlineData(null, "Admin", "NotAnOrder")]
        [InlineData("admin@gmail.com", null, "NotAnOrder")]
        [InlineData("admin@gmail.com", "Admin", "NotAnOrder")]
        public async Task Member_Search_Fail(string? email, string? fullName, string? orderBy)
        {
            var queryParams = new Dictionary<string, string?>()
            {
                { "EmailAddress", email },
                { "FullName", fullName },
                { "OrderBy", orderBy }
            };

            var response = await _client.CallWithQuery("/v1/members", HttpMethod.Get, queryParams, _adminToken);
            _outputHelper.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Member_Search")]
        [Theory, Priority(10)]
        [InlineData(null, null, "DateDesc")]
        [InlineData(null, "Admin", null)]
        [InlineData(null, "Admin", "DateDesc")]
        [InlineData("admin@gmail.com", null, null)]
        [InlineData("admin@gmail.com", null, "DateDesc")]
        [InlineData("admin@gmail.com", "Admin", null)]
        [InlineData("admin@gmail.com", "Admin", "DateDesc")]
        public async Task Member_Search_Success(string? email, string? fullName, string? orderBy)
        {
            var queryParams = new Dictionary<string, string?>()
            {
                { "EmailAddress", email },
                { "FullName", fullName },
                { "OrderBy", orderBy }
            };

            var response = await _client.CallWithQuery("/v1/members", HttpMethod.Get, queryParams, _adminToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Trait("Category", "Member_GetSelf")]
        [Fact, Priority(10)]
        public async Task Member_GetSelf_Null()
        {
            var response = await _client.CallWithToken($"/v1/members/me", HttpMethod.Get, null, _adminToken);
            var resJson = await Helper.DeserializeTo<ResponseDTO<MemberDetailedWithRoleDTO>>(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("admin@gmail.com", resJson.Message.EmailAddress);
        }

        [Trait("Category", "Member_GetByID")]
        [Fact, Priority(10)]
        public async Task Member_GetById_Null()
        {
            var response = await _client.CallWithToken($"/v1/members/null", HttpMethod.Get, null, _adminToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Member_GetByID")]
        [Fact, Priority(10)]
        public async Task Member_GetById_WrongId()
        {
            var response = await _client.CallWithToken($"/v1/members/{Guid.NewGuid()}", HttpMethod.Get, null, _adminToken);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Trait("Category", "Member_GetByID")]
        [Fact, Priority(10)]
        public async Task Member_GetById_ValidId()
        {
            var response = await _client.CallWithToken($"/v1/members/{_memberId}", HttpMethod.Get, null, _adminToken);
            var resJson = await Helper.DeserializeTo<ResponseDTO<MemberAdminDetailedDTO>>(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(_memberId, resJson.Message.MemberId);
        }

        #endregion

        #region Project

        [Trait("Category", "Project_Create")]
        [Theory, Priority(100)]
        [InlineData(null, null, null, null, null)]
        [InlineData(null, "Short", "Descriptionaaa", "member@gmail.com", "100")]
        [InlineData("A name", null, "Descriptionaaa", "member@gmail.com", "100")]
        [InlineData("A name", "Short", null, "member@gmail.com", "100")]
        [InlineData("A name", "Short", "Descriptionaaa", null, "100")]
        [InlineData("A name", "Short", "Descriptionaaa", "member@gmail.com", null)]
        [InlineData("A name", "Short", "Descriptionaaa", "member@gmail.com", "-1")]
        [InlineData("A name", "Short", "Descriptionaaa", "member@gmail.com", "1000000000")]
        [InlineData("A name", "TooLongForShort", "Descriptionaaa", "member@gmail.com", "1000000000")]
        public async Task Project_Create_Failed(string? name, string? shortname, string? desc, string? managerEmail, string? budget)
        {
            object dto = new
            {
                ProjectName = name,
                ProjectShortName = shortname,
                ProjectDescription = desc,
                ProjectManagerEmail = managerEmail,
                Budget = budget,
            };

            var response = await _client.CallWithToken("v1/projects", HttpMethod.Post, dto, _adminToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Project_Create")]
        [Theory, Priority(101)]
        [InlineData("Test Project AB", "TPA", "Description", "member@gmail.com", 1)]
        [InlineData("Test Project BB", "TPB", "Description", "member@gmail.com", 100)]
        public async Task Project_Create_Success(string? name, string? shortname, string? desc, string? managerEmail, double budget)
        {
            ProjectCreateDTO dto = new ProjectCreateDTO()
            {
                ProjectName = name,
                ProjectShortName = shortname,
                ProjectDescription = desc,
                ProjectManagerEmail = managerEmail,
                Budget = budget,
                SendEmailToPM = false
            };

            var response = await _client.CallWithToken("v1/projects", HttpMethod.Post, dto, _adminToken);

            _outputHelper.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Trait("Category", "Project_UpdateAsAdmin")]
        [Fact, Priority(103)]
        public async Task Project_UpdateAsAdmin_Failed_NotFound()
        {
            ProjectAdminUpdateDTO dto = new ProjectAdminUpdateDTO()
            {
                ProjectId = Guid.Empty
            };

            var response = await _client.CallWithToken("v1/projects/admin", HttpMethod.Patch, dto, _adminToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Trait("Category", "Project_UpdateAsAdmin")]
        [Fact, Priority(103)]
        public async Task Project_UpdateAsAdmin_Failed_Null()
        {
            object dto = new
            {
                ProjectId = "null"
            };

            var data = Helper.ToStringContent(dto);
            var response = await _client.CallWithToken("v1/projects/admin", HttpMethod.Patch, dto, _adminToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Project_UpdateAsAdmin")]
        [Fact, Priority(103)]
        public async Task Project_UpdateAsAdmin_Failed_NotFound_Invalid()
        {
            object dto = new
            {
                ProjectId = Guid.NewGuid()
            };

            var data = Helper.ToStringContent(dto);
            var response = await _client.CallWithToken("v1/projects/admin", HttpMethod.Patch, dto, _adminToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Trait("Category", "Project_UpdateAsAdmin")]
        [Theory, Priority(103)]
        [InlineData("A name 2", "TooLongForShort", "A description 2", "100")]
        [InlineData("A name 2", "Short2", "A description 2", "-1")]
        public async Task Project_UpdateAsAdmin_Failed(string? name, string? shortName, string? description, string? budget)
        {
            object dto = new
            {
                ProjectId = _projectId,
                ProjectName = name,
                ProjectShortName = shortName,
                ProjectDescription = description,
                Budget = budget
            };

            var data = Helper.ToStringContent(dto);
            var response = await _client.CallWithToken("v1/projects/admin", HttpMethod.Patch, dto, _adminToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Project_UpdateAsAdmin")]
        [Theory, Priority(103)]
        [InlineData("A name 2", null, null, null)]
        [InlineData("A name 2", "Short2", "Description 2", "1")]
        [InlineData("A name 2", "Short2", "Description 2", "100")]
        public async Task Project_UpdateAsAdmin_Success(string? name, string? shortName, string? description, string? budget)
        {
            object dto = new
            {
                ProjectId = _projectId,
                ProjectName = name,
                ProjectShortName = shortName,
                ProjectDescription = description,
                Budget = budget
            };

            var response = await _client.CallWithToken("v1/projects/admin", HttpMethod.Patch, dto, _adminToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Trait("Category", "Project_UpdateAsPM")]
        [Fact, Priority(104)]
        public async Task Project_UpdateAsPM_Failed_NotFound()
        {
            object dto = new
            {
                ProjectId = Guid.Empty
            };

            var response = await _client.CallWithToken("v1/projects/pm", HttpMethod.Patch, dto, _adminToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Trait("Category", "Project_UpdateAsPM")]
        [Fact, Priority(104)]
        public async Task Project_UpdateAsPM_NoPermission()
        {
            ProjectPMUpdateDTO dto = new()
            {
                ProjectId = _projectId,
                ProjectShortName = "TPP",
                ProjectDescription = "Test Project Description",
            };

            var response = await _client.CallWithToken("v1/projects/pm", HttpMethod.Patch, dto, _member1Token);
            var resJson = await Helper.DeserializeTo<ResponseDTO<bool>>(response);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(resJson.ErrorName, ErrorNameValues.NoPermission);
        }

        [Trait("Category", "Project_UpdateAsPM")]
        [Theory, Priority(104)]
        [InlineData(null, null)]
        //[InlineData("TooLongForShort", null)]
        public async Task Project_UpdateAsPM_Failed(string? projectShortName, string? projectDescription)
        {
            object dto = new
            {
                ProjectId = _projectId,
                ProjectShortName = projectShortName,
                ProjectDescription = projectDescription
            };

            var response = await _client.CallWithToken("v1/projects/pm", HttpMethod.Patch, dto, _memberToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Project_UpdateAsPM")]
        [Theory, Priority(104)]
        [InlineData(null, null)]
        [InlineData("Short2", null)]
        [InlineData("Short2", "A description")]
        public async Task Project_UpdateAsPM_Success(string? projectShortName, string? projectDescription)
        {
            object dto = new
            {
                ProjectId = _projectId,
                ProjectShortName = projectShortName,
                ProjectDescription = projectDescription
            };


            var response = await _client.CallWithToken("v1/projects/pm", HttpMethod.Patch, dto, _memberToken);
            var resJson = await Helper.DeserializeTo<ResponseDTO<bool>>(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(resJson.Message);
        }

        [Trait("Category", "Project_UpdateStatus")]
        [Fact, Priority(105)]
        public async Task Project_UpdateStatus_Failed_NotFound_Null()
        {
            object dto = new
            {
                ProjectId = "null",
                ProjectStatus = ProjectStatus.Started
            };

            var response = await _client.CallWithToken("v1/projects/status", HttpMethod.Put, dto, _memberToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }


        [Trait("Category", "Project_UpdateStatus")]
        [Fact, Priority(105)]
        public async Task Project_UpdateStatus_Failed_NotFound_Invalid()
        {
            object dto = new
            {
                ProjectId = Guid.Empty,
                ProjectStatus = ProjectStatus.Started
            };

            var response = await _client.CallWithToken("v1/projects/status", HttpMethod.Put, dto, _memberToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Trait("Category", "Project_UpdateStatus")]
        [Fact, Priority(105)]
        public async Task Project_UpdateStatus_Failed_NoPermission()
        {
            object dto = new
            {
                ProjectId = _projectId,
                ProjectStatus = ProjectStatus.Started
            };

            var response = await _client.CallWithToken("v1/projects/status", HttpMethod.Put, dto, _member2Token);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Project_UpdateStatus")]
        [Theory, Priority(105)]
        [InlineData("null")]
        [InlineData("Invalid Status")]
        public async Task Project_UpdateStatus_Failed(string? status)
        {
            object dto = new
            {
                ProjectId = _projectId,
                ProjectStatus = status
            };

            var response = await _client.CallWithToken("v1/projects/status", HttpMethod.Put, dto, _memberToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Project_UpdateStatus")]
        [Theory, Priority(105)]
        [InlineData("Started")]
        public async Task Project_UpdateStatus_Success(string? status)
        {
            object dto = new
            {
                ProjectId = _projectId,
                ProjectStatus = status
            };

            var response = await _client.CallWithToken("v1/projects/status", HttpMethod.Put, dto, _memberToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Trait("Category", "Project_GetByID")]
        [Fact, Priority(10)]
        public async Task Project_GetById_Null_Failed()
        {
            var response = await _client.CallWithToken($"/v1/projects/null", HttpMethod.Get, null, _adminToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Project_GetByID")]
        [Fact, Priority(10)]
        public async Task Project_GetById_WrongId_Failed()
        {
            var response = await _client.CallWithToken($"/v1/projects/{Guid.NewGuid()}", HttpMethod.Get, null, _adminToken);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Trait("Category", "Project_GetByID")]
        [Fact, Priority(10)]
        public async Task Project_GetById_ValidId_Success()
        {
            var response = await _client.CallWithToken($"/v1/projects/{_projectId}", HttpMethod.Get, null, _adminToken);
            var resJson = await Helper.DeserializeTo<ResponseDTO<ProjectWithProjectMemberRoleDTO>>(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(_projectId, resJson.Message.ProjectId);
        }


        [Trait("Category", "Project_GetAll")]
        [Fact, Priority(10)]
        public async Task Project_GetAll_Success()
        {
            var response = await _client.CallWithToken($"/v1/projects", HttpMethod.Get, null, _adminToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Trait("Category", "Project_Search")]
        [Theory, Priority(10)]
        [InlineData(null, "Invalid", "Invalid")]
        [InlineData(null, "Invalid", null)]
        [InlineData(null, null, "Invalid")]
        public async Task Project_Search_Failed(string? projectName, string? orderBy, string? status)
        {
            Dictionary<string, string> q = new()
            {
                {"ProjectName", projectName},
                {"OrderBy", orderBy},
                {"Status", status}
            };


            var response = await _client.CallWithQuery($"/v1/projects", HttpMethod.Get, q, _adminToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Project_Search")]
        [Theory, Priority(10)]
        [InlineData(null, null, null)]
        [InlineData(null, null, "Started")]
        [InlineData(null, "DateAsc", "Started")]
        public async Task Project_Search_Success(string? projectName, string? orderBy, string? status)
        {
            Dictionary<string, string> q = new()
            {
                {"ProjectName", projectName},
                {"OrderBy", orderBy},
                {"Status", status}
            };

            var response = await _client.CallWithQuery($"/v1/projects", HttpMethod.Get, q, _adminToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        #endregion

        #region Sponsor
        [Trait("Category", "Sponsor_Create")]
        [Theory, Priority(200)]
        [InlineData(null, null, null)]
        [InlineData("Sponsor Name", null, null)]
        [InlineData(null, "Sponsor Description", null)]
        [InlineData("Sponsor Name", "Sponsor Description", null)]
        public async Task Sponsor_Create_Fail(string? name, string? desc, string? type)
        {
            object dto = new
            {
                SponsorName = name,
                SponsorDescription = desc,
                Type = type,
            };

            var response = await _client.CallWithToken("v1/sponsors", HttpMethod.Post, dto, _adminToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Trait("Category", "Sponsor_Create")]
        [Theory, Priority(200)]
        [InlineData("Test Project A", "TPA", "Organization")]
        public async Task Sponsor_Create_Success(string? name, string? desc, string? type)
        {
            object dto = new
            {
                SponsorName = name,
                SponsorDescription = desc,
                Type = type,
            };

            var response = await _client.CallWithToken("v1/sponsor", HttpMethod.Post, dto, _adminToken);
            _outputHelper.WriteLine(await response.Content.ReadAsStringAsync());
            var resJson = await Helper.DeserializeTo<ResponseDTO<Guid>>(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _sponsorId = resJson.Message;
        }

        [Trait("Category", "Sponsor_Update")]
        [Fact, Priority(201)]
        public async Task Sponsor_Update_Null_NotFound()
        {
            object dto = new
            {
                SponsorId = "null",
            };

            var response = await _client.CallWithToken("v1/sponsor", HttpMethod.Patch, dto, _adminToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Sponsor_Update")]
        [Fact, Priority(201)]
        public async Task Sponsor_Update_Invalid_NotFound()
        {
            object dto = new
            {
                SponsorId = Guid.Empty
            };

            var response = await _client.CallWithToken("v1/sponsor", HttpMethod.Patch, dto, _adminToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }


        [Trait("Category", "Sponsor_Update")]
        [Fact, Priority(201)]
        public async Task Sponsor_Update_Failed()
        {
            object dto = new
            {
                SponsorId = _sponsorId,
                SponsorName = "Test",
                SponsorDescription = "Test",
                Type = "Invalid"
            };

            var response = await _client.CallWithToken("v1/sponsor", HttpMethod.Patch, dto, _adminToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Sponsor_Update")]
        [Theory, Priority(201)]
        [InlineData(null, null, null)]
        [InlineData("SponsorName", null, null)]
        [InlineData("SponsorName", "Sponsor Desc", null)]
        [InlineData("SponsorName", "Sponsor Desc", "Personal")]
        public async Task Sponsor_Update_Success(string? name, string? desc, string? type)
        {
            object dto = new
            {
                SponsorId = _sponsorId,
                SponsorName = name,
                SponsorDescription = desc,
                Type = type
            };

            var response = await _client.CallWithToken("v1/sponsor", HttpMethod.Patch, dto, _adminToken);
            _outputHelper.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Trait("Category", "Sponsor_GetAll")]
        [Fact, Priority(202)]
        public async Task Sponsor_GetAll_Success()
        {
            var response = await _client.CallWithToken("v1/sponsor", HttpMethod.Get, null, _adminToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Trait("Category", "Sponsor_Search")]
        [Theory, Priority(202)]
        [InlineData(null, "Invalid")]
        public async Task Sponsor_Search_Failed(string? name, string? status)
        {
            var query = new Dictionary<string, string?>()
            {
                { "SponsorName", name },
                { "Status", status }
            };
            var response = await _client.CallWithQuery("v1/sponsor", HttpMethod.Get, query, _adminToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }


        [Trait("Category", "Sponsor_Search")]
        [Theory, Priority(202)]
        [InlineData(null, null)]
        [InlineData("Sponsor", null)]
        [InlineData("Sponsor", "Active")]
        public async Task Sponsor_Search_Success(string? name, string? status)
        {
            var query = new Dictionary<string, string?>()
            {
                { "SponsorName", name },
                { "Status", status }
            };
            var response = await _client.CallWithQuery("v1/sponsor", HttpMethod.Get, query, _adminToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        }

        [Trait("Category", "Sponsor_GetByID")]
        [Fact, Priority(203)]
        public async Task Sponsor_GetByID_Failed_Null()
        {
            var response = await _client.CallWithToken($"v1/sponsor/null", HttpMethod.Get, null, _adminToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }


        [Trait("Category", "Sponsor_GetByID")]
        [Fact, Priority(203)]
        public async Task Sponsor_GetByID_NotFound_Invalid()
        {
            var response = await _client.CallWithToken($"v1/sponsor/{Guid.Empty}", HttpMethod.Get, null, _adminToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Trait("Category", "Sponsor_GetByID")]
        [Fact, Priority(203)]
        public async Task Sponsor_GetByID_Success()
        {
            var response = await _client.CallWithToken($"v1/sponsor/{_sponsorId}", HttpMethod.Get, null, _adminToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }


        //[Trait("Category", "Sponsor_Update")]
        //[Theory, Priority(201)]
        //[InlineData("Test Project A", "TPA", "Organization")]
        //public async Task Sponsor_Update_Success(string? name, string? desc, string? type)
        //{
        //    object dto = new
        //    {
        //        SponsorName = name,
        //        SponsorDescription = desc,
        //        Type = type,
        //    };

        //    var response = await _client.CallWithToken("v1/sponsors", HttpMethod.Post, dto, _adminToken);
        //    var resJson = await Helper.DeserializeTo<ResponseDTO<Guid>>(response);

        //    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        //    _sponsorId = resJson.Message;
        //}


        //[Trait("Category", "Sponsor")]
        //[Fact, Priority(301)]
        //public async Task Sponsor_Update_Success()
        //{
        //    SponsorUpdateDTO dto = new SponsorUpdateDTO()
        //    {
        //        SponsorId = _sponsorId,
        //        SponsorName = "Test Sponsor 2",
        //        SponsorDescription = "Sponsor Desc 2",
        //        Type = SponsorType.Bussiness
        //    };


        //    var response = await _client.CallWithToken("v1/sponsor", HttpMethod.Patch, dto, _adminToken);
        //    var resJson = await Helper.DeserializeTo<ResponseDTO<bool>>(response);

        //    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        //}
        #endregion

        #region ProjectMember

        [Trait("Category", "Project_GetMemberInProject")]
        [Fact, Priority(200)]
        public async Task Project_GetMemberInProject_Failed_Null()
        {
            var response = await _client.CallWithToken($"v1/projects/null/members", HttpMethod.Get, null, _member1Token);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Project_GetMemberInProject")]
        [Fact, Priority(200)]
        public async Task Project_GetMemberInProject_Failed_NotFound()
        {
            var response = await _client.CallWithToken($"v1/projects/{Guid.NewGuid()}/members", HttpMethod.Get, null, _memberToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Trait("Category", "Project_GetMemberInProject")]
        [Fact, Priority(200)]
        public async Task Project_GetMemberInProject_Failed_NoPermission()
        {
            var response = await _client.CallWithToken($"v1/projects/{_projectId}/members", HttpMethod.Get, null, _member1Token);
            var resJson = await Helper.DeserializeTo<ResponseDTO<bool>>(response);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(resJson.ErrorName, ErrorNameValues.NoPermission);
        }

        [Trait("Category", "Project_GetMemberInProject")]
        [Fact, Priority(200)]
        public async Task Project_GetMemberInProject_Success()
        {
            var response = await _client.CallWithToken($"v1/projects/{_projectId}/members", HttpMethod.Get, null, _memberToken);
            var resJson = await Helper.DeserializeTo<ResponseDTO<List<ProjectMemberDetailedDTO>>>(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        public static Guid _memberReq1;
        public static Guid _memberReq2;
        public static Guid _projectMember1;

        [Trait("Category", "Project_RequestToJoin")]
        [Fact, Priority(201)]
        public async Task ProjectMember_RequestToJoin_Failed_AlreadyIn()
        {
            ProjectMemberRequestCreateDTO dto = new()
            {
                Major = "Tester",
                Note = "I want to test the world!"
            };

            var response = await _client.CallWithToken($"v1/projects/{_projectId}/members/requests", HttpMethod.Post, dto, _memberToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Project_RequestToJoin")]
        [Fact, Priority(201)]
        public async Task ProjectMember_RequestToJoin_Failed_Null()
        {
            ProjectMemberRequestCreateDTO dto = new()
            {
                Major = "Tester",
                Note = "I want to test the world!"
            };

            var response = await _client.CallWithToken($"v1/projects/{"Null"}/members/requests", HttpMethod.Post, dto, _memberToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Project_RequestToJoin")]
        [Fact, Priority(201)]
        public async Task ProjectMember_RequestToJoin_Failed_NotFound()
        {
            ProjectMemberRequestCreateDTO dto = new()
            {
                Major = "Tester",
                Note = "I want to test the world!"
            };

            var response = await _client.CallWithToken($"v1/projects/{Guid.Empty}/members/requests", HttpMethod.Post, dto, _memberToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Trait("Category", "Project_RequestToJoin")]
        [Theory, Priority(201)]
        [InlineData(null, null)]
        [InlineData("Test", null)]
        [InlineData(null, "Test")]
        public async Task ProjectMember_RequestToJoin_Failed(string? major, string? note)
        {
            ProjectMemberRequestCreateDTO dto = new()
            {
                Major = major,
                Note = note
            };

            var response = await _client.CallWithToken($"v1/projects/{_projectId}/members/requests", HttpMethod.Post, dto, _member1Token);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Project_RequestToJoin")]
        [Fact, Priority(201)]
        public async Task ProjectMember_RequestToJoin1_Success()
        {
            ProjectMemberRequestCreateDTO dto = new()
            {
                Major = "Tester",
                Note = "I want to test the world!"
            };

            var response = await _client.CallWithToken($"v1/projects/{_projectId}/members/requests", HttpMethod.Post, dto, _member1Token);
            var resJson = await Helper.DeserializeTo<ResponseDTO<Guid>>(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _memberReq1 = resJson.Message;
        }


        [Trait("Category", "Project_RequestToJoin")]
        [Fact, Priority(201)]
        public async Task ProjectMember_RequestToJoin2_Success()
        {
            ProjectMemberRequestCreateDTO dto = new()
            {
                Major = "Tester",
                Note = "I want to test the world!"
            };

            var response = await _client.CallWithToken($"v1/projects/{_projectId}/members/requests", HttpMethod.Post, dto, _member2Token);
            var resJson = await Helper.DeserializeTo<ResponseDTO<Guid>>(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _memberReq2 = resJson.Message;
        }

        [Trait("Category", "Project_GetAllRequestToJoin")]
        [Fact, Priority(202)]
        public async Task ProjectMember_GetAllRequestToJoin_NotFound()
        {
            var response = await _client.CallWithToken($"v1/projects/{Guid.Empty}/members/requests", HttpMethod.Get, new ProjectMemberRequestQueryParams(), _memberToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Trait("Category", "Project_GetAllRequestToJoin")]
        [Fact, Priority(202)]
        public async Task ProjectMember_GetAllRequestToJoin_Failed()
        {
            var response = await _client.CallWithToken($"v1/projects/{"null"}/members/requests", HttpMethod.Get, new ProjectMemberRequestQueryParams(), _memberToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Project_GetAllRequestToJoin")]
        [Fact, Priority(202)]
        public async Task ProjectMember_GetAllRequestToJoin_NoPermission()
        {
            var response = await _client.CallWithToken($"v1/projects/{_projectId}/members/requests", HttpMethod.Get, new ProjectMemberRequestQueryParams(), _member2Token);
            var resJson = await Helper.DeserializeTo<ResponseDTO<bool>>(response);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(ErrorNameValues.NoPermission, resJson.ErrorName);
        }

        [Trait("Category", "Project_GetAllRequestToJoin")]
        [Fact, Priority(202)]
        public async Task ProjectMember_GetAllRequestToJoin_Success()
        {
            var response = await _client.CallWithToken($"v1/projects/{_projectId}/members/requests", HttpMethod.Get, new ProjectMemberRequestQueryParams(), _memberToken);
            var resJson = await Helper.DeserializeTo<ResponseDTO<List<ProjectMemberRequestDTO>>>(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, resJson.Message.Count);
        }

        [Trait("Category", "Project_ReviewRequestToJoin")]
        [Fact, Priority(204)]
        public async Task ProjectMember_ApproveRequest_Failed_NotFound()
        {
            var dto = new
            {
                RequestId = Guid.Empty,
                Status = ProjectMemberRequestStatus.Accepted,
                Graduated = true,
                YearOfExp = 1,
                HaveEnghlishCert = true,
                LeadershipSkill = 6,
                CreativitySkill = 7,
                ProblemSolvingSkill = 8,
                PositiveAttitude = 6,
                TeamworkSkill = 4,
                CommnicationSkill = 5
            };

            var response = await _client.CallWithToken($"v1/projects/members/requests", HttpMethod.Put, dto, _member2Token);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Trait("Category", "Project_ReviewRequestToJoin")]
        [Fact, Priority(204)]
        public async Task ProjectMember_ApproveRequest_Failed_Null()
        {
            var dto = new
            {
                RequestId = "null",
                Status = ProjectMemberRequestStatus.Accepted,
                Graduated = true,
                YearOfExp = 1,
                HaveEnghlishCert = true,
                LeadershipSkill = 6,
                CreativitySkill = 7,
                ProblemSolvingSkill = 8,
                PositiveAttitude = 6,
                TeamworkSkill = 4,
                CommnicationSkill = 5
            };

            var response = await _client.CallWithToken($"v1/projects/members/requests", HttpMethod.Put, dto, _member2Token);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Project_ReviewRequestToJoin")]
        [Fact, Priority(205)]
        public async Task ProjectMember_ApproveRequest_NoPermission()
        {
            var dto = new ProjectMemberRequestReviewDTO
            {
                RequestId = _memberReq1,
                Status = ProjectMemberRequestStatus.Accepted,
                Graduated = true,
                YearOfExp = 1,
                HaveEnghlishCert = true,
                LeadershipSkill = 6,
                CreativitySkill = 7,
                ProblemSolvingSkill = 8,
                PositiveAttitude = 6,
                TeamworkSkill = 4,
                CommnicationSkill = 5
            };

            var response = await _client.CallWithToken($"v1/projects/members/requests", HttpMethod.Put, dto, _member2Token);
            var resJson = await Helper.DeserializeTo<ResponseDTO<bool>>(response);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(ErrorNameValues.NoPermission, resJson.ErrorName);
        }

        [Trait("Category", "Project_ReviewRequestToJoin")]
        [Fact, Priority(205)]
        public async Task ProjectMember_ApproveRequest_Success()
        {
            var dto = new ProjectMemberRequestReviewDTO
            {
                RequestId = _memberReq1,
                Status = ProjectMemberRequestStatus.Accepted,
                Graduated = true,
                YearOfExp = 1,
                HaveEnghlishCert = true,
                LeadershipSkill = 6,
                CreativitySkill = 7,
                ProblemSolvingSkill = 8,
                PositiveAttitude = 6,
                TeamworkSkill = 4,
                CommnicationSkill = 5
            };

            var response = await _client.CallWithToken($"v1/projects/members/requests", HttpMethod.Put, dto, _memberToken);
            var resJson = await Helper.DeserializeTo<ResponseDTO<bool>>(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(resJson.Message);

            // After Accept Check If Project Member Now Have 2 Member!

            var response2 = await _client.CallWithToken($"v1/projects/{_projectId}", HttpMethod.Get, null, _adminToken);
            var resJson2 = await Helper.DeserializeTo<ResponseDTO<ProjectDetailWithMemberLevelDTO>>(response2);

            Assert.Equal(2, resJson2.Message.Members.Count);
            Assert.Contains(resJson2.Message.Members, x => x.Member.EmailAddress == "member1@gmail.com");
        }


        [Trait("Category", "Project_ReviewRequestToJoin")]
        [Fact, Priority(206)]
        public async Task ProjectMember_RejectRequest_Success()
        {
            var dto = new ProjectMemberRequestReviewDTO
            {
                RequestId = _memberReq2,
                Status = ProjectMemberRequestStatus.Rejected,
                Graduated = true,
                YearOfExp = 1,
                HaveEnghlishCert = true,
                LeadershipSkill = 6,
                CreativitySkill = 7,
                ProblemSolvingSkill = 8,
                PositiveAttitude = 6,
                TeamworkSkill = 4,
                CommnicationSkill = 5
            };

            var response = await _client.CallWithToken($"v1/projects/members/requests", HttpMethod.Put, dto, _memberToken);
            var resJson = await Helper.DeserializeTo<ResponseDTO<bool>>(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(resJson.Message);

            // After Accept Check If Project Member Now Have 2 Member!

            var response2 = await _client.CallWithToken($"v1/projects/{_projectId}", HttpMethod.Get, null, _adminToken);
            var resJson2 = await Helper.DeserializeTo<ResponseDTO<ProjectDetailWithMemberLevelDTO>>(response2);

            Assert.Equal(2, resJson2.Message.Members.Count);
            Assert.Contains(resJson2.Message.Members, x => x.Member.EmailAddress == "member1@gmail.com");

            var member1InProject = resJson2.Message.Members.First(x => x.Member.EmailAddress == "member1@gmail.com");
            _projectMember1 = member1InProject.ProjectMemberId;
        }

        [Trait("Category", "Project_UpdateProjectMember")]
        [Fact, Priority(206)]
        public async Task ProjectMember_UpdateProjectMember_Failed_Null()
        {
            var dto = new
            {
                ProjectMemberId = "null",

                Graduated = true,
                YearOfExp = 2,
                HaveEnghlishCert = false,
                LeadershipSkill = 6,
                CreativitySkill = 7,
                ProblemSolvingSkill = 8,
                PositiveAttitude = 6,
                TeamworkSkill = 4,
                CommnicationSkill = 5
            };

            var response = await _client.CallWithToken($"v1/projects/members", HttpMethod.Put, dto, _memberToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }


        [Trait("Category", "Project_UpdateProjectMember")]
        [Fact, Priority(206)]
        public async Task ProjectMember_UpdateProjectMember_Failed_NotFound()
        {
            var dto = new
            {
                ProjectMemberId = Guid.NewGuid(),

                Graduated = true,
                YearOfExp = 2,
                HaveEnghlishCert = false,
                LeadershipSkill = 6,
                CreativitySkill = 7,
                ProblemSolvingSkill = 8,
                PositiveAttitude = 6,
                TeamworkSkill = 4,
                CommnicationSkill = 5
            };

            var response = await _client.CallWithToken($"v1/projects/members", HttpMethod.Put, dto, _memberToken);
            _outputHelper.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Trait("Category", "Project_UpdateProjectMember")]
        [Fact, Priority(206)]
        public async Task ProjectMember_UpdateProjectMember_NoPermission()
        {
            ProjectMemberUpdateDTO dto = new ProjectMemberUpdateDTO
            {
                ProjectMemberId = _projectMember1,

                Graduated = true,
                YearOfExp = 2,
                HaveEnghlishCert = false,
                LeadershipSkill = 6,
                CreativitySkill = 7,
                ProblemSolvingSkill = 8,
                PositiveAttitude = 6,
                TeamworkSkill = 4,
                CommnicationSkill = 5
            };

            var response = await _client.CallWithToken($"v1/projects/members", HttpMethod.Put, dto, _member2Token);
            var resJson = await Helper.DeserializeTo<ResponseDTO<bool>>(response);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(ErrorNameValues.NoPermission, resJson.ErrorName);
        }

        [Trait("Category", "Project_UpdateProjectMember")]
        [Fact, Priority(206)]
        public async Task ProjectMember_UpdateProjectMember_Success()
        {
            ProjectMemberUpdateDTO dto = new ProjectMemberUpdateDTO
            {
                ProjectMemberId = _projectMember1,

                Graduated = true,
                YearOfExp = 2,
                HaveEnghlishCert = false,
                LeadershipSkill = 6,
                CreativitySkill = 7,
                ProblemSolvingSkill = 8,
                PositiveAttitude = 6,
                TeamworkSkill = 4,
                CommnicationSkill = 5
            };

            var response = await _client.CallWithToken($"v1/projects/members", HttpMethod.Put, dto, _memberToken);
            var resJson = await Helper.DeserializeTo<ResponseDTO<bool>>(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(resJson.Message);
        }

        [Trait("Category", "Project_UpdateProjectMemberStatus")]
        [Fact, Priority(207)]
        public async Task ProjectMember_UpdateProjectMemberStatus_Failed_NotFound()
        {
            var dto = new
            {
                ProjectMemberId = Guid.NewGuid(),
                Status = ProjectMemberStatus.Inactive
            };

            var response = await _client.CallWithToken($"v1/projects/members/status", HttpMethod.Put, dto, _memberToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Trait("Category", "Project_UpdateProjectMemberStatus")]
        [Fact, Priority(207)]
        public async Task ProjectMember_UpdateProjectMemberStatus_Failed_Null()
        {
            var dto = new
            {
                ProjectMemberId = "null",
                Status = ProjectMemberStatus.Inactive
            };

            var response = await _client.CallWithToken($"v1/projects/members/status", HttpMethod.Put, dto, _memberToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Project_UpdateProjectMemberStatus")]
        [Fact, Priority(207)]
        public async Task ProjectMember_UpdateProjectMemberStatus_NoPermission()
        {
            ProjectMemberUpdateStatusDTO dto = new ProjectMemberUpdateStatusDTO()
            {
                ProjectMemberId = _projectMember1,
                Status = ProjectMemberStatus.Inactive
            };

            var response = await _client.CallWithToken($"v1/projects/members/status", HttpMethod.Put, dto, _member2Token);
            var resJson = await Helper.DeserializeTo<ResponseDTO<bool>>(response);
            _outputHelper.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(ErrorNameValues.NoPermission, resJson.ErrorName);
        }

        [Trait("Category", "Project_UpdateProjectMemberStatus")]
        [Fact, Priority(209)]
        public async Task ProjectMember_UpdateProjectMemberStatus_Success()
        {
            ProjectMemberUpdateStatusDTO dto = new ProjectMemberUpdateStatusDTO()
            {
                ProjectMemberId = _projectMember1,
                Status = ProjectMemberStatus.Inactive
            };

            var response = await _client.CallWithToken($"v1/projects/members/status", HttpMethod.Put, dto, _memberToken);
            var resJson = await Helper.DeserializeTo<ResponseDTO<bool>>(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(resJson.Message);

            ProjectMemberUpdateStatusDTO dto2 = new ProjectMemberUpdateStatusDTO()
            {
                ProjectMemberId = _projectMember1,
                Status = ProjectMemberStatus.Active
            };

            await _client.CallWithToken($"v1/projects/members/status", HttpMethod.Put, dto2, _memberToken);
        }

        #endregion

        #region ProjectSponsor

        [Trait("Category", "Project_AddSponsorToProject")]
        [Fact, Priority(300)]
        public async Task ProjectSponsor_AddSponsorToProject_NullProject_NullSponsor()
        {
            var dto = new
            {
                SponsorId = "null"
            };

            var response = await _client.CallWithToken($"v1/projects/null/sponsors", HttpMethod.Post, dto, _adminToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Project_AddSponsorToProject")]
        [Fact, Priority(300)]
        public async Task ProjectSponsor_AddSponsorToProject_InvalidProject_NullSponsor()
        {
            var dto = new
            {
                SponsorId = "null"
            };

            var response = await _client.CallWithToken($"v1/projects/{Guid.Empty}/sponsors", HttpMethod.Post, dto, _adminToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }


        [Trait("Category", "Project_AddSponsorToProject")]
        [Fact, Priority(300)]
        public async Task ProjectSponsor_AddSponsorToProject_NullProject_InvalidSponsor()
        {
            var dto = new
            {
                SponsorId = Guid.Empty
            };

            var response = await _client.CallWithToken($"v1/projects/null/sponsors", HttpMethod.Post, dto, _adminToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Project_AddSponsorToProject")]
        [Fact, Priority(300)]
        public async Task ProjectSponsor_AddSponsorToProject_InvalidProject_InvalidSponsor()
        {
            var dto = new
            {
                SponsorId = Guid.Empty
            };

            var response = await _client.CallWithToken($"v1/projects/{Guid.Empty}/sponsors", HttpMethod.Post, dto, _adminToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Trait("Category", "Project_AddSponsorToProject")]
        [Fact, Priority(300)]
        public async Task ProjectSponsor_AddSponsorToProject_Success()
        {
            var dto = new
            {
                SponsorId = _sponsorId
            };

            var response = await _client.CallWithToken($"v1/projects/{_projectId}/sponsors", HttpMethod.Post, dto, _adminToken);
            var resJson = await Helper.DeserializeTo<ResponseDTO<ProjectSponsorDTO>>(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _projectSponsorId = resJson.Message.ProjectSponsorId;

            var response2 = await _client.CallWithToken($"v1/projects/{_projectId}/sponsors", HttpMethod.Get, null, _adminToken);
            var resJson2 = await Helper.DeserializeTo<ResponseDTO<List<ProjectSponsorDTO>>>(response2);

            Assert.Equal(1, resJson2.Message.Count);
        }

        [Trait("Category", "Project_GetSponsorInProject")]
        [Fact, Priority(301)]
        public async Task ProjectSponsor_GetSponsorInPorject_Failed_Null()
        {
            var queryParams = new Dictionary<string, string?>()
            {

            };

            var response = await _client.CallWithToken($"v1/projects/{"null"}/sponsors", HttpMethod.Get, queryParams, _adminToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Project_GetSponsorInProject")]
        [Fact, Priority(301)]
        public async Task ProjectSponsor_GetSponsorInPorject_Failed_Invalid()
        {
            var queryParams = new Dictionary<string, string?>()
            {

            };

            var response = await _client.CallWithToken($"v1/projects/{Guid.Empty}/sponsors", HttpMethod.Get, queryParams, _adminToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Trait("Category", "Project_GetSponsorInProject")]
        [Fact, Priority(301)]
        public async Task ProjectSponsor_GetSponsorInProject_Failed_NoPermission()
        {
            var queryParams = new Dictionary<string, string?>()
            {

            };

            var response = await _client.CallWithToken($"v1/projects/{_projectId}/sponsors", HttpMethod.Get, queryParams, _member2Token);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Project_GetSponsorInProject")]
        [Fact, Priority(301)]
        public async Task ProjectSponsor_GetSponsorInProject_Failed()
        {
            var queryParams = new Dictionary<string, string?>()
            {
                {"Status", "AAA"},
                {"OrderBy", "AAA"}
            };

            var response = await _client.CallWithQuery($"v1/projects/{_projectId}/sponsors", HttpMethod.Get, queryParams, _adminToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Project_GetSponsorInProject")]
        [Theory, Priority(301)]
        [InlineData(null, null)]
        [InlineData("Available", "DateDesc")]
        public async Task ProjectSponsor_GetSponsorInProject_Success(string? status, string? order)
        {
            var queryParams = new Dictionary<string, string?>()
            {
                {"Status", status},
                {"OrderBy",order}
            };

            var response = await _client.CallWithQuery($"v1/projects/{_projectId}/sponsors", HttpMethod.Get, queryParams, _adminToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Trait("Category", "Project_UpdateSponsorInProject")]
        [Fact, Priority(302)]
        public async Task ProjectSponsor_UpdateProjectSponsor_Failed_Null()
        {
            var dto = new
            {
                ProjectSponsorId = "null",
                Status = "Available"
            };

            var response = await _client.CallWithToken($"v1/projects/sponsors/status", HttpMethod.Put, dto, _adminToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Project_UpdateSponsorInProject")]
        [Fact, Priority(302)]
        public async Task ProjectSponsor_UpdateProjectSponsor_Failed_NotFound()
        {
            var dto = new
            {
                ProjectSponsorId = Guid.Empty,
                Status = "Available"
            };

            var response = await _client.CallWithToken($"v1/projects/sponsors/status", HttpMethod.Put, dto, _adminToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Trait("Category", "Project_UpdateSponsorInProject")]
        [Theory, Priority(302)]
        [InlineData("null")]
        [InlineData("Invalid")]
        public async Task ProjectSponsor_UpdateProjectSponsor_Failed(string? status)
        {
            var dto = new
            {
                ProjectSponsorId = _projectSponsorId,
                Status = status
            };

            var response = await _client.CallWithToken($"v1/projects/sponsors/status", HttpMethod.Put, dto, _adminToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Project_UpdateSponsorInProject")]
        [Fact, Priority(302)]
        public async Task ProjectSponsor_UpdateProjectSponsor_Success()
        {
            var dto = new
            {
                ProjectSponsorId = _projectSponsorId,
                Status = "Available"
            };

            var response = await _client.CallWithToken($"v1/projects/sponsors/status", HttpMethod.Put, dto, _adminToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        #endregion

        #region Deposit
        [Trait("Category", "Project_DepositFromSystemToProject")]
        [Fact, Priority(302)]
        public async Task Project_DepositFromSystemToProject_Failed_Null()
        {
            var dto = new
            {
                Amount = 100
            };

            var response = await _client.CallWithToken($"v1/projects/null/wallet", HttpMethod.Post, dto, _adminToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Project_DepositFromSystemToProject")]
        [Fact, Priority(302)]
        public async Task Project_DepositFromSystemToProject_Failed_NotFound()
        {
            var dto = new
            {
                Amount = 100
            };

            var response = await _client.CallWithToken($"v1/projects/{Guid.Empty}/wallet", HttpMethod.Post, dto, _adminToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Trait("Category", "Project_DepositFromSystemToProject")]
        [Theory, Priority(302)]
        [InlineData(-1)]
        public async Task Project_DepositFromSystemToProject_Failed(int? amount)
        {
            var dto = new
            {
                Amount = amount
            };

            var response = await _client.CallWithToken($"v1/projects/{_projectId}/wallet", HttpMethod.Post, dto, _adminToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Project_DepositFromSystemToProject")]
        [Theory, Priority(302)]
        [InlineData(1)]
        [InlineData(100)]
        public async Task Project_DepositFromSystemToProject_Success(int? amount)
        {
            var dto = new
            {
                ProjectId = _projectId,
                Amount = amount
            };

            var response = await _client.CallWithToken($"v1/projects/{_projectId}/wallet", HttpMethod.Post, dto, _adminToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Trait("Category", "Project_DepositFromSponsorToProject")]
        [Fact, Priority(302)]
        public async Task Project_DepositFromSponsorToProject_Failed_Null()
        {
            var dto = new
            {
                ProjectSponsorId = "null",
                Amount = 100
            };

            var response = await _client.CallWithToken($"v1/projects/sponsors/deposit", HttpMethod.Post, dto, _adminToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Project_DepositFromSponsorToProject")]
        [Fact, Priority(302)]
        public async Task Project_DepositFromSponsorToProject_Failed_NotFound()
        {
            var dto = new
            {
                ProjectSponsorId = Guid.Empty,
                Amount = 100
            };

            var response = await _client.CallWithToken($"v1/projects/sponsors/deposit", HttpMethod.Post, dto, _adminToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Project_DepositFromSponsorToProject")]
        [Theory, Priority(302)]
        [InlineData(-1)]
        public async Task Project_DepositFromSponsorToProject_Failed(int? amount)
        {
            var dto = new
            {
                ProjectSponsorId = _projectSponsorId,
                Amount = amount
            };

            var response = await _client.CallWithToken($"v1/projects/sponsors/deposit", HttpMethod.Post, dto, _adminToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Project_DepositFromSponsorToProject")]
        [Theory, Priority(302)]
        [InlineData(1)]
        [InlineData(100)]
        public async Task Project_DepositFromSponsorToProject_Success(int? amount)
        {
            var dto = new
            {
                ProjectSponsorId = _projectSponsorId,
                Amount = amount
            };

            var response = await _client.CallWithToken($"v1/projects/sponsors/deposit", HttpMethod.Post, dto, _adminToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        #endregion

        [Trait("Category", "Member_ViewPersonalWallet")]
        [Fact, Priority(400)]
        public async Task Member_ViewPersonalWallets_Success()
        {

            var response = await _client.CallWithToken($"v1/members/me/wallets", HttpMethod.Get, null, _member1Token);
            //var resJson = await Helper.DeserializeTo<ResponseDTO<WalletsInfoDTO>>(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            //Assert.True(resJson.Message.Wallets.Count >= 2);
        }

        [Trait("Category", "Member_ViewPersonalTransaction")]
        [Fact, Priority(401)]
        public async Task Member_ViewPersonalTransactions_Success()
        {

            var response = await _client.CallWithToken($"v1/members/me/transactions", HttpMethod.Get, null, _member1Token);
            var resJson = await Helper.DeserializeTo<ResponseDTO<List<TransactionDTO>>>(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(resJson.Message.Count >= 0);
        }

        [Trait("Category", "Member_ViewSelfProjects")]
        [Theory, Priority(402)]
        [InlineData(null, null)]
        [InlineData(null, "Created")]
        [InlineData("aaa", "Created")]
        public async Task Member_ViewPersonalProjects_Success(string? name, string? status)
        {
            Dictionary<string, string> q = new()
            {
                {"ProjectName", name},
                {"Status", status}
            };

            var response = await _client.CallWithQuery($"v1/members/me/projects", HttpMethod.Get, q, _memberToken);
            var resJson = await Helper.DeserializeTo<ResponseDTO<List<ProjectDetailWithMemberLevelDTO>>>(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(resJson.Message.Count >= 0);
        }

        [Trait("Category", "Member_ViewSelfProjects")]
        [Theory, Priority(402)]
        [InlineData("aaa", "Invalid")]
        public async Task Member_ViewPersonalProjects_Failed(string? name, string? status)
        {
            Dictionary<string, string> q = new()
            {
                { "ProjectName", name },
                { "Status", status }
            };

            var response = await _client.CallWithToken($"v1/members/{_memberId}/projects", HttpMethod.Get, q, _adminToken);
            var resJson = await Helper.DeserializeTo<ResponseDTO<List<ProjectDetailWithMemberLevelDTO>>>(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(resJson.Message.Count >= 0);
        }


        [Trait("Category", "Member_GetMemberProject")]
        [Theory, Priority(403)]
        [InlineData(null, null)]
        [InlineData(null, "Created")]
        [InlineData("aaa", "Created")]
        public async Task Member_GetMemberProject_Success(string? name, string? status)
        {
            Dictionary<string, string> q = new()
            {
                {"ProjectName", name},
                {"Status", status}
            };

            var response = await _client.CallWithQuery($"v1/members/{_memberId}/projects", HttpMethod.Get, q, _adminToken);
            var resJson = await Helper.DeserializeTo<ResponseDTO<List<ProjectDetailWithMemberLevelDTO>>>(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(resJson.Message.Count >= 0);
        }

        [Trait("Category", "Member_GetMemberProject")]
        [Theory, Priority(403)]
        [InlineData("aaa", "Invalid")]
        public async Task Member_GetMemberProject_Failed(string? name, string? status)
        {
            Dictionary<string, string> q = new()
            {
                { "ProjectName", name },
                { "Status", status }
            };

            var response = await _client.CallWithToken($"v1/members/me/projects", HttpMethod.Get, q, _memberToken);
            var resJson = await Helper.DeserializeTo<ResponseDTO<List<ProjectDetailWithMemberLevelDTO>>>(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(resJson.Message.Count >= 0);
        }

        [Trait("Category", "Project_GetWallets")]
        [Fact, Priority(404)]
        public async Task Project_GetWallets_Failed_Null()
        {
            var response = await _client.CallWithToken($"v1/projects/null/wallet", HttpMethod.Get, null, _memberToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Project_GetWallets")]
        [Fact, Priority(404)]
        public async Task Project_GetWallets_Failed_NotFound()
        {
            var response = await _client.CallWithToken($"v1/projects/{Guid.Empty}/wallet", HttpMethod.Get, null, _memberToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Trait("Category", "Project_GetWallets")]
        [Fact, Priority(404)]
        public async Task Project_GetWallets_Failed_NoPermission()
        {
            var response = await _client.CallWithToken($"v1/projects/{_projectId}/wallet", HttpMethod.Get, null, _member2Token);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Project_GetWallets")]
        [Fact, Priority(404)]
        public async Task Project_GetWallets_Failed_Success()
        {
            var response = await _client.CallWithToken($"v1/projects/{_projectId}/wallet", HttpMethod.Get, null, _memberToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Trait("Category", "Project_GetTransactions")]
        [Fact, Priority(405)]
        public async Task Project_GetTransactions_Failed_Null()
        {
            var response = await _client.CallWithToken($"v1/projects/null/transactions", HttpMethod.Get, null, _member2Token);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Project_GetTransactions")]
        [Fact, Priority(405)]
        public async Task Project_GetTransactions_Failed_NotFound()
        {
            var response = await _client.CallWithToken($"v1/projects/{Guid.Empty}/transactions", HttpMethod.Get, null, _member2Token);
            _outputHelper.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Trait("Category", "Project_GetTransactions")]
        [Fact, Priority(405)]
        public async Task Project_GetTransactions_Failed_NoPermission()
        {
            var response = await _client.CallWithToken($"v1/projects/{_projectId}/transactions", HttpMethod.Get, null, _member2Token);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Project_GetTransactions")]
        [Fact, Priority(405)]
        public async Task Project_GetTransactions_Success()
        {
            var response = await _client.CallWithToken($"v1/projects/{_projectId}/transactions", HttpMethod.Get, null, _memberToken);
            _outputHelper.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Trait("Category", "ProjectSponsors_GetTransactions")]
        [Fact, Priority(406)]
        public async Task ProjectSponsors_GetTransactions_Failed_Null()
        {
            var response = await _client.CallWithToken($"v1/projects/sponsors/null/transactions", HttpMethod.Get, null, _adminToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "ProjectSponsors_GetTransactions")]
        [Fact, Priority(406)]
        public async Task ProjectSponsors_GetTransactions_Failed_NotFound()
        {
            var response = await _client.CallWithToken($"v1/projects/sponsors/{Guid.Empty}/transactions", HttpMethod.Get, null, _adminToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "ProjectSponsors_GetTransactions")]
        [Fact, Priority(406)]
        public async Task ProjectSponsors_GetTransactions_Failed_NoPermission()
        {
            var response = await _client.CallWithToken($"v1/projects/sponsors/{_projectSponsorId}/transactions", HttpMethod.Get, null, _member2Token);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Trait("Category", "ProjectSponsors_GetTransactions")]
        [Fact, Priority(406)]
        public async Task ProjectSponsors_GetTransactions_Success()
        {
            var response = await _client.CallWithToken($"v1/projects/sponsors/{_projectSponsorId}/transactions", HttpMethod.Get, null, _adminToken);
            _outputHelper.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        #region SalaryCycle


        [Trait("Category", "SalaryCycle_Create")]
        [Fact, Priority(500)]
        public async Task SalaryCycle_Create_Success()
        {
            SalaryCycleCreateDTO dto = new()
            {
                StartedAt = DateTimeHelper.Now().AddDays(1),
                Name = "Test Cycle"
            };

            var response = await _client.CallWithToken("v1/salarycycle", HttpMethod.Post, dto, _adminToken);
            var resJson = await Helper.DeserializeTo<ResponseDTO<SalaryCycleDTO>>(response);
            _outputHelper.WriteLine(await response.Content.ReadAsStringAsync());

            _salaryCycleId = resJson.Message.SalaryCycleId;
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private DateTime current = DateTimeHelper.Now();

        [Trait("Category", "SalaryCycle_Create")]
        [Theory, Priority(500)]
        [InlineData("null", null)]
        [InlineData("1023-05-01T14:43:36.338Z", null)]
        [InlineData("4023-05-01T14:43:36.338Z", null)]
        public async Task SalaryCycle_Create_Failed(string? time, string? name)
        {
            object dto = new
            {
                StartedAt = time,
                Name = name
            };

            var response = await _client.CallWithToken("v1/salarycycle", HttpMethod.Post, dto, _adminToken);
            var resJson = await Helper.DeserializeTo<ResponseDTO<SalaryCycleDTO>>(response);
            _outputHelper.WriteLine(await response.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "SalaryCycle_Update")]
        [Fact, Priority(501)]
        public async Task SalaryCycle_Update_Failed_Null()
        {
            var dto = new
            {
                SalaryCycleId = "null",
                Status = "Locked"
            };

            var response = await _client.CallWithToken($"v1/salarycycle/status", HttpMethod.Put, dto, _adminToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }


        [Trait("Category", "SalaryCycle_Update")]
        [Fact, Priority(501)]
        public async Task SalaryCycle_Update_Failed_NotFound()
        {
            var dto = new
            {
                SalaryCycleId = Guid.Empty,
                Status = "Locked"
            };

            var response = await _client.CallWithToken($"v1/salarycycle/status", HttpMethod.Put, dto, _adminToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Trait("Category", "SalaryCycle_Update")]
        [Theory, Priority(501)]
        [InlineData("null")]
        [InlineData("InvalidState")]
        public async Task SalaryCycle_Update_Failed(string? status)
        {
            var dto = new
            {
                SalaryCycleId = _salaryCycleId,
                Status = status
            };

            var response = await _client.CallWithToken($"v1/salarycycle/status", HttpMethod.Put, dto, _adminToken);
            //var resJson = await Helper.DeserializeTo<ResponseDTO<bool>>(response);
            _outputHelper.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "SalaryCycle_Update")]
        [Fact, Priority(501)]
        public async Task SalaryCycle_Update_Success()
        {
            SalaryCycleUpdateDTO dto = new()
            {
                SalaryCycleId = _salaryCycleId,
                Status = SalaryCycleStatus.Locked
            };

            var response = await _client.CallWithToken($"v1/salarycycle/status", HttpMethod.Put, dto, _adminToken);
            //var resJson = await Helper.DeserializeTo<ResponseDTO<bool>>(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Thread.Sleep(1000);
            SalaryCycleUpdateDTO dto2 = new()
            {
                SalaryCycleId = _salaryCycleId,
                Status = SalaryCycleStatus.Ongoing
            };

            var response2 = await _client.CallWithToken($"v1/salarycycle/status", HttpMethod.Put, dto2, _adminToken);
            _outputHelper.WriteLine(await response2.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
            Thread.Sleep(1000);
        }

        [Trait("Category", "SalaryCycle_GetAll")]
        [Fact, Priority(502)]
        public async Task SalaryCycle_GetAll()
        {
            var response = await _client.CallWithToken($"v1/salarycycle", HttpMethod.Get, null, _adminToken);
            var resJson = await Helper.DeserializeTo<ResponseDTO<List<SalaryCycleDTO>>>(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Single(resJson.Message);
        }

        [Trait("Category", "SalaryCycle_GetByID")]
        [Fact, Priority(502)]
        public async Task SalaryCycle_GetByID_Failed_Null()
        {
            var response = await _client.CallWithToken($"v1/salarycycle/null", HttpMethod.Get, null, _adminToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "SalaryCycle_GetByID")]
        [Fact, Priority(502)]
        public async Task SalaryCycle_GetByID_Failed_NotFound()
        {
            var response = await _client.CallWithToken($"v1/salarycycle/{Guid.Empty}", HttpMethod.Get, null, _adminToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Trait("Category", "SalaryCycle_GetByID")]
        [Fact, Priority(502)]
        public async Task SalaryCycle_GetByID_Success()
        {
            var response = await _client.CallWithToken($"v1/salarycycle/{_salaryCycleId}", HttpMethod.Get, null, _adminToken);
            //var resJson = await Helper.DeserializeTo<ResponseDTO<SalaryCycleWithPayslipDTO>>(response);

            //var paySlip = resJson.Message.Payslips;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        #endregion

        #region ProjectReport

        [Trait("Category", "ProjectReport_Create")]
        [Fact, Priority(503)]
        public async Task ProjectReport_Create_Failed_Null()
        {
            var dto = new ProjectReportCreateDTO()
            {
                SalaryCycleId = Guid.Empty,
            };

            var response = await _client.CallWithToken($"v1/projects/null/reports", HttpMethod.Post, dto,
                _memberToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "ProjectReport_Create")]
        [Fact, Priority(503)]
        public async Task ProjectReport_Create_Failed_NotFound()
        {
            var dto = new ProjectReportCreateDTO()
            {
                SalaryCycleId = Guid.Empty,
            };

            var response = await _client.CallWithToken($"v1/projects/{Guid.Empty}/reports", HttpMethod.Post, dto,
                _memberToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "ProjectReport_Create")]
        [Fact, Priority(503)]
        public async Task ProjectReport_Failed_SalaryCycleNotFound()
        {
            var dto = new ProjectReportCreateDTO()
            {
                SalaryCycleId = Guid.Empty,
            };

            var response = await _client.CallWithToken($"v1/projects/{_projectId}/reports", HttpMethod.Post, dto,
                _memberToken);
            var resJson = await Helper.DeserializeTo<ResponseDTO<bool>>(response);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorNameValues.SalaryCycleNotFound, resJson.ErrorName);
        }

        [Trait("Category", "ProjectReport_Create")]
        [Fact, Priority(503)]
        public async Task ProjectReport_Failed_WrongReport()
        {
            var dto = new ProjectReportCreateDTO()
            {
                SalaryCycleId = _salaryCycleId,
            };

            var response = await _client.CallWithToken($"v1/projects/{_projectId}/reports", HttpMethod.Post, dto,
                _memberToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "ProjectReport_Create")]
        [Fact, Priority(503)]
        public async Task ProjectReport_Failed_WrongMember()
        {
            var dto = new ProjectReportCreateDTO()
            {
                SalaryCycleId = _salaryCycleId,
            };

            var response = await _client.CallWithToken($"v1/projects/{_projectId}/reports", HttpMethod.Post, dto,
                _memberToken);
            var resJson = await Helper.DeserializeTo<ResponseDTO<bool>>(response);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorNameValues.MemberNotFound, resJson.ErrorName);
        }

        private static Guid _projectReportId;

        [Trait("Category", "ProjectReport_Create")]
        [Fact, Priority(504)]
        public async Task ProjectReport_Create_Success()
        {
            var dto = new ProjectReportCreateDTO()
            {
                SalaryCycleId = _salaryCycleId,
            };

            var response = await _client.CallWithToken($"v1/projects/{_projectId}/reports", HttpMethod.Post, dto,
                _memberToken);
            var resJson = await Helper.DeserializeTo<ResponseDTO<Guid>>(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _projectReportId = resJson.Message;
        }

        [Trait("Category", "ProjectReport_Update")]
        [Fact, Priority(505)]
        public async Task ProjectReport_Update_Failed_Null()
        {
            var dto = new
            {
                ProjectReportId = "null",
                MemberTasks = new List<ProjectReportDTO_Task>
                {
                    new ()
                    {
                        TaskName = "Task Name",
                        MemberEmail = "member@gmail.com",
                        TaskDescription = "AAA",
                        TaskPoint = 1,
                        TaskHour = 1,
                        TaskRealHour = 2
                    }
                }
            };

            var response = await _client.CallWithToken($"v1/projects/reports", HttpMethod.Put, dto,
                _memberToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "ProjectReport_Update")]
        [Fact, Priority(505)]
        public async Task ProjectReport_Update_Failed_NotFound()
        {
            var dto = new
            {
                ProjectReportId = "null",
                MemberTasks = new List<ProjectReportDTO_Task>
                {
                    new ()
                    {
                        TaskName = "Task Name",
                        MemberEmail = "member@gmail.com",
                        TaskDescription = "AAA",
                        TaskPoint = 1,
                        TaskHour = 1,
                        TaskRealHour = 2
                    }
                }
            };

            var response = await _client.CallWithToken($"v1/projects/reports", HttpMethod.Put, dto,
                _memberToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "ProjectReport_Update")]
        [Fact, Priority(505)]
        public async Task ProjectReport_Update_Failed_InvalidMember()
        {
            var dto = new
            {
                ProjectReportId = "null",
                MemberTasks = new List<ProjectReportDTO_Task>
                {
                    new ()
                    {
                        TaskName = "Task Name",
                        MemberEmail = "member123@gmail.com",
                        TaskDescription = "AAA",
                        TaskPoint = 1,
                        TaskHour = 1,
                        TaskRealHour = 2
                    }
                }
            };

            var response = await _client.CallWithToken($"v1/projects/reports", HttpMethod.Put, dto,
                _memberToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }


        [Trait("Category", "ProjectReport_Update")]
        [Fact, Priority(505)]
        public async Task ProjectReport_Update_Failed_InvalidReport()
        {
            var dto = new
            {
                ProjectReportId = "null",
                MemberTasks = new List<ProjectReportDTO_Task>
                {
                    new ()
                    {
                        TaskName = "Task Name",
                        MemberEmail = "member@gmail.com",
                        TaskDescription = "AAA",
                        TaskPoint = 1,
                        TaskHour = 1,
                        TaskRealHour = 0
                    }
                }
            };

            var response = await _client.CallWithToken($"v1/projects/reports", HttpMethod.Put, dto,
                _memberToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }


        [Trait("Category", "ProjectReport_Update")]
        [Fact, Priority(505)]
        public async Task ProjectReport_Update_Success()
        {
            var dto = new ProjectReportUpdateDTO()
            {
                ProjectReportId = _projectReportId,
                MemberTasks = new List<ProjectReportDTO_Task>
                {
                    new ()
                    {
                        TaskName = "Task Name",
                        MemberEmail = "member@gmail.com",
                        TaskDescription = "AAA",
                        TaskPoint = 1,
                        TaskHour = 1,
                        TaskRealHour = 2
                    }
                }
            };

            var response = await _client.CallWithToken($"v1/projects/reports", HttpMethod.Put, dto,
                _memberToken);
            _outputHelper.WriteLine(await response.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Trait("Category", "ProjectReport_Review")]
        [Fact, Priority(506)]
        public async Task ProjectReport_Review_Failed_Null()
        {
            var dto = new
            {
                ReportId = "null",
                Status = ProjectReportStatus.Accepted
            };

            var response = await _client.CallWithToken($"v1/projects/reports/status", HttpMethod.Put, dto,
                _memberToken);
            _outputHelper.WriteLine(await response.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }


        [Trait("Category", "ProjectReport_Review")]
        [Fact, Priority(506)]
        public async Task ProjectReport_Review_Failed_NotFound()
        {
            var dto = new
            {
                ReportId = Guid.Empty,
                Status = ProjectReportStatus.Accepted
            };

            var response = await _client.CallWithToken($"v1/projects/reports/status", HttpMethod.Put, dto,
                _memberToken);
            _outputHelper.WriteLine(await response.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Trait("Category", "ProjectReport_Review")]
        [Fact, Priority(506)]
        public async Task ProjectReport_Review_Failed_NullStatus()
        {
            var dto = new
            {
                ReportId = _projectReportId,
                Status = "Null"
            };

            var response = await _client.CallWithToken($"v1/projects/reports/status", HttpMethod.Put, dto,
                _memberToken);
            _outputHelper.WriteLine(await response.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }


        [Trait("Category", "ProjectReport_Review")]
        [Fact, Priority(506)]
        public async Task ProjectReport_Review_Failed_InvalidStatus()
        {
            var dto = new
            {
                ReportId = _projectReportId,
                Status = "InvalidStatus"
            };

            var response = await _client.CallWithToken($"v1/projects/reports/status", HttpMethod.Put, dto,
                _memberToken);
            _outputHelper.WriteLine(await response.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }



        [Trait("Category", "ProjectReport_Review")]
        [Fact, Priority(506)]
        public async Task ProjectReport_Review_Success()
        {
            var dto = new DTOs.ProjectReport.ProjectReportStatusUpdateDTO
            {
                ReportId = _projectReportId,
                Status = ProjectReportStatus.Created
            };

            var response = await _client.CallWithToken($"v1/projects/reports/status", HttpMethod.Put, dto, _memberToken);

            var dto2 = new DTOs.ProjectReport.ProjectReportStatusUpdateDTO
            {
                ReportId = _projectReportId,
                Status = ProjectReportStatus.Accepted
            };

            var response2 = await _client.CallWithToken($"v1/projects/reports/status", HttpMethod.Put, dto2, _adminToken);

            _outputHelper.WriteLine(await response.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        }

        [Trait("Category", "ProjectReport_GetAll")]
        [Theory, Priority(507)]
        [InlineData(null, "AAA")]
        [InlineData("AAA", null)]
        public async Task ProjectReport_GetAll_Failed(string? status, string? orderBy)
        {
            var q = new Dictionary<string, string?>()
            {
                {"Status", status},
                {"OrderBy", orderBy}
            };

            var response = await _client.CallWithQuery($"v1/projects/reports", HttpMethod.Get, q, _adminToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "ProjectReport_GetAll")]
        [Theory, Priority(507)]
        [InlineData(null, null)]
        [InlineData("Accepted", "CreatedAtDesc")]
        public async Task ProjectReport_GetAll_Success(string? status, string? orderBy)
        {
            var q = new Dictionary<string, string?>()
            {
                {"Status", status},
                {"OrderBy", orderBy}
            };

            var response = await _client.CallWithQuery($"v1/projects/reports", HttpMethod.Get, q, _adminToken);
            var resJson = await Helper.DeserializeTo<ResponseDTO<List<ProjectReportWithProjectAndSalaryCycleDTO>>>(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Trait("Category", "ProjectReport_GetAllFromProject")]
        [Fact, Priority(507)]
        public async Task ProjectReport_GetAllFromProject_Failed_ProjectNotFound()
        {
            var response = await _client.CallWithQuery($"v1/projects/{Guid.Empty}/reports", HttpMethod.Get, null, _memberToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Trait("Category", "ProjectReport_GetAllFromProject")]
        [Fact, Priority(507)]
        public async Task ProjectReport_GetAllFromProject_Failed_Null()
        {
            var response = await _client.CallWithQuery($"v1/projects/null/reports", HttpMethod.Get, null, _memberToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "ProjectReport_GetAllFromProject")]
        [Theory, Priority(507)]
        [InlineData(null, "AAA")]
        [InlineData("AAA", null)]
        public async Task ProjectReport_GetAllFromProject_Failed(string? status, string? orderBy)
        {
            var q = new Dictionary<string, string?>()
            {
                {"Status", status},
                {"OrderBy", orderBy}
            };

            var response = await _client.CallWithQuery($"v1/projects/{_projectId}/reports", HttpMethod.Get, q, _memberToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "ProjectReport_GetAllFromProject")]
        [Theory, Priority(507)]
        [InlineData(null, null)]
        [InlineData("Accepted", "CreatedAtDesc")]
        public async Task ProjectReport_GetAllFromProject_Success(string? status, string? orderBy)
        {
            var q = new Dictionary<string, string?>()
            {
                {"Status", status},
                {"OrderBy", orderBy}
            };

            var response = await _client.CallWithQuery($"v1/projects/{_projectId}/reports", HttpMethod.Get, q, _memberToken);
            _outputHelper.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Trait("Category", "ProjectReport_GetByID")]
        [Fact, Priority(508)]
        public async Task ProjectReport_GetByID_Failed_Null()
        {
            var response = await _client.CallWithToken($"v1/projects/reports/null", HttpMethod.Get, null, _member2Token);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "ProjectReport_GetByID")]
        [Fact, Priority(508)]
        public async Task ProjectReport_GetByID_Failed_NotFound()
        {
            var response = await _client.CallWithToken($"v1/projects/reports/{Guid.Empty}", HttpMethod.Get, null, _member2Token);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }


        [Trait("Category", "ProjectReport_GetByID")]
        [Fact, Priority(508)]
        public async Task ProjectReport_GetByID_Failed_NoPermission()
        {
            var response = await _client.CallWithToken($"v1/projects/reports/{_projectReportId}", HttpMethod.Get, null, _member2Token);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "ProjectReport_GetByID")]
        [Fact, Priority(508)]
        public async Task ProjectReport_GetByID_Success()
        {
            var response = await _client.CallWithToken($"v1/projects/reports/{_projectReportId}", HttpMethod.Get, null, _memberToken);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Trait("Category", "Voucher_Create")]
        [Theory, Priority(600)]
        [InlineData(null, null, "-1", "-1")]
        [InlineData("aaa", null, "-1", "-1")]
        [InlineData("aaa", "aaa", "-1", "-1")]
        [InlineData("aaa", "aaa", "1", "-1")]
        public async Task Voucher_Create_Failed(string? name, string? description, string? cost, string? amount)
        {
            var dto = new
            {
                VoucherName = name,
                VoucherDescription = description,
                VoucherCost = cost,
                VoucherAmount = amount,
                SupplierId = _supplierId,
                VoucherType = VoucherType.Others
            };

            var response = await _client.CallWithToken($"v1/vouchers", HttpMethod.Post, dto, _adminToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Voucher_Create")]
        [Fact, Priority(600)]
        public async Task Voucher_Create_Success()
        {
            var dto = new VoucherCreateDTO()
            {
                VoucherName = "VoucherName",
                VoucherDescription = "Description",
                VoucherCost = 1,
                VoucherAmount = 100,
                SupplierId = _supplierId,
                VoucherType = VoucherType.Others
            };

            var response = await _client.CallWithToken($"v1/vouchers", HttpMethod.Post, dto, _adminToken);
            var resJson = await Helper.DeserializeTo<ResponseDTO<VoucherDTO>>(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _voucherId = resJson.Message.VoucherId;
        }

        [Trait("Category", "Voucher_Create")]
        [Fact, Priority(600)]
        public async Task Voucher_Create_Success2()
        {
            var dto = new VoucherCreateDTO()
            {
                VoucherName = "VoucherName",
                VoucherDescription = "Description",
                VoucherCost = 100,
                VoucherAmount = 100,
                SupplierId = _supplierId,
                VoucherType = VoucherType.Others
            };

            var response = await _client.CallWithToken($"v1/vouchers", HttpMethod.Post, dto, _adminToken);
            var resJson = await Helper.DeserializeTo<ResponseDTO<VoucherDTO>>(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _voucherId = resJson.Message.VoucherId;
        }

        [Trait("Category", "Voucher_Update")]
        [Fact, Priority(601)]
        public async Task Voucher_Update_Failed_Null()
        {
            var dto = new
            {
                VoucherId = "null",
                VoucherName = "VoucherName",
                VoucherDescription = "Description",
                VoucherCost = 100,
                VoucherAmount = 100,
            };

            var response = await _client.CallWithToken($"v1/vouchers", HttpMethod.Patch, dto, _adminToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Voucher_Update")]
        [Fact, Priority(601)]
        public async Task Voucher_Update_Failed_NotFound()
        {
            var dto = new
            {
                VoucherId = Guid.Empty,
                VoucherName = "VoucherName",
                VoucherDescription = "Description",
                VoucherCost = 100,
                VoucherAmount = 100,
            };

            var response = await _client.CallWithToken($"v1/vouchers", HttpMethod.Patch, dto, _adminToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Trait("Category", "Voucher_Update")]
        [Theory, Priority(601)]
        [InlineData("", "", "-1", null)]
        [InlineData("", "", "-1", "-1")]
        public async Task Voucher_Update_Failed(string? name, string? description, string? cost, string? amount)
        {
            var dto = new
            {
                VoucherId = _voucherId,
                VoucherName = name,
                VoucherDescription = description,
                VoucherCost = cost,
                VoucherAmount = amount,
            };

            var response = await _client.CallWithToken($"v1/vouchers", HttpMethod.Patch, dto, _adminToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Voucher_Update")]
        [Theory, Priority(601)]
        [InlineData("", "", null, null)]
        [InlineData("NewVoucherName", "", null, null)]
        [InlineData("NewVoucherName", "NewDesc", null, null)]
        [InlineData("NewVoucherName", "NewDesc", "1", "1")]
        [InlineData("NewVoucherName", "NewDesc", "100", "100")]
        public async Task Voucher_Update_Success(string? name, string? description, string? cost, string? amount)
        {
            var dto = new
            {
                VoucherId = _voucherId,
                VoucherName = name,
                VoucherDescription = description,
                VoucherCost = cost,
                VoucherAmount = amount,
            };

            var response = await _client.CallWithToken($"v1/vouchers", HttpMethod.Patch, dto, _adminToken);
            _outputHelper.WriteLine(await response.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Trait("Category", "Voucher_GetAll")]
        [Fact, Priority(602)]
        public async Task Voucher_GetAll_Success()
        {
            var response = await _client.CallWithToken($"v1/vouchers", HttpMethod.Get, null, _adminToken);
            _outputHelper.WriteLine(await response.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }


        [Trait("Category", "Member_SendPoint")]
        [Fact, Priority(700)]
        public async Task Member_SendPoint_Null()
        {
            var dto = new
            {
                ToMemberId = "null",
                Amount = 1
            };

            var response = await _client.CallWithToken($"v1/members/me/wallets/transactions", HttpMethod.Post, dto, _memberToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Member_SendPoint")]
        [Fact, Priority(700)]
        public async Task Member_SendPoint_NotFound()
        {
            var dto = new
            {
                ToMemberId = Guid.Empty,
                Amount = 1
            };

            var response = await _client.CallWithToken($"v1/members/me/wallets/transactions", HttpMethod.Post, dto, _memberToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Trait("Category", "Member_SendPoint")]
        [Fact, Priority(700)]
        public async Task Member_SendPoint_BadRequest()
        {
            var dto = new
            {
                ToMemberId = _member1Id,
                Amount = -1
            };

            var response = await _client.CallWithToken($"v1/members/me/wallets/transactions", HttpMethod.Post, dto, _memberToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Member_SendPoint")]
        [Theory, Priority(700)]
        [InlineData(1)]
        [InlineData(100)]
        public async Task Member_SendPoint_Success(int amount)
        {
            var dto = new
            {
                ToMemberId = _member1Id,
                Amount = amount
            };

            var response = await _client.CallWithToken($"v1/members/me/wallets/transactions", HttpMethod.Post, dto, _memberToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Thread.Sleep(500);
        }

        [Trait("Category", "Member_BuyVoucher")]
        [Fact, Priority(701)]
        public async Task Member_BuyVoucher_Failed_Null()
        {
            VoucherActionDTO dto = new()
            {
                Action = VoucherAction.Buy
            };

            var response = await _client.CallWithToken($"v1/vouchers/null/action", HttpMethod.Post, dto, _memberToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Member_BuyVoucher")]
        [Fact, Priority(701)]
        public async Task Member_BuyVoucher_Failed_NotFound()
        {
            VoucherActionDTO dto = new()
            {
                Action = VoucherAction.Buy
            };

            var response = await _client.CallWithToken($"v1/vouchers/{Guid.Empty}/action", HttpMethod.Post, dto, _memberToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Trait("Category", "Member_BuyVoucher")]
        [Fact, Priority(701)]
        public async Task Member_BuyVoucher_Failed_NotEnoughToken()
        {
            VoucherActionDTO dto = new()
            {
                Action = VoucherAction.Buy
            };

            var response = await _client.CallWithToken($"v1/vouchers/{_voucherId}/action", HttpMethod.Post, dto, _member2Token);
            var resJson = await Helper.DeserializeTo<ResponseDTO<bool>>(response);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(ErrorNameValues.InsufficentToken, resJson.ErrorName);
        }


        [Trait("Category", "Member_BuyVoucher")]
        [Fact, Priority(702)]
        public async Task Member_BuyVoucher_Success()
        {
            VoucherActionDTO dto = new()
            {
                Action = VoucherAction.Buy
            };

            var response = await _client.CallWithToken($"v1/vouchers/{_voucherId}/action", HttpMethod.Post, dto, _memberToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Thread.Sleep(2000);
        }

        [Trait("Category", "Member_GetBoughtVoucher")]
        [Theory, Priority(703)]
        [InlineData("Invalid", null)]
        [InlineData(null, "Invalid")]
        [InlineData("Invalid", "Invalid")]
        public async Task Member_GetBoughtVoucher_Failed(string? status, string? orderBy)
        {
            var q = new Dictionary<string?, string?>()
            {
                {"Status" ,status},
                {"OrderBy" , orderBy}
            };

            var response = await _client.CallWithQuery($"v1/members/me/vouchers", HttpMethod.Get, q, _memberToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Member_GetBoughtVoucher")]
        [Theory, Priority(703)]
        [InlineData(null, null)]
        [InlineData("Created", "CreatedAtAsc")]
        public async Task Member_GetBoughtVoucher_Success(string? status, string? orderBy)
        {
            var q = new Dictionary<string?, string?>()
            {
                {"Status" ,status},
                {"OrderBy" , orderBy}
            };

            var response = await _client.CallWithQuery($"v1/members/me/vouchers", HttpMethod.Get, q, _memberToken);
            var resJson = await Helper.DeserializeTo<ResponseDTO<List<MemberVoucherDTO>>>(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Single(resJson.Message);
        }

        [Trait("Category", "Member_GetNotifications")]
        [Theory, Priority(704)]
        [InlineData("Invalid")]
        public async Task Member_GetNotifications_Failed(string? orderBy)
        {
            var q = new Dictionary<string?, string?>()
            {
                {"OrderBy" , orderBy}
            };

            var response = await _client.CallWithQuery($"v1/members/me/notifications", HttpMethod.Get, q, _memberToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Member_GetNotifications")]
        [Theory, Priority(704)]
        [InlineData(null)]
        [InlineData("DateDesc")]
        public async Task Member_GetNotifications_Success(string? orderBy)
        {
            var q = new Dictionary<string?, string?>()
            {
                {"OrderBy" , orderBy}
            };

            var response = await _client.CallWithQuery($"v1/members/me/notifications", HttpMethod.Get, q, _memberToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Trait("Category", "Level_Create")]
        [Fact, Priority(800)]
        public async Task Level_Create_Success()
        {
            var dto = new LevelCreateDTO
            {
                LevelName = "Test Level",
                BasePoint = 10,
                BasePointPerHour = 10,
                XPNeeded = 10,
                MinWorkHour = 10,
                MaxWorkHour = 10,
                LevelColor = "#ffffff"
            };

            var response = await _client.CallWithToken($"v1/levels", HttpMethod.Post, dto, _adminToken);
            var resJson = await Helper.DeserializeTo<ResponseDTO<LevelDTO>>(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _levelId = resJson.Message.LevelId;
        }

        [Trait("Category", "Level_Create")]
        [Fact, Priority(800)]
        public async Task Level_Create_Failed_NullName()
        {
            var dto = new
            {
                BasePoint = 10,
                BasePointPerHour = 10,
                Loyal = 10,
                XPNeeded = 10,
                MinWorkHour = 10,
                MaxWorkHour = 10,
                LevelColor = "#ffffff"
            };

            var response = await _client.CallWithToken($"v1/levels", HttpMethod.Post, dto, _adminToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Level_Create")]
        [Fact, Priority(800)]
        public async Task Level_Create_Failed_NullAStts()
        {
            var dto = new
            {
                LevelName = "aaa"
            };

            var response = await _client.CallWithToken($"v1/levels", HttpMethod.Post, dto, _adminToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Level_Update")]
        [Fact, Priority(800)]
        public async Task Level_Update_Failed_Null()
        {
            var dto = new
            {
                LevelName = "Test Level",
                BasePoint = 10,
                BasePointPerHour = 10,
                Loyal = 10,
                XPNeeded = 10,
                MinWorkHour = 10,
                MaxWorkHour = 10,
                LevelColor = "#ffffff"
            };

            var response = await _client.CallWithToken($"v1/levels", HttpMethod.Put, dto, _adminToken);
            var resJson = await Helper.DeserializeTo<ResponseDTO<bool>>(response);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Trait("Category", "Level_Update")]
        [Fact, Priority(800)]
        public async Task Level_Update_Failed_NotFound()
        {
            var dto = new
            {
                LevelId = -33,
                LevelName = "Test Level",
                BasePoint = 10,
                BasePointPerHour = 10,
                Loyal = 10,
                XPNeeded = 10,
                MinWorkHour = 10,
                MaxWorkHour = 10,
                LevelColor = "#ffffff"
            };

            var response = await _client.CallWithToken($"v1/levels", HttpMethod.Put, dto, _adminToken);
            var resJson = await Helper.DeserializeTo<ResponseDTO<bool>>(response);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Trait("Category", "Level_Update")]
        [Fact, Priority(800)]
        public async Task Level_Update_Failed_NoAttrs()
        {
            var dto = new
            {
                LevelId = _levelId,
            };

            var response = await _client.CallWithToken($"v1/levels", HttpMethod.Put, dto, _adminToken);
            var resJson = await Helper.DeserializeTo<ResponseDTO<bool>>(response);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Level_Update")]
        [Fact, Priority(800)]
        public async Task Level_Update_Success()
        {
            var dto = new LevelUpdateDTO()
            {
                LevelId = _levelId,
                LevelName = "Test Level",
                BasePoint = 10,
                BasePointPerHour = 10,
                XPNeeded = 10,
                MinWorkHour = 10,
                MaxWorkHour = 10,
                LevelColor = "#ffffff"
            };

            var response = await _client.CallWithToken($"v1/levels", HttpMethod.Put, dto, _adminToken);
            var resJson = await Helper.DeserializeTo<ResponseDTO<bool>>(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(resJson.Message);
        }

        [Trait("Category", "Level_GetAll")]
        [Fact, Priority(800)]
        public async Task Level_GetAll()
        {
            var response = await _client.CallWithToken($"v1/levels", HttpMethod.Get, null, _adminToken);
            var resJson = await Helper.DeserializeTo<ResponseDTO<List<LevelDTO>>>(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(resJson.Message.Count >= 3);
        }

        [Trait("Category", "Member_GetAchievement")]
        [Fact, Priority(800)]
        public async Task Member_GetAchievement_Failed_Null()
        {
            var response = await _client.CallWithToken($"v1/members/null/achievements", HttpMethod.Get, null, _adminToken);
            var resJson = await Helper.DeserializeTo<ResponseDTO<bool>>(response);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Trait("Category", "Member_GetAchievement")]
        [Fact, Priority(800)]
        public async Task Member_GetAchievement_Failed_NotFound()
        {
            var response = await _client.CallWithToken($"v1/members/{Guid.Empty}/achievements", HttpMethod.Get, null, _adminToken);
            var resJson = await Helper.DeserializeTo<ResponseDTO<bool>>(response);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Trait("Category", "Member_GetAchievement")]
        [Fact, Priority(800)]
        public async Task Member_GetAchievement_Success()
        {
            var response = await _client.CallWithToken($"v1/members/{_memberId}/achievements", HttpMethod.Get, null, _adminToken);
            var resJson = await Helper.DeserializeTo<ResponseDTO<MemberAchievementDTO>>(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private static MemberExportLinkDTO memberExportLinkDto;

        [Trait("Category", "Member_GetSelfCertLink")]
        [Fact, Priority(801)]
        public async Task Member_GetSelfCertLink_Success()
        {
            var response = await _client.CallWithToken($"v1/certificate/me/link", HttpMethod.Get, null, _memberToken);
            var resJson = await Helper.DeserializeTo<ResponseDTO<MemberExportLinkDTO>>(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            memberExportLinkDto = resJson.Message;
        }


        [Trait("Category", "Member_GetCertificate")]
        [Fact, Priority(802)]
        public async Task Member_GetCertificate_Success()
        {
            var q = new Dictionary<string?, string?>()
            {
                {"MemberId" , _memberId.ToString()},
                {"Code", memberExportLinkDto.Code}
            };

            var response = await _client.CallWithQuery($"v1/certificate", HttpMethod.Get, q, _memberToken);
            //var resJson = await Helper.DeserializeTo<ResponseDTO<>>(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        //[Trait("Category", "Project_CreateMilestone")]
        //[Fact, Priority(900)]
        //public async Task Project_CreateMilestone_Success()
        //{
        //    Assert.True(true);
        //}

        //[Trait("Category", "Project_CreateMilestone")]
        //[Fact, Priority(900)]
        //public async Task Project_CreateMilestone_Failed()
        //{
        //    Assert.True(true);
        //}

        //[Trait("Category", "Project_UpdateMilestone")]
        //[Fact, Priority(901)]
        //public async Task Project_UpdateMilestone_Success()
        //{
        //    Assert.True(true);
        //}

        //[Trait("Category", "SalaryCycleFlow")]
        //[Fact, Priority(401)]
        //public async Task SalaryCycle_CheckPayslip()
        //{
        //    var response = await _client.CallWithToken($"v1/salarycycle/{_salaryCycleId}", HttpMethod.Get, null, _adminToken);
        //    var resJson = await Helper.DeserializeTo<ResponseDTO<SalaryCycleWithPayslipDTO>>(response);

        //    var paySlip = resJson.Message.Payslips;

        //    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        //    Assert.Equal(2, paySlip.Count);
        //}

        #endregion


        [Trait("Category", "System")]
        [Fact, Priority(10000)]
        public async Task AfterTest_Cleanup()
        {
            var _unitOfWork = (UnitOfWork)_factory.Services.GetService(typeof(UnitOfWork))!;
            // Delete Projects
            await _unitOfWork.EnsureDeleted();
        }
    }
}