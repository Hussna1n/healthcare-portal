using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using HealthcareAPI.Data;
using HealthcareAPI.Models;
using System.Security.Claims;

namespace HealthcareAPI.Controllers;

[ApiController, Route("api/appointments"), Authorize]
public class AppointmentsController(AppDbContext db) : ControllerBase
{
    private int PatientId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var query = db.Appointments
            .Include(a => a.Doctor)
            .Where(a => a.PatientId == PatientId);

        if (!string.IsNullOrEmpty(status)) query = query.Where(a => a.Status == status);
        if (from.HasValue) query = query.Where(a => a.ScheduledAt >= from);
        if (to.HasValue) query = query.Where(a => a.ScheduledAt <= to);

        return Ok(await query.OrderBy(a => a.ScheduledAt).ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Book([FromBody] BookAppointmentRequest req)
    {
        // Check doctor availability
        var conflict = await db.Appointments.AnyAsync(a =>
            a.DoctorId == req.DoctorId &&
            a.Status != "cancelled" &&
            a.ScheduledAt < req.ScheduledAt.AddMinutes(req.DurationMinutes) &&
            a.ScheduledAt.AddMinutes(a.DurationMinutes) > req.ScheduledAt);

        if (conflict) return BadRequest("Doctor is not available at this time");

        var appt = new Appointment {
            PatientId = PatientId, DoctorId = req.DoctorId,
            ScheduledAt = req.ScheduledAt, DurationMinutes = req.DurationMinutes,
            Type = req.Type, Notes = req.Notes
        };
        db.Appointments.Add(appt);
        await db.SaveChangesAsync();

        await db.Entry(appt).Reference(a => a.Doctor).LoadAsync();
        return CreatedAtAction(nameof(GetById), new { id = appt.Id }, appt);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var a = await db.Appointments
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.Id == id && a.PatientId == PatientId);
        return a is null ? NotFound() : Ok(a);
    }

    [HttpPatch("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var a = await db.Appointments.FindAsync(id);
        if (a is null || a.PatientId != PatientId) return NotFound();
        if (a.ScheduledAt < DateTime.UtcNow.AddHours(24)) return BadRequest("Cannot cancel within 24 hours");
        a.Status = "cancelled";
        await db.SaveChangesAsync();
        return Ok(a);
    }
}

[ApiController, Route("api/doctors")]
public class DoctorsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? specialization, [FromQuery] string? search)
    {
        var query = db.Doctors.AsQueryable();
        if (!string.IsNullOrEmpty(specialization)) query = query.Where(d => d.Specialization == specialization);
        if (!string.IsNullOrEmpty(search)) query = query.Where(d => d.FirstName.Contains(search) || d.LastName.Contains(search) || d.Specialization.Contains(search));
        return Ok(await query.OrderByDescending(d => d.Rating).ToListAsync());
    }

    [HttpGet("{id}/availability")]
    public async Task<IActionResult> GetAvailability(int id, [FromQuery] DateTime date)
    {
        var booked = await db.Appointments
            .Where(a => a.DoctorId == id && a.ScheduledAt.Date == date.Date && a.Status != "cancelled")
            .Select(a => a.ScheduledAt)
            .ToListAsync();

        var slots = Enumerable.Range(0, 16)
            .Select(i => date.Date.AddHours(8).AddMinutes(i * 30))
            .Where(s => !booked.Any(b => Math.Abs((s - b).TotalMinutes) < 30))
            .ToList();

        return Ok(slots);
    }
}

public record BookAppointmentRequest(int DoctorId, DateTime ScheduledAt, int DurationMinutes, string Type, string? Notes);
