using digitalArsv1.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace digitalArsv1.Repositories
{
    public interface IPermisoRepository : IRepository<Permiso>
    {
        Task<IEnumerable<Permiso>> GetPermisosByUsuarioAsync(int nroUsuario);
        Task<bool> ExistePermisoAsync(int nroUsuario, string acceso); // <-- Agregado
       
    }
}