using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MottuProjeto.Models
{
    [Table("T_VM_MOTO")]
    public class Moto
    {
        [Key]
        [Column("ID_MOTO")]
        public int Id { get; set; }

        [Required]
        [Column("DS_PLACA")]
        public string Placa { get; set; }

        [Required]
        [Column("NM_MODELO")]
        public string Modelo { get; set; }

        [Required]
        [Column("ID_AREA")]
        public int IdArea { get; set; }
    }
}
