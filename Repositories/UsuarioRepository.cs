using digitalArsv1.Models;
using digitalArsv1.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace digitalArsv1.Repositories
{
    public class UsuarioRepository : Repository<Usuario>, IUsuarioRepository
    {
        public UsuarioRepository(DigitalArsContext context) : base(context)
        {
        }

        public async Task<List<Usuario>> ObtenerUsuariosSinCuentaAsync()
        {
            return await _context.Usuarios
                .Where(u => !_context.Cuentas.Any(c => c.nro_cliente == u.nro_cliente))
                .ToListAsync();
        }

        public async Task DesactivarUsuariosAsync(List<Usuario> usuarios)
        {
            foreach (var usuario in usuarios)
            {
                usuario.estado = false;
                _context.Usuarios.Update(usuario);
            }
        }

        public async Task<Usuario> ObtenerPorEmailAsync(string mail)
        {
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.mail == mail);
        }

        public async Task CrearAsync(Usuario usuario)
        {
            await _context.Usuarios.AddAsync(usuario);
        }

        public async Task<Usuario?> GetByMailWithCuentasAsync(string mail)
        {
            return await _context.Usuarios
                .Include(u => u.Cuentas)
                .FirstOrDefaultAsync(u => u.mail == mail);
        }
        
        public async Task<UsuarioProfileDTO> GetBasicProfileByIdAsync(int userId)
        {
            return await _context.Usuarios
                .Where(u => u.nro_cliente == userId)
                .Select(u => new UsuarioProfileDTO
                {
                    Nombre = u.nombre,
                    Apellido = u.apellido,
                    Direccion = u.direccion,
                    Mail = u.mail,
                    Telefono = u.telefono
                })
                .FirstOrDefaultAsync();
        }
        public async Task<bool> UpdateProfileAsync(UsuarioUpdateDTO updateDto)
        {
            var usuario = await _context.Usuarios.FindAsync(updateDto.Id);
            if (usuario == null)
                return false;

            usuario.nombre = updateDto.Nombre;
            usuario.apellido = updateDto.Apellido;
            usuario.direccion = updateDto.Direccion;
            usuario.mail = updateDto.Mail;
            usuario.telefono = updateDto.Telefono;

            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<Usuario?> GetUsuarioByIdAsync(int id)
        {
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.nro_cliente == id);
        }
    }

}