using CnGalWebSite.APIServer.Infrastructure;
using CnGalWebSite.DataModel.ExamineModel.Shared;
using CnGalWebSite.DataModel.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
        db.EditWriteBoundaries.Add(this);
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
        if (joins.Contains(entry.Metadata.Name)) return true;
        if (!dependents.Contains(entry.Metadata.ClrType.Name)) return false;
        // A child-tag edit changes only parent linkage, never the child's content or counters.
        if (entry.Entity is Tag)
            return entry.State == EntityState.Modified && entry.Properties.Where(p => p.IsModified)
                .All(p => p.Metadata.GetContainingForeignKeys().Any(f => f.PrincipalEntityType.ClrType == typeof(Tag)));
        return true;
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
