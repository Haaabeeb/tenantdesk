using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using TenantDesk.Application.Interfaces;
using TenantDesk.Application.Interfaces.Services;
using TenantDesk.Application.Services;
using TenantDesk.Domain.Entities;
using TenantDesk.Domain.Enums;
using TenantDesk.UnitTests.Helpers;
using Xunit;

namespace TenantDesk.UnitTests.Application;

public class SlaCalculatorServiceTests
{
    private readonly Mock<ITicketRepository> _ticketRepoMock;
    private readonly Mock<ISlaRepository> _slaRepoMock;
    private readonly Mock<IBusinessHourRepository> _businessHourRepoMock;
    private readonly Mock<IBusinessTimeCalculator> _businessTimeCalcMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly SlaCalculatorService _sut;

    public SlaCalculatorServiceTests()
    {
        _ticketRepoMock = new Mock<ITicketRepository>();
        _slaRepoMock = new Mock<ISlaRepository>();
        _businessHourRepoMock = new Mock<IBusinessHourRepository>();
        _businessTimeCalcMock = new Mock<IBusinessTimeCalculator>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();

        _sut = new SlaCalculatorService(
            _ticketRepoMock.Object,
            _slaRepoMock.Object,
            _businessHourRepoMock.Object,
            _businessTimeCalcMock.Object,
            _dateTimeProviderMock.Object);
    }

    [Fact]
    public async Task CalculateResponseDueAsync_WhenTicketNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        _ticketRepoMock.Setup(r => r.GetByIdAsync(ticketId))
            .ReturnsAsync((Ticket?)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.CalculateResponseDueAsync(ticketId));
        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public async Task CalculateResponseDueAsync_WhenSlaNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var slaId = Guid.NewGuid();
        var ticket = new Ticket(tenantId, Guid.NewGuid(), slaId, Guid.NewGuid(), "Test Ticket", null);

        _ticketRepoMock.Setup(r => r.GetByIdAsync(ticket.TicketId))
            .ReturnsAsync(ticket);

        _slaRepoMock.Setup(r => r.GetByIdAsync(slaId))
            .ReturnsAsync((Sla?)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.CalculateResponseDueAsync(ticket.TicketId));
        Assert.Contains("was not found", ex.Message);
    }

    [Fact]
    public async Task CalculateResponseDueAsync_WhenSlaBelongsToDifferentTenant_ThrowsInvalidOperationException()
    {
        // Arrange
        var ticketTenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        var slaId = Guid.NewGuid();
        var ticket = new Ticket(ticketTenantId, Guid.NewGuid(), slaId, Guid.NewGuid(), "Ticket", null);
        var sla = new Sla(otherTenantId, PriorityLevel.High, 2, 8);
        TestEntityHelper.SetProperty(sla, "SlaId", slaId);

        _ticketRepoMock.Setup(r => r.GetByIdAsync(ticket.TicketId))
            .ReturnsAsync(ticket);

        _slaRepoMock.Setup(r => r.GetByIdAsync(slaId))
            .ReturnsAsync(sla);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CalculateResponseDueAsync(ticket.TicketId));
        Assert.Contains("does not belong to the same tenant", ex.Message);
    }

    [Fact]
    public async Task CalculateResponseDueAsync_WithValidData_CalculatesDueDateViaBusinessTimeCalculator()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var slaId = Guid.NewGuid();
        var ticket = new Ticket(tenantId, Guid.NewGuid(), slaId, Guid.NewGuid(), "Ticket", null);
        var sla = new Sla(tenantId, PriorityLevel.High, 4, 12);
        TestEntityHelper.SetProperty(sla, "SlaId", slaId);

        var businessHours = new List<BusinessHour>();
        var expectedDue = ticket.CreatedAt.AddHours(4);

        _ticketRepoMock.Setup(r => r.GetByIdAsync(ticket.TicketId))
            .ReturnsAsync(ticket);

        _slaRepoMock.Setup(r => r.GetByIdAsync(slaId))
            .ReturnsAsync(sla);

        _businessHourRepoMock.Setup(r => r.GetByTenantAsync(tenantId))
            .ReturnsAsync(businessHours);

        _businessTimeCalcMock.Setup(c => c.CalculateDueDate(ticket.CreatedAt, sla.ResponseTime, businessHours))
            .Returns(expectedDue);

        // Act
        var result = await _sut.CalculateResponseDueAsync(ticket.TicketId);

        // Assert
        Assert.Equal(expectedDue, result);
        _businessTimeCalcMock.Verify(c => c.CalculateDueDate(ticket.CreatedAt, 4, businessHours), Times.Once);
    }

    [Fact]
    public async Task CalculateResolutionDueAsync_WithValidData_CalculatesDueDateUsingResolutionTime()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var slaId = Guid.NewGuid();
        var ticket = new Ticket(tenantId, Guid.NewGuid(), slaId, Guid.NewGuid(), "Ticket", null);
        var sla = new Sla(tenantId, PriorityLevel.Medium, 4, 16);
        TestEntityHelper.SetProperty(sla, "SlaId", slaId);

        var businessHours = new List<BusinessHour>();
        var expectedDue = ticket.CreatedAt.AddHours(16);

        _ticketRepoMock.Setup(r => r.GetByIdAsync(ticket.TicketId))
            .ReturnsAsync(ticket);

        _slaRepoMock.Setup(r => r.GetByIdAsync(slaId))
            .ReturnsAsync(sla);

        _businessHourRepoMock.Setup(r => r.GetByTenantAsync(tenantId))
            .ReturnsAsync(businessHours);

        _businessTimeCalcMock.Setup(c => c.CalculateDueDate(ticket.CreatedAt, sla.ResolutionTime, businessHours))
            .Returns(expectedDue);

        // Act
        var result = await _sut.CalculateResolutionDueAsync(ticket.TicketId);

        // Assert
        Assert.Equal(expectedDue, result);
        _businessTimeCalcMock.Verify(c => c.CalculateDueDate(ticket.CreatedAt, 16, businessHours), Times.Once);
    }

    [Fact]
    public async Task IsBreachedAsync_WhenTicketNotClosedAndCurrentTimeExceedsResolutionDue_ReturnsTrue()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var slaId = Guid.NewGuid();
        var ticket = new Ticket(tenantId, Guid.NewGuid(), slaId, Guid.NewGuid(), "Ticket", null);
        var sla = new Sla(tenantId, PriorityLevel.Critical, 1, 4);

        var resolutionDue = DateTime.UtcNow.AddHours(-1); // Resolution was 1 hour ago
        var currentTime = DateTime.UtcNow;

        _ticketRepoMock.Setup(r => r.GetByIdAsync(ticket.TicketId)).ReturnsAsync(ticket);
        _slaRepoMock.Setup(r => r.GetByIdAsync(slaId)).ReturnsAsync(sla);
        _businessHourRepoMock.Setup(r => r.GetByTenantAsync(tenantId)).ReturnsAsync(new List<BusinessHour>());
        _businessTimeCalcMock.Setup(c => c.CalculateDueDate(ticket.CreatedAt, sla.ResolutionTime, It.IsAny<IEnumerable<BusinessHour>>()))
            .Returns(resolutionDue);

        _dateTimeProviderMock.Setup(d => d.UtcNow).Returns(currentTime);

        // Act
        var isBreached = await _sut.IsBreachedAsync(ticket.TicketId);

        // Assert
        Assert.True(isBreached);
    }

    [Fact]
    public async Task IsBreachedAsync_WhenTicketClosedAndUpdatedAtBeforeResolutionDue_ReturnsFalse()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var slaId = Guid.NewGuid();
        var ticket = new Ticket(tenantId, Guid.NewGuid(), slaId, Guid.NewGuid(), "Ticket", null);
        ticket.Close();

        var closedTime = DateTime.UtcNow.AddHours(-2);
        TestEntityHelper.SetProperty(ticket, "UpdatedAt", closedTime);

        var sla = new Sla(tenantId, PriorityLevel.Low, 24, 48);
        var resolutionDue = DateTime.UtcNow.AddHours(-1); // Resolution was 1 hour ago, but closed 2 hours ago!

        _ticketRepoMock.Setup(r => r.GetByIdAsync(ticket.TicketId)).ReturnsAsync(ticket);
        _slaRepoMock.Setup(r => r.GetByIdAsync(slaId)).ReturnsAsync(sla);
        _businessHourRepoMock.Setup(r => r.GetByTenantAsync(tenantId)).ReturnsAsync(new List<BusinessHour>());
        _businessTimeCalcMock.Setup(c => c.CalculateDueDate(ticket.CreatedAt, sla.ResolutionTime, It.IsAny<IEnumerable<BusinessHour>>()))
            .Returns(resolutionDue);

        // Act
        var isBreached = await _sut.IsBreachedAsync(ticket.TicketId);

        // Assert
        Assert.False(isBreached); // closedTime < resolutionDue
    }

    [Fact]
    public async Task IsResponseBreachedAsync_WhenTicketOpenAndCurrentTimePastResponseDue_ReturnsTrue()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var slaId = Guid.NewGuid();
        var ticket = new Ticket(tenantId, Guid.NewGuid(), slaId, Guid.NewGuid(), "Ticket", null);
        Assert.Equal(TicketStatus.Open, ticket.Status);

        var sla = new Sla(tenantId, PriorityLevel.High, 2, 8);
        var responseDue = DateTime.UtcNow.AddMinutes(-30);
        var currentTime = DateTime.UtcNow;

        _ticketRepoMock.Setup(r => r.GetByIdAsync(ticket.TicketId)).ReturnsAsync(ticket);
        _slaRepoMock.Setup(r => r.GetByIdAsync(slaId)).ReturnsAsync(sla);
        _businessHourRepoMock.Setup(r => r.GetByTenantAsync(tenantId)).ReturnsAsync(new List<BusinessHour>());
        _businessTimeCalcMock.Setup(c => c.CalculateDueDate(ticket.CreatedAt, sla.ResponseTime, It.IsAny<IEnumerable<BusinessHour>>()))
            .Returns(responseDue);

        _dateTimeProviderMock.Setup(d => d.UtcNow).Returns(currentTime);

        // Act
        var isBreached = await _sut.IsResponseBreachedAsync(ticket.TicketId);

        // Assert
        Assert.True(isBreached);
    }

    [Fact]
    public async Task IsResponseBreachedAsync_WhenTicketAssignedAndUpdatedAtBeforeResponseDue_ReturnsFalse()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var slaId = Guid.NewGuid();
        var ticket = new Ticket(tenantId, Guid.NewGuid(), slaId, Guid.NewGuid(), "Ticket", null);
        ticket.AssignTo(Guid.NewGuid()); // Status changed from Open to Assigned

        var respondedTime = DateTime.UtcNow.AddHours(-1);
        TestEntityHelper.SetProperty(ticket, "UpdatedAt", respondedTime);

        var sla = new Sla(tenantId, PriorityLevel.High, 2, 8);
        var responseDue = DateTime.UtcNow; // Due later than responded time

        _ticketRepoMock.Setup(r => r.GetByIdAsync(ticket.TicketId)).ReturnsAsync(ticket);
        _slaRepoMock.Setup(r => r.GetByIdAsync(slaId)).ReturnsAsync(sla);
        _businessHourRepoMock.Setup(r => r.GetByTenantAsync(tenantId)).ReturnsAsync(new List<BusinessHour>());
        _businessTimeCalcMock.Setup(c => c.CalculateDueDate(ticket.CreatedAt, sla.ResponseTime, It.IsAny<IEnumerable<BusinessHour>>()))
            .Returns(responseDue);

        // Act
        var isBreached = await _sut.IsResponseBreachedAsync(ticket.TicketId);

        // Assert
        Assert.False(isBreached);
    }
}
