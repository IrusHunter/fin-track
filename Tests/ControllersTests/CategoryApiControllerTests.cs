using System.Net;
using System.Net.Http.Json;
using FinTrack.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace WebApp.Tests.Integration
{

    public class CategoryApiTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public CategoryApiTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAllV1_ShouldReturnCategories()
        {
            var response = await _client.GetAsync("/api/v1.0/CategoryApi");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var categories = await response.Content.ReadFromJsonAsync<Category[]>();
            Assert.NotNull(categories);
        }

        [Fact]
        public async Task CreateV1_ShouldReturnCreatedCategory()
        {
            var newCategory = new Category
            {
                Name = "Test Category",
                TaxAmount = 10,
                TaxType = TaxType.GeneralTax
            };

            var response = await _client.PostAsJsonAsync("/api/v1.0/CategoryApi", newCategory);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var created = await response.Content.ReadFromJsonAsync<Category>();
            Assert.NotNull(created);
            Assert.Equal("Test Category", created.Name);
        }

        [Fact]
        public async Task GetV1ById_ShouldReturnCategory()
        {
            var newCategory = new Category
            {
                Name = "Fetch Test",
                TaxAmount = 5,
                TaxType = TaxType.ExpenseTax
            };

            var createdResponse = await _client.PostAsJsonAsync("/api/v1.0/CategoryApi", newCategory);
            var createdCategory = await createdResponse.Content.ReadFromJsonAsync<Category>();
            Assert.NotNull(createdCategory);

            var response = await _client.GetAsync($"/api/v1.0/CategoryApi/{createdCategory.Id}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var fetched = await response.Content.ReadFromJsonAsync<Category>();
            Assert.NotNull(fetched);
            Assert.Equal("Fetch Test", fetched.Name);
        }

        [Fact]
        public async Task UpdateV1_ShouldModifyCategory()
        {
            var newCategory = new Category
            {
                Name = "Update Test",
                TaxAmount = 5,
                TaxType = TaxType.ExpenseTax
            };

            var createdResponse = await _client.PostAsJsonAsync("/api/v1.0/CategoryApi", newCategory);
            var createdCategory = await createdResponse.Content.ReadFromJsonAsync<Category>();
            Assert.NotNull(createdCategory);

            createdCategory.Name = "Updated Name";
            var updateResponse = await _client.PutAsJsonAsync($"/api/v1.0/CategoryApi/{createdCategory.Id}", createdCategory);
            Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

            var getResponse = await _client.GetAsync($"/api/v1.0/CategoryApi/{createdCategory.Id}");
            var updatedCategory = await getResponse.Content.ReadFromJsonAsync<Category>()
                ?? throw new InvalidOperationException("Category not found after update"); ;
            Assert.Equal("Updated Name", updatedCategory.Name);
        }

        [Fact]
        public async Task DeleteV1_ShouldRemoveCategory()
        {
            var newCategory = new Category
            {
                Name = "Delete Test",
                TaxAmount = 5,
                TaxType = TaxType.GeneralTax
            };

            var createdResponse = await _client.PostAsJsonAsync("/api/v1.0/CategoryApi", newCategory);
            var createdCategory = await createdResponse.Content.ReadFromJsonAsync<Category>();
            Assert.NotNull(createdCategory);

            var deleteResponse = await _client.DeleteAsync($"/api/v1.0/CategoryApi/{createdCategory.Id}");
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            var getResponse = await _client.GetAsync($"/api/v1.0/CategoryApi/{createdCategory.Id}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task GetAllV2_ShouldReturnSimplifiedCategories()
        {
            var response = await _client.GetAsync("/api/v2.0/CategoryApi");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var categories = await response.Content.ReadFromJsonAsync<object[]>();
            Assert.NotNull(categories);
        }
    }
}
