namespace digitalArsv1.DTOs
{
    
    /// DTO para solicitar una transferencia entre dos cuentas.
  
    public class TransferenciaDTO
    {

        /// La cuenta de origen (pertenece al usuario autenticado;selecciona cuenta orig  porque un usario puede tener 2 cuentas (pesos y usd)).
        /// La cuenta de destino (puede existir o no; si no existe, igualmente se debita origen).
        /// El monto a transferir (decimal(12,2)), debe ser > 0.
        public decimal Monto { get; set; }
        public string Destino { get; set; } = default!;  // puede ser alias o CBU
        public int NroCuentaOrigen { get; set; } // para validar que la cuenta origen es del usuario
    }
}
