using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using TenantDesk.Application.Dto.Dashboard;
using TenantDesk.Application.Interfaces;
using TenantDesk.Application.Interfaces.Identity;
using TenantDesk.Application.Services;
using TenantDesk.Domain.Enums;
using Xunit;

namespace TenantDesk.UnitTests.Application;

public class DashboardServiceTests
{
    private readonly Mock<ITicketRepository> _ticketRepoMock;
    private readonly Mock<ICurrentTenantService> _currentTenantServiceMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<ILogger<DashboardService>> _loggerMock;
    private readonly DashboardService _sut;
    private readonly Guid _currentTenantId;
    private readonly string _cacheKey;

    public DashboardServiceTests()
    {
        _ticketRepoMock = new Mock<ITicketRepository>();
        _currentTenantServiceMock = new Mock<ICurrentTenantService>();
        _cacheMock = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<DashboardService>>();

        _currentTenantId = Guid.NewGuid();
        _cacheKey = $"dashboard:{_currentTenantId}";

        _currentTenantServiceMock.Setup(c => c.GetTenantId())
            .Returns(_currentTenantId);

        _sut = new DashboardService(
            _ticketRepoMock.Object,
            _currentTenantServiceMock.Object,
            _cacheMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetAsync_WhenCacheHit_ReturnsCachedDashboardWithoutQueryingRepository()
    {
        // Arrange
        var cachedDto = new DashboardResponseDto(10, 4, 3, 2, 1);
        var cachedJson = JsonSerializer.Serialize(cachedDto);
        var cachedBytes = Encoding.UTF8.GetBytes(cachedJson);

        _cacheMock.Setup(c => c.GetAsync(_cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedBytes);

        // Act
        var result = await _sut.GetAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.TotalTickets);
        Assert.Equal(4, result.OpenTickets);
        Assert.Equal(3, result.AssignedTickets);
        Assert.Equal(2, result.InProgressTickets);
        Assert.Equal(1, result.ClosedTickets);

        _ticketRepoMock.Verify(r => r.CountByTenantAsync(It.IsAny<Guid>()), Times.Never);
        _ticketRepoMock.Verify(r => r.CountByStatusAsync(It.IsAny<Guid>(), It.IsAny<TicketStatus>()), Times.Never);
    }

    [Fact]
    public async Task GetAsync_WhenCacheMiss_QueriesRepositoryCachesAndReturnsDashboard()
    {
        // Arrange
        _cacheMock.Setup(c => c.GetAsync(_cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        _ticketRepoMock.Setup(r => r.CountByTenantAsync(_currentTenantId)).ReturnsAsync(20);
        _ticketRepoMock.Setup(r => r.CountByStatusAsync(_currentTenantId, TicketStatus.Open)).ReturnsAsync(8);
        _ticketRepoMock.Setup(r => r.CountByStatusAsync(_currentTenantId, TicketStatus.Assigned)).ReturnsAsync(5);
        _ticketRepoMock.Setup(r => r.CountByStatusAsync(_currentTenantId, TicketStatus.InProgress)).ReturnsAsync(4);
        _ticketRepoMock.Setup(r => r.CountByStatusAsync(_currentTenantId, TicketStatus.Closed)).ReturnsAsync(3);

        // Act
        var result = await _sut.GetAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(20, result.TotalTickets);
        Assert.Equal(8, result.OpenTickets);
        Assert.Equal(5, result.AssignedTickets);
        Assert.Equal(4, result.InProgressTickets);
        Assert.Equal(3, result.ClosedTickets);

        _ticketRepoMock.Verify(r => r.CountByTenantAsync(_currentTenantId), Times.Once);
        _ticketRepoMock.Verify(r => r.CountByStatusAsync(_currentTenantId, TicketStatus.Open), Times.Once);
        _ticketRepoMock.Verify(r => r.CountByStatusAsync(_currentTenantId, TicketStatus.Assigned), Times.Once);
        _ticketRepoMock.Verify(r => r.CountByStatusAsync(_currentTenantId, TicketStatus.InProgress), Times.Once);
        _ticketRepoMock.Verify(r => r.CountByStatusAsync(_currentTenantId, TicketStatus.Closed), Times.Once);

        _cacheMock.Verify(c => c.SetAsync(
            _cacheKey,
            It.IsAny<byte[]>(),
            It.Is<DistributedCacheEntryOptions>(o => o.SlidingExpiration == TimeSpan.FromMinutes(5)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_WhenCacheMissInitially_ButCacheHitAfterAcquiringLock_DoesNotQueryRepository()
    {
        // Arrange
        var cachedDto = new DashboardResponseDto(15, 5, 5, 3, 2);
        var cachedJson = JsonSerializer.Serialize(cachedDto);
        var cachedBytes = Encoding.UTF8.GetBytes(cachedJson);

        var firstCall = true;
        _cacheMock.Setup(c => c.GetAsync(_cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                if (firstCall)
                {
                    firstCall = false;
                    return null; // First check (before lock): MISS
                }
                return cachedBytes; // Double check (after lock): HIT
            });

        // Act
        var result = await _sut.GetAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(15, result.TotalTickets);
        _ticketRepoMock.Verify(r => r.CountByTenantAsync(It.IsAny<Guid>()), Times.Never);
        _ticketRepoMock.Verify(r => r.CountByStatusAsync(It.IsAny<Guid>(), It.IsAny<TicketStatus>()), Times.Never);
    }

    [Fact]
    public async Task GetAsync_ConcurrentCalls_ExecuteSafelyUnderLock()
    {
        // Arrange
        _cacheMock.Setup(c => c.GetAsync(_cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        _ticketRepoMock.Setup(r => r.CountByTenantAsync(_currentTenantId)).ReturnsAsync(12);
        _ticketRepoMock.Setup(r => r.CountByStatusAsync(_currentTenantId, It.IsAny<TicketStatus>())).ReturnsAsync(3);

        // Act
        var task1 = Task.Run(() => _sut.GetAsync());
        var task2 = Task.Run(() => _sut.GetAsync());

        var results = await Task.WhenAll(task1, task2);

        // Assert
        Assert.NotNull(results[0]);
        Assert.NotNull(results[1]);
        Assert.Equal(12, results[0].TotalTickets);
        Assert.Equal(12, results[1].TotalTickets);
    }
}
