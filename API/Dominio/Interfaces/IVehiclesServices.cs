using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.Dominio.Entities;

namespace minimal_api.Dominio.Interfaces
{
    public interface IVehiclesServices
    {
           List<Vehicles> All(int? pagina = 1, string? nome = null, string? marca = null); 
           Vehicles? SearchId(int id);
           void Include(Vehicles vehicles);
           void Update(Vehicles vehicles);
           void Remove(Vehicles vehicles);

    }

}