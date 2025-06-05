using EduSyncAPI.Data;
using EduSyncAPI.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using EduSyncAPI.Dto;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace YourProjectNamespace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultController : ControllerBase
    {
        private readonly EduSyncDbContext _context;
        private readonly ILogger<ResultController> _logger;

        public ResultController(EduSyncDbContext context, ILogger<ResultController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // POST: api/Result
        [HttpPost]
        public async Task<IActionResult> CreateResult(ResultCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var assessmentExists = await _context.Assessments.AnyAsync(a => a.AssessmentId == dto.AssessmentId);
            var userExists = await _context.Users.AnyAsync(u => u.UserId == dto.UserId);

            if (!assessmentExists)
                return NotFound(new { message = "Assessment not found." });

            if (!userExists)
                return NotFound(new { message = "User not found." });

            var result = new Result
            {
                ResultId = Guid.NewGuid(),
                AssessmentId = dto.AssessmentId,
                UserId = dto.UserId,
                Score = dto.Score,
                AttemptDate = dto.AttemptDate
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetResultById), new { id = result.ResultId }, result);
        }

        // POST: api/Result/attempt
        [HttpPost("attempt")]
        public async Task<IActionResult> AttemptAssessment([FromBody] AssessmentAttemptDto dto)
        {
            var assessment = await _context.Assessments.FindAsync(dto.AssessmentId);
            if (assessment == null)
                return NotFound("Assessment not found.");

            List<McqQuestionDto> questions;
            try
            {
                questions = JsonSerializer.Deserialize<List<McqQuestionDto>>(assessment.Questions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize questions for assessment {AssessmentId}", dto.AssessmentId);
                return BadRequest("Invalid questions format.");
            }

            if (questions == null || questions.Count == 0)
                return BadRequest("No questions found in assessment.");

            int score = 0;
            for (int i = 0; i < questions.Count; i++)
            {
                if (i < dto.SelectedAnswers.Count && dto.SelectedAnswers[i] == questions[i].CorrectIndex)
                    score++;
            }

            var result = new Result
            {
                ResultId = Guid.NewGuid(),
                AssessmentId = dto.AssessmentId,
                UserId = dto.UserId,
                Score = score,
                AttemptDate = DateTime.UtcNow
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return Ok(new { score, total = questions.Count });
        }

        // GET: api/Result
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Result>>> GetAllResults()
        {
            var results = await _context.Results
                .Include(r => r.User)
                .Include(r => r.Assessment)
                .ToListAsync();

            return Ok(results);
        }

        // GET: api/Result/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Result>> GetResultById(Guid id)
        {
            var result = await _context.Results
                .Include(r => r.User)
                .Include(r => r.Assessment)
                .FirstOrDefaultAsync(r => r.ResultId == id);

            if (result == null)
                return NotFound(new { message = "Result not found." });

            return Ok(result);
        }
    }
}
