namespace CnGalWebSite.APIServer.Application.Tasks
{
    public interface IBackgroundTaskService
    {
        public bool IsRuning { get;}

        void Fail();

        void Runing();
    }
}
