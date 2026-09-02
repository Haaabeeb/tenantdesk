using Microsoft.AspNetCore.Mvc;
using TenantDesk.Application.Common;
using TenantDesk.Application.Dto.Category;
using TenantDesk.Application.Interfaces.Persistence;
using TenantDesk.Application.Services;

namespace TenantDesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController(CategoryService _categoryService) : ControllerBase
{
    // POST: api/categories
    [HttpPost]
    public async Task<ActionResult<CategoryResponseDto>> Create(CreateCategoryDto dto)
    {
        var category = await _categoryService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = category.CategoryId },
            category);
    }

    // GET: api/categories/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CategoryResponseDto>> GetById(Guid id)
    {
        var category = await _categoryService.GetByIdAsync(id);

        return Ok(category);
    }

    // GET: api/categories?isActive=true&page=1&pageSize=20
    [HttpGet]
    public async Task<ActionResult<PagedResult<CategoryResponseDto>>> GetFiltered([FromQuery] CategoryFilter filter)
    {
        var result = await _categoryService.GetFilteredAsync(filter);

        return Ok(result);
    }

    // PUT: api/categories/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CategoryResponseDto>> Update(Guid id, UpdateCategoryDto dto)
    {
        var category = await _categoryService.UpdateAsync(id, dto);

        return Ok(category);
    }

    // DELETE: api/categories/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _categoryService.DeleteAsync(id);

        return NoContent();
    }
}