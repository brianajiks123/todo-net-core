using AutoMapper;
using Moq;
using TodoApi.Web.Features.Todos;
using Xunit;

namespace TodoApi.Web.Tests;

public class TodoServiceTests
{
    private readonly TodoService _service;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly Mock<IMapper> _mockMapper = new();

    public TodoServiceTests()
    {
        _service = new TodoService(_mockUnitOfWork.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task CreateV2_Should_Create_Todo_And_Return_ResponseDto()
    {
        // Arrange
        var dto = new CreateTodoDtoV2("Belajar xUnit Testing", DateTimeOffset.UtcNow.AddDays(1));
        var userId = 1;

        var todoEntity = new Todo
        {
            Id = 999,
            Title = dto.Title,
            UserId = userId,
            CreatedBy = userId,
            UpdatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsCompleted = false
        };

        var expectedResponse = new TodoResponseDto(
            999, dto.Title, false, DateTime.UtcNow, userId, userId, DateTime.UtcNow);

        _mockMapper.Setup(m => m.Map<Todo>(It.IsAny<CreateTodoDtoV2>()))
                   .Returns(todoEntity);

        _mockMapper.Setup(m => m.Map<TodoResponseDto>(It.IsAny<Todo>()))
                   .Returns(expectedResponse);

        _mockUnitOfWork.Setup(u => u.Todos.AddAsync(It.IsAny<Todo>()))
                       .Verifiable();

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(default))
                       .ReturnsAsync(1);

        // Act
        var result = await _service.CreateV2(dto, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Title, result.Title);
        Assert.Equal(userId, result.CreatedBy);

        _mockUnitOfWork.Verify(u => u.Todos.AddAsync(It.IsAny<Todo>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task GetById_Should_Return_Null_When_Todo_Not_Found()
    {
        // Arrange
        _mockUnitOfWork.Setup(u => u.Todos.GetByIdAsync(999, 1))
                      .ReturnsAsync((Todo?)null);

        // Act
        var result = await _service.GetById(999, 1);

        // Assert
        Assert.Null(result);
    }
}
