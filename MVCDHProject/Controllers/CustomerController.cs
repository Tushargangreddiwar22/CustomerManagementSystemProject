using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCDHProject.Models;
namespace MVCDHProject.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly ICustomerDAL obj;

        public CustomerController(ICustomerDAL obj)
        {
            this.obj = obj;
        }
        [AllowAnonymous]
        public ViewResult DisplayCustomers(string name, string city, bool? status, decimal? minBalance)
        {
            var customers = obj.Customers_Select();

            // 🔍 Filter by Name
            if (!string.IsNullOrEmpty(name))
            {
                customers = customers
                    .Where(c => c.Name != null &&
                                c.Name.ToLower().Contains(name.ToLower()))
                    .ToList();
            }

            // 🔍 Filter by City
            if (!string.IsNullOrEmpty(city))
            {
                customers = customers
                    .Where(c => c.City != null &&
                                c.City.ToLower().Contains(city.ToLower()))
                    .ToList();
            }

            // 🔍 Filter by Status
            if (status.HasValue)
            {
                customers = customers
                    .Where(c => c.Status == status.Value)
                    .ToList();
            }

            // 🔍 Filter by Minimum Balance
            if (minBalance.HasValue)
            {
                customers = customers
                    .Where(c => c.Balance >= minBalance.Value)
                    .ToList();
            }

            return View(customers);
        }
        [AllowAnonymous]
        public ViewResult DisplayCustomer(int Custid)
        {
            return View(obj.Customer_Select(Custid));
        }
        [HttpGet]
        public ViewResult AddCustomer()
        {
            var customer = new Customer();
            customer.Custid = obj.GenerateCustomerId(); // 🔥 auto ID

            return View(customer);
        }
        [HttpPost]
        public RedirectToActionResult AddCustomer(Customer customer)
        {
            customer.Custid = obj.GenerateCustomerId(); // 🔥 ensure u
            customer.Status = true;

            obj.Customer_Insert(customer);

            return RedirectToAction("DisplayCustomers");
        }
        
        public ViewResult EditCustomer(int Custid)
        {
            return View(obj.Customer_Select(Custid));
        }
        public RedirectToActionResult UpdateCustomer(Customer customer)
        {
            obj.Customer_Update(customer);
            return RedirectToAction("DisplayCustomers");
        }

        public RedirectToActionResult DeleteCustomer(int Custid)
        {
            var customer = obj.Customer_Select(Custid);

            if (customer != null)
            {
                customer.Status = false; // mark as inactive
                obj.Customer_Update(customer);
            }

            return RedirectToAction("DisplayCustomers");
        }
        public RedirectToActionResult RestoreCustomer(int Custid)
        {
            obj.Customer_Restore(Custid);
            return RedirectToAction("DisplayCustomers");
        }
    }
}
