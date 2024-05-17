using Microsoft.AspNetCore.Components;
using Quick.Blazor.Bootstrap.ReverseProxy.Model;
using Quick.EntityFrameworkCore.Plus;
using System;
using System.Threading.Tasks;

namespace Quick.Blazor.Bootstrap.ReverseProxy
{
    public partial class ReverseProxyManageControl : ComponentBase
    {
        private ModalAlert modalAlert;
        private ModalWindow modalWindow;
        private string searchKeywords;

        [Parameter]
        public string TextNew { get; set; } = Locale.Catalog.GetString("New");
        [Parameter]
        public string TextKeywords { get; set; } = Locale.Catalog.GetString("Keywords");
        [Parameter]
        public string TextName{ get; set; } = Locale.Catalog.GetString("Name");
        [Parameter]
        public string TextPath { get; set; } = Locale.Catalog.GetString("Path");
        [Parameter]
        public string TextUrl { get; set; } = Locale.Catalog.GetString("URL");
        [Parameter]
        public string TextOK { get; set; } = Locale.Catalog.GetString("OK");
        [Parameter]
        public string TextVisit { get; set; } = Locale.Catalog.GetString("Visit");
        [Parameter]
        public string TextEdit { get; set; } = Locale.Catalog.GetString("Edit");
        [Parameter]
        public string TextDelete { get; set; } = Locale.Catalog.GetString("Delete");
        [Parameter]
        public string TextError { get; set; } = Locale.Catalog.GetString("Error");
        [Parameter]
        public string TextConfirmDelete { get; set; } = Locale.Catalog.GetString("Do you want to delete Rule[{0}]?");
        
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
                throw new ArgumentException(Locale.Catalog.GetString("Path must start with '/'."));
            if ((oldModel == null || oldModel.Path != newModel.Path) && ReverseProxyManager.Instance.Exists(newModel.Path))
                throw new ArgumentException(Locale.Catalog.GetString("Path [{0}] already exist.", newModel.Path));
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
