using NewgramWeb.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace NewgramWeb
{
    public class DB : DbContext
    {

        public DB() : base("SQLServer")
        {

        }


        public DbSet<AccessToken> AccessTokens { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<UsuarioToken> UsuarioTokens { get; set; }
        public DbSet<Seguidor> Seguidores { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostInteracao> PostsInteracoes { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove <PluralizingTableNameConvention>();
        }

    }
}