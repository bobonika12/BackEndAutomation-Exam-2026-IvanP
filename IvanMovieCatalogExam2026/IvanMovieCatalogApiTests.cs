using IvanMovieCatalogExam2026.DTOs;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;

namespace IvanMovieCatalogExam2026
{
    [TestFixture]
    public class Tests
    {
        private RestClient client;
        private static string createdMovieId;

        private const string BaseUrl = "http://144.91.123.158:5000";

        private const string Email = "ivan.p123@softuni.bg";
        private const string Password = "ofmanqk";

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
                var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = json.GetProperty("accessToken").GetString();

                if (string.IsNullOrEmpty(token))
                    throw new Exception("Token missing!");

                return token;
            }

            throw new Exception("Authentication failed!");
        }

        [Order(1)]
        [Test]
        public void CreateMovie_ShouldSucceed()
        {
            var movie = new MovieDTO
            {
                Title = "Test Movie",
                Description = "Test Description",
                PosterUrl = "",
                TrailerLink = "",
                IsWatched = false
            };

            var request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(movie);

            var response = client.Execute(request);
            var data = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

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
            var movie = new MovieDTO
            {
                Title = "Edited Movie",
                Description = "Edited Description",
                PosterUrl = "",
                TrailerLink = "",
                IsWatched = true
            };

            var request = new RestRequest("/api/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", createdMovieId);
            request.AddJsonBody(movie);

            var response = client.Execute(request);
            var data = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(data.Msg, Is.EqualTo("Movie edited successfully!"));
        }

        [Order(3)]
        [Test]
        public void GetAllMovies_ShouldReturnList()
        {
            var request = new RestRequest("/api/Catalog/All", Method.Get);

            var response = client.Execute(request);
            var movies = JsonSerializer.Deserialize<List<MovieDTO>>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(movies, Is.Not.Null);
            Assert.That(movies, Is.Not.Empty);
        }

        [Order(4)]
        [Test]
        public void DeleteMovie_ShouldSucceed()
        {
            var request = new RestRequest("/api/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", createdMovieId);

            var response = client.Execute(request);
            var data = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(data.Msg, Is.EqualTo("Movie deleted successfully!"));
        }

        [Order(5)]
        [Test]
        public void CreateMovie_WithoutRequiredFields_ShouldFail()
        {
            var movie = new MovieDTO
            {
                Title = "",
                Description = ""
            };

            var request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(movie);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Order(6)]
        [Test]
        public void EditNonExistingMovie_ShouldFail()
        {
            var movie = new MovieDTO
            {
                Title = "Test",
                Description = "Test",
                IsWatched = false
            };

            var request = new RestRequest("/api/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", "999999");
            request.AddJsonBody(movie);

            var response = client.Execute(request);
            var data = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("Unable to edit the movie! Check the movieId parameter or user verification!"));
        }

        [Order(7)]
        [Test]
        public void DeleteNonExistingMovie_ShouldFail()
        {
            var request = new RestRequest("/api/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", "999999");

            var response = client.Execute(request);
            var data = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

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