using ITSystem.Data;
using ITSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITSystem.Services
{
    internal class ShopApp
    {
        private readonly ShopDbContext dbContext;

        public ShopApp(ShopDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        //internal void Init()
        //{
        // dbContext.Database.Migrate();

        //  if (dbContext.Products.Count() == 0)
        //  {
        // Skapa några produkter
        //  dbContext.Products.AddRange(
        //   new Product { Name = "Laptop", Description = "Gaming Laptop", Price = 15000 },
        //  new Product { Name = "Smartphone", Description = "Latest Smartphone", Price = 8000 },
        //   new Product { Name = "Tablet", Description = "10 inch Tablet", Price = 5000 },
        // );
        //   dbContext.SaveChanges();
        //  }
        // }
        internal void Init()
        {
            dbContext.Database.Migrate();

            // Mållista: 10 produkter (svenska texter)
            var wanted = new List<Product>
    {
        new Product { Name = "Bärbar dator",           Description = "Bärbar speldator",           Price = 15000 },
        new Product { Name = "Smartphone",             Description = "Senaste modellen",           Price = 8000  },
        new Product { Name = "Surfplatta",             Description = "10-tum surfplatta",          Price = 5000  },
        new Product { Name = "Hörlurar",               Description = "Aktiv brusreducering",       Price = 1500  },
        new Product { Name = "Smartklocka",            Description = "Tränings- och hälsospårning",Price = 2500  },
        new Product { Name = "Mekaniskt tangentbord",  Description = "RGB, blå brytare",           Price = 1299  },
        new Product { Name = "Gamingmus",              Description = "Ergonomisk, 6 knappar",      Price = 599   },
        new Product { Name = "4K-skärm",               Description = "27-tum 4K IPS",              Price = 4490  },
        new Product { Name = "Extern SSD",             Description = "1 TB USB-C NVMe",            Price = 1790  },
        new Product { Name = "Webbkamera",             Description = "1080p med mikrofon",         Price = 799   }
    };

            // Lägg bara till de som saknas (match på Name)
            foreach (var p in wanted)
            {
                if (!dbContext.Products.Any(x => x.Name == p.Name))
                    dbContext.Products.Add(p);
            }

            dbContext.SaveChanges();
        }


        internal void RunMenu()
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("╔══════════════════════════════════════════╗");
                Console.WriteLine("║   Rabar Tech – Beställningscentral       ║");
                Console.WriteLine("╚══════════════════════════════════════════╝");
                Console.WriteLine("[1] Visa produktkatalog");
                Console.WriteLine("[2] Skapa kundorder");
                Console.WriteLine("[3] Orderöversikt");
                Console.WriteLine("[0] Avsluta (till skrivbordet)");
                Console.Write("Välj ett alternativ: ");
                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": ListProducts(); break;
                    case "2": CreateOrder(); break;
                    case "3": ListOrders(); break;
                    case "0": return;
                    default: Console.WriteLine("Ogiltigt val."); break;
                }
            }
        }

        private void ListProducts()
        {
            Console.WriteLine("\n— Produkter —");
            var products = dbContext.Products.OrderBy(p => p.Id).ToList();

            for (int i = 0; i < products.Count; i++)
            {
                var p = products[i];
                Console.WriteLine($"Nr:{i + 1} (Id:{p.Id}) | {p.Name} | {p.Description} | {p.Price} kr");
            }
        }


        private void CreateOrder()
        {
            Console.Write("\nKundens namn: ");
            var customer = Console.ReadLine() ?? "";
            var order = new Order { CustomerName = customer };

            while (true)
            {
                // Hämta och visa samma ordning varje varv
                var products = dbContext.Products.OrderBy(p => p.Id).ToList();

                Console.WriteLine("\n— Produkter —");
                for (int i = 0; i < products.Count; i++)
                {
                    var p = products[i];
                    Console.WriteLine($"Nr:{i + 1} (Id:{p.Id}) | {p.Name} | {p.Description} | {p.Price} kr");
                }

                Console.Write("Ange Nr eller Id (ENTER för klar): ");
                var s = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(s)) break;

                if (!int.TryParse(s, out var sel))
                {
                    Console.WriteLine("Fel inmatning.");
                    continue;
                }

                // 1) Försök tolka som riktigt Id
                var product = products.FirstOrDefault(p => p.Id == sel);

                // 2) Om ej träff – tolka som visningsnummer (Nr)
                if (product == null && sel >= 1 && sel <= products.Count)
                    product = products[sel - 1];

                if (product == null)
                {
                    Console.WriteLine("Produkt finns ej.");
                    continue;
                }

                Console.Write("Antal: ");
                if (!int.TryParse(Console.ReadLine(), out var qty) || qty <= 0)
                {
                    Console.WriteLine("Fel antal.");
                    continue;
                }

                order.Items.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = qty,
                    UnitPrice = product.Price
                });

                Console.WriteLine($"+ {qty} x {product.Name} tillagd.");
            }

            if (order.Items.Count == 0)
            {
                Console.WriteLine("Ingen rad lades till.");
                return;
            }

            dbContext.Orders.Add(order);
            dbContext.SaveChanges();
            Console.WriteLine($"Order #{order.Id} skapad med {order.Items.Count} rader.");
        }


        private void ListOrders()
        {
            Console.WriteLine("\n— Ordrar —");

            var orders = dbContext.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .OrderByDescending(o => o.Id)
                .ToList();

            if (!orders.Any()) { Console.WriteLine("Inga ordrar."); return; }

            foreach (var o in orders)
            {
                Console.WriteLine($"Order #{o.Id} | {o.CreatedAt:g} | Kund: {o.CustomerName}");
                decimal total = 0;
                foreach (var i in o.Items)
                {
                    var name = i.Product?.Name ?? $"ProductId {i.ProductId}";
                    var row = i.UnitPrice * i.Quantity;
                    total += row;
                    Console.WriteLine($"   - {i.Quantity} x {name} , {i.UnitPrice} kr = {row} kr");
                }
                Console.WriteLine($"   Totalt: {total} kr");
            }
        }
    }
}
