using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MottuProjeto.Models
{
    [Table("T_VM_USUARIO")]
    public class Usuario
    {
        [Key]
        [Column("ID_USUARIO")]
        public int Id { get; set; }

        [Required, StringLength(100)]
        [Column("NM_USUARIO")]
        public string Nome { get; set; } = string.Empty;

        [Required, StringLength(150), EmailAddress]
        [Column("DS_EMAIL")]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(60)]
        [Column("DS_USERNAME")]
        public string Username { get; set; } = string.Empty;

        // OBS: o nome indica HASH; por simplicidade estamos comparando texto puro no login.
        // Troque para armazenar hash (BCrypt) quando possível.
        [Required]
        [Column("DS_PASSWORD_HASH")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, StringLength(20)]
        [Column("DS_ROLE")]
        public string Role { get; set; } = "User";
    }
}
