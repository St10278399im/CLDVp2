using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApplicationTest.Models;

namespace WebApplicationTest.Controllers
{
    public class BookingController : Controller
    {
        private TestDBContext db = new TestDBContext();

        // GET: Booking
        public ActionResult Index(string searchString)
        {
            var bookings = db.Bookings.Include(b => b.Event);

            if (!String.IsNullOrEmpty(searchString))
            {
                bookings = bookings.Where(b =>
                    b.CustomerName.Contains(searchString) ||
                    b.CustomerEmail.Contains(searchString) ||
                    b.Event.Title.Contains(searchString));
            }

            return View(bookings.ToList());
        }

        // GET: Booking/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Booking booking = db.Bookings.Include(b => b.Event).FirstOrDefault(b => b.Id == id);

            if (booking == null)
                return HttpNotFound();

            return View(booking);
        }

        // GET: Booking/Create
        public ActionResult Create()
        {
            ViewBag.EventId = new SelectList(db.Events, "Id", "Title");
            return View();
        }

        // POST: Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,CustomerName,CustomerEmail,EventId")] Booking booking)
        {
            var selectedEvent = db.Events.Include(e => e.Venue).FirstOrDefault(e => e.Id == booking.EventId);

            if (selectedEvent == null)
            {
                ModelState.AddModelError("", "Invalid event selected.");
            }
            else
            {
                var venueId = selectedEvent.VenueId;
                var eventDate = selectedEvent.Date;

                bool isDoubleBooked = db.Bookings
                    .Include(b => b.Event)
                    .Any(b => b.Event.VenueId == venueId && b.Event.Date == eventDate);

                if (isDoubleBooked)
                {
                    ModelState.AddModelError("", "This venue is already booked at the selected date and time.");
                }
            }

            if (ModelState.IsValid)
            {
                db.Bookings.Add(booking);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.EventId = new SelectList(db.Events, "Id", "Title", booking.EventId);
            return View(booking);
        }

        // GET: Booking/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Booking booking = db.Bookings.Find(id);
            if (booking == null)
                return HttpNotFound();

            ViewBag.EventId = new SelectList(db.Events, "Id", "Title", booking.EventId);
            return View(booking);
        }

        // POST: Booking/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,CustomerName,CustomerEmail,EventId")] Booking booking)
        {
            var selectedEvent = db.Events.Include(e => e.Venue).FirstOrDefault(e => e.Id == booking.EventId);

            if (selectedEvent == null)
            {
                ModelState.AddModelError("", "Invalid event selected.");
            }
            else
            {
                var venueId = selectedEvent.VenueId;
                var eventDate = selectedEvent.Date;

                bool isDoubleBooked = db.Bookings
                    .Include(b => b.Event)
                    .Any(b => b.Id != booking.Id && b.Event.VenueId == venueId && b.Event.Date == eventDate);

                if (isDoubleBooked)
                {
                    ModelState.AddModelError("", "This venue is already booked at the selected date and time.");
                }
            }

            if (ModelState.IsValid)
            {
                db.Entry(booking).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.EventId = new SelectList(db.Events, "Id", "Title", booking.EventId);
            return View(booking);
        }

        // GET: Booking/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Booking booking = db.Bookings.Include(b => b.Event).FirstOrDefault(b => b.Id == id);

            if (booking == null)
                return HttpNotFound();

            return View(booking);
        }

        // POST: Booking/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Booking booking = db.Bookings.Find(id);
            db.Bookings.Remove(booking);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}