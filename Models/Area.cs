using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MottuProjeto.Models
{
    [Table("T_VM_AREA")]
    public class Area
    {
        [Key]
        [Column("ID_AREA")]
        public int Id { get; set; }

        [Required]
        [Column("NM_AREA")]
        public string Nome { get; set; }
    }
}
