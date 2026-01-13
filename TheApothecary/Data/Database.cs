using Microsoft.EntityFrameworkCore;
using TheApothecary.Models;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

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
                try
                {
                    // Закрываем все соединения
                    _instance.Database.CloseConnection();

                    // Отсоединяем все отслеживаемые сущности
                    var changedEntries = _instance.ChangeTracker.Entries()
                        .Where(e => e.State != EntityState.Detached)
                        .ToList();

                    foreach (var entry in changedEntries)
                    {
                        entry.State = EntityState.Detached;
                    }

                    // Освобождаем ресурсы
                    _instance.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при закрытии контекста: {ex.Message}");
                }
                finally
                {
                    _instance = null;
                }
            }
        }

        public PharmacyDbContext() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pharmacy.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Medicine>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Category).HasMaxLength(50);
                entity.Property(e => e.ImagePath).HasMaxLength(500);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.Property(e => e.Role).IsRequired();
            });

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
                // Создаем базу если ее нет
                Database.EnsureCreated();

                // Добавляем тестовых пользователей если их нет
                if (!Users.Any())
                {
                    Users.AddRange(GetDefaultUsers());
                    SaveChanges();
                }

                // Добавляем лекарства если нет
                if (!Medicines.Any())
                {
                    Medicines.AddRange(GetDefaultMedicines());
                    SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка инициализации базы: {ex.Message}");
                throw;
            }
        }

        // ТЕСТОВЫЕ ПОЛЬЗОВАТЕЛИ
        private User[] GetDefaultUsers()
        {
            return new User[]
            {
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@pharmacy.com",
                    Password = "admin123",
                    Role = UserRole.Admin
                },
                new User
                {
                    Id = 2,
                    Username = "employee",
                    Email = "employee@pharmacy.com",
                    Password = "employee123",
                    Role = UserRole.Employee
                },
                new User
                {
                    Id = 3,
                    Username = "customer",
                    Email = "customer@pharmacy.com",
                    Password = "customer123",
                    Role = UserRole.Customer
                }
            };
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
                    Category = "Обезболивающие",
                    ImagePath = "/Images/default_medicine.jpg"
                },
                new Medicine
                {
                    Id = 2,
                    Name = "Амоксициллин",
                    Description = "Антибиотик широкого спектра действия",
                    Price = 450.00m,
                    StockQuantity = 20,
                    RequiresPrescription = true,
                    Category = "Антибиотики",
                    ImagePath = "/Images/default_medicine.jpg"
                },
                new Medicine
                {
                    Id = 3,
                    Name = "Ибупрофен",
                    Description = "Противовоспалительное средство",
                    Price = 200.00m,
                    StockQuantity = 35,
                    RequiresPrescription = false,
                    Category = "Обезболивающие",
                    ImagePath = "/Images/default_medicine.jpg"
                }
            };
        }

        // ОСТАЛЬНЫЕ МЕТОДЫ
        public List<Medicine> GetAllMedicines()
        {
            try
            {
                return Medicines.ToList();
            }
            catch (Exception)
            {
                return new List<Medicine>();
            }
        }

        public List<Medicine> SearchMedicines(string query)
        {
            return Medicines
                .Where(m => m.Name.Contains(query) ||
                           m.Description.Contains(query) ||
                           m.Category.Contains(query))
                .ToList();
        }

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
            try
            {
                // Ищем по username (логину)
                var user = Users.FirstOrDefault(u =>
                    u.Username.ToLower() == username.ToLower() &&
                    u.Password == password);

                // Если не нашли по username, пробуем найти по email
                if (user == null)
                {
                    user = Users.FirstOrDefault(u =>
                        u.Email.ToLower() == username.ToLower() &&
                        u.Password == password);
                }

                Console.WriteLine($"Login attempt: {username}, found: {user != null}");
                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                return null;
            }
        }

        public bool DatabaseExists()
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pharmacy.db");
            return File.Exists(dbPath);
        }
    }
}