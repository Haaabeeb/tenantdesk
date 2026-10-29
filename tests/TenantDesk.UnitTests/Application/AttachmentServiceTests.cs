using System.Text;
using Moq;
using TenantDesk.Application.Common;
using TenantDesk.Application.Dto.Attachment;
using TenantDesk.Application.Interfaces.Identity;
using TenantDesk.Application.Interfaces.Persistence;
using TenantDesk.Application.Interfaces.Persistence.Filters;
using TenantDesk.Application.Services;
using TenantDesk.Domain.Entities;
using TenantDesk.Domain.Enums;
using Xunit;

namespace TenantDesk.UnitTests.Application;

public class AttachmentServiceTests
{
    private readonly Mock<IAttachementRepository> _mockAttachmentRepository;
    private readonly Mock<ITicketRepository> _mockTicketRepository;
    private readonly Mock<IFileStorageService> _mockFileStorageService;
    private readonly Mock<ICurrentTenantService> _mockCurrentTenantService;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly AttachmentService _sut;
    private readonly Guid _tenantId;
    private readonly Guid _userId;

    public AttachmentServiceTests()
    {
        _mockAttachmentRepository = new Mock<IAttachementRepository>();
        _mockTicketRepository = new Mock<ITicketRepository>();
        _mockFileStorageService = new Mock<IFileStorageService>();
        _mockCurrentTenantService = new Mock<ICurrentTenantService>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();

        _tenantId = Guid.NewGuid();
        _userId = Guid.NewGuid();

        _mockCurrentTenantService.Setup(s => s.GetTenantId()).Returns(_tenantId);
        _mockCurrentUserService.Setup(s => s.GetUserId()).Returns(_userId);
        _mockCurrentUserService.Setup(s => s.GetUserRole()).Returns(UserRole.Customer);

        _sut = new AttachmentService(
            _mockAttachmentRepository.Object,
            _mockTicketRepository.Object,
            _mockFileStorageService.Object,
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

    private Attachment CreateAttachment(Guid id, Guid uploadedBy, string path)
    {
        var attachment = new Attachment(_tenantId, Guid.NewGuid(), null, uploadedBy, path, "file.txt", 1024);
        typeof(Attachment).GetProperty("AttachmentId")!.SetValue(attachment, id);
        return attachment;
    }

    [Fact]
    public async Task CreateAsync_ValidInput_UploadsFileAndSavesAttachment()
    {
        var ticketId = Guid.NewGuid();
        var ticket = CreateTicket(ticketId, _tenantId, _userId);
        var dto = new CreateAttachmentDto(null, "file.txt", 1024);
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("test content"));

        _mockTicketRepository.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(ticket);

        var result = await _sut.CreateAsync(ticketId, dto, stream);

        Assert.NotNull(result);
        Assert.Equal("file.txt", result.FileName);
        _mockFileStorageService.Verify(s => s.UploadAsync(stream, It.IsAny<string>()), Times.Once);
        _mockAttachmentRepository.Verify(r => r.AddAsync(It.IsAny<Attachment>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_CustomerDifferentCreator_ThrowsUnauthorizedAccessException()
    {
        var ticketId = Guid.NewGuid();
        var ticket = CreateTicket(ticketId, _tenantId, Guid.NewGuid()); // Not _userId
        var dto = new CreateAttachmentDto(null, "file.txt", 1024);
        var stream = new MemoryStream();

        _mockTicketRepository.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(ticket);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.CreateAsync(ticketId, dto, stream));
    }

    [Fact]
    public async Task CreateAsync_AgentNotAssigned_ThrowsUnauthorizedAccessException()
    {
        var ticketId = Guid.NewGuid();
        var ticket = CreateTicket(ticketId, _tenantId, Guid.NewGuid());
        _mockCurrentUserService.Setup(s => s.GetUserRole()).Returns(UserRole.Agent); // Agent

        var dto = new CreateAttachmentDto(null, "file.txt", 1024);
        var stream = new MemoryStream();

        _mockTicketRepository.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(ticket);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.CreateAsync(ticketId, dto, stream));
    }

    [Fact]
    public async Task DeleteAsync_ValidAccess_DeletesFileAndDatabaseRecord()
    {
        var attachmentId = Guid.NewGuid();
        var storagePath = "path/to/file.txt";
        var attachment = CreateAttachment(attachmentId, _userId, storagePath);

        _mockAttachmentRepository.Setup(r => r.GetByIdAsync(attachmentId)).ReturnsAsync(attachment);

        await _sut.DeleteAsync(attachmentId);

        _mockFileStorageService.Verify(s => s.DeleteAsync(storagePath), Times.Once);
        _mockAttachmentRepository.Verify(r => r.DeleteAsync(attachmentId), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_DifferentUploader_ThrowsUnauthorizedAccessException()
    {
        var attachmentId = Guid.NewGuid();
        var storagePath = "path/to/file.txt";
        var attachment = CreateAttachment(attachmentId, Guid.NewGuid(), storagePath); // Not uploaded by _userId

        _mockAttachmentRepository.Setup(r => r.GetByIdAsync(attachmentId)).ReturnsAsync(attachment);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.DeleteAsync(attachmentId));
    }
}
