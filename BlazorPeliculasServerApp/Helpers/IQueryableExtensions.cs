using BlazorPeliculasServerApp.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorPeliculasServerApp.Helpers
{
    public static class IQueryableExtensions
    {
        public async static Task<int> GetTotalPages<T>(this IQueryable<T> queryable, int qtyRecordsToShow)
        {
            double count = await queryable.CountAsync();
            int totalPages = (int)Math.Ceiling(count / qtyRecordsToShow);
            return totalPages;
        }

        public static IQueryable<T> Page<T>(this IQueryable<T> queryable, Paginacion paginacion)
        {
            return queryable
                .Skip((paginacion.Pagina - 1) * paginacion.CantidadRegistros)
                .Take(paginacion.CantidadRegistros);
        }
    }
}
