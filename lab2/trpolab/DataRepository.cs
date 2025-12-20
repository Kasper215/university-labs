    using System.Collections.Generic;
    using System.Linq;
    using System;
    using System.IO;
    using System.Text.Json;

    public static class DataRepository
    {
        public static List<User> Users = new();
        public static List<Client> Clients = new();
        public static List<Product> Products = new();
        public static List<Material> Materials = new();
        public static List<Order> Orders = new();
        public static List<Employee> Employees = new();
        public static List<Supplier> Suppliers = new();

        private static readonly string DataDir = Path.Combine(AppContext.BaseDirectory, "data");
        private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions { WriteIndented = true };

        public static void LoadAll()
        {
            Directory.CreateDirectory(DataDir);
            if (!File.Exists(Path.Combine(DataDir, "users.json")))
            {
                InitTestData();
                SaveAll();
                return;
            }
            Users = Read<List<User>>("users.json") ?? new();
            Clients = Read<List<Client>>("clients.json") ?? new();
            Employees = Read<List<Employee>>("employees.json") ?? new();
            Products = Read<List<Product>>("products.json") ?? new();
            Materials = Read<List<Material>>("materials.json") ?? new();
            Suppliers = Read<List<Supplier>>("suppliers.json") ?? new();
            var orderDtos = Read<List<OrderDto>>("orders.json") ?? new();
            // Восстановление заказов из DTO
            Orders = new List<Order>();
            foreach (var dto in orderDtos)
            {
                RestoreOrderFromDto(dto);
            }
        }

        private static void RestoreOrderFromDto(OrderDto dto)
        {
            var ord = new Order
            {
                Id = dto.Id,
                Client = Clients.FirstOrDefault(c => c.Id == dto.ClientId),
                Manager = Employees.FirstOrDefault(e => e.Id == dto.ManagerId),
                Status = dto.Status,
                Amount = dto.Amount,
                History = dto.History ?? new()
            };
            ord.Items = new List<OrderItem>();
            foreach (var it in dto.Items ?? new())
            {
                object itemObj = it.Kind == "Product"
                    ? (object)Products.FirstOrDefault(p => p.Id == it.RefId)
                    : (object)Materials.FirstOrDefault(m => m.Id == it.RefId);
                ord.Items.Add(new OrderItem { Id = it.Id, Item = itemObj, Quantity = it.Quantity, Price = it.Price });
            }
            Orders.Add(ord);
            if (ord.Client != null) ord.Client.Orders.Add(ord);
        }

        public static void SaveAll()
        {
            Directory.CreateDirectory(DataDir);
            Write("users.json", Users);
            Write("clients.json", Clients);
            Write("employees.json", Employees);
            Write("products.json", Products);
            Write("materials.json", Materials);
            Write("suppliers.json", Suppliers);
            // Заказы → DTO
            var orderDtos = Orders.Select(o => new OrderDto
            {
                Id = o.Id,
                ClientId = o.Client?.Id ?? 0,
                ManagerId = o.Manager?.Id ?? 0,
                Status = o.Status,
                Amount = o.Amount,
                History = o.History,
                Items = o.Items.Select(i => new OrderItemDto
                {
                    Id = i.Id,
                    Kind = i.Item is Product ? "Product" : "Material",
                    RefId = i.Item is Product p ? p.Id : (i.Item is Material m ? m.Id : 0),
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList()
            }).ToList();
            Write("orders.json", orderDtos);
        }

        private static T? Read<T>(string fileName)
        {
            if (!File.Exists(Path.Combine(DataDir, fileName))) return default;
            var json = File.ReadAllText(Path.Combine(DataDir, fileName));
            return JsonSerializer.Deserialize<T>(json, JsonOpts);
        }

        private static void Write<T>(string fileName, T obj)
        {
            var path = Path.Combine(DataDir, fileName);
            var json = JsonSerializer.Serialize(obj, JsonOpts);
            File.WriteAllText(path, json);
        }

        public static void InitTestData(int multiplier = 1)
        {
            if (multiplier < 1) multiplier = 1;
            Users.Add(new User { Login = "admin", Password = "admin", FullName = "Администратор", Role = "Администратор" });
            Users.Add(new User { Login = "manager", Password = "manager", FullName = "Менеджер", Role = "Менеджер" });
            for (int i = 1; i <= 5; i++)
                Employees.Add(new Employee { Id = i, FullName = $"Сотрудник {i}", Position = "Оператор", Contacts = $"phone{i}@mail.ru" });
            for (int i = 1; i <= 10; i++)
                Clients.Add(new Client { Id = i, FullName = $"Клиент {i}", Contacts = $"client{i}@local.ru" });
            for (int i = 1; i <= 20; i++)
                Products.Add(new Product { Id = i, Article = $"P-{i:000}", Description = $"Товар {i}", Price = 1000 + i * 50, PhotoPath = "" });
            for (int i = 1; i <= 10; i++)
                Materials.Add(new Material { Id = i, Article = $"M-{i:000}", Quantity = 200 - i * 5, Price = 100 + i * 7 });
            for (int i = 1; i <= 3; i++)
                Suppliers.Add(new Supplier { Id = i, Name = $"Поставщик {i}", Contacts = $"supplier{i}@firm.ru", Materials = Materials.Skip((i-1)*3).Take(3).ToList() });
            for (int i = 1; i <= 5; i++)
            {
                var client = Clients[i - 1];
                var manager = Employees[0];
                var order = new Order
                {
                    Id = i,
                    Client = client,
                    Manager = manager,
                    Status = "Создан",
                    Amount = 2500 + i * 100,
                    Items = new List<OrderItem> {
                        new OrderItem { Id = i*10+1, Item = Products[i], Quantity = 2, Price = Products[i].Price },
                        new OrderItem { Id = i*10+2, Item = Materials[i], Quantity = 5, Price = Materials[i].Price }
                    },
                    History = new List<OrderHistory> {
                       new OrderHistory { DateTime = DateTime.Today.AddDays(-i), Status = "Создан", Comment = "Первичное создание заказа" }
                    }
                };
                Orders.Add(order);
                client.Orders.Add(order);
            }
        }

        private class OrderDto
        {
            public int Id { get; set; }
            public int ClientId { get; set; }
            public int ManagerId { get; set; }
            public string Status { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public List<OrderItemDto>? Items { get; set; }
            public List<OrderHistory>? History { get; set; }
        }

        private class OrderItemDto
        {
            public int Id { get; set; }
            public string Kind { get; set; } = string.Empty; // Product|Material
            public int RefId { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
        }
    }
