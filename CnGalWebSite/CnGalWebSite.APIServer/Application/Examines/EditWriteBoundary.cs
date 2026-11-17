using CnGalWebSite.APIServer.Infrastructure;
using CnGalWebSite.DataModel.ExamineModel.Shared;
using CnGalWebSite.DataModel.ExamineModel.Entries;
using CnGalWebSite.DataModel.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CnGalWebSite.APIServer.Application.Examines;

/// <summary>The write contract for each independently submitted content section.</summary>
public sealed class EditWriteBoundary : IDisposable
{
    private readonly AppDbContext db;
    private readonly object target;
    private readonly HashSet<string> fields;
    private readonly IReadOnlySet<string> keys;
    private readonly HashSet<string> dependents;
    private readonly HashSet<string> joins = new();
    private readonly Operation operation;

    /// <summary>
    /// <paramref name="Fields"/> are entity columns; <paramref name="Keys"/> are extra keys the
    /// submitted edit record may carry without mapping to a column (for example <c>ParentTagId</c>,
    /// which is turned into a navigation assignment by the apply step).
    /// </summary>
    private sealed record Contract(string Fields, string Dependents = "", string Navigations = "", string Keys = "");
    private static readonly Dictionary<Operation, Contract> Contracts = new()
    {
        [Operation.EstablishMain] = new("Name DisplayName AnotherName BriefIntroduction MainPicture Thumbnail BackgroundPicture SmallBackgroundPicture Type Template"),
        [Operation.EstablishMainPage] = new("MainPage"),
        [Operation.EstablishAddInfor] = new("PubulishTime", "BasicEntryInformation BasicEntryInformationAdditional EntryStaff GameRelease Booking BookingGoal"),
        [Operation.EstablishImages] = new("", "EntryPicture"),
        [Operation.EstablishAudio] = new("", "EntryAudio"),
        [Operation.EstablishWebsite] = new("", "EntryWebsite EntryWebsiteImage"),
        [Operation.EstablishTags] = new("", "", "Tags"),
        [Operation.EstablishRelevances] = new("", "EntryRelation Outlink", "Articles Videos"),
        [Operation.EditArticleMain] = new("Name DisplayName BriefIntroduction MainPicture BackgroundPicture SmallBackgroundPicture Type OriginalAuthor OriginalLink PubishTime RealNewsTime NewsType IsCreatedByCurrentUser"),
        [Operation.EditArticleMainPage] = new("MainPage"),
        [Operation.EditArticleRelevanes] = new("", "ArticleRelation Outlink", "Entries Videos"),
        [Operation.EditVideoMain] = new("Name DisplayName BriefIntroduction MainPicture BackgroundPicture SmallBackgroundPicture Type OriginalAuthor PubishTime Duration IsCreatedByCurrentUser IsInteractive Copyright"),
        [Operation.EditVideoMainPage] = new("MainPage"),
        [Operation.EditVideoImages] = new("", "EntryPicture"),
        [Operation.EditVideoRelevanes] = new("", "VideoRelation Outlink", "Entries Articles"),
        [Operation.EditPeripheryMain] = new("Name DisplayName BriefIntroduction MainPicture BackgroundPicture SmallBackgroundPicture Thumbnail Author Material Brand IndividualParts IsAvailableItem IsReprint PageCount Price Size SongCount Type Category SaleLink"),
        [Operation.EditPeripheryImages] = new("", "EntryPicture"),
        [Operation.EditPeripheryRelatedEntries] = new("", "", "RelatedEntries"),
        [Operation.EditPeripheryRelatedPeripheries] = new("", "PeripheryRelation"),
        [Operation.EditTagMain] = new("Name BriefIntroduction MainPicture BackgroundPicture SmallBackgroundPicture Thumbnail ParentCode", "", "ParentCodeNavigation", "ParentTagId"),
        [Operation.EditTagChildTags] = new("", "Tag", "InverseParentCodeNavigation"),
        [Operation.EditTagChildEntries] = new("", "", "Entries")
    };

    // Content columns only. Ownership foreign keys are validated separately; counters and IDs are never command keys.
    private static readonly Dictionary<Type, HashSet<string>> DependentFields = new()
    {
        [typeof(BasicEntryInformation)] = Words("Modifier DisplayName DisplayValue"),
        [typeof(BasicEntryInformationAdditional)] = Words("DisplayName DisplayValue"),
        [typeof(EntryStaff)] = Words("Name CustomName PositionOfficial PositionGeneral SubordinateOrganization Modifier"),
        [typeof(GameRelease)] = Words("Name PublishPlatformType PublishPlatformName Type GamePlatformTypes Time TimeNote Engine Link"),
        [typeof(Booking)] = Words("IsNeedNotification Open LotteryId"),
        [typeof(BookingGoal)] = Words("Name Target"),
        [typeof(EntryPicture)] = Words("Modifier Note Url Priority"),
        [typeof(EntryAudio)] = Words("Name BriefIntroduction Url Priority Duration Thumbnail"),
        [typeof(EntryWebsite)] = Words("Introduction Html FirstPage Logo SubTitle Impressions Color"),
        [typeof(EntryWebsiteImage)] = Words("Type Size Url Priority Note"),
        [typeof(Outlink)] = Words("Name BriefIntroduction Link"),
        [typeof(EntryRelation)] = Words(""),
        [typeof(ArticleRelation)] = Words(""),
        [typeof(VideoRelation)] = Words(""),
        [typeof(PeripheryRelation)] = Words("")
    };

    public EditWriteBoundary(AppDbContext db, object target, Operation operation, object data)
    {
        this.db = db;
        this.target = target;
        this.operation = operation;
        if (!Contracts.TryGetValue(operation, out var contract)) return;
        fields = Words(contract.Fields);
        fields.Add("LastEditTime");
        keys = AllowedKeys(operation);
        dependents = Words(contract.Dependents);
        var metadata = db.Entry(target).Metadata;
        foreach (var navigation in metadata.GetSkipNavigations().Where(n => Words(contract.Navigations).Contains(n.Name)))
            joins.Add(navigation.JoinEntityType.Name);
        foreach (var navigation in metadata.GetNavigations().Where(n => Words(contract.Navigations).Contains(n.Name) && n.IsOnDependent))
            foreach (var property in navigation.ForeignKey.Properties) fields.Add(property.Name);
        if (data is ExamineMain main)
        {
            foreach (var item in main.Items)
                if (!keys.Contains(item.Key))
                    throw new InvalidOperationException($"{operation} 不允许修改字段 {item.Key}。");
        }
        if (data is EntryAddInfor addInfo)
            ValidateNestedKeys(addInfo.Booking.MainInfor, typeof(Booking));
        if (data is EntryWebsiteExamineModel website)
            ValidateNestedKeys(website.MainInfor, typeof(EntryWebsite));
        db.EditWriteBoundaries.Add(this);
    }

    private void ValidateNestedKeys(IEnumerable<ExamineMainAlone> items, Type type)
    {
        foreach (var item in items)
            if (!DependentFields[type].Contains(item.Key))
                throw new InvalidOperationException($"{operation} 不允许修改 {type.Name}.{item.Key}。");
    }

    /// <summary>该操作是否受写入边界约束。</summary>
    internal static bool IsGuarded(Operation operation) => Contracts.ContainsKey(operation);

    /// <summary>该操作的编辑记录允许携带的 key（实体字段与命令键，不含 LastEditTime）。</summary>
    internal static IReadOnlySet<string> AllowedKeys(Operation operation)
    {
        if (!Contracts.TryGetValue(operation, out var contract)) return new HashSet<string>();
        var result = Words(contract.Fields);
        result.UnionWith(Words(contract.Keys));
        // LastEditTime is an entity column the apply step maintains, never an incoming key.
        result.Remove("LastEditTime");
        return result;
    }

    internal bool Allows(EntityEntry entry)
    {
        if (fields is null) return false;
        if (ReferenceEquals(entry.Entity, target))
            return entry.State == EntityState.Modified && entry.Properties.Where(p => p.IsModified).All(p => fields.Contains(p.Metadata.Name));
        // 类型匹配只说明“这类子项允许改”，还得证明这一行确实挂在目标对象上，
        // 否则一份词条图片的编辑记录能把别的词条的图片一起删掉。
        if (joins.Contains(entry.Metadata.Name)) return IsOwned(entry);
        if (!dependents.Contains(entry.Metadata.ClrType.Name)) return false;
        // A child-tag edit changes only parent linkage, never the child's content or counters.
        if (entry.Entity is Tag)
            return entry.State == EntityState.Modified && entry.Properties.Where(p => p.IsModified)
                .All(p => p.Metadata.GetContainingForeignKeys().Any(f => f.PrincipalEntityType.ClrType == typeof(Tag)))
                && IsOwned(entry);
        if (!DependentFields.TryGetValue(entry.Metadata.ClrType, out var allowed)) return false;
        var properties = entry.Properties.Where(p => entry.State == EntityState.Added
            ? !Equals(p.CurrentValue, p.Metadata.ClrType.IsValueType ? Activator.CreateInstance(p.Metadata.ClrType) : null)
            : p.IsModified).ToArray();
        if (entry.State == EntityState.Modified && properties.Any(p => p.Metadata.GetContainingForeignKeys().Any()
            && !allowed.Contains(p.Metadata.Name) && !p.Metadata.Name.StartsWith("To", StringComparison.Ordinal)
            && p.OriginalValue is not null && p.CurrentValue is not null && !Equals(p.CurrentValue, p.OriginalValue))) return false;
        return properties.All(p => allowed.Contains(p.Metadata.Name) || p.Metadata.IsPrimaryKey()
            || p.Metadata.GetContainingForeignKeys().Any()) && IsOwned(entry);
    }

    /// <summary>
    /// 变更行是否挂在目标对象上。沿外键向上追溯到目标，允许经由中间子项
    /// （例如 <c>BookingGoal → Booking → Entry</c>、<c>EntryWebsiteImage → EntryWebsite → Entry</c>），
    /// 同时接受当前关系与原始关系：删除可选子项时外键会被置空，只有原始关系还留着归属信息。
    /// </summary>
    private bool IsOwned(EntityEntry entry)
    {
        var root = db.Entry(target);
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance) { entry.Entity };
        var pending = new Queue<EntityEntry>();
        pending.Enqueue(entry);
        while (pending.Count > 0)
        {
            var current = pending.Dequeue();
            foreach (var foreignKey in current.Metadata.GetForeignKeys())
            {
                // Relation rows belong to their source, not whichever object they happen to link to.
                if (current.Entity is EntryRelation or EntryStaff or ArticleRelation or VideoRelation or PeripheryRelation
                    && foreignKey.Properties.Any(p => p.Name.StartsWith("To", StringComparison.Ordinal))) continue;
                foreach (var useOriginal in new[] { false, true })
                {
                    // Original ownership permits removal, never transferring another object's row into this section.
                    if (useOriginal && current.State != EntityState.Deleted
                        && ForeignKeyValues(current, foreignKey, false).All(value => value is not null)) continue;
                    var values = ForeignKeyValues(current, foreignKey, useOriginal);
                    if (values.Any(value => value is null)) continue;
                    if (ReferencesTarget(foreignKey, values, root)) return true;
                    var principal = FindTracked(foreignKey, values, current);
                    if (principal is not null && visited.Add(principal.Entity)) pending.Enqueue(principal);
                }
            }
        }
        return false;
    }

    private static object?[] ForeignKeyValues(EntityEntry entry, IForeignKey foreignKey, bool useOriginal)
    {
        var values = new object?[foreignKey.Properties.Count];
        for (var i = 0; i < values.Length; i++)
        {
            var property = entry.Property(foreignKey.Properties[i].Name);
            values[i] = useOriginal ? property.OriginalValue : property.CurrentValue;
        }
        return values;
    }

    private bool ReferencesTarget(IForeignKey foreignKey, object?[] values, EntityEntry root)
    {
        if (!foreignKey.PrincipalEntityType.ClrType.IsInstanceOfType(target)) return false;
        var keys = foreignKey.PrincipalKey.Properties;
        if (keys.Count != values.Length) return false;
        for (var i = 0; i < keys.Count; i++)
        {
            if (!Equals(root.Property(keys[i].Name).CurrentValue, values[i])) return false;
        }
        return true;
    }

    private EntityEntry? FindTracked(IForeignKey foreignKey, object?[] values, EntityEntry current)
    {
        foreach (var candidate in db.ChangeTracker.Entries())
        {
            if (ReferenceEquals(candidate.Entity, current.Entity)) continue;
            if (!foreignKey.PrincipalEntityType.IsAssignableFrom(candidate.Metadata)) continue;
            var keys = foreignKey.PrincipalKey.Properties;
            if (keys.Count != values.Length) continue;
            var matches = true;
            for (var i = 0; i < keys.Count && matches; i++)
            {
                matches = Equals(candidate.Property(keys[i].Name).CurrentValue, values[i]);
            }
            if (matches) return candidate;
        }
        return null;
    }

    /// <summary>根实体上被修改、但不在本契约允许列内的属性；用于生成可诊断的错误信息。</summary>
    internal IEnumerable<string> DisallowedProperties(EntityEntry entry)
    {
        if (fields is null || !ReferenceEquals(entry.Entity, target) || entry.State != EntityState.Modified)
        {
            return [];
        }
        return entry.Properties.Where(p => p.IsModified && !fields.Contains(p.Metadata.Name)).Select(p => p.Metadata.Name);
    }

    internal static void Validate(AppDbContext db)
    {
        if (db.EditWriteBoundaries.Count == 0) return;
        db.ChangeTracker.DetectChanges();
        foreach (var entry in db.ChangeTracker.Entries().Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
        {
            if (entry.Entity is Examine or UserReviewEditRecord or Message) continue;
            if (db.EditWriteBoundaries.Any(boundary => boundary.Allows(entry))) continue;
            // 把越界列写进异常信息，否则线上只能看到 “Entry (Modified)” 无法定位。
            var disallowed = db.EditWriteBoundaries.SelectMany(boundary => boundary.DisallowedProperties(entry)).Distinct().ToArray();
            var detail = disallowed.Length == 0 ? "" : $"，不允许修改的列：{string.Join("、", disallowed)}";
            throw new InvalidOperationException($"编辑操作越界：{entry.Metadata.ClrType.Name} ({entry.State}){detail}。");
        }
    }

    private static HashSet<string> Words(string value) => new(value.Split(' ', StringSplitOptions.RemoveEmptyEntries), StringComparer.Ordinal);
    public void Dispose() => db.EditWriteBoundaries.Remove(this);
}
