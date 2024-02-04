namespace CnGalWebSite.APIServer.Application.Tasks
{
    public class BackgroundTaskService:IBackgroundTaskService
    {
        public bool IsRuning { get; set; }

        public void Fail()
        {
            IsRuning = false;
        }

        public void Runing()
        {
            IsRuning = true;
        }
    }
}
