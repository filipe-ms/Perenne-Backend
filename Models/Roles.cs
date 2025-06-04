using System.ComponentModel.DataAnnotations;

namespace perenne.Models
{
    public enum SystemRole
    {
        None,

        [Display(Name = "Diretor")]
        SuperAdmin,

        [Display(Name = "Administrador")]
        Admin,

        [Display(Name = "Moderador")]
        Moderator,

        [Display(Name = "Usuário")]
        User
    }

    public enum GroupRole
    {
        None,

        [Display(Name = "Coordenador")]
        Coordinator,

        [Display(Name = "Contribuidor")]
        Contributor,

        [Display(Name = "Membro")]
        Member
    }
}
