
using System.Net.Mail;
using TenantDesk.Application.Dto.Attachment;

public interface IAttachementRepository{
    
    public Task<Attachment> GetIdByAsync(Guid id);
    public Task <IEnumerable<Attachment>> GetAllByTicketAsync(Guid ticketId);
    public Task<IEnumerable<Attachment>> GetAllByCommentAsync(Guid commentId);
    public Task<IEnumerable<Attachment>> GetByUoloaderAsync(Guid userId);
    public Task<long> GetTotalSizeByTenantAsync(Guid tenantId);
    public Task<long> GetTotalSizeByTicketAsync(Guid ticketId);
    public Task AddAsync(Attachment attachment);
    public Task DeleteAsync();
    public Task ExistsAsync();

}