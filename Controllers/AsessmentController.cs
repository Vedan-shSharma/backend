using EduSyncAPI.Data;
using EduSyncAPI.Dto;
using EduSyncAPI.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduSyncAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssessmentController : ControllerBase
    {
        private readonly EduSyncDbContext _context;

        public AssessmentController(EduSyncDbContext context)
        {
            _context = context;
        }

        // GET: api/Assessment
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AssessmentResponseDto>>> GetAssessments()
        {
            var assessments = await _context.Assessments
                .Include(a => a.Course)
                .ThenInclude(c => c.Instructor)
                .Select(a => new AssessmentResponseDto
                {
                    AssessmentId = a.AssessmentId,
                    Title = a.Title,
                    Questions = a.Questions,
                    MaxScore = a.MaxScore,
                    CourseId = a.CourseId,
                    CourseTitle = a.Course.Title,
                    InstructorName = a.Course.Instructor.Name
                })
                .ToListAsync();

            return Ok(assessments);
        }

        // ✅ NEW: GET /api/Assessment/available
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableAssessments()
        {
            var assessments = await _context.Assessments
                .Include(a => a.Course)
                .Where(a => !string.IsNullOrEmpty(a.Questions)) // Ensures assessment has questions
                .Select(a => new
                {
                    a.AssessmentId,
                    a.Title,
                    a.MaxScore,
                    CourseTitle = a.Course.Title
                })
                .ToListAsync();

            return Ok(assessments);
        }

        // GET: api/Assessment/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Assessment>> GetAssessment(Guid id)
        {
            var assessment = await _context.Assessments.FindAsync(id);

            if (assessment == null)
                return NotFound();

            return assessment;
        }

        // GET: api/Assessment/by-course/{courseId}
        [HttpGet("by-course/{courseId}")]
        public async Task<IActionResult> GetAssessmentByCourse(Guid courseId)
        {
            var assessment = await _context.Assessments
                .Include(a => a.Course)
                .ThenInclude(c => c.Instructor)
                .FirstOrDefaultAsync(a => a.CourseId == courseId);

            if (assessment == null)
                return NotFound("Assessment not found for the course.");

            var questions = JsonSerializer.Deserialize<List<McqQuestionDto>>(assessment.Questions);

            return Ok(new
            {
                AssessmentId = assessment.AssessmentId,
                CourseTitle = assessment.Course.Title,
                InstructorName = assessment.Course.Instructor?.Name,
                Questions = questions.Select(q => new
                {
                    q.Question,
                    q.Options
                }).ToList()
            });
        }

        // POST: api/Assessment/attempt
        [HttpPost("attempt")]
        public async Task<IActionResult> SubmitAssessmentAttempt([FromBody] AssessmentAttemptDto attemptDto)
        {
            var assessment = await _context.Assessments.FindAsync(attemptDto.AssessmentId);
            if (assessment == null)
                return NotFound("Assessment not found.");

            var questionList = JsonSerializer.Deserialize<List<McqQuestionDto>>(assessment.Questions);
            if (questionList == null || questionList.Count != attemptDto.SelectedAnswers.Count)
                return BadRequest("Invalid number of answers submitted.");

            int score = 0;
            for (int i = 0; i < questionList.Count; i++)
            {
                if (questionList[i].CorrectIndex == attemptDto.SelectedAnswers[i])
                    score++;
            }

            int percentage = (score * 100) / questionList.Count;

            return Ok(new
            {
                UserId = attemptDto.UserId,
                AssessmentId = assessment.AssessmentId,
                Score = score,
                Total = questionList.Count,
                Percentage = percentage,
                Status = percentage >= 50 ? "Passed" : "Failed"
            });
        }

        // POST: api/Assessment
        [HttpPost]
        public async Task<IActionResult> CreateAssessment([FromBody] AssessmentCreateDto dto)
        {
            var assessment = new Assessment
            {
                AssessmentId = Guid.NewGuid(),
                CourseId = dto.CourseId,
                Title = dto.Title,
                Questions = dto.Questions,
                MaxScore = dto.MaxScore
            };

            _context.Assessments.Add(assessment);
            await _context.SaveChangesAsync();

            return Ok(assessment);
        }

        //// PUT: api/Assessment/{id}
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutAssessment(Guid id, Assessment assessment)
        //{
        //    if (id != assessment.AssessmentId)
        //        return BadRequest();

        //    _context.Entry(assessment).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!AssessmentExists(id))
        //            return NotFound();
        //        else
        //            throw;
        //    }

        //    return NoContent();
        //}

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAssessment(Guid id, AssessmentUpdateDto assessmentDto)
        {
            var existingAssessment = await _context.Assessments.FindAsync(id);
            if (existingAssessment == null)
                return NotFound();

            // Only update the basic assessment information
            existingAssessment.Title = assessmentDto.Title;
            existingAssessment.Questions = assessmentDto.Questions;
            existingAssessment.MaxScore = assessmentDto.MaxScore;
            // Keep the existing CourseId

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AssessmentExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/Assessment/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAssessment(Guid id)
        {
            var assessment = await _context.Assessments.FindAsync(id);
            if (assessment == null)
                return NotFound();

            _context.Assessments.Remove(assessment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AssessmentExists(Guid id)
        {
            return _context.Assessments.Any(e => e.AssessmentId == id);
        }
    }
}
