using Quick.EntityFrameworkCore.Plus;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quick.Blazor.Bootstrap.ReverseProxy.Model
{
    [Table($"{nameof(Quick)}_{nameof(Blazor)}_{nameof(Bootstrap)}_{nameof(ReverseProxy)}_{nameof(ReverseProxyRule)}")]
    public class ReverseProxyRule : BaseModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Path { get; set; }
        [Required]
        public string Url { get; set; }
        [NotMapped]
        public ReverseProxyRuleLinkInfo[] Links { get; set; }
    }
}
