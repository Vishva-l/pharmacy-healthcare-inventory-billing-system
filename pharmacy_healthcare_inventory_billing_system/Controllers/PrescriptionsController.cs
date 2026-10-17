﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pharmacy_healthcare_inventory_billing_system.Data;
using pharmacy_healthcare_inventory_billing_system.Models;
using pharmacy_healthcare_inventory_billing_system.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace pharmacy_healthcare_inventory_billing_system.Controllers
{
    public class PrescriptionsController : Controller
    {
        private readonly PharmacyDbContext _context;

        private readonly IWebHostEnvironment _environment;

        public PrescriptionsController(
            PharmacyDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;

            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var prescriptions =
                await _context.Prescriptions
                .Include(x => x.Items)
                .ThenInclude(x => x.Medicine)
                .OrderByDescending(x => x.UploadedAt)
                .ToListAsync();

            return View(prescriptions);
        }

        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(
            PrescriptionUploadViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.PrescriptionFile == null || model.PrescriptionFile.Length == 0)
            {
                ModelState.AddModelError(nameof(model.PrescriptionFile), "Please select a prescription file.");
                return View(model);
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
            var extension = Path.GetExtension(model.PrescriptionFile.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(nameof(model.PrescriptionFile), "Only JPG, JPEG, PNG and PDF files are allowed.");
                return View(model);
            }

            if (model.PrescriptionFile.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError(nameof(model.PrescriptionFile), "File size must be 5 MB or less.");
                return View(model);
            }

            model.Quantity = Math.Max(1, model.Quantity);

            string folder = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "prescriptions");

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string fileName =
                Guid.NewGuid().ToString()
                + extension;

            string filePath =
                Path.Combine(folder, fileName);

            using (var stream =
                   new FileStream(
                       filePath,
                       FileMode.Create))
            {
                await model.PrescriptionFile
                    .CopyToAsync(stream);
            }

            var prescription = new Prescription
            {
                PatientName = model.PatientName,

                DoctorName = model.DoctorName,

                FileName =
                    model.PrescriptionFile.FileName,

                FilePath =
                    "/uploads/prescriptions/"
                    + fileName,

                UploadedAt = DateTime.Now
            };

            _context.Prescriptions.Add(
                prescription);

            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(
                    model.MedicineNames))
            {
                var names =
                    model.MedicineNames
                    .Split(
                        ',',
                        StringSplitOptions
                            .RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .ToList();

                foreach (var name in names)
                {
                    var medicine =
                        await _context.Medicines
                        .FirstOrDefaultAsync(x =>
                            x.Name.ToLower()
                            == name.ToLower());

                    if (medicine != null)
                    {
                        _context.PrescriptionItems.Add(
                            new PrescriptionItem
                            {
                                PrescriptionId =
                                    prescription
                                    .PrescriptionId,

                                MedicineId =
                                    medicine.MedicineId,

                                Quantity =
                                    model.Quantity
                            });
                    }
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(
                nameof(Index));
        }
    }
}
