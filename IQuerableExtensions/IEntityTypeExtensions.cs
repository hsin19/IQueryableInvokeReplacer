using Microsoft.EntityFrameworkCore.Metadata;
using System.Reflection;

namespace IQuerableExtensions;

public static class IEntityTypeExtensions
{
    private static readonly Dictionary<IEntityType, List<PropertyInfo>> CacheKey = new();

    /// <summary>
    /// Get the <see cref="PropertyInfo"/>s of <see cref="IEntityType"/> which can identify uniqueness
    /// </summary>
    /// <remarks>
    /// Identify uniqueness rank:
    /// <list type="number">
    /// <item> Primary Key</item>
    /// <item> Alternate Key with fewest properties</item>
    /// <item> All Columns</item>
    /// </list>
    /// </remarks>
    public static List<PropertyInfo> GetUniquePropertyInfo(this IEntityType entityType)
    {
        if (!CacheKey.TryGetValue(entityType, out var properties))
        {
            var keys = entityType.GetKeys();
            var key = keys.FirstOrDefault(k => k.IsPrimaryKey());
            key ??= keys.MinBy(e => e.Properties.Count);
            properties = key?.Properties.Select(k => k.PropertyInfo).OfType<PropertyInfo>().ToList();
            properties ??= entityType.GetProperties().Select(p => p.PropertyInfo).OfType<PropertyInfo>().ToList();
            CacheKey[entityType] = properties;
        }
        return properties;
    }
}
