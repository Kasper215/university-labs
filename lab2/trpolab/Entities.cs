using System.Text.Json.Serialization;

// Бизнес-сущности
public class User
{
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class Client
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Contacts { get; set; } = string.Empty;
    [JsonIgnore]
    public List<Order> Orders { get; set; } = new();
}

public class Employee
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Contacts { get; set; } = string.Empty;
}

public class Product
{
    public int Id { get; set; }
    public string Article { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string PhotoPath { get; set; } = string.Empty;
}

public class Material
{
    public int Id { get; set; }
    public string Article { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public class Supplier
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Contacts { get; set; } = string.Empty;
    public List<Material> Materials { get; set; } = new();
}

public class Order
{
    public int Id { get; set; }
    public Client Client { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public Employee Manager { get; set; }
    public List<OrderHistory> History { get; set; } = new();
    
    public void RecalculateAmount()
    {
        Amount = Items.Sum(x => x.Price * x.Quantity);
    }
}

public class OrderItem
{
    public int Id { get; set; }
    public object Item { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public class OrderHistory
{
    public DateTime DateTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
}

public class ProductionTask
{
    public int Id { get; set; }
    public Order Order { get; set; }
    public string Workshop { get; set; } = string.Empty;
    public string Stage { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

