using digitalArsv1.Models;
using System.ComponentModel.DataAnnotations;

public class Usuario
{
    [Key]
    public int nro_cliente { get; set; }

    public string nombre { get; set; }
    public string apellido { get; set; }
    public string direccion { get; set; }
    public string mail { get; set; }
    public bool estado { get; set; }
    public string tipo_cliente { get; set; }
    public string telefono { get; set; }
    public string password_hash { get; set; }

    // Relación 1:N con Cuenta
    public ICollection<Cuenta> Cuentas { get; set; } = new List<Cuenta>();

    public ICollection<Permiso> Permisos { get; set; } = new List<Permiso>();


}