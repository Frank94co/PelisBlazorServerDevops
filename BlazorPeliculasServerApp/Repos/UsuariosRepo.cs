using BlazorPeliculasServerApp.Data;
using BlazorPeliculasServerApp.DTOs;
using BlazorPeliculasServerApp.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlazorPeliculasServerApp.Repos
{
    public class UsuariosRepo
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;

        public UsuariosRepo(ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        public async Task<PagedResponse<UsuarioDTO>> Get(Paginacion paginacion)
        {
            var queryable = context.Users.AsQueryable();
            var response = new PagedResponse<UsuarioDTO>();
            response.TotalPages = await queryable.GetTotalPages(paginacion.CantidadRegistros);
            response.Records= await queryable.Page(paginacion)
                .Select(x => new UsuarioDTO { Email = x.Email, UserId = x.Id })
                .ToListAsync();

            return response;
        }

        public async Task<List<RolDTO>> GetRoles()
        {
            return await context.Roles
                .Select(x => new RolDTO { Nombre = x.Name, RoleId = x.Id })
                .ToListAsync();
        }

        public async Task AsignarRolUsuario(EditarRolDTO editarRolDTO)
        {
            var usuario = await userManager.FindByIdAsync(editarRolDTO.UserId);
            await userManager.AddClaimAsync(usuario, new Claim(ClaimTypes.Role, editarRolDTO.RoleId));
            await userManager.AddToRoleAsync(usuario, editarRolDTO.RoleId);
        }
        
        public async Task RemoverUsuarioRol(EditarRolDTO editarRolDTO)
        {
            var usuario = await userManager.FindByIdAsync(editarRolDTO.UserId);
            await userManager.RemoveClaimAsync(usuario, new Claim(ClaimTypes.Role, editarRolDTO.RoleId));
            await userManager.RemoveFromRoleAsync(usuario, editarRolDTO.RoleId);
        }
    }
}
