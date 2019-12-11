using BlazorPeliculasServerApp.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorPeliculasServerApp.DTOs
{
    public class PeliculaUpdateDTO
    {
        public Pelicula Pelicula { get; set; }
        public List<Persona> Actores { get; set; }
        public List<Genero> GenerosSeleccionados { get; set; }
        public List<Genero> GenerosNoSeleccionados { get; set; }
    }
}
