//using AutoMapper;
//using FakeItEasy;
//using ITI_Project.Api.Controllers.UserControllers;
//using ITI_Project.Api.DTO.Users;
//using ITI_Project.Core;
//using ITI_Project.Core.Constants;
//using ITI_Project.Core.IServices;
//using ITI_Project.Core.Models.Location;
//using ITI_Project.Core.Models.Users;
//using ITI_Project.Services.User.DTOs.ClientDTOs;
//using ITI_Project.Services.User.UserServices.ClientService;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Security.Claims;
//using System.Text;
//using System.Threading.Tasks;

//namespace ITI_Project.Tests.Controller
//{
//    public class ClientControllerTest
//    {
//        private readonly IMapper mapper;
//        private readonly IUnitOfWork unitOfWork;
//        private readonly IFileStorageService fileStorageService;
//        private readonly IClientService clientService;

//        private readonly ClientController _clientController;
//        public ClientControllerTest()
//        {
//            // Set up dependencies and test data here
//            this.mapper = A.Fake<IMapper>();
//            this.unitOfWork = A.Fake<IUnitOfWork>();
//            this.fileStorageService = A.Fake<IFileStorageService>();
//            this.clientService = A.Fake<IClientService>();
//            // SUT -> System Under Test
//            _clientController = new ClientController(mapper, unitOfWork, fileStorageService, clientService);


//        }

//        private static ClaimsPrincipal CreateClientPrincipal(int clientId)
//        {
//            var claims = new[]
//            {
//                new Claim(Identifiers.ClientId, clientId.ToString())
//            };

//            var identity = new ClaimsIdentity(claims, "TestAuth");

//            return new ClaimsPrincipal(identity);
//        }


//        [Fact]
//        public async Task GetClientProfile_ReturnsOkResult_WhenClientExists()
//        {
//            // Arrange
//            var clientId = 1;

//            var client = A.Fake<Client>();
//            var clientDto = new ServiceClientDTO();

//            // Set up authenticated user's ClientId claim
//            var claimsPrincipal = CreateClientPrincipal(clientId);

//            _clientController.ControllerContext = new ControllerContext
//            {
//                HttpContext = new DefaultHttpContext
//                {
//                    User = claimsPrincipal,
//                },
//            };

//            A.CallTo(() => unitOfWork.Repository<Client>()
//                .GetByIdWithIncludesAsync(
//                    clientId,
//                    A<Expression<Func<Client, object>>>.Ignored))
//                .Returns(client);

//            A.CallTo(() => mapper.Map<ServiceClientDTO>(client))
//                .Returns(clientDto);

//            // Act
//            var result = await _clientController.GetClientProfile();

//            // Assert
//            Assert.IsType<OkObjectResult>(result.Result);
//        }

//        [Fact]
//        public async Task UpdateClientProfile_ReturnsOkResult_WhenClientExists()
//        {
//            // Arrange
//            var clientId = 1;
//            var clientUpdateDTO = new ClientUpdateDTO
//            {
//                GovernorateId = 1,
//                RegionId = 1,
//                Picture = null // No picture for this test
//            };
//            var client = A.Fake<Client>();
//            var region = new Region
//            {
//                Id = 1,
//                GovernorateId = 1
//            };

//            // Set up authenticated user's ClientId claim
//            var claimsPrincipal = CreateClientPrincipal(clientId);
//            _clientController.ControllerContext = new ControllerContext
//            {
//                HttpContext = new DefaultHttpContext
//                {
//                    User = claimsPrincipal,
//                },
//            };
//            A.CallTo(() => unitOfWork.Repository<Client>()
//                .GetByIdWithIncludesAsync(
//                    clientId,
//                    A<Expression<Func<Client, object>>>.Ignored))
//                .Returns(client);

//            A.CallTo(() => unitOfWork.Repository<Region>().GetByIdAsync(clientUpdateDTO.RegionId))
//                .Returns(region);
//            // Act
//            var result = await _clientController.UpdateClientProfile(clientUpdateDTO);
//            // Assert
//            Assert.IsType<OkObjectResult>(result.Result);
//        }
//    }
//}
