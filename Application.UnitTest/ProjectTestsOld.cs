//using System.ComponentModel;
//using Application.Controllers;
//using Application.Domain.Models;
//using Application.DTOs.Project;
//using Application.Helpers;
//using Application.Persistence.Repositories;
//using Application.QueryParams.Project;
//using Application.Services;
//using AutoMapper;
//using FakeItEasy;
//using FluentAssertions;

//namespace Application.UnitTest
//{
//    public class ProjectTests
//    {
//        //public readonly ProjectRepository ProjectRepository;
//        public readonly UnitOfWork _unitOfWork;
//        public readonly IProjectService _projectService;
//        public readonly IMapper _mapper;

//        public ProjectTests()
//        {
//            _unitOfWork = A.Fake<UnitOfWork>();
//            _projectService = A.Fake<IProjectService>();
//            _mapper = A.Fake<Mapper>();
//        }

//        [Fact]
//        public async void GetProjects_ReturnOK()
//        {
//            // Arrange
//            var projects = A.Fake<PagedList<Project>>();
//            var projectList = A.Fake<List<ProjectDetailDTO>>();

//            A.CallTo(() => _mapper.Map<List<ProjectDetailDTO>>(projects)).Returns(projectList);
//            A.CallTo(() => _projectService.GetAll(new ProjectQueryParams())).Returns(projects);

//            var controller = new ProjectsController(_projectService, null, null, null, null, null, null, _mapper);

//            // Act
//            var result = await controller.GetProjects(new ProjectQueryParams());

//            // Assert
//            result.Should().NotBeNull();
//        }
//    }
//}