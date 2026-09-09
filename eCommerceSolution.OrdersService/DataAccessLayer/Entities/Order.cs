using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace eCommerce.OrderMicroservice.DataAccessLayer.Entities;

public class Order
{
    [BsonId] // Marks this property as the primary key (_id field) in the MongoDB document.
    [BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public Guid _id { get; set; }

    [BsonRepresentation(MongoDB.Bson.BsonType.String)] // Use String format instead of Binary format (Bson => Binary JSON)
    public Guid OrderID { get; set; }

    [BsonRepresentation(MongoDB.Bson.BsonType.String)] 
    public Guid UserID { get; set; }
    [BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public DateTime OrderDate { get; set; }
    [BsonRepresentation(MongoDB.Bson.BsonType.Double)]
    public decimal TotalBill { get; set; }
    public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
