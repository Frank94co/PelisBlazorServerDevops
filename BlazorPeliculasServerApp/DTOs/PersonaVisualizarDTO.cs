using BlazorPeliculasServerApp.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorPeliculasServerApp.DTOs
{
    public class PersonaVisualizarDTO
    {
        public Persona Persona { get; set; }
        public List<Pelicula> Peliculas { get; set; }
    }
}
