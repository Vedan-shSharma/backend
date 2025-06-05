using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduSyncAPI.Data;
using EduSyncAPI.Model;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace EduSyncAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseEnrollmentController : ControllerBase
    {
        private readonly EduSyncDbContext _context;

        public CourseEnrollmentController(EduSyncDbContext context)
        {
            _context = context;
        }

        // GET: api/CourseEnrollment/student/{studentId}
        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetStudentEnrollments(Guid studentId)
        {
            var enrollments = await _context.CourseEnrollments
                .Include(e => e.Course)
                .Where(e => e.StudentId == studentId)
                .Select(e => new
                {
                    e.EnrollmentId,
                    e.CourseId,
                    CourseName = e.Course.Title,
                    e.EnrollmentDate
                })
                .ToListAsync();

            return Ok(enrollments);
        }

        // GET: api/CourseEnrollment/course/{courseId}/students
        [HttpGet("course/{courseId}/students")]
        public async Task<IActionResult> GetCourseEnrollments(Guid courseId)
        {
            var enrollments = await _context.CourseEnrollments
                .Include(e => e.Student)
                .Where(e => e.CourseId == courseId)
                .Select(e => new
                {
                    e.EnrollmentId,
                    e.StudentId,
                    StudentName = e.Student.Name,
                    e.EnrollmentDate
                })
                .ToListAsync();

            return Ok(enrollments);
        }

        // POST: api/CourseEnrollment
        [HttpPost]
        public async Task<IActionResult> EnrollStudent([FromBody] CourseEnrollment enrollment)
        {
            // Check if student exists
            var student = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == enrollment.StudentId && u.Role == "Student");
            if (student == null)
                return BadRequest("Student not found");

            // Check if course exists
            var course = await _context.Courses.FindAsync(enrollment.CourseId);
            if (course == null)
                return BadRequest("Course not found");

            // Check if already enrolled
            var existingEnrollment = await _context.CourseEnrollments
                .FirstOrDefaultAsync(e => e.StudentId == enrollment.StudentId && e.CourseId == enrollment.CourseId);
            if (existingEnrollment != null)
                return BadRequest("Student is already enrolled in this course");

            // Create enrollment
            enrollment.EnrollmentDate = DateTime.UtcNow;
            _context.CourseEnrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Enrollment successful" });
        }

        // DELETE: api/CourseEnrollment/{enrollmentId}
        [HttpDelete("{enrollmentId}")]
        public async Task<IActionResult> UnenrollStudent(Guid enrollmentId)
        {
            var enrollment = await _context.CourseEnrollments.FindAsync(enrollmentId);
            if (enrollment == null)
                return NotFound();

            _context.CourseEnrollments.Remove(enrollment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Successfully unenrolled from the course" });
        }

        // GET: api/CourseEnrollment/check/{studentId}/{courseId}
        [HttpGet("check/{studentId}/{courseId}")]
        public async Task<IActionResult> CheckEnrollment(Guid studentId, Guid courseId)
        {
            var enrollment = await _context.CourseEnrollments
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);

            return Ok(new { isEnrolled = enrollment != null });
        }
    }
} 