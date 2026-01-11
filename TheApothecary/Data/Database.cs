using Microsoft.EntityFrameworkCore;
using TheApothecary.Models;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace TheApothecary.Data
{
    public class PharmacyDbContext : DbContext
    {
        public DbSet<Medicine> Medicines { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        // СИНГЛТОН для удобства
        private static PharmacyDbContext _instance;
        public static PharmacyDbContext Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PharmacyDbContext();
                    _instance.Initialize();
                }
                return _instance;
            }
        }

        // Публичный метод для сброса инстанса
        public static void ResetInstance()
        {
            if (_instance != null)
            {
                _instance.Dispose();
                _instance = null;
            }
        }

        public PharmacyDbContext() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pharmacy.db");
            Console.WriteLine($"База данных: {dbPath}");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настраиваем модель Medicine
            modelBuilder.Entity<Medicine>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Category).HasMaxLength(50);
            });

            // Настраиваем модель User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Username).IsUnique();
            });

            // Настраиваем модель CartItem
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(e => e.Id);
            });
        }

        // ПЕРВИЧНАЯ ИНИЦИАЛИЗАЦИЯ БАЗЫ
        public void Initialize()
        {
            try
            {
                // 1. Создаем базу если ее нет
                Database.EnsureCreated();

                // 2. Добавляем админа если нет
                if (!Users.Any(u => u.Username == "admin"))
                {
                    Users.Add(new User
                    {
                        Username = "admin",
                        Email = "admin@pharmacy.com",
                        Password = "admin123",
                        Role = UserRole.Admin
                    });
                    SaveChanges();
                }

                // 3. Добавляем лекарства если нет
                if (!Medicines.Any())
                {
                    Medicines.AddRange(GetDefaultMedicines());
                    SaveChanges();
                }

                Console.WriteLine($"База инициализирована: {Database.GetDbConnection().DataSource}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка инициализации базы: {ex.Message}");
                throw;
            }
        }

        // ДЕФОЛТНЫЕ ЛЕКАРСТВА
        private Medicine[] GetDefaultMedicines()
        {
            return new Medicine[]
            {
                new Medicine
                {
                    Id = 1,
                    Name = "Парацетамол",
                    Description = "Обезболивающее и жаропонижающее средство",
                    Price = 150.00m,
                    StockQuantity = 50,
                    RequiresPrescription = false,
                    Category = "Обезболивающие"
                },
                new Medicine
                {
                    Id = 2,
                    Name = "Амоксициллин",
                    Description = "Антибиотик широкого спектра действия",
                    Price = 450.00m,
                    StockQuantity = 20,
                    RequiresPrescription = true,
                    Category = "Антибиотики"
                },
                new Medicine
                {
                    Id = 3,
                    Name = "Ибупрофен",
                    Description = "Противовоспалительное средство",
                    Price = 200.00m,
                    StockQuantity = 35,
                    RequiresPrescription = false,
                    Category = "Обезболивающие"
                },
                new Medicine
                {
                    Id = 4,
                    Name = "Лоратадин",
                    Description = "Противоаллергическое средство",
                    Price = 180.00m,
                    StockQuantity = 40,
                    RequiresPrescription = false,
                    Category = "Антигистаминные"
                },
                new Medicine
                {
                    Id = 5,
                    Name = "Трамадол",
                    Description = "Сильное обезболивающее",
                    Price = 650.00m,
                    StockQuantity = 15,
                    RequiresPrescription = true,
                    Category = "Обезболивающие"
                }
            };
        }

        // ПРОСТОЙ МЕТОД ДЛЯ ПОЛУЧЕНИЯ ВСЕХ ЛЕКАРСТВ
        public List<Medicine> GetAllMedicines()
        {
            try
            {
                return Medicines.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения лекарств: {ex.Message}");
                return new List<Medicine>();
            }
        }

        // МЕТОД ДЛЯ ПОИСКА ЛЕКАРСТВ
        public List<Medicine> SearchMedicines(string query)
        {
            return Medicines
                .Where(m => m.Name.Contains(query) ||
                           m.Description.Contains(query) ||
                           m.Category.Contains(query))
                .ToList();
        }

        // МЕТОД ДЛЯ ДОБАВЛЕНИЯ ПОЛЬЗОВАТЕЛЯ
        public bool AddUser(User user)
        {
            try
            {
                if (Users.Any(u => u.Username == user.Username))
                    return false;

                Users.Add(user);
                SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // МЕТОД ДЛЯ ПРОВЕРКИ ЛОГИНА
        public User Login(string username, string password)
        {
            return Users.FirstOrDefault(u =>
                u.Username == username &&
                u.Password == password);
        }

        // Метод для проверки существования базы данных
        public bool DatabaseExists()
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pharmacy.db");
            return File.Exists(dbPath);
        }
    }
}