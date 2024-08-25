using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.Entities;
using minimal_api.Dominio.Interfaces;
using minimal_api.Infraestrutura.DB;

namespace minimal_api.Dominio.Services
{
    public class AdministratorServices : IAdministratorServices
    {
        public readonly DbContexto _context;

        public AdministratorServices(DbContexto contexto)
        {
            _context = contexto;
        }
        public Administrators? Login(LoginDTO loginDTO)
        {
            var adm = _context.Administradores.Where(a => a.Email == loginDTO.Email 
                && a.Senha == loginDTO.Senha).FirstOrDefault();
            
            return adm;

        }
    }
}