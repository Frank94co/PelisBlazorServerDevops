using AutoMapper;
using BlazorPeliculasServerApp.Data;
using BlazorPeliculasServerApp.DTOs;
using BlazorPeliculasServerApp.Entities;
using BlazorPeliculasServerApp.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorPeliculasServerApp.Repos
{
    public class PersonasRepo
    {
        private readonly ApplicationDbContext context;
        private readonly IFileStore fileStore;
        private readonly IMapper mapper;

        public PersonasRepo(ApplicationDbContext context, IFileStore filestore, IMapper mapper)
        {
            this.context = context;
            this.fileStore = filestore;
            this.mapper = mapper;
        }

        public async Task<PagedResponse<Persona>> Get(Paginacion paginacion)
        {
            var queryable = context.Personas.AsQueryable();
            var pagedResponse = new PagedResponse<Persona>();
            pagedResponse.TotalPages = await queryable.GetTotalPages(paginacion.CantidadRegistros);
            pagedResponse.Records = await queryable.AsNoTracking().Page(paginacion).ToListAsync();
            return pagedResponse;
        }

        public async Task<Persona> Get(int id)
        {
            return await context.Personas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<PersonaVisualizarDTO> GetPut(int id)
        {
            var persona = await context.Personas.Where(x => x.Id == id)
                                                .Include(x => x.PeliculasActor).ThenInclude(x => x.Pelicula)
                                                .FirstOrDefaultAsync();
            if (persona == null)
            {
                return null;
            }

            persona.PeliculasActor = persona.PeliculasActor
                                            .OrderByDescending(x => x.Pelicula.Lanzamiento)
                                            .ToList();
            var model = new PersonaVisualizarDTO();
            model.Persona = persona;
            model.Peliculas = persona.PeliculasActor.Select(x => x.Pelicula).ToList();
            return model;
        }

        public async Task<List<Persona>> Get(string textoBusqueda)
        {
            if (string.IsNullOrWhiteSpace(textoBusqueda))
            {
                return new List<Persona>();
            }
            textoBusqueda = textoBusqueda.ToLower();
            return await context.Personas
                                .Where(x => x.Nombre.ToLower().Contains(textoBusqueda))
                                .AsNoTracking().ToListAsync();
        }

        public async Task<int> Post(Persona persona)
        {
            if (!string.IsNullOrWhiteSpace(persona.Foto))
            {
                var fotoPersona = Convert.FromBase64String(persona.Foto);
                persona.Foto = await fileStore.GuardarArchivo(fotoPersona, "jpg", "personas");
            }
            context.Add(persona);
            await context.SaveChangesAsync();
            return persona.Id;
        }

        public async Task Put(Persona persona)
        {
            var personaDB = await context.Personas.FirstOrDefaultAsync(x => x.Id == persona.Id);
            if (personaDB == null)
            {
                throw new ApplicationException($"Persona {persona.Id} no encontrada");
            }
            personaDB = mapper.Map(persona, personaDB);
            if (!string.IsNullOrWhiteSpace(persona.Foto))
            {
                var fotoImg = Convert.FromBase64String(persona.Foto);
                personaDB.Foto = await fileStore.EditarArchivo(fotoImg, "jpg", "personas", personaDB.Foto);
            }
            await context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var exists = await context.Personas.AnyAsync(x => x.Id == id);
            if (!exists)
            {
                throw new ApplicationException($"Persona {id} no encontrada");
            }
            context.Remove(new Persona { Id = id });
            await context.SaveChangesAsync();
        }
    }
}
