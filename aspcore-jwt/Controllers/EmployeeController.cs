﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using aspcore_jwt.Models;
using System.Net.Http;
using System.Net;
using System.IO;
using Microsoft.AspNetCore.Authorization;

namespace aspcore_jwt.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly EmployeeDB2Context _context;

        public EmployeeController(EmployeeDB2Context context)
        {
            _context = context;
        }

        // GET: api/Employee
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employees>>> GetEmployees()
        {
            return await _context.Employees.ToListAsync();
        }

        // GET: api/Employee/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Employees>> GetEmployees(int id)
        {
            var employees = await _context.Employees.FindAsync(id);

            if (employees == null)
            {
                return NotFound();
            }

            return employees;
        }

        // PUT: api/Employee/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        //[HttpPut("{id}")]
        [HttpPut]
//        public async Task<IActionResult> PutEmployees(int id, Employees employees)
        public async Task<IActionResult> PutEmployees(Employees employees)
        {
            //if (id != employees.EmployeeId)
            //{
            //    return BadRequest();
            //}

            _context.Entry(employees).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                //if (!EmployeesExists(id))
                if (!EmployeesExists(employees.EmployeeId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Employee
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Employees>> PostEmployees(Employees employees)
        {
            _context.Employees.Add(employees);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEmployees", new { id = employees.EmployeeId }, employees);
        }

        // DELETE: api/Employee/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Employees>> DeleteEmployees(int id)
        {
            var employees = await _context.Employees.FindAsync(id);
            if (employees == null)
            {
                return NotFound();
            }

            _context.Employees.Remove(employees);
            await _context.SaveChangesAsync();

            return employees;
        }

        private bool EmployeesExists(int id)
        {
            return _context.Employees.Any(e => e.EmployeeId == id);
        }

        [Route("GetAllDepartmentNames")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Departments>>> GetAllDepartmentNames()
        {
            //return await _context.Departments.Select(d => new { DepartmentName = d.DepartmentName }).ToListAsync();
            return await _context.Departments.ToListAsync();
        }

        //[Route("Photos")]
        [HttpGet("Photos/{id}")]
        public async Task<IActionResult> Download(string id)
        {
            var directory = Path.Combine(Directory.GetCurrentDirectory(), "Photos");
            var filename = Path.Combine(directory, id);
            var mimeType = "application/octet-stream";
            //var mimeType = "image/png";
            var stream = new FileStream(filename, FileMode.Open);

            if (stream == null)
                return NotFound(); // returns a NotFoundResult with Status404NotFound response.

            return File(stream, mimeType); // returns a FileStreamResult
        }

        [Route("SaveFile")]
        [HttpPost]
        public string SaveFile()
        {
            try
            {
                var postedFile = Request.Form.Files[0];
                string filename = postedFile.FileName;
                var directory = Path.Combine(Directory.GetCurrentDirectory(), "Photos");
                var physicalPath = Path.Combine(directory, filename);

                using (var stream = new FileStream(physicalPath, FileMode.Create))
                {
                    postedFile.CopyTo(stream);
                }
                return $"\"{filename}\"";
            }
            catch (Exception)
            {
                return "anonymous.png";
            }
        }
    }
}
