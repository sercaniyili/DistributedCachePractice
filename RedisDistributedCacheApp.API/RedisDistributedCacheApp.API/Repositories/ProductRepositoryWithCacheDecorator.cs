﻿using RedisDistributedCacheApp.API.Model;
using RedisDistributedCacheApp.Cache;
using StackExchange.Redis;
using System.Reflection;
using System.Text.Json;

namespace RedisDistributedCacheApp.API.Repositories
{
    public class ProductRepositoryWithCacheDecorator : IProductRepository
    {
        private const string productKey = "productCaches";

        private readonly IProductRepository _productRepository;
        private readonly RedisService _redisService;
        private readonly IDatabase _cacheRepository;

        public ProductRepositoryWithCacheDecorator(RedisService redisService, IProductRepository productRepository)
        {
            _redisService = redisService;
            _productRepository = productRepository;
            _cacheRepository = redisService.GetDb(1);
        }

        public async Task<Product> CreateAsync(Product product)
        {
            var newProduct = await _productRepository.CreateAsync(product);

            if (!await _cacheRepository.KeyExistsAsync(productKey))
                await _cacheRepository.HashSetAsync(productKey, product.Id, JsonSerializer.Serialize(newProduct));

            return newProduct;

        }
        public async Task<List<Product>> GetAsync()
        {
            if (!await _cacheRepository.KeyExistsAsync(productKey))
                return await LoadCacheFromDbAsync();

            var products = new List<Product>();

            var cacheProducts = await _cacheRepository.HashGetAllAsync(productKey);

            foreach (var item in cacheProducts.ToList())
            {
                var product = JsonSerializer.Deserialize<Product>(item.Value);
                products.Add(product);
            }
                return products;
        }
        public async Task<Product> GetByIdAsync(int id)
        {
            if (_cacheRepository.KeyExists(productKey))
            {
                var product = await _cacheRepository.HashGetAsync(productKey, id);
                return product.HasValue ? JsonSerializer.Deserialize<Product>(product) : null;
            }

            var products = await LoadCacheFromDbAsync();

            return products.FirstOrDefault(x=> x.Id==id);
        }
        private async Task<List<Product>> LoadCacheFromDbAsync()
        {
            var products= await _productRepository.GetAsync();

            products.ForEach(x =>
            {
                _cacheRepository.HashSetAsync(productKey, x.Id, JsonSerializer.Serialize(x));
            });

            return products;
        }
    }
}

