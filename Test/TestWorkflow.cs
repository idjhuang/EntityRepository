using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EntityRepository;
using ObjectResourceManager;
using Workflow;
using WorkflowImpl;

namespace Test
{
    public static class TestWorkflow
    {
        // test data
        public static Dictionary<string, TransactionalEntity<Product>> Products { get; }
        public static Dictionary<string, TransactionalEntity<BankAccount>> BankAccounts { get; }
        public static List<TransactionalEntity<Order>> Orders { get; }

        static TestWorkflow()
        {
            Products = new Dictionary<string, TransactionalEntity<Product>>();
            BankAccounts = new Dictionary<string, TransactionalEntity<BankAccount>>();
            Orders = new List<TransactionalEntity<Order>>();
        }

        public static void Init(string connStr)
        {
            TransactionScopeUtil.ConnectionString = connStr;
            // load test data
            var products =
                CollectionRepository.GetCollection(typeof (Product))
                    .GetAllEntities(typeof (Product))
                    .Select(e => e as TransactionalEntity<Product>);
            foreach (var product in products)
            {
                Products.Add(product.GetEntity().Name, product);
            }
            var accounts =
                CollectionRepository.GetCollection(typeof (BankAccount))
                    .GetAllEntities(typeof (BankAccount))
                    .Select(e => e as TransactionalEntity<BankAccount>);
            foreach (var account in accounts)
            {
                BankAccounts.Add(account.GetEntity().Name, account);
            }
            var orders =
                CollectionRepository.GetCollection(typeof (Order))
                    .GetAllEntities(typeof (Order))
                    .Select(e => e as TransactionalEntity<Order>);
            foreach (var order in orders)
            {
                Orders.Add(order);
            }
        }

        public static void InitData()
        {
            var productEntities = new Product[]
            {
                new Product(Guid.NewGuid()) {Name = "A", UnitPrice = 50, Stock = 100},
                new Product(Guid.NewGuid()) {Name = "B", UnitPrice = 10, Stock = 500},
                new Product(Guid.NewGuid()) {Name = "C", UnitPrice = 5, Stock = 1000}
            };
            foreach (var productEntity in productEntities)
            {
                if (Products.ContainsKey(productEntity.Name)) continue;
                var product = new TransactionalEntity<Product>(productEntity);
                product.Update();
                Products.Add(productEntity.Name, product);
            }

            var accountEntities = new BankAccount[]
            {
                new BankAccount(Guid.NewGuid()) {Name = "User1", Balance = 10000},
                new BankAccount(Guid.NewGuid()) {Name = "User2", Balance = 5000},
                new BankAccount(Guid.NewGuid()) {Name = "User3", Balance = 15000}
            };
            foreach (var accountEntity in accountEntities)
            {
                if (BankAccounts.ContainsKey(accountEntity.Name)) continue;
                var account = new TransactionalEntity<BankAccount>(accountEntity);
                account.Update();
                BankAccounts.Add(accountEntity.Name, account);
            }
        }
    }

    public interface InvokeTask
    {
        void Invoke(Guid workflowId, object[] parameters);
        string Name { get; }
    }

    public class TaskPickupProduct : ITask, InvokeTask
    {
        // parameters of task
        [Serializable]
        public class ParamPickupProduct
        {
            public string Customer { get; set; }
            public string Product { get; set; }
            public int Quantity { get; set; }
        }

        public void Invoke(Guid workflowId, object[] parameters)
        {
            var paramPickupProduct = new ParamPickupProduct
            {
                Customer = parameters[0] as string,
                Product = parameters[1] as string,
                Quantity = int.Parse(parameters[2] as string)
            };
            try
            {
                WorkflowRepository.Execute("Test.TaskPickupProduct", workflowId, paramPickupProduct);
            }
            catch (Exception e)
            {
                Debug.Print($"Invoke Task Pickup Product failure: {e.Message}");
            }
        }

        public string Name => GetType().AssemblyQualifiedName;

        public void Execute(WorkflowContext context, TaskExecutionRecord execRecord, object parameters)
        {
            // convert parameters
            var param = parameters as ParamPickupProduct;
            // check pre-condition
            if (!TestWorkflow.Products.ContainsKey(param.Product))
            {
                context.Status = WorkflowStatus.Failed;
                context.Message = $"Product {param.Product} not found.";
                return;
            }
            // retrieve resources
            Order cart;
            if (context.State.ContainsKey("Cart"))
                cart = context.State["Cart"] as Order;
            else
            {
                cart = new Order(Guid.NewGuid())
                {
                    Customer = param.Customer,
                    Total = 0,
                    Items = new List<OrderItem>()
                };
                context.State.Add("Cart", cart);
            }
            try
            {
                using (var ts = TransactionScopeUtil.GetTransactionScope())
                using (var dbConn = TransactionScopeUtil.CreateDbConnection())
                {
                    dbConn.Open();
                    var product = TestWorkflow.Products[param.Product];
                    var productEntity = product.GetEntity(LockMode.Exclusive);
                    // process
                    if (context.Status == WorkflowStatus.Ready)
                    {
                        bool isUpdate = false;
                        int deltaQty;
                        var orderItem = cart.Items.FirstOrDefault(i => i.Product.Equals(param.Product));
                        if (orderItem == null)
                        {
                            deltaQty = param.Quantity;
                            orderItem = new OrderItem
                            {
                                Product = param.Product,
                                Qty = param.Quantity,
                                UnitPrice = productEntity.UnitPrice,
                                Subtotal = param.Quantity*productEntity.UnitPrice
                            };
                        }
                        else
                        {
                            isUpdate = true;
                            execRecord.CompensationData = new OrderItem
                            {
                                Product = orderItem.Product,
                                Qty = orderItem.Qty,
                                UnitPrice = orderItem.UnitPrice,
                                Subtotal = orderItem.Subtotal
                            };
                            deltaQty = param.Quantity - orderItem.Qty;
                        }
                        if (productEntity.Stock < deltaQty)
                        {
                            context.Status = WorkflowStatus.Failed;
                            context.Message = $"Product {param.Product} is out of stock.";
                            return;
                        }
                        if (!isUpdate)
                        {
                            cart.Items.Add(orderItem);
                        }
                        else
                        {
                            orderItem.Qty = param.Quantity;
                            orderItem.Subtotal = param.Quantity*productEntity.UnitPrice;
                        }
                        cart.Total += deltaQty*productEntity.UnitPrice;
                        productEntity.Stock -= deltaQty;
                    }
                    else
                    {
                        int deltaQty;
                        var orderItem = cart.Items.FirstOrDefault(i => i.Product.Equals(param.Product));
                        var origItem = execRecord.CompensationData as OrderItem;
                        if (execRecord.CompensationData == null)
                        {
                            deltaQty = orderItem.Qty;
                            cart.Items.Remove(orderItem);
                            cart.Total -= orderItem.Subtotal;
                        }
                        else
                        {
                            deltaQty = orderItem.Qty - origItem.Qty;
                            orderItem.Qty = origItem.Qty;
                            orderItem.Subtotal = origItem.Subtotal;
                            cart.Total -= (orderItem.Subtotal - origItem.Subtotal);
                        }
                        productEntity.Stock += deltaQty;
                    }
                    product.Update(productEntity);
                    ts.Complete();
                }
            }
            catch (Exception e)
            {
                context.Status = WorkflowStatus.Failed;
                context.Message = $"Task Pickup Product failure: {e.Message}";
            }
        }
    }

    public class TaskDropProduct : ITask, InvokeTask
    {
        // parameters of task
        [Serializable]
        public class ParamDropProduct
        {
            public string Product { get; set; }
        }

        public void Invoke(Guid workflowId, object[] parameters)
        {
            var paramDropProduct = new ParamDropProduct()
            {
                Product = parameters[0] as string
            };
            try
            {
                WorkflowRepository.Execute("Test.TaskDropProduct", workflowId, paramDropProduct);
            }
            catch (Exception e)
            {
                Debug.Print($"Invoke Task Drop Product failure: {e.Message}");
            }
        }

        public string Name => GetType().AssemblyQualifiedName;

        public void Execute(WorkflowContext context, TaskExecutionRecord execRecord, object parameters)
        {
            // convert parameters
            var param = parameters as ParamDropProduct;
            // check pre-condition
            if (!TestWorkflow.Products.ContainsKey(param.Product))
            {
                context.Status = WorkflowStatus.Failed;
                context.Message = $"Product {param.Product} not found.";
                return;
            }
            // retrieve resources
            Order cart;
            if (context.State.ContainsKey("Cart"))
                cart = context.State["Cart"] as Order;
            else
            {
                context.Status = WorkflowStatus.Failed;
                context.Message = "Cart is empty.";
                return;
            }
            try
            {
                using (var ts = TransactionScopeUtil.GetTransactionScope())
                using (var dbConn = TransactionScopeUtil.CreateDbConnection())
                {
                    dbConn.Open();
                    var product = TestWorkflow.Products[param.Product];
                    var productEntity = product.GetEntity(LockMode.Exclusive);
                    // process
                    if (context.Status == WorkflowStatus.Ready)
                    {
                        var orderItem = cart.Items.FirstOrDefault(i => i.Product.Equals(param.Product));
                        if (orderItem == null)
                        {
                            context.Status = WorkflowStatus.Failed;
                            context.Message = $"Product {param.Product} not in cart.";
                            return;
                        }
                        execRecord.CompensationData = new OrderItem
                        {
                            Product = orderItem.Product,
                            Qty = orderItem.Qty,
                            UnitPrice = orderItem.UnitPrice,
                            Subtotal = orderItem.Subtotal
                        };
                        cart.Items.Remove(orderItem);
                        cart.Total -= orderItem.Qty*productEntity.UnitPrice;
                        productEntity.Stock += orderItem.Qty;
                    }
                    else
                    {
                        var origItem = execRecord.CompensationData as OrderItem;
                        cart.Items.Add(origItem);
                        cart.Total += origItem.Qty*productEntity.UnitPrice;
                        productEntity.Stock -= origItem.Qty;
                    }
                    product.Update(productEntity);
                    ts.Complete();
                }
            }
            catch (Exception e)
            {
                context.Status = WorkflowStatus.Failed;
                context.Message = $"Task Drop Product failure: {e.Message}";
            }
        }
    }

    public class TaskPlaceOrder : ITask, InvokeTask
    {
        public void Invoke(Guid workflowId, object[] parameters)
        {
            try
            {
                WorkflowRepository.Execute("Test.TaskPlaceOrder", workflowId, null);
            }
            catch (Exception e)
            {
                Debug.Print($"Invoke Task Place Order failure: {e.Message}");
            }
        }

        public string Name => GetType().AssemblyQualifiedName;

        public void Execute(WorkflowContext context, TaskExecutionRecord execRecord, object parameters)
        {
            try
            {
                using (var ts = TransactionScopeUtil.GetTransactionScope())
                using (var dbConn = TransactionScopeUtil.CreateDbConnection())
                {
                    dbConn.Open();
                    // process
                    if (context.Status == WorkflowStatus.Ready)
                    {
                        Order cart;
                        if (context.State.ContainsKey("Cart"))
                            cart = context.State["Cart"] as Order;
                        else
                        {
                            context.Status = WorkflowStatus.Failed;
                            context.Message = "Cart is empty.";
                            return;
                        }
                        // retrieve resources
                        if (!TestWorkflow.BankAccounts.ContainsKey(cart.Customer))
                        {
                            context.Status = WorkflowStatus.Failed;
                            context.Message = $"Bank Account {cart.Customer} not found.";
                            return;
                        }
                        var bankAccount = TestWorkflow.BankAccounts[cart.Customer];
                        var bankAccountEntity = bankAccount.GetEntity(LockMode.Exclusive);
                        if (bankAccountEntity.Balance < cart.Total)
                        {
                            context.Status = WorkflowStatus.Failed;
                            context.Message = $"Balance of {cart.Customer} not enough.";
                            return;
                        }
                        bankAccountEntity.Balance -= cart.Total;
                        bankAccount.Update(bankAccountEntity);
                        var order = new TransactionalEntity<Order>(cart);
                        order.Update();
                        TestWorkflow.Orders.Add(order);
                        execRecord.CompensationData = cart;
                        context.State.Remove("Cart");
                    }
                    else
                    {
                        var orderEntity = execRecord.CompensationData as Order;
                        context.State.Add("Cart", orderEntity);
                        var bankAccount = TestWorkflow.BankAccounts[orderEntity.Customer];
                        var bankAccountEntity = bankAccount.GetEntity(LockMode.Exclusive);
                        bankAccountEntity.Balance += orderEntity.Total;
                        bankAccount.Update(bankAccountEntity);
                        var collection = CollectionRepository.GetCollection(typeof(Order));
                        collection.DeleteEntity(collection.GetEntity(orderEntity.Id));
                    }
                    ts.Complete();
                }
            }
            catch (Exception e)
            {
                context.Status = WorkflowStatus.Failed;
                context.Message = $"Task Drop Product failure: {e.Message}";
            }
        }
    }
}