﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorPeliculasServerApp.Entities
{
    public class GeneroPelicula
    {
        public int PeliculaId { get; set; }
        public int GeneroId { get; set; }
        public Genero Genero { get; set; }
        public Pelicula Pelicula { get; set; }
    }
}
