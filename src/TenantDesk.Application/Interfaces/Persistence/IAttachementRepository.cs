using TenantDesk.Application.Common;
using TenantDesk.Application.Interfaces.Persistence.Filters;
using TenantDesk.Domain.Entities;

namespace TenantDesk.Application.Interfaces.Persistence;

public interface IAttachementRepository
{
    Task<Attachment?> GetByIdAsync(Guid id);
    Task AddAsync(Attachment attachment);
    Task DeleteAsync(Guid id);
    Task<PagedResult<Attachment>> GetFilteredAsync(AttachmentFilter filter);
}