using MovieCatalogExam.MovieCatalogExam.Models;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;

namespace MovieCatalogExam
{
    [TestFixture]
    public class MovieCatalogTests
    {
        private RestClient client;
        private static string createdMovieId;

        private const string BaseUrl = "http://144.91.123.158:5000";
        private const string Email = "2026BE@softuni.com";
        private const string Password = "2026BE";

        [OneTimeSetUp]
        public void Setup()
        {
            string token = GetJwtToken(Email, Password);

            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            client = new RestClient(options);
        }

        private string GetJwtToken(string email, string password)
        {
            var tempClient = new RestClient(BaseUrl);

            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { email, password });

            var response = tempClient.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                return content.GetProperty("accessToken").GetString();
            }

            throw new Exception("Authentication failed");
        }

        [Order(1)]
        [Test]
      
        public void CreateMovie_ShouldSucceed()
        {
            var movie = new MovieDto
            {
                Title = "Test Movie",
                Description = "Test Description"
            };

            var request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(movie);

            var response = client.Execute(request);
            var data = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(data.Movie, Is.Not.Null);
            Assert.That(data.Movie.Id, Is.Not.Null.And.Not.Empty);
            Assert.That(data.Msg, Is.EqualTo("Movie created successfully!"));

            createdMovieId = data.Movie.Id;
        }

        [Order(2)]
        [Test]
        
        public void EditMovie_ShouldSucceed()
        {
            var updatedMovie = new MovieDto
            {
                Title = "Updated Movie",
                Description = "Updated Description"
            };

            var request = new RestRequest("/api/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", createdMovieId);
            request.AddJsonBody(updatedMovie);

            var response = client.Execute(request);
            var data = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(data.Msg, Is.EqualTo("Movie edited successfully!"));
        }

        [Order(3)]
        [Test]
      
        public void GetAllMovies_ShouldReturnNonEmpty()
        {
            var request = new RestRequest("/api/Catalog/All", Method.Get);

            var response = client.Execute(request);

            var data = JsonSerializer.Deserialize<List<MovieDto>>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(data, Is.Not.Null);
            Assert.That(data, Is.Not.Empty);
        }


        [Order(4)]
        [Test]
        
        public void DeleteMovie_ShouldSucceed()
        {
            var request = new RestRequest("/api/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", createdMovieId);

            var response = client.Execute(request);
            var data = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(data.Msg, Is.EqualTo("Movie deleted successfully!"));
        }

        [Order(5)]
        [Test]
        
        public void CreateMovie_WithoutRequired_ShouldFail()
        {
            var request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(new { });

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Order(6)]
        [Test]
        
        public void EditNonExistingMovie_ShouldFail()
        {
            var request = new RestRequest("/api/Movie/Edit", Method.Put);

            request.AddQueryParameter("movieId", "999999");

            request.AddJsonBody(new MovieDto
            {
                Title = "Test",
                Description = "Test"
            });

            var response = client.Execute(request);
            var data = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(data.Msg, Is.EqualTo("Unable to edit the movie! Check the movieId parameter or user verification!"));
        }

        [Order(7)]
        [Test]
        
        public void DeleteNonExistingMovie_ShouldFail()
        {
            var request = new RestRequest("/api/Movie/Delete", Method.Delete);

            request.AddQueryParameter("movieId", "999999");

            var response = client.Execute(request);
            var data = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(data.Msg, Is.EqualTo("Unable to delete the movie! Check the movieId parameter or user verification!"));
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            client?.Dispose();
        }
    }
}
