using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace perenne.Models
{
    public enum GroupRole
    {
        [Display(Name = "Coordenador")]
        Coordinator,

        [Display(Name = "Contribuidor")]
        Contributor,

        [Display(Name = "Membro")]
        Member
    }

    public enum SystemRole
    {
        [Display(Name = "Diretor")]
        SuperAdmin,

        [Display(Name = "Administrador")]
        Admin,

        [Display(Name = "Moderador")]
        Moderator,

        [Display(Name = "Usuário")]
        User
    }
}
