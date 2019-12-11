using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorPeliculasServerApp.Helpers
{
    public class AzStorageFileStore : IFileStore
    {
        private readonly string connectionString;

        public AzStorageFileStore(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("AzureStorage");
        }
        public async Task<string> EditarArchivo(byte[] contenido, string extension, 
            string nombreContenedor, string rutaArchivo)
        {
            if (!string.IsNullOrEmpty(rutaArchivo))
            {
                await EliminarArchivo(rutaArchivo, nombreContenedor);
            }
            return await GuardarArchivo(contenido, extension, nombreContenedor);
        }

        public async Task EliminarArchivo(string ruta, string nombreContenedor)
        {
            var cuenta = CloudStorageAccount.Parse(connectionString);
            var servicioCliente = cuenta.CreateCloudBlobClient();
            var contenedor = servicioCliente.GetContainerReference(nombreContenedor);

            var blobName = Path.GetFileName(ruta);
            var blob = contenedor.GetBlobReference(blobName);
            await blob.DeleteIfExistsAsync();
        }

        public async Task<string> GuardarArchivo(byte[] contenido, string extension, string nombreContenedor)
        {
            var cuenta = CloudStorageAccount.Parse(connectionString);
            var servicioCliente = cuenta.CreateCloudBlobClient();
            var contenedor = servicioCliente.GetContainerReference(nombreContenedor);
            await contenedor.CreateIfNotExistsAsync();
            await contenedor.SetPermissionsAsync(new BlobContainerPermissions
            {
                PublicAccess=BlobContainerPublicAccessType.Blob
            });

            var nombreArchivo = $"{Guid.NewGuid()}.{extension}";
            var blob = contenedor.GetBlockBlobReference(nombreArchivo);
            await blob.UploadFromByteArrayAsync(contenido, 0, contenido.Length);
            blob.Properties.ContentType = "image/jpg";
            return blob.Uri.ToString();
        }
    }
}
