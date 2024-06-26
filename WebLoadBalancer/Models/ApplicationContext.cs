﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace WebLoadBalancer.Models
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) 
        {
           
        }

        public DbSet<web_user> Users { get; set; }
        public DbSet<EquationSol> EquationSols { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<web_user>().ToTable("web_user");
            modelBuilder.Entity<EquationSol>().ToTable("equation");
        }


        public bool IsDatabaseConnected()
        {
            using (IDbContextTransaction transaction = Database.BeginTransaction())
            {
                try
                {
                    transaction.Commit();
                    Console.WriteLine("Підключено до бази даних.");
                    return true;
                }
                catch
                {
                    Console.WriteLine("Не вдалося підключитися до бази даних.");
                    return false;
                }
            }
        }
    }
}
