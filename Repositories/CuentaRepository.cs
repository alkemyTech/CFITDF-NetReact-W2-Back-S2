using digitalArsv1.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace digitalArsv1.Repositories
{
    // CuentaRepository hereda de Repository<Cuenta> e implementa ICuentaRepository
    public class CuentaRepository : Repository<Cuenta>, ICuentaRepository
    {
        private readonly DigitalArsContext _context;

        public CuentaRepository(DigitalArsContext context) : base(context)
        {
            _context = context;
        }

        // Devuelve todas las cuentas con su usuario relacionado
        public async Task<IEnumerable<Cuenta>> GetAllWithUsuarioAsync()
        {
            return await _context.Cuentas
                                 .Include(c => c.Usuario)    // Incluye datos del Usuario
                                 .ToListAsync();
        }

        // Crea una nueva cuenta y guarda los cambios
        public async Task CrearAsync(Cuenta nroCuenta)
        {
            _context.Cuentas.Add(nroCuenta);
            await _context.SaveChangesAsync();             // Persiste en BD
        }

        // Verifica si existe alguna cuenta para el nro_cliente dado
        public async Task<bool> ExisteCuenta(int nroCliente)
        {
            return await _context.Cuentas
                                 .AnyAsync(c => c.nro_cliente == nroCliente);
        }

        // Obtiene una cuenta por su ID (nro_cuenta) incluyendo el Usuario
        public async Task<Cuenta> GetByIdWithUsuarioAsync(int id)
        {
            return await _context.Cuentas
                                 .Include(c => c.Usuario)    // Incluye datos del Usuario
                                 .FirstOrDefaultAsync(c => c.nro_cuenta == id);
        }

        // Método específico para calcular el saldo de una cuenta:
        // Suma los montos recibidos (destino) y resta los montos enviados (origen)
        public async Task<decimal> ObtenerSaldoAsync(int nroCuenta)
        {
            var cuenta = await _context.Cuentas.FirstOrDefaultAsync(c => c.nro_cuenta == nroCuenta);
            return cuenta?.saldo ?? 0; // Si la cuenta no existe o el saldo es null, retorna 0
        }
        // ✅ NUEVO: Devuelve todas las cuentas asociadas a un cliente específico (nro_cliente)
        public async Task<List<Cuenta>> GetByClienteAsync(int nroCliente)
        {
            return await _context.Cuentas
         .Include(c => c.Usuario)
         .Where(c => c.nro_cliente == nroCliente && c.estado == true)
         .ToListAsync();
        }

        // ✅ NUEVO: Guarda cambios pendientes en el contexto (SaveChangesAsync)
        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task<Cuenta?> GetByCBUAsync(string cbu)
        {
            return await _context.Cuentas
                                 .Include(c => c.Usuario)
                                 .FirstOrDefaultAsync(c => c.CBU == cbu);
        }

        public async Task<Cuenta?> GetByAliasAsync(string alias)
        {
            return await _context.Cuentas
                                 .Include(c => c.Usuario)
                                 .FirstOrDefaultAsync(c => c.alias == alias);
        }
    }
}
