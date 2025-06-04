using System.ComponentModel.DataAnnotations;

namespace perenne.Models
{
    public enum RequestStatus
    {
        [Display(Name = "Pendente")]
        Pending,

        [Display(Name = "Aprovado")]
        Approved,

        [Display(Name = "Rejeitado")]
        Rejected
    }
}
