using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace minimal_api.Dominio.DTOs
{
    public record VehiclesDTO
    {
        public string Nome { get; set; } = default!;
        public string Marca { get; set; } = default!;
        public  int Ano { get; set; } = default!;
}

}