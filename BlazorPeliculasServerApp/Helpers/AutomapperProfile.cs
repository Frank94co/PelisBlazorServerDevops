using AutoMapper;
using BlazorPeliculasServerApp.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorPeliculasServerApp.Helpers
{
    public class AutomapperProfile : Profile
    {
        public AutomapperProfile()
        {
            CreateMap<Persona, Persona>().ForMember(x => x.Foto, option => option.Ignore());
            CreateMap<Pelicula, Pelicula>().ForMember(x => x.Poster, option => option.Ignore());
        }
    }
}
