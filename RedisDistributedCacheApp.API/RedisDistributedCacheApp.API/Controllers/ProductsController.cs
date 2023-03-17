using Microsoft.AspNetCore.Mvc;
using RedisDistributedCacheApp.API.Model;
using RedisDistributedCacheApp.API.Repositories;
using RedisDistributedCacheApp.API.Services;
using RedisDistributedCacheApp.Cache;
using StackExchange.Redis;
using System.Security.AccessControl;

namespace RedisDistributedCacheApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _productService.GetAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            return Ok(await _productService.GetByIdAsync(id));
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            return Created(string.Empty, await _productService.CreateAsync(product));
        }

    }
}
