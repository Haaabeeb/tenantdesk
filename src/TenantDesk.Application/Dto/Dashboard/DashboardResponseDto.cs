namespace TenantDesk.Application.Dto.Dashboard;

public record DashboardResponseDto(
    int TotalTickets,
    int OpenTickets,
    int AssignedTickets,
    int InProgressTickets,
    int ClosedTickets
);