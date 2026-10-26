using System.Security.Cryptography;
using System.Text;
using CnGalWebSite.APIServer.Application.Users;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Robots;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CnGalWebSite.APIServer.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/robot/v2")]
public sealed class RobotIntegrationController(
    IRepository<Entry, int> entries,
    IRepository<StoreInfo, long> stores,
    IUserService users,
    IConfiguration configuration) : ControllerBase
{
    [HttpGet("random-game")]
    public async Task<ActionResult<RobotItem>> RandomGame(CancellationToken cancellationToken)
    {
        var query = entries.GetAll().AsNoTracking()
            .Where(x => !x.IsHidden && x.Type == EntryType.Game && x.Name != null && x.Name != "");
        var count = await query.CountAsync(cancellationToken);
        if (count == 0)
        {
            return NoContent();
        }

        // Next(count) includes the last row and also works for a single public game.
        var item = await query.OrderBy(x => x.Id).Skip(Random.Shared.Next(count))
            .Select(x => new RobotItem
            {
                Id = x.Id,
                Title = x.DisplayName ?? x.Name,
                Summary = x.BriefIntroduction ?? "",
                Url = "https://www.cngal.org/entries/" + x.Id
            }).FirstOrDefaultAsync(cancellationToken);
        return item is null ? NoContent() : Ok(item);
    }

    [HttpGet("discount-summary")]
    public async Task<ActionResult<RobotDiscountSummary>> DiscountSummary(CancellationToken cancellationToken)
    {
        var count = await stores.GetAll().AsNoTracking()
            .Where(x => x.PlatformType == PublishPlatformType.Steam && x.CutNow > 0
                && x.Entry != null && x.Entry.Type == EntryType.Game && !x.Entry.IsHidden && x.Entry.Name != null && x.Entry.Name != "")
            .Select(x => x.Entry.Id).Distinct().CountAsync(cancellationToken);
        return new RobotDiscountSummary { Count = count };
    }

    [HttpGet("role-birthdays")]
    public async Task<ActionResult<List<RobotItem>>> RoleBirthdays(
        [FromQuery] int month, [FromQuery] int day,
        [FromServices] IRepository<RoleBirthday, long> birthdays, CancellationToken cancellationToken)
    {
        // Leap-year validation permits February 29 without depending on the current year.
        if (month is < 1 or > 12 || day < 1 || day > DateTime.DaysInMonth(2000, month))
            return BadRequest(new { error = "Invalid birthday date." });

        // The legacy birthday DTO has no visibility flag; filter at the database boundary.
        return await birthdays.GetAll().AsNoTracking()
            .Where(x => x.Birthday.Month == month && x.Birthday.Day == day
                && x.Role != null && x.Role.Type == EntryType.Role && !x.Role.IsHidden
                && x.Role.Name != null && x.Role.Name != "")
            .Select(x => x.Role).Distinct().OrderBy(x => x.Id).Take(6)
            .Select(x => new RobotItem
            {
                Id = x.Id, Title = x.DisplayName ?? x.Name,
                Summary = x.BriefIntroduction ?? "",
                Url = "https://www.cngal.org/entries/" + x.Id
            }).ToListAsync(cancellationToken);
    }

    [HttpPost("bind-qq")]
    public async Task<IActionResult> BindQQ([FromBody] RobotBindRequest request)
    {
        var expected = configuration["RobotIntegration:BindingToken"];
        if (string.IsNullOrWhiteSpace(expected) || expected.Length < 32)
        {
            return StatusCode(503, new { error = "Robot binding is not configured." });
        }
        var authorization = Request.Headers.Authorization.ToString();
        var actual = authorization.StartsWith("Bearer ", StringComparison.Ordinal)
            ? authorization[7..] : "";
        if (!CryptographicOperations.FixedTimeEquals(
                SHA256.HashData(Encoding.UTF8.GetBytes(actual)),
                SHA256.HashData(Encoding.UTF8.GetBytes(expected))))
        {
            return Unauthorized();
        }

        // Reuse the existing binding-code semantics. Never log the code or credential.
        return Ok(await users.BindGroupQQ(request.Code.Trim(), request.SenderQQ));
    }
}
