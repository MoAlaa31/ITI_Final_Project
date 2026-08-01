using FakeItEasy;
using FluentAssertions;
using ITI_Project.Core;
using ITI_Project.Core.Enums;
using ITI_Project.Core.Errors;
using ITI_Project.Core.IRepository;
using ITI_Project.Core.IServices;
using ITI_Project.Core.Models.Location;
using ITI_Project.Core.Models.Users;
using ITI_Project.Services.User.DTOs.ClientDTOs;
using ITI_Project.Services.User.UserServices.ClientService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ITI_Project.Tests.Services.UserServiceTests
{
    public class ClientServiceTests
    {
        #region Fakes

        private readonly IUnitOfWork unitOfWork;
        private readonly IGenericRepository<Client> clientRepository;
        private readonly IGenericRepository<Region> regionRepository;
        private readonly IGenericRepository<UserPhoneNumber> phoneRepository;
        private readonly IFileStorageService fileStorageService;

        #endregion

        #region SUT

        private readonly ClientService service;

        #endregion

        public ClientServiceTests()
        {
            unitOfWork = A.Fake<IUnitOfWork>();

            clientRepository = A.Fake<IGenericRepository<Client>>();
            regionRepository = A.Fake<IGenericRepository<Region>>();
            phoneRepository = A.Fake<IGenericRepository<UserPhoneNumber>>();

            fileStorageService = A.Fake<IFileStorageService>();

            A.CallTo(() => unitOfWork.Repository<Client>())
            .Returns(clientRepository);

            A.CallTo(() => unitOfWork.Repository<Region>())
                .Returns(regionRepository);

            A.CallTo(() => unitOfWork.Repository<UserPhoneNumber>())
                .Returns(phoneRepository);

            service = new ClientService(unitOfWork, fileStorageService);
        }

        [Fact]
        public async Task GetClientProfileAsync_Should_Return_Client_When_Client_Exists()
        {
            // Arrange
            var client = CreateClient();
            A.CallTo(() =>
                clientRepository.GetByIdWithIncludesAsync(
                    client.Id,
                    A<Expression<Func<Client, object>>[]>.Ignored))
                .Returns(client);

            // Act
            var result = await service.GetClientProfileAsync(client.Id);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEquivalentTo(client);
        }

        [Fact]
        public async Task GetClientProfileAsync_Should_Return_NotFound_When_Client_Does_Not_Exist()
        {
            // Arrange
            var client = CreateClient();
            A.CallTo(() =>
                clientRepository.GetByIdWithIncludesAsync(
                    client.Id,
                    A<Expression<Func<Client, object>>[]>.Ignored))
                .Returns((Client?)null);
            // Act
            var result = await service.GetClientProfileAsync(1);
            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().BeEquivalentTo(new Error("Client.NotFound", "Client not found", HttpStatusCode.NotFound));
        }


        [Fact]
        public async Task UpdateClientProfileAsync_Should_Return_NotFound_When_Client_Does_Not_Exist()
        {
            // Arrange
            var dto = CreateDto();

            A.CallTo(() =>
                clientRepository.GetByIdWithIncludesAsync(
                    A<int>.Ignored,
                    A<Expression<Func<Client, object>>[]>.Ignored))
                .Returns((Client?)null);

            // Act
            var result = await service.UpdateClientProfileAsync(1, dto);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().BeEquivalentTo(new Error("Client.NotFound", "Client not found", HttpStatusCode.NotFound));
        }


        [Fact]
        public async Task UpdateClientProfileAsync_Should_Return_InvalidRegion_When_Region_Does_Not_Exist()
        {
            // Arrange
            var client = CreateClient();
            var dto = CreateDto();

            A.CallTo(() =>
                clientRepository.GetByIdWithIncludesAsync(
                    client.Id,
                    A<Expression<Func<Client, object>>[]>.Ignored))
                .Returns(client);

            A.CallTo(() =>
                regionRepository.GetByIdAsync(dto.RegionId))
                .Returns((Region?)null);

            // Act

            var result = await service.UpdateClientProfileAsync(client.Id, dto);

            // Assert

            result.IsSuccess.Should().BeFalse();

            result.Error.Should().BeEquivalentTo(new Error(
                "Region.Invalid",
                "Invalid Region",
                HttpStatusCode.BadRequest));
        }

        [Fact]
        public async Task UpdateClientProfileAsync_Should_Return_RegionMismatch_When_Governorate_Does_Not_Match()
        {
            // Arrange

            var client = CreateClient();
            var dto = CreateDto();

            var region = CreateRegion();
            region.GovernorateId = 999;

            A.CallTo(() =>
                clientRepository.GetByIdWithIncludesAsync(
                    client.Id,
                    A<Expression<Func<Client, object>>[]>.Ignored))
                .Returns(client);

            A.CallTo(() =>
                regionRepository.GetByIdAsync(dto.RegionId))
                .Returns(region);

            // Act
            var result = await service.UpdateClientProfileAsync(client.Id, dto);

            // Assert
            result.IsSuccess.Should().BeFalse();

            result.Error.Should().BeEquivalentTo(new Error(
                "Region.Mismatch",
                "Region does not belong to the selected governorate",
                HttpStatusCode.BadRequest));
        }

        [Fact]
        public async Task UpdateClientProfileAsync_Should_Update_Client_Profile_When_Request_Is_Valid()
        {
            // Arrange
            var client = CreateClient();
            var dto = CreateDto();
            var region = CreateRegion();

            A.CallTo(() =>
                clientRepository.GetByIdWithIncludesAsync(
                    client.Id,
                    A<Expression<Func<Client, object>>[]>.Ignored))
                .Returns(client);

            A.CallTo(() =>
                regionRepository.GetByIdAsync(dto.RegionId))
                .Returns(region);

            // Act
            var result = await service.UpdateClientProfileAsync(client.Id, dto);

            // Assert
            result.IsSuccess.Should().BeTrue();

            result.Data.FirstName.Should().Be(dto.FirstName);
            result.Data.LastName.Should().Be(dto.LastName);
            result.Data.RegionId.Should().Be(dto.RegionId);
            result.Data.GovernorateId.Should().Be(dto.GovernorateId);

            A.CallTo(() => clientRepository.Update(client))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => unitOfWork.CompleteAsync())
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task UpdateClientProfileAsync_Should_Return_Failure_When_Picture_Upload_Fails()
        {
            // Arrange

            var client = CreateClient();
            var dto = CreateDto();

            dto.Picture = CreatePicture();

            var region = CreateRegion();

            A.CallTo(() =>
                clientRepository.GetByIdWithIncludesAsync(
                    client.Id,
                    A<Expression<Func<Client, object>>[]>.Ignored))
                .Returns(client);

            A.CallTo(() =>
                regionRepository.GetByIdAsync(dto.RegionId))
                .Returns(region);

            A.CallTo(() =>
                fileStorageService.UploadFileAsync(
                    A<FileUploadRequest>.Ignored,
                    A<CancellationToken>.Ignored))
                .Returns(Task.FromResult((Success: false, Message: "Upload failed", FilePath: (string?)null)));

            // Act

            var result = await service.UpdateClientProfileAsync(client.Id, dto);

            // Assert

            result.IsSuccess.Should().BeFalse();

            A.CallTo(() =>
                fileStorageService.DeleteFile(
                    A<string>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task UpdateClientProfileAsync_Should_Upload_New_Picture_When_Picture_Is_Provided()
        {
            // Arrange

            var client = CreateClient();
            var dto = CreateDto();
            var region = CreateRegion();

            dto.Picture = CreatePicture();

            A.CallTo(() =>
                clientRepository.GetByIdWithIncludesAsync(
                    client.Id,
                    A<Expression<Func<Client, object>>[]>.Ignored))
                .Returns(client);

            A.CallTo(() =>
                regionRepository.GetByIdAsync(dto.RegionId))
                .Returns(region);

            A.CallTo(() =>
                fileStorageService.UploadFileAsync(
                    A<FileUploadRequest>.Ignored,
                    A<CancellationToken>.Ignored))
                .Returns(Task.FromResult((Success: true, Message: "File uploaded successfully.", FilePath: (string?)"client-pictures/image.jpg")));

            // Act

            var result = await service.UpdateClientProfileAsync(client.Id, dto);

            // Assert

            result.IsSuccess.Should().BeTrue();

            client.PictureUrl.Should().Be("client-pictures/image.jpg");

            A.CallTo(() =>
                fileStorageService.UploadFileAsync(
                    A<FileUploadRequest>.Ignored,
                    A<CancellationToken>.Ignored))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task UpdateClientProfileAsync_Should_Delete_Old_Picture_When_New_Picture_Is_Uploaded()
        {
            // Arrange

            var client = CreateClient();
            client.PictureUrl = "old-picture.jpg";

            var dto = CreateDto();
            dto.Picture = CreatePicture();

            var region = CreateRegion();

            A.CallTo(() =>
                clientRepository.GetByIdWithIncludesAsync(
                    client.Id,
                    A<Expression<Func<Client, object>>[]>.Ignored))
                .Returns(client);

            A.CallTo(() =>
                regionRepository.GetByIdAsync(dto.RegionId))
                .Returns(region);

            A.CallTo(() =>
                fileStorageService.UploadFileAsync(
                    A<FileUploadRequest>.Ignored,
                    A<CancellationToken>.Ignored))
                .Returns((Success: true, Message: "File uploaded successfully.", FilePath: "new-picture.jpg"));

            // Act

            await service.UpdateClientProfileAsync(client.Id, dto);

            // Assert

            A.CallTo(() =>
                fileStorageService.DeleteFile("old-picture.jpg"))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task UpdateClientProfileAsync_Should_Update_PhoneNumbers()
        {
            // Arrange

            var client = CreateClient();

            client.phoneNumbers =
            [
                new UserPhoneNumber
                {
                    ClientId = client.Id,
                    PhoneNumber = "01000000000"
                }
            ];

            var dto = CreateDto();
            var region = CreateRegion();

            A.CallTo(() =>
                clientRepository.GetByIdWithIncludesAsync(
                    client.Id,
                    A<Expression<Func<Client, object>>[]>.Ignored))
                .Returns(client);

            A.CallTo(() =>
                regionRepository.GetByIdAsync(dto.RegionId))
                .Returns(region);

            // Act

            var result = await service.UpdateClientProfileAsync(client.Id, dto);

            // Assert

            result.IsSuccess.Should().BeTrue();

            A.CallTo(() =>
                phoneRepository.DeleteRange(
                    A<IEnumerable<UserPhoneNumber>>.Ignored))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() =>
                phoneRepository.AddRangeAsync(
                    A<IEnumerable<UserPhoneNumber>>.That.Matches(
                        x => x.Count() == dto.PhoneNumbers!.Count)))
                .MustHaveHappenedOnceExactly();
        }


        private static Client CreateClient()
        {
            return new Client
            {
                Id = 1,
                AppUserId = Guid.NewGuid().ToString(),

                FirstName = "Mohamed",
                LastName = "Alaa",

                GovernorateId = 1,
                RegionId = 1,

                Gender = Gender.Male,

                DateOfBirth = new DateOnly(2002, 1, 1),

                phoneNumbers = new List<UserPhoneNumber>()
            };
        }

        private static ServiceUpdateClientProfileDTO CreateDto()
        {
            return new()
            {
                FirstName = "Ahmed",

                LastName = "Ali",

                GovernorateId = 5,

                RegionId = 10,

                Gender = Gender.Male,

                DateOfBirth = new DateOnly(2000, 5, 1),

                PhoneNumbers =
                [
                    "01000000000",
                    "01111111111"
                ]
            };
        }

        private static Region CreateRegion()
        {
            return new()
            {
                Id = 10,

                GovernorateId = 5
            };
        }

        private static FileData CreatePicture(string fileName = "profile-picture.jpg")
        {
            return new FileData
            {
                FileName = fileName,
                Content = new MemoryStream(new byte[] { 1, 2, 3 })
            };
        }
    }
}
