using Microsoft.AspNetCore.Mvc;
using RedisDistributedCacheApp.API.Model;
using RedisDistributedCacheApp.API.Repositories;
using RedisDistributedCacheApp.Cache;
using StackExchange.Redis;
using System.Security.AccessControl;

namespace RedisDistributedCacheApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : Controller
    {
        private readonly IProductRepository _productRepository;
        //private readonly RedisService redisService;
        private readonly IDatabase _database;


        public ProductsController(IProductRepository productRepository)//, IDatabase database)//RedisService redisService)
        {
            _productRepository = productRepository;
        //    _database = database;
        //  _database.StringSet("soyad","asdaasd");

            //var db = redisService.GetDb(0);
            //db.StringSet("isim", "Ahmet");
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _productRepository.GetAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            return Ok(await _productRepository.GetByIdAsync(id));
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            return Created(string.Empty, await _productRepository.CreateAsync(product));
        }

    }
}
