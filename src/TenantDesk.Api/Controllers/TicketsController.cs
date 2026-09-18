using Microsoft.AspNetCore.Mvc;
using TenantDesk.Application.Common;
using TenantDesk.Application.Dto.Ticket;
using TenantDesk.Application.Interfaces.Persistence.Filters;
using TenantDesk.Application.Services;

namespace TenantDesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController(TicketService _ticketService) : ControllerBase
{
    // POST: api/tickets
    [HttpPost]
    public async Task<ActionResult<TicketResponseDto>> Create(CreateTicketDto dto)
    {
        var ticket = await _ticketService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = ticket.TicketId },
            ticket);
    }

    // GET: api/tickets/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TicketResponseDto>> GetById(Guid id)
    {
        var ticket = await _ticketService.GetByIdAsync(id);

        return Ok(ticket);
    }

    // GET: api/tickets?status=Open&categoryId=...&page=1&pageSize=20
    [HttpGet]
    public async Task<ActionResult<PagedResult<TicketResponseDto>>> GetFiltered([FromQuery] TicketFilter filter)
    {
        var result = await _ticketService.GetFilteredAsync(filter);

        return Ok(result);
    }

    // PUT: api/tickets/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TicketResponseDto>> UpdateDetails(Guid id, UpdateTicketDto dto)
    {
        var ticket = await _ticketService.UpdateDetailsAsync(id, dto);

        return Ok(ticket);
    }

    // PATCH: api/tickets/{id}/status
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<TicketResponseDto>> ChangeStatus(Guid id, ChangeTicketStatusDto dto)
    {
        var ticket = await _ticketService.ChangeStatusAsync(id, dto.Status);

        return Ok(ticket);
    }

    // PATCH: api/tickets/{id}/assign
    [HttpPatch("{id:guid}/assign")]
    public async Task<ActionResult<TicketResponseDto>> Assign(Guid id, AssignTicketDto dto)
    {
        var ticket = await _ticketService.AssignAsync(id, dto.AssignedTo);

        return Ok(ticket);
    }

    // PATCH: api/tickets/{id}/close
    [HttpPatch("{id:guid}/close")]
    public async Task<ActionResult<TicketResponseDto>> Close(Guid id)
    {
        var ticket = await _ticketService.CloseAsync(id);

        return Ok(ticket);
    }
}