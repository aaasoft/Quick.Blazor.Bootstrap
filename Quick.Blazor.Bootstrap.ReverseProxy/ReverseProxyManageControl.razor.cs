using Microsoft.AspNetCore.Components;
using Quick.Blazor.Bootstrap.ReverseProxy.Model;
using Quick.EntityFrameworkCore.Plus;
using Quick.Localize;
using System;
using System.Threading.Tasks;

namespace Quick.Blazor.Bootstrap.ReverseProxy
{
    public partial class ReverseProxyManageControl : ComponentBase_WithGettextSupport
    {
        private ModalAlert modalAlert;
        private ModalWindow modalWindow;
        private string searchKeywords;

        private static string TextNew => Locale.GetString("New");
        private static string TextKeywords =>Locale.GetString("Keywords");
        private static string TextName => Locale.GetString("Name");
        private static string TextPath => Locale.GetString("Path");
        private static string TextUrl => Locale.GetString("URL");
        private static string TextOK => Locale.GetString("OK");
        private static string TextVisit => Locale.GetString("Visit");
        private static string TextEdit => Locale.GetString("Edit");
        private static string TextDelete => Locale.GetString("Delete");
        private static string TextError => Locale.GetString("Error");
        private static string TextConfirmDelete => Locale.GetString("Do you want to delete Rule[{0}]?");
        
        [Parameter]
        public string IconNew { get; set; }="fa fa-plus";
        [Parameter]
        public string IconSearch { get; set; }="fa fa-search";
        [Parameter]
        public string IconRule { get; set; }="fa fa-paper-plane mr-1";
        [Parameter]
        public string IconVisit { get; set; }="fa fa-globe";
        [Parameter]
        public string IconEdit { get; set; }="fa fa-pencil";
        [Parameter]
        public string IconDelete { get; set; }="fa fa-trash";

        private void validateModel(ReverseProxyRule oldModel, ReverseProxyRule newModel)
        {
            if (!newModel.Path.StartsWith("/"))
                throw new ArgumentException(Locale.GetString("Path must start with '/'."));
            if ((oldModel == null || oldModel.Path != newModel.Path) && ReverseProxyManager.Instance.Exists(newModel.Path))
                throw new ArgumentException(Locale.GetString("Path [{0}] already exist.", newModel.Path));
        }

        private void Create()
        {
            modalWindow.Show<Controls.ReverseProxyRuleCreateControl>(TextNew,
                Controls.ReverseProxyRuleCreateControl.PrepareParameter(
                    null,
                    t =>
                    {
                        var model = new ReverseProxyRule()
                        {
                            Id = Guid.NewGuid().ToString("N"),
                            Name = t.Name,
                            Path = t.Path,
                            Url = t.Url
                        };
                        try
                        {
                            validateModel(null, model);
                            ConfigDbContext.CacheContext.Add(model);
                        }
                        catch (Exception ex)
                        {
                            modalAlert.Show(TextError, ex.Message);
                            return;
                        }
                        ReverseProxyManager.Instance.AddRule(model);
                        InvokeAsync(StateHasChanged);
                        modalWindow.Close();
                    }, TextName, TextPath, TextUrl, TextOK)
                );
        }

        private void Edit(ReverseProxyRule model)
        {
            modalWindow.Show<Controls.ReverseProxyRuleCreateControl>(TextEdit,
                Controls.ReverseProxyRuleCreateControl.PrepareParameter(
                    model,
                    t =>
                    {
                        var newModel = new ReverseProxyRule()
                        {
                            Id = model.Id,
                            Name = t.Name,
                            Path = t.Path,
                            Url = t.Url
                        };
                        try
                        {
                            validateModel(model, newModel);
                            ConfigDbContext.CacheContext.Update(newModel);
                        }
                        catch (Exception ex)
                        {
                            modalAlert.Show(TextError, ex.Message);
                            return;
                        }
                        ReverseProxyManager.Instance.RemoveRule(model);
                        ReverseProxyManager.Instance.AddRule(newModel);
                        InvokeAsync(StateHasChanged);
                        modalWindow.Close();
                    }, TextName, TextPath, TextUrl, TextOK)
                );
        }

        private void Delete(ReverseProxyRule model)
        {
            modalAlert.Show(TextDelete, string.Format(TextConfirmDelete, model.Name), () =>
              {
                  try
                  {
                      ConfigDbContext.CacheContext.Remove(model);
                      ReverseProxyManager.Instance.RemoveRule(model);
                      InvokeAsync(StateHasChanged);
                  }
                  catch (Exception ex)
                  {
                      Task.Delay(100).ContinueWith(t =>
                      {
                          modalAlert.Show(TextError, ex.Message);
                      });
                  }
              });
        }
    }
}
