using Microsoft.AspNetCore.Components;
using Quick.Blazor.Bootstrap.ReverseProxy.Model;
using Quick.EntityFrameworkCore.Plus;
using System;
using System.Threading.Tasks;

namespace Quick.Blazor.Bootstrap.ReverseProxy
{
    public partial class ReverseProxyManageControl
    {
        private ModalAlert modalAlert;
        private ModalWindow modalWindow;
        private string searchKeywords;

        [Parameter]
        public string TextNew { get; set; } = "New";
        [Parameter]
        public string TextKeywords { get; set; } = "Keywords";
        [Parameter]
        public string TextName{ get; set; } = "Name";
        [Parameter]
        public string TextPath { get; set; } = "Path";
        [Parameter]
        public string TextUrl { get; set; } = "URL";
        [Parameter]
        public string TextOK { get; set; } = "OK";
        [Parameter]
        public string TextVisit { get; set; } = "Visit";
        [Parameter]
        public string TextEdit { get; set; } = "Edit";
        [Parameter]
        public string TextDelete { get; set; } = "Delete";
        [Parameter]
        public string TextError { get; set; } = "Error";
        [Parameter]
        public string TextConfirmDelete { get; set; } = "Do you want to delete Rule[{0}]?";
        
        [Parameter]
        public RenderFragment IconNew { get; set; }
        [Parameter]
        public RenderFragment IconSearch { get; set; }
        [Parameter]
        public RenderFragment IconRule { get; set; }
        [Parameter]
        public RenderFragment IconVisit { get; set; }
        [Parameter]
        public RenderFragment IconEdit { get; set; }
        [Parameter]
        public RenderFragment IconDelete { get; set; }

        private void validateModel(ReverseProxyRule oldModel, ReverseProxyRule newModel)
        {
            if (!newModel.Path.StartsWith("/"))
                throw new ArgumentException("Path must start with '/'.");
            if ((oldModel == null || oldModel.Path != newModel.Path) && ReverseProxyManager.Instance.Exists(newModel.Path))
                throw new ArgumentException($"Path [{newModel.Path}] already exist.");
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
