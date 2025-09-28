using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MottuProjeto.Models
{
    /// <summary>
    /// Representa uma �rea/regi�o operacional.
    /// </summary>
    [Table("T_VM_AREA")]
    public class Area
    {
        /// <summary>
        /// Identificador �nico da �rea.
        /// </summary>
        [Key]
        [Column("ID_AREA")]
        public int Id { get; set; }

        /// <summary>
        /// Nome da �rea (ex.: Centro, Zona Leste).
        /// </summary>
        [Required]
        [StringLength(100)]
        [Column("NM_AREA")]
        public string Nome { get; set; } = string.Empty;
    }
}
