using AutoMapper;
using Domain.Entities.Product;
using EStoreX.Core.Domain.Entities.Rating;
using EStoreX.Core.DTO.Ratings.Requests;
using EStoreX.Core.DTO.Ratings.Response;
using EStoreX.Core.RepositoryContracts.Common;
using EStoreX.Core.RepositoryContracts.Ratings;
using EStoreX.Core.ServiceContracts.Ratings;
using EStoreX.Core.Services.Ratings;
using FluentAssertions;
using Microsoft.Extensions.Localization;
using Moq;

namespace E_StoreX.ServiceTests
{
    public class RatingServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly IRatingService _ratingService;
        private readonly Mock<IRatingRepository> _ratingRepositoryMock;
        private readonly Mock<IStringLocalizer<RatingService>> _localizerMock;
        public RatingServiceTests()
        {
            _mapperMock = new Mock<IMapper>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _ratingRepositoryMock = new Mock<IRatingRepository>();
            _localizerMock = new Mock<IStringLocalizer<RatingService>>();
            _unitOfWorkMock.Setup(r => r.RatingRepository).Returns(_ratingRepositoryMock.Object);
            _ratingService = new RatingService(_unitOfWorkMock.Object, _mapperMock.Object, _localizerMock.Object);
        }

        #region AddRatingAsync Tests

        [Fact]
        public async Task AddRatingAsync_ShouldThrowInvalidOperationException_WhenUserAlreadyRated()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var request = new RatingAddRequest { ProductId = productId, Score = 5 };

            _ratingRepositoryMock
                .Setup(r => r.GetUserRatingForProductAsync(productId, userId))
                .ReturnsAsync(new Rating());

            // Act
            Func<Task> act = async () => await _ratingService.AddRatingAsync(request, userId);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("User has already rated this product.");
        }

        [Fact]
        public async Task AddRatingAsync_ShouldThrowKeyNotFoundException_WhenProductNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var request = new RatingAddRequest { ProductId = productId, Score = 4 };

            _ratingRepositoryMock
                .Setup(r => r.GetUserRatingForProductAsync(productId, userId))
                .ReturnsAsync((Rating?)null);

            _unitOfWorkMock
                .Setup(u => u.ProductRepository.GetByIdAsync(productId))
                .ReturnsAsync((Product?)null);

            // Act
            Func<Task> act = async () => await _ratingService.AddRatingAsync(request, userId);

            // Assert
            await act.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage("Product not found.");
        }

        [Fact]
        public async Task AddRatingAsync_ShouldAddRating_WhenValidRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var request = new RatingAddRequest { ProductId = productId, Score = 5 };
            var product = new Product { Id = productId };
            var rating = new Rating { ProductId = productId, UserId = userId, Score = 5 };
            var ratingResponse = new RatingResponse { ProductId = productId, UserName = userId.ToString(), Score = 5 };

            //_ratingRepositoryMock
            //    .Setup(r => r.GetUserRatingForProductAsync(productId, userId))
            //    .ReturnsAsync((Rating?)null);
            _ratingRepositoryMock
                .SetupSequence(r => r.GetUserRatingForProductAsync(productId, userId))
                .ReturnsAsync((Rating?)null) 
                .ReturnsAsync(rating);      


            _unitOfWorkMock
                .Setup(u => u.ProductRepository.GetByIdAsync(productId))
                .ReturnsAsync(product);

            _mapperMock.Setup(m => m.Map<Rating>(request)).Returns(rating);
            _ratingRepositoryMock.Setup(r => r.AddAsync(rating)).ReturnsAsync(rating);
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            //_ratingRepositoryMock
            //    .Setup(r => r.GetUserRatingForProductAsync(productId, userId))
            //    .ReturnsAsync(rating);

            _mapperMock.Setup(m => m.Map<RatingResponse>(rating)).Returns(ratingResponse);

            // Act
            var result = await _ratingService.AddRatingAsync(request, userId);

            // Assert
            result.Should().NotBeNull();
            result.UserName.Should().Be(userId.ToString());
            result.ProductId.Should().Be(productId);
            result.Score.Should().Be(5);

            _ratingRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Rating>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }
        #endregion

        #region UpdateRatingAsync Tests

        [Fact]
        public async Task UpdateRatingAsync_ShouldReturnNull_WhenRatingNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var ratingId = Guid.NewGuid();
            var request = new RatingUpdateRequest { Score = 3, Comment = "Updated" };

            _ratingRepositoryMock
                .Setup(r => r.GetByIdAsync(ratingId))
                .ReturnsAsync((Rating?)null);

            // Act
            var result = await _ratingService.UpdateRatingAsync(ratingId, request, userId);

            // Assert
            result.Should().BeNull();
            _ratingRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Rating>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateRatingAsync_ShouldReturnNull_WhenRatingDoesNotBelongToUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var ratingId = Guid.NewGuid();
            var request = new RatingUpdateRequest { Score = 4, Comment = "Nice" };

            var rating = new Rating { Id = ratingId, UserId = Guid.NewGuid(), Score = 2 };

            _ratingRepositoryMock
                .Setup(r => r.GetByIdAsync(ratingId))
                .ReturnsAsync(rating);

            // Act
            var result = await _ratingService.UpdateRatingAsync(ratingId, request, userId);

            // Assert
            result.Should().BeNull();
            _ratingRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Rating>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateRatingAsync_ShouldUpdateAndReturnResponse_WhenValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var ratingId = Guid.NewGuid();
            var request = new RatingUpdateRequest { Score = 5, Comment = "Perfect!" };

            var rating = new Rating { Id = ratingId, UserId = userId, ProductId = Guid.NewGuid(), Score = 3 };
            var updatedResponse = new RatingResponse { UserName = userId.ToString(), ProductId = rating.ProductId, Score = 5, Comment = "Perfect!" };

            _ratingRepositoryMock
                .Setup(r => r.GetByIdAsync(ratingId))
                .ReturnsAsync(rating);

            _ratingRepositoryMock
                .Setup(r => r.GetUserRatingForProductAsync(rating.ProductId, userId))
                .ReturnsAsync(rating);

            _mapperMock
                .Setup(m => m.Map<RatingResponse>(rating))
                .Returns(updatedResponse);

            // Act
            var result = await _ratingService.UpdateRatingAsync(ratingId, request, userId);

            // Assert
            result.Should().NotBeNull();
            result.Score.Should().Be(5);
            result.Comment.Should().Be("Perfect!");
            result.UserName.Should().Be(userId.ToString());

            _ratingRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Rating>(ra =>
                ra.Score == 5 && ra.Comment == "Perfect!")), Times.Once);

            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }


        #endregion

        #region DeleteRatingAsync Tests

        [Fact]
        public async Task DeleteRatingAsync_ShouldReturnFalse_WhenRatingNotFound()
        {
            // Arrange
            var ratingId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _ratingRepositoryMock
                .Setup(r => r.GetByIdAsync(ratingId))
                .ReturnsAsync((Rating)null);

            // Act
            var result = await _ratingService.DeleteRatingAsync(ratingId, userId);

            // Assert
            result.Should().BeFalse();
            _ratingRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteRatingAsync_ShouldReturnFalse_WhenRatingDoesNotBelongToUser()
        {
            // Arrange
            var ratingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var rating = new Rating { Id = ratingId, UserId = Guid.NewGuid() };

            _ratingRepositoryMock
                .Setup(r => r.GetByIdAsync(ratingId))
                .ReturnsAsync(rating);

            // Act
            var result = await _ratingService.DeleteRatingAsync(ratingId, userId);

            // Assert
            result.Should().BeFalse();
            _ratingRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteRatingAsync_ShouldReturnTrue_WhenRatingIsDeletedSuccessfully()
        {
            // Arrange
            var ratingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var rating = new Rating { Id = ratingId, UserId = userId };

            _ratingRepositoryMock
                .Setup(r => r.GetByIdAsync(ratingId))
                .ReturnsAsync(rating);

            // Act
            var result = await _ratingService.DeleteRatingAsync(ratingId, userId);

            // Assert
            result.Should().BeTrue();

            _ratingRepositoryMock.Verify(r => r.DeleteAsync(ratingId), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }


        #endregion

        #region GetUserRatingForProductAsync Tests
        [Fact]
        public async Task GetRatingsForProductAsync_ShouldReturnEmptyList_WhenNoRatingsExist()
        {
            // Arrange
            var productId = Guid.NewGuid();
            _ratingRepositoryMock
                .Setup(r => r.GetRatingsByProductAsync(productId))
                .ReturnsAsync(new List<Rating>());

            _mapperMock
                .Setup(m => m.Map<IEnumerable<RatingResponse>>(It.IsAny<List<Rating>>()))
                .Returns(new List<RatingResponse>());

            // Act
            var result = await _ratingService.GetRatingsForProductAsync(productId);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetRatingsForProductAsync_ShouldReturnMappedRatings_WhenRatingsExist()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var ratings = new List<Rating>
            {
                new Rating { Id = Guid.NewGuid(), ProductId = productId, Score = 5 },
                new Rating { Id = Guid.NewGuid(), ProductId = productId, Score = 4 }
            };

            var mappedRatings = new List<RatingResponse>
            {
                new RatingResponse { Id = ratings[0].Id, Score = 5 },
                new RatingResponse { Id = ratings[1].Id, Score = 4 }
            };

            _ratingRepositoryMock
                .Setup(r => r.GetRatingsByProductAsync(productId))
                .ReturnsAsync(ratings);

            _mapperMock
                .Setup(m => m.Map<IEnumerable<RatingResponse>>(ratings))
                .Returns(mappedRatings);

            // Act
            var result = await _ratingService.GetRatingsForProductAsync(productId);

            // Assert
            result.Should().HaveCount(2)
                  .And.ContainSingle(r => r.Score == 5)
                  .And.ContainSingle(r => r.Score == 4);
        }


        #endregion

        #region GetProductRatingSummaryAsync Tests
        [Fact]
        public async Task GetProductRatingSummaryAsync_ShouldReturnZeroSummary_WhenNoRatingsExist()
        {
            // Arrange
            var productId = Guid.NewGuid();
            _ratingRepositoryMock
                .Setup(r => r.GetRatingsByProductAsync(productId))
                .ReturnsAsync(new List<Rating>());

            // Act
            var result = await _ratingService.GetProductRatingSummaryAsync(productId);

            // Assert
            result.ProductId.Should().Be(productId);
            result.AverageScore.Should().Be(0);
            result.TotalRatings.Should().Be(0);
            result.ScoreDistribution.Should().BeEmpty();
        }

        [Fact]
        public async Task GetProductRatingSummaryAsync_ShouldReturnCorrectSummary_WhenRatingsExist()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var ratings = new List<Rating>
            {
                new Rating { Id = Guid.NewGuid(), ProductId = productId, Score = 5 },
                new Rating { Id = Guid.NewGuid(), ProductId = productId, Score = 4 },
                new Rating { Id = Guid.NewGuid(), ProductId = productId, Score = 5 }
            };

            _ratingRepositoryMock
                .Setup(r => r.GetRatingsByProductAsync(productId))
                .ReturnsAsync(ratings);

            // Act
            var result = await _ratingService.GetProductRatingSummaryAsync(productId);

            // Assert
            result.ProductId.Should().Be(productId);
            result.TotalRatings.Should().Be(3);
            result.AverageScore.Should().BeApproximately(4.67, 0.01);
            result.ScoreDistribution.Should().ContainKey(5).WhoseValue.Should().Be(2);
            result.ScoreDistribution.Should().ContainKey(4).WhoseValue.Should().Be(1);
        }


        #endregion

        #region GetUserRatingForProductAsync Tests

        [Fact]
        public async Task GetUserRatingForProductAsync_ShouldReturnNull_WhenNoRatingExists()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _ratingRepositoryMock
                .Setup(r => r.GetUserRatingForProductAsync(productId, userId))
                .ReturnsAsync((Rating?)null);

            // Act
            var result = await _ratingService.GetUserRatingForProductAsync(productId, userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUserRatingForProductAsync_ShouldReturnMappedResponse_WhenRatingExists()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var rating = new Rating { Id = Guid.NewGuid(), ProductId = productId, UserId = userId, Score = 5, Comment = "Great!" };
            var expectedResponse = new RatingResponse { ProductId = productId, UserName = userId.ToString(), Score = 5, Comment = "Great!" };

            _ratingRepositoryMock
                .Setup(r => r.GetUserRatingForProductAsync(productId, userId))
                .ReturnsAsync(rating);

            _mapperMock
                .Setup(m => m.Map<RatingResponse>(rating))
                .Returns(expectedResponse);

            // Act
            var result = await _ratingService.GetUserRatingForProductAsync(productId, userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedResponse);
        }


        #endregion
    }
}
