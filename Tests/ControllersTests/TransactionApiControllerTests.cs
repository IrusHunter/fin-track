using System.Net;
using System.Net.Http.Json;
using FinTrack.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Xunit.Abstractions;

namespace WebApp.Tests.Integration
{
    public class TransactionApiTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;

        public TransactionApiTests(CustomWebApplicationFactory factory, ITestOutputHelper output)
        {
            _client = factory.CreateClient();
            _output = output;
        }

        private async Task<Category> CreateTestCategoryAsync(string name = "Test Category2")
        {
            var newCategory = new Category
            {
                Name = name,
                TaxAmount = 10,
                TaxType = TaxType.GeneralTax
            };

            var categoryResponse = await _client.PostAsJsonAsync("/api/v1.0/CategoryApi", newCategory);
            categoryResponse.EnsureSuccessStatusCode();

            var createdCategory = await categoryResponse.Content.ReadFromJsonAsync<Category>();
            return createdCategory!;
        }

        [Fact]
        public async Task GetAllV1_ShouldReturnTransactions()
        {
            var response = await _client.GetAsync("/api/v1.0/TransactionApi");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var transactions = await response.Content.ReadFromJsonAsync<Transaction[]>();
            Assert.NotNull(transactions);
        }

        [Fact]
        public async Task CreateV1_ShouldReturnCreatedTransaction()
        {
            var category = await CreateTestCategoryAsync();

            var newTransaction = new Transaction
            {
                Name = "Test Transaction",
                Sum = 100,
                SumAfterTax = 110,
                CategoryId = category.Id
            };

            var response = await _client.PostAsJsonAsync("/api/v1.0/TransactionApi", newTransaction);
            // var content = await response.Content.ReadAsStringAsync();
            // _output.WriteLine($"Response content:\n{content}");
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var created = await response.Content.ReadFromJsonAsync<Transaction>();
            Assert.NotNull(created);
            Assert.Equal("Test Transaction", created.Name);
            Assert.Equal(category.Id, created.CategoryId);
        }

        [Fact]
        public async Task GetV1ById_ShouldReturnTransaction()
        {
            var category = await CreateTestCategoryAsync("Fetch Category");

            var newTransaction = new Transaction
            {
                Name = "Fetch Test",
                Sum = 50,
                SumAfterTax = 55,
                CategoryId = category.Id
            };

            var createdResponse = await _client.PostAsJsonAsync("/api/v1.0/TransactionApi", newTransaction);
            var createdTransaction = await createdResponse.Content.ReadFromJsonAsync<Transaction>();
            Assert.NotNull(createdTransaction);

            var response = await _client.GetAsync($"/api/v1.0/TransactionApi/{createdTransaction.Id}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var fetched = await response.Content.ReadFromJsonAsync<Transaction>();
            Assert.NotNull(fetched);
            Assert.Equal("Fetch Test", fetched.Name);
        }

        [Fact]
        public async Task UpdateV1_ShouldModifyTransaction()
        {
            var category = await CreateTestCategoryAsync("Update Category");

            var newTransaction = new Transaction
            {
                Name = "Update Test",
                Sum = 50,
                SumAfterTax = 55,
                CategoryId = category.Id
            };

            var createdResponse = await _client.PostAsJsonAsync("/api/v1.0/TransactionApi", newTransaction);
            var createdTransaction = await createdResponse.Content.ReadFromJsonAsync<Transaction>();
            Assert.NotNull(createdTransaction);

            createdTransaction.Name = "Updated Name";
            var updateResponse = await _client.PutAsJsonAsync($"/api/v1.0/TransactionApi/{createdTransaction.Id}", createdTransaction);
            Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

            var getResponse = await _client.GetAsync($"/api/v1.0/TransactionApi/{createdTransaction.Id}");
            var updatedTransaction = await getResponse.Content.ReadFromJsonAsync<Transaction>()
                ?? throw new InvalidOperationException("Transaction not found after update");
            Assert.Equal("Updated Name", updatedTransaction.Name);
        }

        [Fact]
        public async Task DeleteV1_ShouldRemoveTransaction()
        {
            var category = await CreateTestCategoryAsync("Delete Category");

            var newTransaction = new Transaction
            {
                Name = "Delete Test",
                Sum = 50,
                SumAfterTax = 55,
                CategoryId = category.Id
            };

            var createdResponse = await _client.PostAsJsonAsync("/api/v1.0/TransactionApi", newTransaction);
            var createdTransaction = await createdResponse.Content.ReadFromJsonAsync<Transaction>();
            Assert.NotNull(createdTransaction);

            var deleteResponse = await _client.DeleteAsync($"/api/v1.0/TransactionApi/{createdTransaction.Id}");
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            var getResponse = await _client.GetAsync($"/api/v1.0/TransactionApi/{createdTransaction.Id}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }
    }
}
