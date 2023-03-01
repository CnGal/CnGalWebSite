namespace CnGalWebSite.APIServer.Application.Helper
{
    public interface IViewRenderService
    {
        string Render(string viewPath);
        string Render<Model>(string viewPath, Model model);
    }
}
