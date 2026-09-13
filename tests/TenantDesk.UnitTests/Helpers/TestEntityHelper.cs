
using System;
using System.Reflection;

namespace TenantDesk.UnitTests.Helpers;

public static class TestEntityHelper
{
    
    public static void SetProperty<T>(T entity, string propertyName, object? value)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        if (string.IsNullOrWhiteSpace(propertyName))
            throw new ArgumentNullException(nameof(propertyName));

        var type = entity.GetType();
        var property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (property is null)
            throw new ArgumentException($"Property '{propertyName}' was not found on type '{type.FullName}'.", nameof(propertyName));

        var setter = property.GetSetMethod(nonPublic: true);
        if (setter is null)
        {
            // search backing field if auto-property with only get
            var field = type.GetField($"<{propertyName}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field is not null)
            {
                field.SetValue(entity, value);
                return;
            }

            throw new ArgumentException($"Property '{propertyName}' on type '{type.FullName}' has no accessible setter or backing field.", nameof(propertyName));
        }

        property.SetValue(entity, value, null);
    }
}
