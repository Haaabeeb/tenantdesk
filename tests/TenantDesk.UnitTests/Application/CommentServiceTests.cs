using Moq;
using TenantDesk.Application.Common;
using TenantDesk.Application.Dto.Comment;
using TenantDesk.Application.Interfaces.Identity;
using TenantDesk.Application.Interfaces.Persistence;
using TenantDesk.Application.Interfaces.Persistence.Filters;
using TenantDesk.Application.Services;
using TenantDesk.Domain.Entities;
using TenantDesk.Domain.Enums;
using Xunit;

namespace TenantDesk.UnitTests.Application;

public class CommentServiceTests
{
    private readonly Mock<ICommentRepository> _mockCommentRepository;
    private readonly Mock<ITicketRepository> _mockTicketRepository;
    private readonly Mock<ICurrentTenantService> _mockCurrentTenantService;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly CommentService _sut;
    private readonly Guid _tenantId;
    private readonly Guid _userId;

    public CommentServiceTests()
    {
        _mockCommentRepository = new Mock<ICommentRepository>();
        _mockTicketRepository = new Mock<ITicketRepository>();
        _mockCurrentTenantService = new Mock<ICurrentTenantService>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();

        _tenantId = Guid.NewGuid();
        _userId = Guid.NewGuid();

        _mockCurrentTenantService.Setup(s => s.GetTenantId()).Returns(_tenantId);
        _mockCurrentUserService.Setup(s => s.GetUserId()).Returns(_userId);
        _mockCurrentUserService.Setup(s => s.GetUserRole()).Returns(UserRole.Customer);

        _sut = new CommentService(
            _mockCommentRepository.Object,
            _mockTicketRepository.Object,
            _mockCurrentTenantService.Object,
            _mockCurrentUserService.Object);
    }

    private Ticket CreateTicket(Guid ticketId, Guid tenantId, Guid createdBy, Guid? assignedTo = null)
    {
        var ticket = new Ticket(tenantId, Guid.NewGuid(), Guid.NewGuid(), createdBy, "Title", "Desc");
        typeof(Ticket).GetProperty("TicketId")!.SetValue(ticket, ticketId);
        if (assignedTo.HasValue)
        {
            ticket.AssignTo(assignedTo.Value);
        }
        return ticket;
    }

    private Comment CreateComment(Guid id, Guid userId, string content)
    {
        var comment = new Comment(_tenantId, Guid.NewGuid(), userId, content);
        typeof(Comment).GetProperty("CommentId")!.SetValue(comment, id);
        return comment;
    }

    [Fact]
    public async Task CreateAsync_ValidCustomer_CreatesAndReturnsComment()
    {
        var ticketId = Guid.NewGuid();
        var ticket = CreateTicket(ticketId, _tenantId, _userId); // Customer is creator
        var dto = new CreateCommentDto("Looks good!");

        _mockTicketRepository.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(ticket);
        
        var result = await _sut.CreateAsync(ticketId, dto);

        Assert.NotNull(result);
        Assert.Equal("Looks good!", result.Content);
        _mockCommentRepository.Verify(r => r.AddAsync(It.IsAny<Comment>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_CustomerDifferentCreator_ThrowsUnauthorizedAccessException()
    {
        var ticketId = Guid.NewGuid();
        var ticket = CreateTicket(ticketId, _tenantId, Guid.NewGuid()); // Not _userId
        var dto = new CreateCommentDto("Looks good!");

        _mockTicketRepository.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(ticket);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.CreateAsync(ticketId, dto));
    }

    [Fact]
    public async Task UpdateAsync_ValidOwner_UpdatesComment()
    {
        var commentId = Guid.NewGuid();
        var comment = CreateComment(commentId, _userId, "Old content");
        var dto = new UpdateCommentDto("New content"); // Assuming it's a record with Content

        _mockCommentRepository.Setup(r => r.GetByIdAsync(commentId)).ReturnsAsync(comment);

        var result = await _sut.UpdateAsync(commentId, dto);

        Assert.Equal("New content", result.Content);
        _mockCommentRepository.Verify(r => r.UpdateAsync(comment), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NotOwner_ThrowsUnauthorizedAccessException()
    {
        var commentId = Guid.NewGuid();
        var comment = CreateComment(commentId, Guid.NewGuid(), "Old content"); // Not _userId
        var dto = new UpdateCommentDto("New content"); 

        _mockCommentRepository.Setup(r => r.GetByIdAsync(commentId)).ReturnsAsync(comment);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.UpdateAsync(commentId, dto));
    }

    [Fact]
    public async Task DeleteAsync_ValidOwner_DeletesComment()
    {
        var commentId = Guid.NewGuid();
        var comment = CreateComment(commentId, _userId, "Content");

        _mockCommentRepository.Setup(r => r.GetByIdAsync(commentId)).ReturnsAsync(comment);

        await _sut.DeleteAsync(commentId);

        _mockCommentRepository.Verify(r => r.DeleteAsync(commentId), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_AdminNotOwner_DeletesComment()
    {
        var commentId = Guid.NewGuid();
        var comment = CreateComment(commentId, Guid.NewGuid(), "Content"); // Not _userId
        _mockCurrentUserService.Setup(s => s.GetUserRole()).Returns(UserRole.Admin); // Admin

        _mockCommentRepository.Setup(r => r.GetByIdAsync(commentId)).ReturnsAsync(comment);

        await _sut.DeleteAsync(commentId);

        _mockCommentRepository.Verify(r => r.DeleteAsync(commentId), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NotOwnerNotAdmin_ThrowsUnauthorizedAccessException()
    {
        var commentId = Guid.NewGuid();
        var comment = CreateComment(commentId, Guid.NewGuid(), "Content"); // Not _userId
        _mockCurrentUserService.Setup(s => s.GetUserRole()).Returns(UserRole.Customer); // Customer

        _mockCommentRepository.Setup(r => r.GetByIdAsync(commentId)).ReturnsAsync(comment);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.DeleteAsync(commentId));
    }
}
