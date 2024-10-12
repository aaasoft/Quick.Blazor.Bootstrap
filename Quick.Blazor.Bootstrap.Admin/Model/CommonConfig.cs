using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Quick.LiteDB.Plus;

namespace Quick.Blazor.Bootstrap.Admin.Model;

[Table($"{nameof(Quick)}_{nameof(Blazor)}_{nameof(Bootstrap)}_{nameof(Admin)}_{nameof(CommonConfig)}")]
public class CommonConfig : BaseModel
{
    [Required]
    public string Value { get; set; }
}