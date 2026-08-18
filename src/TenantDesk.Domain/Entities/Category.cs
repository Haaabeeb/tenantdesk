using System;

namespace TenantDesk.Domain.Entities
{
    public class Category
    {
        public Guid CategoryId { get; private set; }
        public Guid TenantId { get; private set; }
        public string CategoryName { get; private set; }
        public string? Description { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }

        // For EF Core
        private Category() { }

        public Category(Guid tenantId, string categoryName, string? description)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                throw new ArgumentException("Category name is required.", nameof(categoryName));

            CategoryId = Guid.NewGuid();
            TenantId = tenantId;
            CategoryName = categoryName;
            Description = description;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }

        public void Rename(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("Category name is required.", nameof(newName));

            CategoryName = newName;
        }

        public void UpdateDescription(string? description)
        {
            Description = description;
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}