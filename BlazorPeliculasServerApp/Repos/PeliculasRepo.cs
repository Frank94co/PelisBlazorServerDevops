using AutoMapper;
using BlazorPeliculasServerApp.Data;
using BlazorPeliculasServerApp.DTOs;
using BlazorPeliculasServerApp.Entities;
using BlazorPeliculasServerApp.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorPeliculasServerApp.Repos
{
    public class PeliculasRepo
    {
        private readonly ApplicationDbContext context;
        private readonly IFileStore fileStore;
        private readonly IMapper mapper;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IHttpContextAccessor httpContextAccessor;

        public PeliculasRepo(ApplicationDbContext context, IFileStore filestore,
                             IMapper mapper, UserManager<IdentityUser> userManager,
                             IHttpContextAccessor httpContextAccessor)
        {
            this.context = context;
            this.fileStore = filestore;
            this.mapper = mapper;
            this.userManager = userManager;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<HomePageDTO> Get()
        {
            var limite = 6;

            var peliculasEnCartelera = await context.Peliculas
                .AsNoTracking()
                .Where(x => x.EnCartelera).Take(limite)
                .OrderByDescending(x => x.Lanzamiento)
                .ToListAsync();

            var fechaActual = DateTime.Today;

            var proximosEstrenos = await context.Peliculas
                .AsNoTracking()
                .Where(x => x.Lanzamiento > fechaActual)
                .OrderBy(x => x.Lanzamiento).Take(limite)
                .ToListAsync();

            var response = new HomePageDTO()
            {
                PeliculasEnCartelera = peliculasEnCartelera,
                ProximosEstrenos = proximosEstrenos
            };
            return response;
        }

        public async Task<PeliculaVisualizarDTO> Get(int id)
        {
            var pelicula = await context.Peliculas
                                      .Where(x => x.Id == id)
                                      .AsNoTracking()
                                      .Include(x => x.GenerosPelicula).ThenInclude(x => x.Genero)
                                      .Include(x => x.PeliculasActor).ThenInclude(x => x.Persona)
                                      .FirstOrDefaultAsync();
            if (pelicula == null)
            {
                return null;
            }

            //todo: vote
            var promedioVotos = 0.0;
            var votoUsuario = 0;

            if (await context.VotosPeliculas.AnyAsync(x => x.PeliculaId == id))
            {
                promedioVotos = await context.VotosPeliculas
                                     .Where(x => x.PeliculaId == id)
                                     .AverageAsync(x => x.Voto);

                if (httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
                {
                    var user = await userManager.FindByEmailAsync(httpContextAccessor.HttpContext.User.Identity.Name);
                    var userId = user.Id;
                    var votoUsuarioDB = await context.VotosPeliculas
                    .FirstOrDefaultAsync(x => x.PeliculaId == id && x.UserId == userId);

                    if (votoUsuarioDB != null)
                    {
                        votoUsuario = votoUsuarioDB.Voto;
                    }
                }
            }

            pelicula.PeliculasActor = pelicula.PeliculasActor.OrderBy(x => x.Orden).ToList();

            var model = new PeliculaVisualizarDTO
            {
                Pelicula = pelicula,
                Generos = pelicula.GenerosPelicula.Select(x => x.Genero).ToList(),
                Actores = pelicula.PeliculasActor.Select(x => new Persona
                {
                    Nombre = x.Persona.Nombre,
                    Foto = x.Persona.Foto,
                    Personaje = x.Personaje,
                    Id = x.Persona.Id
                }).ToList(),

                PromedioVotos = promedioVotos,
                VotoUsuario = votoUsuario
            };

            return model;
        }

        public async Task<PagedResponse<Pelicula>> Get(ParametrosBusquedaPeliculas parametrosBusqueda)
        {
            var peliculasQueryable = context.Peliculas.AsQueryable();

            if (!string.IsNullOrWhiteSpace(parametrosBusqueda.Titulo))
            {
                peliculasQueryable = peliculasQueryable
                    .Where(x => x.Titulo.ToLower().Contains(parametrosBusqueda.Titulo.ToLower()));
            }

            if (parametrosBusqueda.EnCartelera)
            {
                peliculasQueryable = peliculasQueryable.Where(x => x.EnCartelera);
            }
            if (parametrosBusqueda.Estrenos)
            {
                var hoy = DateTime.Today;
                peliculasQueryable = peliculasQueryable.Where(x => x.Lanzamiento >= hoy);
            }
            if (parametrosBusqueda.GeneroID != 0)
            {
                peliculasQueryable = peliculasQueryable
                    .Where(x => x.GenerosPelicula
                                 .Select(y => y.GeneroId).Contains(parametrosBusqueda.GeneroID));
            }

            var respuesta = new PagedResponse<Pelicula>
            {
                TotalPages = await peliculasQueryable.GetTotalPages(parametrosBusqueda.CantidadRegistros),
                Records = await peliculasQueryable.AsNoTracking().Page(parametrosBusqueda.Paginacion).ToListAsync()
            };
            return respuesta;
        }

        public async Task<PeliculaUpdateDTO> PutGet(int id)
        {
            var peliculaActionResult = await Get(id);
            if (peliculaActionResult==null)
            {
                return null;
            }

            var peliculaVisualizarDTO = peliculaActionResult;
            var generosSeleccionadosIds = peliculaVisualizarDTO.Generos.Select(x => x.Id).ToList();
            var generosNoSeleccionados = await context.Generos
                .AsNoTracking()
                .Where(x => !generosSeleccionadosIds.Contains(x.Id)).ToListAsync();

            var model = new PeliculaUpdateDTO
            {
                Pelicula = peliculaVisualizarDTO.Pelicula,
                GenerosNoSeleccionados = generosNoSeleccionados,
                GenerosSeleccionados = peliculaVisualizarDTO.Generos,
                Actores = peliculaVisualizarDTO.Actores
            };
            return model;
        }

        public async Task<int> Post(Pelicula pelicula)
        {
            if (!string.IsNullOrWhiteSpace(pelicula.Poster))
            {
                var fotoPersona = Convert.FromBase64String(pelicula.Poster);
                pelicula.Poster = await fileStore.GuardarArchivo(fotoPersona, "jpg", "peliculas");
            }
            if (pelicula.PeliculasActor != null)
            {
                for (int i = 0; i < pelicula.PeliculasActor.Count; i++)
                {
                    pelicula.PeliculasActor[i].Orden = i + 1;
                }
            }
            context.Add(pelicula);
            await context.SaveChangesAsync();
            return pelicula.Id;
        }

        public async Task Put(Pelicula pelicula)
        {
            var peliculaDB = await context.Peliculas.FirstOrDefaultAsync(x => x.Id == pelicula.Id);

            if (peliculaDB == null)
            {
                throw new ApplicationException($"Película {pelicula.Id} no encontrada");
            }

            peliculaDB = mapper.Map(pelicula, peliculaDB);

            if (!string.IsNullOrWhiteSpace(pelicula.Poster))
            {
                var posterImagen = Convert.FromBase64String(pelicula.Poster);
                peliculaDB.Poster = await fileStore.EditarArchivo(posterImagen, "jpg", "peliculas", peliculaDB.Poster);
            }

            await context.Database
                .ExecuteSqlInterpolatedAsync($"delete from generospeliculas where peliculaid={pelicula.Id}; delete from peliculasactores where peliculaid={pelicula.Id}");

            if (pelicula.PeliculasActor != null)
            {
                for (int i = 0; i < pelicula.PeliculasActor.Count; i++)
                {
                    pelicula.PeliculasActor[i].Orden = i + 1;
                }
            }

            peliculaDB.PeliculasActor = pelicula.PeliculasActor;
            peliculaDB.GenerosPelicula = pelicula.GenerosPelicula;

            await context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var exists = await context.Peliculas.AnyAsync(x => x.Id == id);
            if (!exists)
            {
                throw new ApplicationException($"Película {id} no encontrada");
            }
            context.Remove(new Pelicula { Id = id });
            await context.SaveChangesAsync();
        }
    }
}
