using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Jackson_Brynne_HW5.DAL;
using Jackson_Brynne_HW5.Models;

namespace Jackson_Brynne_HW5.Controllers
{
    public class OrderDetailsController : Controller
    {
        private readonly AppDbContext _context;

        public OrderDetailsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: OrderDetails
        public IActionResult Index(int? orderID)
        {
            if (orderID == null)
            {
                return View("Error", new String[] { "Please specify an order to view!" });
            }

            //limit the list to only the registration details that belong to this registration
            List<OrderDetail> rds = _context.OrderDetails
                                          .Include(rd => rd.Product)
                                          .Where(rd => rd.Order.OrderID == orderID)
                                          .ToList();

            return View(rds);
        }

       

        // GET: OrderDetails/Create
        public IActionResult Create(int orderID)
        {
            //create a new instance of the RegistrationDetail class
            OrderDetail od = new OrderDetail();

            //find the registration that should be associated with this registration
            Order dbOrder = _context.Orders.Find(orderID);

            //set the new registration detail's registration equal to the registration you just found
            od.Order = dbOrder;

            //populate the ViewBag with a list of existing courses
            ViewBag.AllProducts = GetAllProducts();

            //pass the newly created registration detail to the view
            return View(od);
        }

        // POST: OrderDetails/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Order,OrderDetailID,Quantity")] OrderDetail orderDetail, int SelectedProduct)
        {
            //if user has not entered all fields, send them back to try again
            if (ModelState.IsValid == false)
            {
                ViewBag.AllProducts = GetAllProducts();
                return View(orderDetail);
            }

            //find the course to be associated with this order
            Product dbProduct = _context.Products.Find(SelectedProduct);

            //set the registration detail's course to be equal to the one we just found
            orderDetail.Product = dbProduct;

            //find the registration on the database that has the correct registration id
            //unfortunately, the HTTP request will not contain the entire registration object, 
            //just the registration id, so we have to find the actual object in the database
            Order dbOrder = _context.Orders.Find(orderDetail.Order.OrderID);

            //set the registration on the registration detail equal to the registration that we just found
            orderDetail.Order = dbOrder;

            //set the registration detail's price equal to the course price
            //this will allow us to to store the price that the user paid
            orderDetail.ProductPrice = dbProduct.Price;

            //calculate the extended price for the registration detail
            orderDetail.ExtendedPrice = orderDetail.Quantity * orderDetail.ProductPrice;

            //add the registration detail to the database
            _context.Add(orderDetail);
            await _context.SaveChangesAsync();

            //send the user to the details page for this registration
            return RedirectToAction("Details", "Orders", new { id = orderDetail.Order.OrderID });

        }

        // GET: OrderDetails/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderDetail = await _context.OrderDetails.FindAsync(id);
            if (orderDetail == null)
            {
                return NotFound();
            }
            return View(orderDetail);
        }

        // POST: OrderDetails/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderDetailID,Quantity")] OrderDetail orderDetail)
        {
            if (id != orderDetail.OrderDetailID)
            {
                return View("Error", new String[] { "There was a problem editing this record. Try again!" });
            }

            //information is not valid, try again
            if (ModelState.IsValid == false)
            {
                return View(orderDetail);
            }


            //create a new registration detail
            OrderDetail dbRD;
            //if code gets this far, update the record
            try
            {
                //find the existing registration detail in the database
                //include both registration and course
                dbRD = _context.OrderDetails
                      .Include(rd => rd.Product)
                      .Include(rd => rd.Order)
                      .FirstOrDefault(rd => rd.OrderDetailID == orderDetail.OrderDetailID);

                //update the scalar properties
                dbRD.Quantity = orderDetail.Quantity;
                dbRD.ProductPrice = dbRD.Product.Price;
                dbRD.ExtendedPrice = dbRD.Quantity * dbRD.ProductPrice;

                //save changes
                _context.Update(dbRD);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return View("Error", new String[] { "There was a problem editing this record", ex.Message });
            }

            //if code gets this far, go back to the registration details index page
            return RedirectToAction("Details", "Orders", new { id = dbRD.Order.OrderID });

        }

        // GET: OrderDetails/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return View("Error", new String[] { "Please specify an order detail to delete!" });
            }

            //find the registration detail in the database
            OrderDetail orderDetail = await _context.OrderDetails
                                                    .Include(r => r.Order)
                                                   .FirstOrDefaultAsync(m => m.OrderDetailID == id);



           
            if (orderDetail == null)
            {
                return View("Error", new String[] { "This order detail was not in the database!" });
            }

            return View(orderDetail);
        }

        // POST: OrderDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            //find the registration detail to delete
            OrderDetail orderDetail = await _context.OrderDetails
                                                   .Include(r => r.Order)
                                                   .FirstOrDefaultAsync(r => r.OrderDetailID == id);

            //delete the registration detail
            _context.OrderDetails.Remove(orderDetail);
            await _context.SaveChangesAsync();

            //return the user to the registration/details page
            return RedirectToAction("Details", "Orders", new { id = orderDetail.Order.OrderID });
        }

        private bool OrderDetailExists(int id)
        {
            return _context.OrderDetails.Any(e => e.OrderDetailID == id);
        }

        private SelectList GetAllProducts()
        {
            //create a list for all the courses
            List<Product> allProducts = _context.Products.ToList();

            //the user MUST select a course, so you don't need a dummy option for no course

            //use the constructor on select list to create a new select list with the options
            SelectList slAllProducts = new SelectList(allProducts, nameof(Product.ProductID), nameof(Product.Name));

            return slAllProducts;
        }
    }
}
