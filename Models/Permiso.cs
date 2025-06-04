using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace digitalArsv1.Models
{
    public class Permiso
    {
        [Key, Column(Order = 0)]
        public int nro_usuario { get; set; }

        [Key, Column(Order = 1)]
        public string acceso { get; set; } = string.Empty;

        [ForeignKey("nro_usuario")]
        public Usuario Usuario { get; set; }
    }
}