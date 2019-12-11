using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorPeliculasServerApp.DTOs
{
    public class PagedResponse<T>
    {
        public int TotalPages { get; set; }
        public List<T> Records { get; set; }
    }
}
