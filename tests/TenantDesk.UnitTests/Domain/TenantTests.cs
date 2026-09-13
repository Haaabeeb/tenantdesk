
using System;
using System.Threading;
using TenantDesk.Domain.Entities;
using TenantDesk.UnitTests.Helpers;
using Xunit;

namespace TenantDesk.UnitTests.Domain;

public class TenantTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesTenantSuccessfully()
    {
        // Arrange
        var planId = Guid.NewGuid();
        const string tenantName = "Acme Corp";
        const string subdomain = "acme";

        // Act
        var tenant = new Tenant(planId, tenantName, subdomain);

        // Assert
        Assert.Equal(planId, tenant.PlanId);
        Assert.Equal(tenantName, tenant.TenantName);
        Assert.Equal(subdomain, tenant.Subdomain);
        Assert.True(tenant.IsActive);
        Assert.NotEqual(default, tenant.CreatedAt);
        Assert.Null(tenant.UpdatedAt);
    }

    [Fact]
    public void Constructor_WithEmptyPlanId_ThrowsArgumentException()
    {
        // Arrange
        var emptyPlanId = Guid.Empty;

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => new Tenant(emptyPlanId, "Valid Name", "valid-subdomain"));
        Assert.Equal("planId", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrWhitespaceTenantName_ThrowsArgumentException(string? invalidName)
    {
        // Arrange
        var planId = Guid.NewGuid();

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => new Tenant(planId, invalidName!, "valid-subdomain"));
        Assert.Equal("tenantName", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrWhitespaceSubdomain_ThrowsArgumentException(string? invalidSubdomain)
    {
        // Arrange
        var planId = Guid.NewGuid();

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => new Tenant(planId, "Valid Name", invalidSubdomain!));
        Assert.Equal("subdomain", ex.ParamName);
    }

    [Fact]
    public void Constructor_WithWhitespaceAndMixedCase_TrimsNameAndNormalizesSubdomain()
    {
        // Arrange
        var planId = Guid.NewGuid();
        const string untrimmedName = "   Contoso Ltd   ";
        const string untrimmedSubdomain = "   ConToso-App   ";

        // Act
        var tenant = new Tenant(planId, untrimmedName, untrimmedSubdomain);

        // Assert
        Assert.Equal("Contoso Ltd", tenant.TenantName);
        Assert.Equal("contoso-app", tenant.Subdomain);
    }

    [Fact]
    public void Rename_WithValidName_UpdatesTenantNameAndTimestamp()
    {
        // Arrange
        var tenant = new Tenant(Guid.NewGuid(), "Old Name", "subdomain");
        const string newName = "New Corporation";

        // Act
        tenant.Rename(newName);

        // Assert
        Assert.Equal(newName, tenant.TenantName);
        Assert.NotNull(tenant.UpdatedAt);
        Assert.True(tenant.UpdatedAt >= tenant.CreatedAt);
    }

    [Fact]
    public void Rename_WithUntrimmedName_TrimsNameAndUpdatesTimestamp()
    {
        // Arrange
        var tenant = new Tenant(Guid.NewGuid(), "Old Name", "subdomain");

        // Act
        tenant.Rename("   Trimmed Name   ");

        // Assert
        Assert.Equal("Trimmed Name", tenant.TenantName);
        Assert.NotNull(tenant.UpdatedAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rename_WithNullOrWhitespace_ThrowsArgumentException(string? invalidName)
    {
        // Arrange
        var tenant = new Tenant(Guid.NewGuid(), "Original Name", "subdomain");

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => tenant.Rename(invalidName!));
        Assert.Equal("name", ex.ParamName);
    }

    [Fact]
    public void ChangePlan_WithValidPlanId_UpdatesPlanIdAndTimestamp()
    {
        // Arrange
        var initialPlanId = Guid.NewGuid();
        var tenant = new Tenant(initialPlanId, "Acme", "acme");
        var newPlanId = Guid.NewGuid();

        // Act
        tenant.ChangePlan(newPlanId);

        // Assert
        Assert.Equal(newPlanId, tenant.PlanId);
        Assert.NotNull(tenant.UpdatedAt);
    }

    [Fact]
    public void ChangePlan_WithEmptyPlanId_ThrowsArgumentException()
    {
        // Arrange
        var tenant = new Tenant(Guid.NewGuid(), "Acme", "acme");

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => tenant.ChangePlan(Guid.Empty));
        Assert.Equal("planId", ex.ParamName);
    }

    [Fact]
    public void ChangeSubdomain_WithValidSubdomain_NormalizesSubdomainAndUpdatesTimestamp()
    {
        // Arrange
        var tenant = new Tenant(Guid.NewGuid(), "Acme", "old-subdomain");

        // Act
        tenant.ChangeSubdomain("   NEW-subdomain   ");

        // Assert
        Assert.Equal("new-subdomain", tenant.Subdomain);
        Assert.NotNull(tenant.UpdatedAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ChangeSubdomain_WithNullOrWhitespace_ThrowsArgumentException(string? invalidSubdomain)
    {
        // Arrange
        var tenant = new Tenant(Guid.NewGuid(), "Acme", "subdomain");

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => tenant.ChangeSubdomain(invalidSubdomain!));
        Assert.Equal("subdomain", ex.ParamName);
    }

    [Fact]
    public void Deactivate_WhenActive_SetsIsActiveFalseAndUpdatesTimestamp()
    {
        // Arrange
        var tenant = new Tenant(Guid.NewGuid(), "Acme", "acme");
        Assert.True(tenant.IsActive);
        Assert.Null(tenant.UpdatedAt);

        // Act
        tenant.Deactivate();

        // Assert
        Assert.False(tenant.IsActive);
        Assert.NotNull(tenant.UpdatedAt);
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_DoesNotUpdateTimestamp()
    {
        // Arrange
        var tenant = new Tenant(Guid.NewGuid(), "Acme", "acme");
        tenant.Deactivate();
        var firstUpdatedAt = tenant.UpdatedAt;

        // Act
        tenant.Deactivate();

        // Assert
        Assert.False(tenant.IsActive);
        Assert.Equal(firstUpdatedAt, tenant.UpdatedAt);
    }

    [Fact]
    public void Activate_WhenInactive_SetsIsActiveTrueAndUpdatesTimestamp()
    {
        // Arrange
        var tenant = new Tenant(Guid.NewGuid(), "Acme", "acme");
        tenant.Deactivate();
        var deactivatedUpdatedAt = tenant.UpdatedAt;

        // Act
        Thread.Sleep(10); // Ensure noticeable timestamp change
        tenant.Activate();

        // Assert
        Assert.True(tenant.IsActive);
        Assert.NotNull(tenant.UpdatedAt);
        Assert.True(tenant.UpdatedAt >= deactivatedUpdatedAt);
    }

    [Fact]
    public void Activate_WhenAlreadyActive_DoesNotUpdateTimestamp()
    {
        // Arrange
        var tenant = new Tenant(Guid.NewGuid(), "Acme", "acme");
        Assert.True(tenant.IsActive);
        Assert.Null(tenant.UpdatedAt);

        // Act
        tenant.Activate();

        // Assert
        Assert.True(tenant.IsActive);
        Assert.Null(tenant.UpdatedAt);
    }

    [Fact]
    public void TestEntityHelper_SetsPrivateTenantId_Successfully()
    {
        // Arrange
        var tenant = new Tenant(Guid.NewGuid(), "Acme", "acme");
        var expectedTenantId = Guid.NewGuid();

        // Act
        TestEntityHelper.SetProperty(tenant, "TenantId", expectedTenantId);

        // Assert
        Assert.Equal(expectedTenantId, tenant.TenantId);
    }
}
