using System.Text.Json.Serialization;

namespace EduSyncAPI.Dto
{
    public class McqQuestionDto
    {
        [JsonPropertyName("question")]
        public string Question { get; set; }

        [JsonPropertyName("options")]
        public List<string> Options { get; set; }

        [JsonPropertyName("correctIndex")]
        public int CorrectIndex { get; set; }
    }
}
