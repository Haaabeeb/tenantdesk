using AutoMapper;
using Moq;
using TenantDesk.Application.Dto.TicketFeedback;
using TenantDesk.Application.Interfaces.Identity;
using TenantDesk.Application.Interfaces.Persistence;
using TenantDesk.Application.Services;
using TenantDesk.Domain.Entities;
using Xunit;

namespace TenantDesk.UnitTests.Application;

public class TicketFeedbackTests
{
    private readonly Mock<ITicketFeedbackRepository> _mockFeedbackRepository;
    private readonly Mock<ITicketRepository> _mockTicketRepository;
    private readonly Mock<ICurrentTenantService> _mockCurrentTenantService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly TicketFeedbackService _sut;
    private readonly Guid _tenantId;

    public TicketFeedbackTests()
    {
        _mockFeedbackRepository = new Mock<ITicketFeedbackRepository>();
        _mockTicketRepository = new Mock<ITicketRepository>();
        _mockCurrentTenantService = new Mock<ICurrentTenantService>();
        _mockMapper = new Mock<IMapper>();

        _tenantId = Guid.NewGuid();
        _mockCurrentTenantService.Setup(s => s.GetTenantId()).Returns(_tenantId);

        _sut = new TicketFeedbackService(
            _mockFeedbackRepository.Object,
            _mockTicketRepository.Object,
            _mockCurrentTenantService.Object,
            _mockMapper.Object);
    }

    private Ticket CreateValidTicket(Guid ticketId, Guid tenantId)
    {
        var categoryId = Guid.NewGuid();
        var reportedBy = Guid.NewGuid();
        var ticket = new Ticket(tenantId, categoryId, Guid.NewGuid(), reportedBy, "Issue", "Description");
        typeof(Ticket).GetProperty("TicketId")!.SetValue(ticket, ticketId);
        return ticket;
    }

    [Fact]
    public async Task SubmitFeedbackAsync_ValidFeedback_CreatesAndReturnsFeedback()
    {
        var ticketId = Guid.NewGuid();
        var ticket = CreateValidTicket(ticketId, _tenantId);
        var dto = new CreateTicketFeedbackDto(5, "Great!");
        var expectedDto = new TicketFeedbackResponseDto(Guid.NewGuid(), ticketId, 5, "Great!", DateTime.UtcNow);

        _mockTicketRepository.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(ticket);
        _mockFeedbackRepository.Setup(r => r.GetByTicketAsync(ticketId)).ReturnsAsync((TicketFeedback?)null);
        _mockMapper.Setup(m => m.Map<TicketFeedbackResponseDto>(It.IsAny<TicketFeedback>())).Returns(expectedDto);

        var result = await _sut.SubmitFeedbackAsync(ticketId, dto);

        Assert.NotNull(result);
        Assert.Equal(5, result.Rating);
        _mockFeedbackRepository.Verify(r => r.AddAsync(It.Is<TicketFeedback>(f => f.TicketId == ticketId && f.Rating == 5)), Times.Once);
    }

    [Fact]
    public async Task SubmitFeedbackAsync_TicketNotFound_ThrowsKeyNotFoundException()
    {
        var ticketId = Guid.NewGuid();
        var dto = new CreateTicketFeedbackDto(5, "Great!");
        _mockTicketRepository.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync((Ticket?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.SubmitFeedbackAsync(ticketId, dto));
    }

    [Fact]
    public async Task SubmitFeedbackAsync_TicketOtherTenant_ThrowsUnauthorizedAccessException()
    {
        var ticketId = Guid.NewGuid();
        var ticket = CreateValidTicket(ticketId, Guid.NewGuid());
        var dto = new CreateTicketFeedbackDto(5, "Great!");
        
        _mockTicketRepository.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(ticket);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.SubmitFeedbackAsync(ticketId, dto));
    }

    [Fact]
    public async Task SubmitFeedbackAsync_FeedbackExists_ThrowsInvalidOperationException()
    {
        var ticketId = Guid.NewGuid();
        var ticket = CreateValidTicket(ticketId, _tenantId);
        var existingFeedback = new TicketFeedback(_tenantId, ticketId, 3, "Ok");
        var dto = new CreateTicketFeedbackDto(5, "Great!");
        
        _mockTicketRepository.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(ticket);
        _mockFeedbackRepository.Setup(r => r.GetByTicketAsync(ticketId)).ReturnsAsync(existingFeedback);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.SubmitFeedbackAsync(ticketId, dto));
    }

    [Fact]
    public async Task GetFeedbackByTicketIdAsync_FeedbackExists_ReturnsFeedback()
    {
        var ticketId = Guid.NewGuid();
        var ticket = CreateValidTicket(ticketId, _tenantId);
        var feedback = new TicketFeedback(_tenantId, ticketId, 4, "Good");
        var expectedDto = new TicketFeedbackResponseDto(Guid.NewGuid(), ticketId, 4, "Good", DateTime.UtcNow);

        _mockTicketRepository.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(ticket);
        _mockFeedbackRepository.Setup(r => r.GetByTicketAsync(ticketId)).ReturnsAsync(feedback);
        _mockMapper.Setup(m => m.Map<TicketFeedbackResponseDto>(feedback)).Returns(expectedDto);

        var result = await _sut.GetFeedbackByTicketIdAsync(ticketId);

        Assert.NotNull(result);
        Assert.Equal(4, result.Rating);
    }

    [Fact]
    public async Task GetFeedbackByTicketIdAsync_FeedbackNotFound_ThrowsKeyNotFoundException()
    {
        var ticketId = Guid.NewGuid();
        var ticket = CreateValidTicket(ticketId, _tenantId);
        
        _mockTicketRepository.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(ticket);
        _mockFeedbackRepository.Setup(r => r.GetByTicketAsync(ticketId)).ReturnsAsync((TicketFeedback?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetFeedbackByTicketIdAsync(ticketId));
    }
}
