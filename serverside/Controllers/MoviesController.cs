using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace serverside.Controllers
{
	[Route("api")]
	[ApiController]
	public class MoviesController : ControllerBase
	{
		private readonly HttpClient _httpClient;
		private readonly string _apiKey;

		public MoviesController(HttpClient httpClient)
		{
			_httpClient = httpClient;
			_apiKey = Environment.GetEnvironmentVariable("API_KEY");
		}

		// GET api/popular
		[HttpGet("popular")]
		public async Task<ActionResult> GetPopularMovies()
		{
			try
			{
				if (string.IsNullOrEmpty(_apiKey))
				{
					return BadRequest("API key not configured");
				}

				string url = $"https://api.themoviedb.org/3/movie/popular?api_key={_apiKey}";
				HttpResponseMessage response = await _httpClient.GetAsync(url);

				if (response.IsSuccessStatusCode)
				{
					string jsonContent = await response.Content.ReadAsStringAsync();
					dynamic movieData = JsonConvert.DeserializeObject(jsonContent);

					// Take only the first 20 movies as required
					var movies = new List<object>();
					for (int i = 0; i < Math.Min(20, movieData.results.Count); i++)
					{
						movies.Add(movieData.results[i]);
					}

					return Ok(new { results = movies });
				}
				else
				{
					return StatusCode((int)response.StatusCode, "Error fetching movies from TMDB API");
				}
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error: {ex.Message}");
			}
		}

		// GET api/search?query=birdbox
		[HttpGet("search")]
		public async Task<ActionResult> SearchMovies([FromQuery] string query)
		{
			try
			{
				if (string.IsNullOrEmpty(_apiKey))
				{
					return BadRequest("API key not configured");
				}

				if (string.IsNullOrEmpty(query))
				{
					return BadRequest("Search query is required");
				}

				string url = $"https://api.themoviedb.org/3/search/movie?api_key={_apiKey}&query={Uri.EscapeDataString(query)}";
				HttpResponseMessage response = await _httpClient.GetAsync(url);

				if (response.IsSuccessStatusCode)
				{
					string jsonContent = await response.Content.ReadAsStringAsync();
					return Ok(JsonConvert.DeserializeObject(jsonContent));
				}
				else
				{
					return StatusCode((int)response.StatusCode, "Error searching movies from TMDB API");
				}
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error: {ex.Message}");
			}
		}

		// GET api/movie/1145
		[HttpGet("movie/{id}")]
		public async Task<ActionResult> GetMovieById(int id)
		{
			try
			{
				if (string.IsNullOrEmpty(_apiKey))
				{
					return BadRequest("API key not configured");
				}

				string url = $"https://api.themoviedb.org/3/movie/{id}?api_key={_apiKey}";
				HttpResponseMessage response = await _httpClient.GetAsync(url);

				if (response.IsSuccessStatusCode)
				{
					string jsonContent = await response.Content.ReadAsStringAsync();
					return Ok(JsonConvert.DeserializeObject(jsonContent));
				}
				else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
				{
					return NotFound("Movie not found");
				}
				else
				{
					return StatusCode((int)response.StatusCode, "Error fetching movie from TMDB API");
				}
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error: {ex.Message}");
			}
		}
	}
}