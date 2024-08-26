using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using minimal_api.Dominio.Entities;
using minimal_api.Dominio.Interfaces;
using minimal_api.Infraestrutura.DB;

namespace minimal_api.Dominio.Services
{
    public class VehiclesServices : IVehiclesServices
    {
        public readonly DbContexto _context;

        public VehiclesServices(DbContexto contexto)
        {
            _context = contexto;
        }

        public void Remove(Vehicles vehicles)
        {
            _context.Vehicles.Remove(vehicles);
            _context.SaveChanges();

        }

        public void Update(Vehicles vehicles)
        {
            _context.Vehicles.Update(vehicles);
            _context.SaveChanges();
        }

        public Vehicles? SearchId(int id)
        {
            return _context.Vehicles.Where(v => v.Id == id).FirstOrDefault();
            
        }

        public void Include(Vehicles vehicles)
        {
            _context.Vehicles.Add(vehicles);
            _context.SaveChanges();
        }

        public List<Vehicles> All(int? pagina = 1, string? nome = null, string? marca = null)
        {
            var query = _context.Vehicles.AsQueryable();

            if (!string.IsNullOrEmpty(nome))
            {
                query = query.Where(v => EF.Functions.Like(v.Nome.ToLower(), $"%{nome}%"));
            }

            int ItemsPerPage = 10;  

            if (pagina != null) 
            query = query.Skip(((int)pagina - 1) * ItemsPerPage).Take(ItemsPerPage); 

            return query.ToList();
        }
    }
}