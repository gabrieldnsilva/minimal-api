using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.Dominio.Enums;

namespace minimal_api.Dominio.DTOs
{
    public class AdministratorsDTO
{
    public string Email { get; set; } = default!;
    public string Senha { get; set; } = default!;
    public Profile? Profile { get; set; } = default!;
}

}