using digitalArsv1.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace digitalArsv1.Repositories
{
    public class PermisoRepository : Repository<Permiso>, IPermisoRepository
    {
        private readonly DigitalArsContext _context;

        public PermisoRepository(DigitalArsContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Permiso>> GetPermisosByUsuarioAsync(int nroUsuario)
        {
            return await _context.Permisos
                .Where(p => p.nro_usuario == nroUsuario)
                .ToListAsync();
        }
    
    public async Task<bool> ExistePermisoAsync(int nroUsuario, string acceso)
        {
            return await _context.Permisos
                .AnyAsync(p => p.nro_usuario == nroUsuario && p.acceso == acceso);
        }
       
    } 
}