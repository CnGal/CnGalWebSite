using CnGalWebSite.RobotClientX.Models.Configurations;

namespace CnGalWebSite.RobotClientX.Services.Configurations
{
    public interface IConfigurationService
    {
        ConfigurationModel GetConfiguration();

        void Save();
    }
}
