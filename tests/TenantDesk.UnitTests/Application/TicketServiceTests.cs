using AutoMapper;
using Moq;
using TenantDesk.Application.Common;
using TenantDesk.Application.Dto.Ticket;
using TenantDesk.Application.Interfaces;
using TenantDesk.Application.Interfaces.Identity;
using TenantDesk.Application.Interfaces.Persistence;
using TenantDesk.Application.Interfaces.Persistence.Filters;
using TenantDesk.Application.Interfaces.Services;
using TenantDesk.Application.Services;
using TenantDesk.Domain.Entities;
using TenantDesk.Domain.Enums;
using Xunit;

namespace TenantDesk.UnitTests.Application;

public class TicketServiceTests
{
    private readonly Mock<ITicketRepository> _mockTicketRepository;
    private readonly Mock<ICategoryRepository> _mockCategoryRepository;
    private readonly Mock<ISlaRepository> _mockSlaRepository;
    private readonly Mock<ICurrentTenantService> _mockCurrentTenantService;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<INotificationDispatcher> _mockNotificationDispatcher;
    private readonly Mock<IMapper> _mockMapper;
    private readonly TicketService _sut;
    private readonly Guid _tenantId;
    private readonly Guid _userId;

    public TicketServiceTests()
    {
        _mockTicketRepository = new Mock<ITicketRepository>();
        _mockCategoryRepository = new Mock<ICategoryRepository>();
        _mockSlaRepository = new Mock<ISlaRepository>();
        _mockCurrentTenantService = new Mock<ICurrentTenantService>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockNotificationDispatcher = new Mock<INotificationDispatcher>();
        _mockMapper = new Mock<IMapper>();

        _tenantId = Guid.NewGuid();
        _userId = Guid.NewGuid();

        _mockCurrentTenantService.Setup(s => s.GetTenantId()).Returns(_tenantId);
        _mockCurrentUserService.Setup(s => s.GetUserId()).Returns(_userId);
        _mockCurrentUserService.Setup(s => s.GetUserRole()).Returns(UserRole.Customer);

        _sut = new TicketService(
            _mockTicketRepository.Object,
            _mockCategoryRepository.Object,
            _mockSlaRepository.Object,
            _mockCurrentTenantService.Object,
            _mockCurrentUserService.Object,
            _mockNotificationDispatcher.Object,
            _mockMapper.Object);
    }

    private Ticket CreateTicket(Guid ticketId, Guid createdBy, TicketStatus status = TicketStatus.Open, Guid? assignedTo = null)
    {
        var ticket = new Ticket(_tenantId, Guid.NewGuid(), Guid.NewGuid(), createdBy, "Title", "Desc");
        typeof(Ticket).GetProperty("TicketId")!.SetValue(ticket, ticketId);
        if (assignedTo.HasValue)
        {
            ticket.AssignTo(assignedTo.Value);
        }
        if (status != TicketStatus.Open)
        {
            ticket.ChangeStatus(status);
        }
        return ticket;
    }

    [Fact]
    public async Task GetByIdAsync_CustomerOwnTicket_ReturnsTicket()
    {
        var ticketId = Guid.NewGuid();
        var ticket = CreateTicket(ticketId, _userId);
        var expectedDto = new TicketResponseDto(ticketId, _tenantId, ticket.CategoryId, ticket.SlaId, _userId, null, "Title", "Desc", TicketStatus.Open, DateTime.UtcNow, DateTime.UtcNow);

        _mockTicketRepository.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(ticket);
        _mockMapper.Setup(m => m.Map<TicketResponseDto>(ticket)).Returns(expectedDto);

        var result = await _sut.GetByIdAsync(ticketId);

        Assert.NotNull(result);
        Assert.Equal("Title", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_CustomerOtherTicket_ThrowsUnauthorizedAccessException()
    {
        var ticketId = Guid.NewGuid();
        var ticket = CreateTicket(ticketId, Guid.NewGuid()); // Not created by _userId

        _mockTicketRepository.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(ticket);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.GetByIdAsync(ticketId));
    }

    [Fact]
    public async Task CreateAsync_ValidInput_CreatesAndReturnsTicket()
    {
        var dto = new CreateTicketDto(Guid.NewGuid(), Guid.NewGuid(), "New Ticket", "Description");
        var category = new Category(_tenantId, "Cat", "Desc");
        var sla = new Sla(_tenantId, PriorityLevel.High, 4, 10);
        var expectedDto = new TicketResponseDto(Guid.NewGuid(), _tenantId, dto.CategoryId, dto.SlaId, _userId, null, dto.Title, dto.Description, TicketStatus.Open, DateTime.UtcNow, DateTime.UtcNow);

        _mockCategoryRepository.Setup(r => r.GetByIdAsync(dto.CategoryId)).ReturnsAsync(category);
        _mockSlaRepository.Setup(r => r.GetByIdAsync(dto.SlaId)).ReturnsAsync(sla);
        _mockMapper.Setup(m => m.Map<TicketResponseDto>(It.IsAny<Ticket>())).Returns(expectedDto);

        var result = await _sut.CreateAsync(dto);

        Assert.NotNull(result);
        Assert.Equal("New Ticket", result.Title);
        _mockTicketRepository.Verify(r => r.AddAsync(It.IsAny<Ticket>()), Times.Once);
    }

    [Fact]
    public async Task UpdateDetailsAsync_AdminUpdatesTicket_UpdatesSuccessfully()
    {
        var ticketId = Guid.NewGuid();
        var ticket = CreateTicket(ticketId, Guid.NewGuid());
        var dto = new UpdateTicketDto("Updated Title", "Updated Desc");
        var expectedDto = new TicketResponseDto(ticketId, _tenantId, ticket.CategoryId, ticket.SlaId, ticket.CreatedBy, null, dto.Title, dto.Description, TicketStatus.Open, DateTime.UtcNow, DateTime.UtcNow);

        _mockCurrentUserService.Setup(s => s.GetUserRole()).Returns(UserRole.Admin); // Admin can update any
        _mockTicketRepository.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(ticket);
        _mockMapper.Setup(m => m.Map<TicketResponseDto>(ticket)).Returns(expectedDto);

        var result = await _sut.UpdateDetailsAsync(ticketId, dto);

        Assert.NotNull(result);
        Assert.Equal("Updated Title", result.Title);
        _mockTicketRepository.Verify(r => r.UpdateAsync(ticket), Times.Once);
        _mockNotificationDispatcher.Verify(d => d.DispatchAsync(ticket.CreatedBy, NotificationType.TicketUpdated, It.IsAny<string>(), ticketId), Times.Once);
    }

    [Fact]
    public async Task UpdateDetailsAsync_Customer_ThrowsUnauthorizedAccessException()
    {
        var ticketId = Guid.NewGuid();
        var ticket = CreateTicket(ticketId, _userId);
        var dto = new UpdateTicketDto("Updated Title", "Updated Desc");

        // Customer role already setup in constructor
        _mockTicketRepository.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(ticket);

        // UpdateDetailsAsync has agentOrAdminOnly = true
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.UpdateDetailsAsync(ticketId, dto));
    }

    [Fact]
    public async Task CloseAsync_CustomerClosesOwnTicket_Success()
    {
        var ticketId = Guid.NewGuid();
        var ticket = CreateTicket(ticketId, _userId); // Created by customer
        var expectedDto = new TicketResponseDto(ticketId, _tenantId, ticket.CategoryId, ticket.SlaId, ticket.CreatedBy, null, ticket.Title, ticket.Description, TicketStatus.Closed, DateTime.UtcNow, DateTime.UtcNow);

        _mockTicketRepository.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(ticket);
        _mockMapper.Setup(m => m.Map<TicketResponseDto>(ticket)).Returns(expectedDto);

        var result = await _sut.CloseAsync(ticketId);

        Assert.Equal(TicketStatus.Closed, ticket.Status);
        _mockTicketRepository.Verify(r => r.UpdateAsync(ticket), Times.Once);
    }
}
