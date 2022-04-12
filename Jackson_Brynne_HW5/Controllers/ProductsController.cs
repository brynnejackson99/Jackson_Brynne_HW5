using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Jackson_Brynne_HW5.DAL;
using Jackson_Brynne_HW5.Models;
using Microsoft.AspNetCore.Authorization;

namespace Jackson_Brynne_HW5.Controllers
{
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            return View(await _context.Products.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return View("Error", new String[] { "That product was not found in the database." });
            }

            //find the product in the database
            //be sure to include the relevant navigational data
            Product product = await _context.Products
                .Include(c => c.Suppliers)
                .FirstOrDefaultAsync(m => m.ProductID == id);


            if (product == null)
            {
                return View("Error", new String[] { "That product was not found in the database." });
            }

            return View(product);
        }

        // GET: Products/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            //TODO: is this where I put this?
            ViewBag.AllSuppliers = GetAllSuppliers();
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductID,Name,Description,Price,Type")] Product product, int[] SelectedSuppliers)
        {
            //This code has been modified so that if the model state is not valid
            //we immediately go to the "sad path" and give the user a chance to try again
            if (ModelState.IsValid == false)
            {
                //re-populate the view bag with the departments
                ViewBag.AllSuppliers = GetAllSuppliers();
                //go back to the Create view to try again
                return View(product);
            }

            //if code gets to this point, we know the model is valid and
            //we can add the course to the database

            //add the course to the database and save changes
            _context.Add(product);
            await _context.SaveChangesAsync();

            //add the associated departments to the course
            //loop through the list of deparment ids selected by the user
            foreach (int supplierID in SelectedSuppliers)
            {
                //find the department associated with that id
                Supplier dbSupplier = _context.Suppliers.Find(supplierID);

                //add the department to the course's list of departments and save changes
                product.Suppliers.Add(dbSupplier);
                _context.SaveChanges();
            }

            //Send the user to the page with all the departments
            return RedirectToAction(nameof(Index));


        }

        // GET: Products/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return View("Error", new string[] { "Please specify a product to edit!" });
            }

            //find the products in the database
            //be sure to change the data type to product instead of 'var'
            Product product = await _context.Products.Include(c => c.Suppliers)
                                           .FirstOrDefaultAsync(c => c.ProductID == id);


            if (product == null)
            {
                return View("Error", new string[] { "This product was not found!" });
            }
            //populate the viewbag with existing suppliers
            ViewBag.AllSuppliers = GetAllSuppliers(product);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("ProductID,Name,Description,Price,Type")] Product product, int[] SelectedSuppliers)
        {
            if (id != product.ProductID)
            {
                return View("Error", new string[] { "Please try again!" });
            }

            if (ModelState.IsValid == false) //there is something wrong
            {
                ViewBag.AllSuppliers = GetAllSuppliers(product);
                return View(product);
            }

            //if code gets this far, attempt to edit the product
            try
            {
                //Find the product to edit in the database and include relevant 
                //navigational properties
                Product dbProduct = _context.Products
                    .Include(c => c.Suppliers)
                    .FirstOrDefault(c => c.ProductID == product.ProductID);

                //create a list of departments that need to be removed
                List<Supplier> SuppliersToRemove = new List<Supplier>();

                //find the departments that should no longer be selected because the
                //user removed them
                //remember, SelectedDepartments = the list from the HTTP request (listbox)
                foreach (Supplier supplier in dbProduct.Suppliers)
                {
                    //see if the new list contains the department id from the old list
                    if (SelectedSuppliers.Contains(supplier.SupplierID) == false)//this department is not on the new list
                    {
                        SuppliersToRemove.Add(supplier);
                    }
                }

                //remove the departments you found in the list above
                //this has to be 2 separate steps because you can't iterate (loop)
                //over a list that you are removing things from
                foreach (Supplier supplier in SuppliersToRemove)
                {
                    //remove this course department from the course's list of departments
                    dbProduct.Suppliers.Remove(supplier);
                    _context.SaveChanges();
                }

                //add the departments that aren't already there
                foreach (int supplierID in SelectedSuppliers)
                {
                    if (dbProduct.Suppliers.Any(d => d.SupplierID == supplierID) == false)//this department is NOT already associated with this course
                    {
                        //Find the associated department in the database
                        Supplier dbSupplier = _context.Suppliers.Find(supplierID);

                        //Add the department to the course's list of departments
                        dbProduct.Suppliers.Add(dbSupplier);
                        _context.SaveChanges();
                    }
                }

                //update the course's scalar properties
                dbProduct.Price = product.Price;
                dbProduct.Name = product.Name;
                dbProduct.Description = product.Description;
                dbProduct.Type = product.Type;



                //save the changes
                _context.Products.Update(dbProduct);
                _context.SaveChanges();

            }
            catch (Exception ex)
            {
                return View("Error", new string[] { "There was an error editing this product.", ex.Message });
            }

            //if code gets this far, everything is okay
            //send the user back to the page with all the courses
            return RedirectToAction(nameof(Index));
        }

        

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductID == id);
        }

        private MultiSelectList GetAllSuppliers()
        {
            //Create a new list of departments and get the list of the departments
            //from the database
            List<Supplier> allSuppliers = _context.Suppliers.ToList();

            //Multi-select lists do not require a selection, so you don't need 
            //to add a dummy record like you do for select lists

            //use the MultiSelectList constructor method to get a new MultiSelectList
            MultiSelectList mslAllSuppliers = new MultiSelectList(allSuppliers.OrderBy(d => d.SupplierName), "SupplierID", "SupplierName");

            //return the MultiSelectList
            return mslAllSuppliers;
        }
        private MultiSelectList GetAllSuppliers(Product product)
        {
            //Get the list of suppliers from the database
            
            List<Supplier> allSuppliers = _context.Suppliers.ToList();

            //loop through the list of product suppliers to find a list of supplier ids
            //create a list to store the supplier ids
            List<Int32> selectedSupplierIDs = new List<Int32>();

            //Loop through the list to find the SupplierIDs
            foreach (Supplier associatedSupplier in product.Suppliers)
            {
                selectedSupplierIDs.Add(associatedSupplier.SupplierID);
            }


            //convert the list to a SelectList by calling SelectList constructor
            //SupplierID and SupplierName are the names of the properties on the Supplier class
            //SupplierID is the primary key
            MultiSelectList mslAllSuppliers = new MultiSelectList(allSuppliers.OrderBy(s => s.SupplierName), "SupplierID", "SupplierName", selectedSupplierIDs);

            //return the MultiSelectList
            return mslAllSuppliers;
        }
    }
}
