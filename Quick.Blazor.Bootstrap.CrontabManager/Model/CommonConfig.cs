using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Quick.LiteDB.Plus;

namespace Quick.Blazor.Bootstrap.CrontabManager.Model;

[Table($"{nameof(Quick)}_{nameof(Blazor)}_{nameof(Bootstrap)}_{nameof(CrontabManager)}_{nameof(CommonConfig)}")]
public class CommonConfig : BaseModel
{
    [Required]
    public string Value { get; set; }
}