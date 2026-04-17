using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaatcoBMS.Application.Interfaces;

namespace VaatcoBMS.Web.Controllers;

[Authorize] // Ensure only logged-in users can access this
public class ProductsController(IProductService productService) : Controller
{
    private readonly IProductService _productService = productService;

    public async Task<IActionResult> Index()
    {
        // 1. Fetch all products from the database via your application service
        var products = await _productService.GetAllAsync();
        
        // 2. Pass the list of ProductDto to the Index.cshtml view
        return View(products);
    }
}
