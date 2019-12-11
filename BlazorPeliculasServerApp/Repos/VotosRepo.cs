using BlazorPeliculasServerApp.Data;
using BlazorPeliculasServerApp.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorPeliculasServerApp.Repos
{
    public class VotosRepo
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IHttpContextAccessor httpContextAccessor;

        public VotosRepo(ApplicationDbContext context, UserManager<IdentityUser> userManager,
                         IHttpContextAccessor httpContextAccessor)
        {
            this.context = context;
            this.userManager = userManager;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task Votar(VotoPelicula votoPelicula)
        {
            var user = await userManager.FindByEmailAsync(httpContextAccessor.HttpContext.User.Identity.Name);
            var userId = user.Id;
            var votoActual = await context.VotosPeliculas.
                FirstOrDefaultAsync(x => x.PeliculaId == votoPelicula.PeliculaId && x.UserId == userId);
            if (votoActual == null)
            {
                votoPelicula.UserId = userId;
                votoPelicula.FechaVoto = DateTime.Now;
                context.Add(votoPelicula);
                await context.SaveChangesAsync();
            }
            else
            {
                votoActual.Voto = votoPelicula.Voto;
                votoPelicula.FechaVoto = DateTime.Now;
                await context.SaveChangesAsync();
            }
        }
    }
}
