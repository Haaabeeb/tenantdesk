using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using TenantDesk.Application.Dto.BusinessHour;
using TenantDesk.Application.Interfaces;
using TenantDesk.Application.Interfaces.Identity;
using TenantDesk.Application.Services;
using TenantDesk.Domain.Entities;
using TenantDesk.UnitTests.Helpers;
using Xunit;

namespace TenantDesk.UnitTests.Application;

public class BusinessHourServiceTests
{
    private readonly Mock<IBusinessHourRepository> _businessHourRepoMock;
    private readonly Mock<ICurrentTenantService> _currentTenantServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly BusinessHourService _sut;
    private readonly Guid _currentTenantId;

    public BusinessHourServiceTests()
    {
        _businessHourRepoMock = new Mock<IBusinessHourRepository>();
        _currentTenantServiceMock = new Mock<ICurrentTenantService>();
        _mapperMock = new Mock<IMapper>();

        _currentTenantId = Guid.NewGuid();
        _currentTenantServiceMock.Setup(c => c.GetTenantId()).Returns(_currentTenantId);

        _sut = new BusinessHourService(
            _businessHourRepoMock.Object,
            _currentTenantServiceMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WhenDayAlreadyConfigured_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new CreateBusinessHourDto(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(17, 0));
        var existing = new BusinessHour(_currentTenantId, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(17, 0));

        _businessHourRepoMock.Setup(r => r.GetByDayAsync(_currentTenantId, dto.DayOfWeek))
            .ReturnsAsync(existing);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateAsync(dto));
        Assert.Contains("already exists", ex.Message);
        _businessHourRepoMock.Verify(r => r.AddAsync(It.IsAny<BusinessHour>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithValidDto_CreatesRecordAndReturnsDto()
    {
        // Arrange
        var dto = new CreateBusinessHourDto(DayOfWeek.Tuesday, new TimeOnly(8, 0), new TimeOnly(16, 0));

        _businessHourRepoMock.Setup(r => r.GetByDayAsync(_currentTenantId, dto.DayOfWeek))
            .ReturnsAsync((BusinessHour?)null);

        var expectedDto = new BusinessHourResponseDto(Guid.NewGuid(), dto.DayOfWeek, dto.OpenTime, dto.CloseTime, true);
        _mapperMock.Setup(m => m.Map<BusinessHourResponseDto>(It.IsAny<BusinessHour>()))
            .Returns(expectedDto);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.DayOfWeek, result.DayOfWeek);
        _businessHourRepoMock.Verify(r => r.AddAsync(It.Is<BusinessHour>(b => b.TenantId == _currentTenantId && b.DayOfWeek == dto.DayOfWeek)), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _businessHourRepoMock.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((BusinessHour?)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetByIdAsync(id));
        Assert.Equal("Business hour record not found.", ex.Message);
    }

    [Fact]
    public async Task GetByIdAsync_WhenFromDifferentTenant_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var otherRecord = new BusinessHour(Guid.NewGuid(), DayOfWeek.Wednesday, new TimeOnly(9, 0), new TimeOnly(17, 0));
        TestEntityHelper.SetProperty(otherRecord, "BusinessHourId", id);

        _businessHourRepoMock.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(otherRecord);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.GetByIdAsync(id));
        Assert.Equal("You cannot access resources belonging to another tenant.", ex.Message);
    }

    [Fact]
    public async Task GetByIdAsync_WhenFoundAndOwned_ReturnsMappedDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var record = new BusinessHour(_currentTenantId, DayOfWeek.Thursday, new TimeOnly(9, 0), new TimeOnly(17, 0));
        TestEntityHelper.SetProperty(record, "BusinessHourId", id);

        _businessHourRepoMock.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(record);

        var expectedDto = new BusinessHourResponseDto(id, DayOfWeek.Thursday, new TimeOnly(9, 0), new TimeOnly(17, 0), true);
        _mapperMock.Setup(m => m.Map<BusinessHourResponseDto>(record))
            .Returns(expectedDto);

        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.BusinessHourId);
    }

    [Fact]
    public async Task GetByDayAsync_WhenNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        _businessHourRepoMock.Setup(r => r.GetByDayAsync(_currentTenantId, DayOfWeek.Sunday))
            .ReturnsAsync((BusinessHour?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetByDayAsync(DayOfWeek.Sunday));
    }

    [Fact]
    public async Task GetByDayAsync_WhenFound_ReturnsMappedDto()
    {
        // Arrange
        var record = new BusinessHour(_currentTenantId, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(17, 0));
        _businessHourRepoMock.Setup(r => r.GetByDayAsync(_currentTenantId, DayOfWeek.Monday))
            .ReturnsAsync(record);

        var expectedDto = new BusinessHourResponseDto(Guid.NewGuid(), DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(17, 0), true);
        _mapperMock.Setup(m => m.Map<BusinessHourResponseDto>(record))
            .Returns(expectedDto);

        // Act
        var result = await _sut.GetByDayAsync(DayOfWeek.Monday);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(DayOfWeek.Monday, result.DayOfWeek);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedBusinessHourList()
    {
        // Arrange
        var records = new List<BusinessHour>
        {
            new BusinessHour(_currentTenantId, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(17, 0)),
            new BusinessHour(_currentTenantId, DayOfWeek.Tuesday, new TimeOnly(9, 0), new TimeOnly(17, 0))
        };

        var dtos = records.Select(b => new BusinessHourResponseDto(Guid.NewGuid(), b.DayOfWeek, b.OpenTime, b.CloseTime, b.IsWorkingDay)).ToList();

        _businessHourRepoMock.Setup(r => r.GetByTenantAsync(_currentTenantId))
            .ReturnsAsync(records);

        _mapperMock.Setup(m => m.Map<IEnumerable<BusinessHourResponseDto>>(records))
            .Returns(dtos);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
        _businessHourRepoMock.Verify(r => r.GetByTenantAsync(_currentTenantId), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenRecordFromDifferentTenant_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var otherRecord = new BusinessHour(Guid.NewGuid(), DayOfWeek.Friday, new TimeOnly(9, 0), new TimeOnly(17, 0));
        var dto = new UpdateBusinessHourDto(new TimeOnly(8, 0), new TimeOnly(16, 0));

        _businessHourRepoMock.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(otherRecord);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.UpdateAsync(id, dto));
        _businessHourRepoMock.Verify(r => r.UpdateAsync(It.IsAny<BusinessHour>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithValidHours_UpdatesHoursAndPersists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var record = new BusinessHour(_currentTenantId, DayOfWeek.Friday, new TimeOnly(9, 0), new TimeOnly(17, 0));
        TestEntityHelper.SetProperty(record, "BusinessHourId", id);

        var newOpen = new TimeOnly(8, 30);
        var newClose = new TimeOnly(16, 30);
        var dto = new UpdateBusinessHourDto(newOpen, newClose);

        _businessHourRepoMock.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(record);

        var expectedDto = new BusinessHourResponseDto(id, DayOfWeek.Friday, newOpen, newClose, true);
        _mapperMock.Setup(m => m.Map<BusinessHourResponseDto>(record))
            .Returns(expectedDto);

        // Act
        var result = await _sut.UpdateAsync(id, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(newOpen, record.OpenTime);
        Assert.Equal(newClose, record.CloseTime);
        _businessHourRepoMock.Verify(r => r.UpdateAsync(record), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenRecordFromDifferentTenant_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var otherRecord = new BusinessHour(Guid.NewGuid(), DayOfWeek.Monday, null, null);

        _businessHourRepoMock.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(otherRecord);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.DeleteAsync(id));
        _businessHourRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenFoundAndOwned_DeletesFromRepository()
    {
        // Arrange
        var id = Guid.NewGuid();
        var record = new BusinessHour(_currentTenantId, DayOfWeek.Monday, null, null);

        _businessHourRepoMock.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(record);

        // Act
        await _sut.DeleteAsync(id);

        // Assert
        _businessHourRepoMock.Verify(r => r.DeleteAsync(id), Times.Once);
    }
}
