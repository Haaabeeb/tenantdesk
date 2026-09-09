using AutoMapper;
using Moq;
using TenantDesk.Application.Common;
using TenantDesk.Application.Dto.Category;
using TenantDesk.Application.Interfaces.Identity;
using TenantDesk.Application.Interfaces.Persistence;
using TenantDesk.Application.Services;
using TenantDesk.Domain.Entities;
using Xunit;

namespace TenantDesk.UnitTests.Application;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _mockCategoryRepository;
    private readonly Mock<ICurrentTenantService> _mockCurrentTenantService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly CategoryService _sut;
    private readonly Guid _tenantId;

    public CategoryServiceTests()
    {
        _mockCategoryRepository = new Mock<ICategoryRepository>();
        _mockCurrentTenantService = new Mock<ICurrentTenantService>();
        _mockMapper = new Mock<IMapper>();

        _tenantId = Guid.NewGuid();
        _mockCurrentTenantService.Setup(x => x.GetTenantId()).Returns(_tenantId);

        _sut = new CategoryService(
            _mockCategoryRepository.Object,
            _mockCurrentTenantService.Object,
            _mockMapper.Object);
    }

    [Fact]
    public async Task GetByIdAsync_CategoryExistsAndBelongsToTenant_ReturnsCategory()
    {
        var categoryId = Guid.NewGuid();
        var category = new Category(_tenantId, "Hardware", "Hardware issues");
        
        _mockCategoryRepository.Setup(r => r.GetByIdAsync(categoryId)).ReturnsAsync(category);
        _mockMapper.Setup(m => m.Map<CategoryResponseDto>(category)).Returns(new CategoryResponseDto(categoryId, "Hardware", "Hardware issues", true, DateTime.UtcNow));

        var result = await _sut.GetByIdAsync(categoryId);

        Assert.NotNull(result);
        Assert.Equal("Hardware", result.CategoryName);
    }

    [Fact]
    public async Task GetByIdAsync_CategoryNotFound_ThrowsKeyNotFoundException()
    {
        var categoryId = Guid.NewGuid();
        _mockCategoryRepository.Setup(r => r.GetByIdAsync(categoryId)).ReturnsAsync((Category?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetByIdAsync(categoryId));
    }

    [Fact]
    public async Task GetByIdAsync_CategoryBelongsToOtherTenant_ThrowsUnauthorizedAccessException()
    {
        var categoryId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        var category = new Category(otherTenantId, "Hardware", "Hardware issues");

        _mockCategoryRepository.Setup(r => r.GetByIdAsync(categoryId)).ReturnsAsync(category);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.GetByIdAsync(categoryId));
    }

    [Fact]
    public async Task CreateAsync_ValidName_CreatesAndReturnsCategory()
    {
        var dto = new CreateCategoryDto("Software", "Software issues");
        _mockCategoryRepository.Setup(r => r.ExistsByNameAsync(_tenantId, dto.CategoryName)).ReturnsAsync(false);
        _mockMapper.Setup(m => m.Map<CategoryResponseDto>(It.IsAny<Category>())).Returns(new CategoryResponseDto(Guid.NewGuid(), dto.CategoryName, dto.Description, true, DateTime.UtcNow));

        var result = await _sut.CreateAsync(dto);

        Assert.NotNull(result);
        Assert.Equal(dto.CategoryName, result.CategoryName);
        _mockCategoryRepository.Verify(r => r.AddAsync(It.Is<Category>(c => c.CategoryName == dto.CategoryName && c.TenantId == _tenantId)), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_NameTaken_ThrowsInvalidOperationException()
    {
        var dto = new CreateCategoryDto("Software", "Software issues");
        _mockCategoryRepository.Setup(r => r.ExistsByNameAsync(_tenantId, dto.CategoryName)).ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateAsync(dto));
    }

    [Fact]
    public async Task UpdateAsync_ValidUpdate_UpdatesAndReturnsCategory()
    {
        var categoryId = Guid.NewGuid();
        var category = new Category(_tenantId, "Old Name", "Old Desc");
        var dto = new UpdateCategoryDto("New Name", "New Desc", true);

        _mockCategoryRepository.Setup(r => r.GetByIdAsync(categoryId)).ReturnsAsync(category);
        _mockCategoryRepository.Setup(r => r.ExistsByNameAsync(_tenantId, dto.CategoryName)).ReturnsAsync(false);
        _mockMapper.Setup(m => m.Map<CategoryResponseDto>(category)).Returns(new CategoryResponseDto(categoryId, dto.CategoryName, dto.Description, true, DateTime.UtcNow));

        var result = await _sut.UpdateAsync(categoryId, dto);

        Assert.NotNull(result);
        Assert.Equal(dto.CategoryName, result.CategoryName);
        _mockCategoryRepository.Verify(r => r.UpdateAsync(category), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NameTakenByAnotherCategory_ThrowsInvalidOperationException()
    {
        var categoryId = Guid.NewGuid();
        var category = new Category(_tenantId, "Old Name", "Old Desc");
        var dto = new UpdateCategoryDto("New Name", "New Desc", true);

        _mockCategoryRepository.Setup(r => r.GetByIdAsync(categoryId)).ReturnsAsync(category);
        _mockCategoryRepository.Setup(r => r.ExistsByNameAsync(_tenantId, dto.CategoryName)).ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.UpdateAsync(categoryId, dto));
    }

    [Fact]
    public async Task DeleteAsync_ValidCategory_DeletesSuccessfully()
    {
        var categoryId = Guid.NewGuid();
        var category = new Category(_tenantId, "Name", "Desc");

        _mockCategoryRepository.Setup(r => r.GetByIdAsync(categoryId)).ReturnsAsync(category);
        _mockCategoryRepository.Setup(r => r.GetTicketCountByCategoryAsync(categoryId)).ReturnsAsync(0);

        await _sut.DeleteAsync(categoryId);

        _mockCategoryRepository.Verify(r => r.DeleteAsync(categoryId), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_CategoryHasTickets_ThrowsInvalidOperationException()
    {
        var categoryId = Guid.NewGuid();
        var category = new Category(_tenantId, "Name", "Desc");

        _mockCategoryRepository.Setup(r => r.GetByIdAsync(categoryId)).ReturnsAsync(category);
        _mockCategoryRepository.Setup(r => r.GetTicketCountByCategoryAsync(categoryId)).ReturnsAsync(1);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.DeleteAsync(categoryId));
    }
}
