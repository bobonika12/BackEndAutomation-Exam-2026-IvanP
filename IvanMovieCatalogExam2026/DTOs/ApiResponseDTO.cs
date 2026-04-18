using System.Text.Json.Serialization;

namespace IvanMovieCatalogExam2026.DTOs
{
    internal class ApiResponseDTO
    {
        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        [JsonPropertyName("movie")]
        public MovieDTO? Movie { get; set; }
    }
}