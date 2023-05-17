using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quick.Blazor.Bootstrap.ReverseProxy
{
    public class Global
    {
        public static Global Instance { get; } = new Global();

        public void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Model.ReverseProxyRule>();
        }
    }
}
