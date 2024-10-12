using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quick.LiteDB.Plus;

namespace Quick.Blazor.Bootstrap.ReverseProxy
{
    public class Global
    {
        public static Global Instance { get; } = new Global();

        public void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Model.ReverseProxyRule>(c => c.EnsureIndex(t => t.Id, true));
        }
    }
}
