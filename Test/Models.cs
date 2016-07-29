using System;
using System.Collections.Generic;
using EntityRepository;

namespace Test
{
    [Serializable]
    public class Simple : Entity
    {
        public Simple(Guid id) : base(id) { }
        public int IntProperty { get; set; }
        public string StrProperty { get; set; }
    }

    [Serializable]
    public class Container : Entity
    {
        public Container(Guid id) : base(id) { }
        public TransactionalEntityReference<Simple> SimpleReference;
        public string StrProperty { get; set; }
    }

    [Serializable]
    public class ObjList : Entity
    {
        public ObjList(Guid id) : base(id) { }
        public TransactionalEntityList<Simple> SimpleList;
        public int IntProperty { get; set; }
    }

    [Serializable]
    public class BankAccount : Entity
    {
        public BankAccount(Guid id) : base(id) { }

        public string Name { get; set; }
        public int Balance { get; set; }
    }

    [Serializable]
    public class Product : Entity
    {
        public Product(Guid id) : base(id) { }

        public string Name { get; set; }
        public int UnitPrice { get; set; }
        public int Stock { get; set; }
    }

    [Serializable]
    public class Order : Entity
    {
        public Order(Guid id) : base(id) { }

        public string Customer { get; set; }
        public int Total { get; set; }
        public List<OrderItem> Items { get; set; }
    }

    [Serializable]
    public class OrderItem
    {
        public string Product { get; set; }
        public int Qty { get; set; }
        public int UnitPrice { get; set; }
        public int Subtotal { get; set; }
    }
}
