namespace MVCDHProject.Models
{
    public class CustomerSqlDAL : ICustomerDAL
    {
        private readonly MVCCoreDbContext context;
        public CustomerSqlDAL(MVCCoreDbContext context)
        {
            this.context = context;
        }
        public int GenerateCustomerId()
        {
            if (!context.Customers.Any())
                return 1;

            return context.Customers.Max(c => c.Custid) + 1;
        }
        public List<Customer> Customers_Select()
        {
            var customers = context.Customers.ToList();
            return customers;
        }
        public Customer Customer_Select(int Custid)
        {
            Customer? customer = context.Customers.Find(Custid);
            if (customer == null)
            {
                throw new Exception("No customer exist's with given Custid.");
            }
            return customer;
        }

        public void Customer_Insert(Customer customer)
        {
            context.Customers.Add(customer);
            context.SaveChanges();
        }
        public void Customer_Update(Customer customer)
        {
            
            context.Update(customer);
            context.SaveChanges();
        }
        public void Customer_Delete(int Custid)
        {
            var customer = context.Customers.Find(Custid);

            if (customer != null)
            {
                customer.Status = false;
                context.SaveChanges();
            }
        }
        public void Customer_Restore(int Custid)
        {
            var customer = context.Customers.Find(Custid);

            if (customer != null)
            {
                customer.Status = true; // 🔥 make active again
                context.SaveChanges();
            }
        }
    }
}