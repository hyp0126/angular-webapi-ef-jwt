using System;
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
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace aspcore_jwt.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly EmployeeDB2Context _context;
        BlobServiceClient _blobServiceClient;

        public EmployeeController(EmployeeDB2Context context, BlobServiceClient blobServiceClient)
        {
            _context = context;
            _blobServiceClient = blobServiceClient;
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

        [HttpGet("Photos/{id}")]
        public async Task<IActionResult> Download(string id)
        {
            var directory = Path.Combine(Directory.GetCurrentDirectory(), "Photos");
            var filename = Path.Combine(directory, id);
            var mimeType = "application/octet-stream";

            // Local
            //var stream = new FileStream(filename, FileMode.Open);

            // Azure Storage
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient("photos");
            Stream stream = null;
            
            if (await containerClient.ExistsAsync())
            {
                BlobClient blobClient = containerClient.GetBlobClient(id);

                if (await blobClient.ExistsAsync())
                {
                    stream = new MemoryStream();
                    BlobDownloadInfo download = await blobClient.DownloadAsync();
                    await download.Content.CopyToAsync(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                }
            }

            if (stream == null)
                return NotFound(); // returns a NotFoundResult with Status404NotFound response.

            return File(stream, mimeType); // returns a FileStreamResult
        }

        //Single File
        //[Route("SaveFile")]
        //[HttpPost]
        //public string SaveFile()
        //{
        //    try
        //    {
        //        var postedFile = Request.Form.Files[0];
        //        string filename = postedFile.FileName;
        //        var directory = Path.Combine(Directory.GetCurrentDirectory(), "Photos");
        //        var physicalPath = Path.Combine(directory, filename);

        //        using (var stream = new FileStream(physicalPath, FileMode.Create))
        //        {
        //            postedFile.CopyTo(stream);
        //        }
        //        return $"\"{filename}\"";
        //    }
        //    catch (Exception)
        //    {
        //        return "anonymous.png";
        //    }
        //}


        // Multiple files
        [Route("SaveFile")]
        [HttpPost]
        async public Task<string> SaveFile()
        {
            try
            {
                var postedFiles = Request.Form.Files;
                var directory = Path.Combine(Directory.GetCurrentDirectory(), "Photos");

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                for (int i = 0; i < Request.Form.Files.Count; i++)
                {
                    string filename = postedFiles[i].FileName;
                    var physicalPath = Path.Combine(directory, filename);

                    using (var stream = new FileStream(physicalPath, FileMode.Create))
                    {
                        // Local
                        //postedFiles[i].CopyTo(stream);

                        // Azure Storage
                        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient("photos");
                        BlobClient blobClient = containerClient.GetBlobClient(filename);
                        await blobClient.UploadAsync(postedFiles[i].OpenReadStream(), true);
                    }
                }

                // return first file name
                return $"\"{postedFiles[0].FileName}\"";
            }
            catch (Exception)
            {
                return "anonymous.png";
            }
        }
    }
}
