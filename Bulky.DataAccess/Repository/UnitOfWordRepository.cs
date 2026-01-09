using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository
{
    public class UnitOfWordRepository: IUnitOfWorkRepository
    {
        private readonly ApplicationDbContext _db;
        public ICategoryRepository Category { get; private set; }
        public IProductRepository Product { get; private set; }
        public ICompanyRepository Company { get; private set; }
        
        public UnitOfWordRepository(ApplicationDbContext db)
        {
            _db = db;
            Category = new CategoryRepository(_db);
            Product = new ProductRepository(_db);
            Company = new CompanyRepository(_db);
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
