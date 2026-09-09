using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using TenantDesk.Application.Dto.Plan;
using TenantDesk.Application.Interfaces;
using TenantDesk.Application.Services;
using TenantDesk.Domain.Entities;
using TenantDesk.UnitTests.Helpers;
using Xunit;

namespace TenantDesk.UnitTests.Application;

public class PlanServiceTests
{
    private readonly Mock<IPlanRepository> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly PlanService _sut;

    public PlanServiceTests()
    {
        _repositoryMock = new Mock<IPlanRepository>();
        _mapperMock = new Mock<IMapper>();

        _sut = new PlanService(
            _repositoryMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenPlanFound_ReturnsMappedDto()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var plan = new Plan("Gold Plan", 500);
        TestEntityHelper.SetProperty(plan, "PlanId", planId);

        var expectedDto = new PlanResponseDto(planId, "Gold Plan", 500, plan.CreatedAt);

        _repositoryMock.Setup(r => r.GetByIdAsync(planId))
            .ReturnsAsync(plan);

        _mapperMock.Setup(m => m.Map<PlanResponseDto>(plan))
            .Returns(expectedDto);

        // Act
        var result = await _sut.GetByIdAsync(planId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto.PlanId, result.PlanId);
        Assert.Equal(expectedDto.Name, result.Name);
        _repositoryMock.Verify(r => r.GetByIdAsync(planId), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFoundInRepo_ThrowsKeyNotFoundException()
    {
        // Arrange
        var planId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(planId))
            .ReturnsAsync((Plan?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetByIdAsync(planId));
    }

    [Fact]
    public async Task GetByNameAsync_WhenFoundInRepo_ReturnsMappedDto()
    {
        // Arrange
        const string planName = "Bronze";
        var plan = new Plan(planName, 100);
        var expectedDto = new PlanResponseDto(Guid.NewGuid(), planName, 100, plan.CreatedAt);

        _repositoryMock.Setup(r => r.GetByNameAsync(planName))
            .ReturnsAsync(plan);

        _mapperMock.Setup(m => m.Map<PlanResponseDto>(plan))
            .Returns(expectedDto);

        // Act
        var result = await _sut.GetByNameAsync(planName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(planName, result.Name);
        _repositoryMock.Verify(r => r.GetByNameAsync(planName), Times.Once);
    }

    [Fact]
    public async Task GetByNameAsync_WhenNotFoundInRepo_ThrowsKeyNotFoundException()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByNameAsync("NonExistent"))
            .ReturnsAsync((Plan?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetByNameAsync("NonExistent"));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedDtoList()
    {
        // Arrange
        var plans = new List<Plan> { new Plan("Plan A", 100), new Plan("Plan B", 200) };
        var dtos = plans.Select(p => new PlanResponseDto(Guid.NewGuid(), p.Name, p.RateLimitPerMinute, p.CreatedAt)).ToList();

        _repositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(plans);

        _mapperMock.Setup(m => m.Map<IEnumerable<PlanResponseDto>>(plans))
            .Returns(dtos);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
        _repositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenPlanNameAlreadyExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new CreatePlanDto("Existing Plan", 100);
        var existingPlan = new Plan("Existing Plan", 100);

        _repositoryMock.Setup(r => r.GetByNameAsync(dto.Name))
            .ReturnsAsync(existingPlan);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateAsync(dto));
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Plan>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithValidDto_CreatesPlanAndReturnsDto()
    {
        // Arrange
        var dto = new CreatePlanDto("New Unique Plan", 150);
        _repositoryMock.Setup(r => r.GetByNameAsync(dto.Name))
            .ReturnsAsync((Plan?)null);

        var expectedDto = new PlanResponseDto(Guid.NewGuid(), dto.Name, dto.RateLimitPerMinute, DateTime.UtcNow);
        _mapperMock.Setup(m => m.Map<PlanResponseDto>(It.IsAny<Plan>()))
            .Returns(expectedDto);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Name, result.Name);
        _repositoryMock.Verify(r => r.AddAsync(It.Is<Plan>(p => p.Name == dto.Name && p.RateLimitPerMinute == dto.RateLimitPerMinute)), Times.Once);
    }

    [Fact]
    public async Task ChangeRateLimitAsync_WhenPlanNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var dto = new ChangePlanRateLimitDto(200);

        _repositoryMock.Setup(r => r.GetByIdAsync(planId))
            .ReturnsAsync((Plan?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.ChangeRateLimitAsync(planId, dto));
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Plan>()), Times.Never);
    }

    [Fact]
    public async Task ChangeRateLimitAsync_WhenFound_UpdatesRateLimitAndReturnsDto()
    {
        // Arrange
        var planId = Guid.NewGuid();
        const string planName = "Enterprise";
        var plan = new Plan(planName, 100);
        TestEntityHelper.SetProperty(plan, "PlanId", planId);

        var dto = new ChangePlanRateLimitDto(500);

        _repositoryMock.Setup(r => r.GetByIdAsync(planId))
            .ReturnsAsync(plan);

        var expectedDto = new PlanResponseDto(planId, planName, 500, plan.CreatedAt);
        _mapperMock.Setup(m => m.Map<PlanResponseDto>(plan))
            .Returns(expectedDto);

        // Act
        var result = await _sut.ChangeRateLimitAsync(planId, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(500, plan.RateLimitPerMinute);
        _repositoryMock.Verify(r => r.UpdateAsync(plan), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenPlanNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var planId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(planId))
            .ReturnsAsync((Plan?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.DeleteAsync(planId));
        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenPlanFound_DeletesFromRepository()
    {
        // Arrange
        var planId = Guid.NewGuid();
        const string planName = "To Delete";
        var plan = new Plan(planName, 100);
        TestEntityHelper.SetProperty(plan, "PlanId", planId);

        _repositoryMock.Setup(r => r.GetByIdAsync(planId))
            .ReturnsAsync(plan);

        // Act
        await _sut.DeleteAsync(planId);

        // Assert
        _repositoryMock.Verify(r => r.DeleteAsync(planId), Times.Once);
    }
}
