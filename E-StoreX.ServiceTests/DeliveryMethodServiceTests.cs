using AutoMapper;
using EStoreX.Core.Domain.Entities.Orders;
using EStoreX.Core.DTO.Orders.Requests;
using EStoreX.Core.DTO.Orders.Responses;
using EStoreX.Core.RepositoryContracts.Common;
using EStoreX.Core.RepositoryContracts.Orders;
using EStoreX.Core.Services.Categories;
using EStoreX.Core.Services.Orders;
using FluentAssertions;
using Microsoft.Extensions.Localization;
using Moq;

namespace E_StoreX.ServiceTests
{
    public class DeliveryMethodServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IDeliveryMethodRepository> _repositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly DeliveryMethodService _service;
        private readonly Mock<IStringLocalizer<DeliveryMethodService>> _localizerMock;

        public DeliveryMethodServiceTests()
        {
            _mapperMock = new Mock<IMapper>();
            _repositoryMock = new Mock<IDeliveryMethodRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _localizerMock = new Mock<IStringLocalizer<DeliveryMethodService>>();
            _unitOfWorkMock.Setup(u => u.DeliveryMethodRepository).Returns(_repositoryMock.Object);
            _service = new DeliveryMethodService(_unitOfWorkMock.Object, _mapperMock.Object, _localizerMock.Object);
        }

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_ShouldReturnMappedDeliveryMethods_WhenTheyExist()
        {
            // Arrange
            var deliveryMethods = new List<DeliveryMethod>
            {
                new DeliveryMethod { Id = Guid.NewGuid(), NameEn = "Standard", Price = 50 },
                new DeliveryMethod { Id = Guid.NewGuid(), NameEn = "Express", Price = 100 }
            };

            var expectedResponses = new List<DeliveryMethodResponse>
            {
                new DeliveryMethodResponse { Id = deliveryMethods[0].Id, Name = "Standard", Price = 50 },
                new DeliveryMethodResponse { Id = deliveryMethods[1].Id, Name = "Express", Price = 100 }
            };

            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(deliveryMethods);
            _mapperMock.Setup(m => m.Map<IEnumerable<DeliveryMethodResponse>>(deliveryMethods))
                       .Returns(expectedResponses);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Should().BeEquivalentTo(expectedResponses);
            _repositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
            _mapperMock.Verify(m => m.Map<IEnumerable<DeliveryMethodResponse>>(deliveryMethods), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoDeliveryMethodsExist()
        {
            // Arrange
            var deliveryMethods = new List<DeliveryMethod>();
            var expectedResponses = new List<DeliveryMethodResponse>();

            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(deliveryMethods);
            _mapperMock.Setup(m => m.Map<IEnumerable<DeliveryMethodResponse>>(deliveryMethods))
                       .Returns(expectedResponses);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
            _repositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
            _mapperMock.Verify(m => m.Map<IEnumerable<DeliveryMethodResponse>>(deliveryMethods), Times.Once);
        }


        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_ShouldReturnMappedDeliveryMethod_WhenExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var deliveryMethod = new DeliveryMethod { Id = id, NameEn = "Standard", Price = 50 };
            var expectedResponse = new DeliveryMethodResponse { Id = id, Name = "Standard", Price = 50 };

            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(deliveryMethod);
            _mapperMock.Setup(m => m.Map<DeliveryMethodResponse>(deliveryMethod)).Returns(expectedResponse);

            // Act
            var result = await _service.GetByIdAsync(id);

            // Assert
            result.Should().BeEquivalentTo(expectedResponse);
            _repositoryMock.Verify(r => r.GetByIdAsync(id), Times.Once);
            _mapperMock.Verify(m => m.Map<DeliveryMethodResponse>(deliveryMethod), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenDeliveryMethodNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((DeliveryMethod?)null);

            // Act
            var result = await _service.GetByIdAsync(id);

            // Assert
            result.Should().BeNull();
            _repositoryMock.Verify(r => r.GetByIdAsync(id), Times.Once);
            _mapperMock.Verify(m => m.Map<DeliveryMethodResponse>(It.IsAny<DeliveryMethod>()), Times.Never);
        }


        #endregion

        #region CreateAsync Tests


        [Fact]
        public async Task CreateAsync_ShouldMapRequest_AddToRepository_SaveChanges_AndReturnResponse()
        {
            // Arrange
            var request = new DeliveryMethodRequest { NameEn = "Express", Price = 100 };
            var entity = new DeliveryMethod { Id = Guid.NewGuid(), NameEn = "Express", Price = 100 };
            var expectedResponse = new DeliveryMethodResponse { Id = entity.Id, Name = "Express", Price = 100 };

            _mapperMock.Setup(m => m.Map<DeliveryMethod>(request)).Returns(entity);
            _mapperMock.Setup(m => m.Map<DeliveryMethodResponse>(entity)).Returns(expectedResponse);

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            result.Should().BeEquivalentTo(expectedResponse);
            _repositoryMock.Verify(r => r.AddAsync(entity), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
            _mapperMock.Verify(m => m.Map<DeliveryMethod>(request), Times.Once);
            _mapperMock.Verify(m => m.Map<DeliveryMethodResponse>(entity), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldAssignNewGuidToEntity()
        {
            // Arrange
            var request = new DeliveryMethodRequest { NameEn = "Standard", Price = 50 };
            var entity = new DeliveryMethod(); // Guid.Empty by default

            _mapperMock.Setup(m => m.Map<DeliveryMethod>(request)).Returns(entity);

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            entity.Id.Should().NotBe(Guid.Empty);
        }


        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ShouldReturnNull_WhenEntityNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetByIdAsync(id))
                           .ReturnsAsync((DeliveryMethod?)null);

            // Act
            var result = await _service.UpdateAsync(id, new DeliveryMethodRequest());

            // Assert
            result.Should().BeNull();
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<DeliveryMethod>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateEntity_AndReturnMappedResponse()
        {
            // Arrange
            var id = Guid.NewGuid();
            var entity = new DeliveryMethod { Id = id, NameEn = "Old", Price = 10 };
            var request = new DeliveryMethodRequest { NameEn = "Updated", Price = 20 };
            var expectedResponse = new DeliveryMethodResponse { Id = id, Name = "Updated", Price = 20 };

            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);
            _mapperMock.Setup(m => m.Map(request, entity)); // mapping in-place
            _mapperMock.Setup(m => m.Map<DeliveryMethodResponse>(entity)).Returns(expectedResponse);

            // Act
            var result = await _service.UpdateAsync(id, request);

            // Assert
            result.Should().BeEquivalentTo(expectedResponse);
            _repositoryMock.Verify(r => r.UpdateAsync(entity), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
            _mapperMock.Verify(m => m.Map(request, entity), Times.Once);
        }


        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenEntityNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((DeliveryMethod?)null);

            // Act
            var result = await _service.DeleteAsync(id);

            // Assert
            result.Should().BeFalse();
            _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnTrue_WhenEntityExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var entity = new DeliveryMethod { Id = id, NameEn = "Express", Price = 50 };

            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);

            // Act
            var result = await _service.DeleteAsync(id);

            // Assert
            result.Should().BeTrue();
            _repositoryMock.Verify(r => r.DeleteAsync(id), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }


        #endregion

        #region GetByNameAsync Tests

        [Fact]
        public async Task GetByNameAsync_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            var name = "Standard";
            _repositoryMock.Setup(r => r.GetByNameAsync(name))
                           .ReturnsAsync((DeliveryMethod?)null);

            // Act
            var result = await _service.GetByNameAsync(name);

            // Assert
            result.Should().BeNull();
            _mapperMock.Verify(m => m.Map<DeliveryMethodResponse>(It.IsAny<DeliveryMethod>()), Times.Never);
        }

        [Fact]
        public async Task GetByNameAsync_ShouldReturnResponse_WhenFound()
        {
            // Arrange
            var name = "Express";
            var entity = new DeliveryMethod { Id = Guid.NewGuid(), NameEn = name, Price = 30 };
            var expectedResponse = new DeliveryMethodResponse { Id = entity.Id, Name = name, Price = 30 };

            _repositoryMock.Setup(r => r.GetByNameAsync(name)).ReturnsAsync(entity);
            _mapperMock.Setup(m => m.Map<DeliveryMethodResponse>(entity)).Returns(expectedResponse);

            // Act
            var result = await _service.GetByNameAsync(name);

            // Assert
            result.Should().BeEquivalentTo(expectedResponse);
            _mapperMock.Verify(m => m.Map<DeliveryMethodResponse>(entity), Times.Once);
        }


        #endregion
    }
}
