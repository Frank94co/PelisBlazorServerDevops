using System;
using System.Collections.Generic;
using System.Text;
using BlazorPeliculasServerApp.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlazorPeliculasServerApp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GeneroPelicula>().HasKey(x => new { x.GeneroId, x.PeliculaId });
            modelBuilder.Entity<PeliculaActor>().HasKey(x => new { x.PeliculaId, x.PersonaId });

            var roleAdminID = "7bd27378-e23a-4c7d-8748-205873fe5587";
            var usuarioAdminID = "cc0d78c1-55aa-4f34-b147-41076bda53e5";

            var roleAdmin = new IdentityRole()
            { Id = roleAdminID, Name = "admin", NormalizedName = "admin" };

            var hasher = new PasswordHasher<IdentityUser>();
            var usuarioAdmin = new IdentityUser()
            {
                Id = usuarioAdminID,
                Email = "frank9412_co@hotmail.com",
                UserName = "frank9412_co@hotmail.com",
                NormalizedEmail = "frank9412_co@hotmail.com",
                NormalizedUserName = "frank9412_co@hotmail.com",
                EmailConfirmed = true,
                PasswordHash = hasher.HashPassword(null, "loco9412")
            };
            //modelBuilder.Entity<IdentityUser>().HasData(usuarioAdmin);

            //modelBuilder.Entity<IdentityUserRole<string>>().HasData(
            //    new IdentityUserRole<string>() { RoleId = roleAdminID, UserId = usuarioAdminID }
            //    );
            //modelBuilder.Entity<IdentityRole>().HasData(roleAdmin);

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<GeneroPelicula> GenerosPeliculas { get; set; }
        public DbSet<Pelicula> Peliculas { get; set; }
        public DbSet<Genero> Generos { get; set; }
        public DbSet<Persona> Personas { get; set; }
        public DbSet<PeliculaActor> PeliculasActores { get; set; }
        public DbSet<VotoPelicula> VotosPeliculas { get; set; }
    }
}
