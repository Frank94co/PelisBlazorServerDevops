using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorPeliculasServerApp.Helpers
{
    public interface IFileStore
    {
        Task<string> EditarArchivo(byte[] contenido, string extension, string nombreContenedor, string rutaArchivo);
        Task EliminarArchivo(string ruta, string nombreContenedor);
        Task<string> GuardarArchivo(byte[] contenido, string extension, string nombreContenedor);
    }
}
