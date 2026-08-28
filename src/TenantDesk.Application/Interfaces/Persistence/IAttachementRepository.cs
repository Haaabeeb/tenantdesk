
using System.Net.Mail;
using TenantDesk.Application.Common;
using TenantDesk.Application.Dto.Attachment;
using TenantDesk.Application.Interfaces.Persistence.Filters;

public interface IAttachementRepository{
    
    public Task<Attachment> GetIdByAsync(Guid id);
    public Task<long> GetTotalSizeByTenantAsync(Guid tenantId);
    public Task<long> GetTotalSizeByTicketAsync(Guid ticketId);
    public Task AddAsync(Attachment attachment);
    public Task DeleteAsync();
    public Task ExistsAsync();
    Task<PagedResult<Attachment>> GetFilteredAsync(AttachmentFilter filter);
}