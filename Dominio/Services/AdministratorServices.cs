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
            var adm = _context.Administrators.Where(a => a.Email == loginDTO.Email 
                && a.Senha == loginDTO.Senha).FirstOrDefault();
            
            return adm;

        }

        public Administrators? SearchId(int id)
        {
            return _context.Administrators.Where(v => v.Id == id).FirstOrDefault();
            
        }

        public List<Administrators> All(int? pagina)
        {
            var query = _context.Administrators.AsQueryable();
            int ItemsPerPage = 10;  

            if (pagina != null) 
            query = query.Skip(((int)pagina - 1) * ItemsPerPage).Take(ItemsPerPage); 

            return query.ToList();
        }

        public Administrators Include(Administrators administrators)
        {
            _context.Administrators.Add(administrators);
            _context.SaveChanges();

            return administrators;
        }
    }
}