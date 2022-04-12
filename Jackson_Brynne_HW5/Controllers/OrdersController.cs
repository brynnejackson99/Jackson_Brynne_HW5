using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Jackson_Brynne_HW5.DAL;
using Jackson_Brynne_HW5.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jackson_Brynne_HW5.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Orders
        public IActionResult Index()
        {
            //Set up a list of orders to display
            List<Order> orders;
            if (User.IsInRole("Admin"))
            {
                orders = _context.Orders
                                .Include(r => r.OrderDetails)
                                .ToList();
            }
            else //user is a customer, so only display their records
            //TODO: help here why is this not working
            {
                orders = _context.Orders
                                .Include(r => r.OrderDetails)
                                .Where(r => r.User.UserName == User.Identity.Name)
                                .ToList();
            }

            
            return View(orders);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            //the user did not specify an order to view
            if (id == null)
            {
                return View("Error", new String[] { "Please specify an order to view!" });
            }

            //find the registration in the database
            Order order = await _context.Orders
                                              .Include(r => r.OrderDetails)
                                              .ThenInclude(r => r.Product)
                                              .Include(r => r.User)
                                              .FirstOrDefaultAsync(m => m.OrderID == id);

            //registration was not found in the database
            if (order == null)
            {
                return View("Error", new String[] { "This order was not found!" });
            }

            //make sure this registration belongs to this user
            if (User.IsInRole("Customer") && order.User.UserName != User.Identity.Name)
            {
                return View("Error", new String[] { "This is not your order!  Don't be such a snoop!" });
            }

            //Send the user to the details page
            return View(order);
        }

        // GET: Orders/Create
        [Authorize(Roles = "Customer")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Create([Bind("OrderNotes")] Order order)
        {
            //Find the next registration number from the utilities class
            order.OrderNumber = Utilities.GenerateNextOrderNumber.GetNextOrderNumber(_context);

            //Set the date of this order
            order.OrderDate = DateTime.Now;

            //Associate the registration with the logged-in customer
            order.User = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);

            //make sure all properties are valid
            if (ModelState.IsValid == false)
            {
                return View(order);
            }

            //if code gets this far, add the registration to the database
            _context.Add(order);
            await _context.SaveChangesAsync();

            //send the user on to the action that will allow them to 
            //create a registration detail.  Be sure to pass along the RegistrationID
            //that you created when you added the registration to the database above
            return RedirectToAction("Create", "OrderDetails", new { orderID = order.OrderID });
        }

        // GET: Orders/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return View("Error", new String[] { "Please specify an order to edit" });
            }

            //find the registration in the database, and be sure to include details
            Order order = _context.Orders
                                       .Include(r => r.OrderDetails)
                                       .ThenInclude(r => r.Product)
                                       .Include(r => r.User)
                                       .FirstOrDefault(r => r.OrderID == id);

            //registration was nout found in the database
            if (order == null)
            {
                return View("Error", new String[] { "This order was not found in the database!" });
            }

            //registration does not belong to this user
            if (User.IsInRole("Customer") && order.User.UserName != User.Identity.Name)
            {
                return View("Error", new String[] { "You are not authorized to edit this order!" });
            }

            //send the user to the registration edit view
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderID, OrderNotes")] Order order)
        {
            if (id != order.OrderID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(order);
        }

        
        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderID == id);
        }
    }
}
