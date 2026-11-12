using CnGalWebSite.DataModel.Sorting;
using CnGalWebSite.MainSite.Shared.Components.DesignSystem;
using CnGalWebSite.MainSite.Shared.Components.Features.Examine;
using NUnit.Framework;

namespace CnGalWebSite.UnitTest;

[TestFixture]
public class UserCenterQueryStateTests
{
    [Test]
    public void PageSizeRestorationUsesTheSharedTableOptions()
    {
        var accepted = UserCenterQueryState.GetTableState(
            "1", "50", null, "PassedTime", "true",
            ExamineSortField.PassedTime, ExamineSortField.PassedTime);
        var rejected = UserCenterQueryState.GetTableState(
            "1", "7", null, "PassedTime", "true",
            ExamineSortField.PassedTime, ExamineSortField.PassedTime);

        Assert.Multiple(() =>
        {
            Assert.That(accepted.PageSize, Is.EqualTo(50));
            Assert.That(rejected.PageSize, Is.EqualTo(CgDataTableDefaults.PageSize));
        });
    }
}
