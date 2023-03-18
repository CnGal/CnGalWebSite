namespace CnGalWebSite.IdentityServer.Services.Messages
{
    public interface IViewRenderService
    {
        string Render(string viewPath);
        string Render<Model>(string viewPath, Model model);
    }
}
