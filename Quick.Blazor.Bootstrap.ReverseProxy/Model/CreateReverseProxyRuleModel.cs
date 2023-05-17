using System.ComponentModel.DataAnnotations;

namespace Quick.Blazor.Bootstrap.ReverseProxy.Model
{
    public class CreateReverseProxyRuleModel : PropertyNotifyModel
    {
        private string _Name;
        [Required]
        public string Name
        {
            get { return _Name; }
            set
            {
                RaisePropertyChanging();
                _Name = value;
                RaisePropertyChanged();
            }
        }

        private string _Path;
        [Required]
        public string Path
        {
            get { return _Path; }
            set
            {
                RaisePropertyChanging();
                _Path = value?.Trim();
                RaisePropertyChanged();
            }
        }

        private string _Url;
        [Required]
        public string Url
        {
            get { return _Url; }
            set
            {
                RaisePropertyChanging();
                _Url = value;
                RaisePropertyChanged();
            }
        }
    }
}
