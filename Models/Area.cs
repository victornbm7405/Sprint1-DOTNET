using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MottuProjeto.Models
{
    [Table("T_VM_AREA")] // mantenha o nome da sua tabela se já tiver diferente
    public class Area
    {
        [Key]
        [Column("ID_AREA")]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Column("NM_AREA")]
        public string Nome { get; set; } = string.Empty; // <= remove o CS8618
    }
}
