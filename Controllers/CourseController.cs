using EduSyncAPI.Data;
using EduSyncAPI.Dto;
using EduSyncAPI.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduSyncAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly EduSyncDbContext _context;

        public CourseController(EduSyncDbContext context)
        {
            _context = context;
        }

        // GET: api/Course
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseWithInstructorDto>>> GetCourses()
        {
            var coursesWithInstructors = await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Materials)
                .Select(c => new CourseWithInstructorDto
                {
                    CourseId = c.CourseId,
                    Title = c.Title,
                    Description = c.Description,
                    InstructorId = c.InstructorId,
                    InstructorName = c.Instructor != null ? c.Instructor.Name : string.Empty,
                    Materials = c.Materials.Select(m => new CourseMaterialDto
                    {
                        MaterialId = m.MaterialId,
                        FileName = m.FileName,
                        FileType = m.FileType,
                        FileUrl = m.FileUrl,
                        Description = m.Description,
                        UploadDate = m.UploadDate
                    }).ToList()
                })
                .ToListAsync();

            return Ok(coursesWithInstructors);
        }

        // GET: api/Course/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CourseWithInstructorDto>> GetCourse(Guid id)
        {
            var course = await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Materials)
                .Where(c => c.CourseId == id)
                .Select(c => new CourseWithInstructorDto
                {
                    CourseId = c.CourseId,
                    Title = c.Title,
                    Description = c.Description,
                    InstructorId = c.InstructorId,
                    InstructorName = c.Instructor != null ? c.Instructor.Name : string.Empty,
                    Materials = c.Materials.Select(m => new CourseMaterialDto
                    {
                        MaterialId = m.MaterialId,
                        FileName = m.FileName,
                        FileType = m.FileType,
                        FileUrl = m.FileUrl,
                        Description = m.Description,
                        UploadDate = m.UploadDate
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (course == null)
                return NotFound();

            return Ok(course);
        }

        // POST: api/Course
        [HttpPost]
        public async Task<IActionResult> PostCourse([FromBody] CourseCreateDto courseDto)
        {
            var instructor = await _context.Users.FirstOrDefaultAsync(u => u.UserId == courseDto.InstructorId && u.Role == "Instructor");
            if (instructor == null)
                return BadRequest("Instructor not found.");

            var course = new Course
            {
                CourseId = Guid.NewGuid(),
                Title = courseDto.Title,
                Description = courseDto.Description,
                InstructorId = courseDto.InstructorId
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCourse), new { id = course.CourseId }, course);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCourse(Guid id, CourseUpdateDto courseDto)
        {
            var existingCourse = await _context.Courses.FindAsync(id);
            if (existingCourse == null)
                return NotFound();

            existingCourse.Title = courseDto.Title;
            existingCourse.Description = courseDto.Description;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/Course/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(Guid id)
        {
            var course = await _context.Courses
                .Include(c => c.Materials)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
                return NotFound();

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CourseExists(Guid id)
        {
            return _context.Courses.Any(e => e.CourseId == id);
        }
    }
} 