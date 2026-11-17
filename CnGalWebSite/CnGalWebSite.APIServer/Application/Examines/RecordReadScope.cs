using Microsoft.EntityFrameworkCore;
using System;

namespace CnGalWebSite.APIServer.Application.Examines;

/// <summary>Every query in a preview/replay returns independent objects, even if the context already tracks the same keys.</summary>
public sealed class RecordReadScope : IDisposable
{
    private readonly DbContext context;
    private readonly QueryTrackingBehavior previous;

    public RecordReadScope(DbContext context)
    {
        this.context = context;
        previous = context.ChangeTracker.QueryTrackingBehavior;
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        if (context is Infrastructure.AppDbContext app) app.RecordReadDepth++;
    }

    public void Dispose()
    {
        if (context is Infrastructure.AppDbContext app) app.RecordReadDepth--;
        context.ChangeTracker.QueryTrackingBehavior = previous;
    }
}
