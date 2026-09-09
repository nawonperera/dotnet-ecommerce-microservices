using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace eCommerce.DataAccessLayer.Entities;

public class Product
{
    [Key]
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public string Category {  get; set; }
    public double? UnitPrice { get; set; }
    public int? QuantityInStock { get; set; }
}
