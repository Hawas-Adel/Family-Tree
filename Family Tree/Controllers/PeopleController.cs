using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Family_Tree;

namespace Family_Tree.Controllers
{
	public class PeopleController : Controller
	{
		private Family_DBEntities db = new Family_DBEntities();

		// GET: People
		public ActionResult Index()
		{
			var people = db.People.Include(p => p.Person1).Include(p => p.Person2);
			return View(people.ToList());
		}

		// GET: People/Details/5
		public ActionResult Details(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			Person person = db.People.Find(id);
			if (person == null)
			{
				return HttpNotFound();
			}
			List<(Person Relative, string Relation)> Relatives = new List<(Person Relative, string Relation)>();
			if (person.FatherID.HasValue)
			{
				var Father = db.People.Find(person.FatherID.Value);
				Relatives.Add((Father, "Father"));
			}
			if (person.MotherID.HasValue)
			{
				var Mother = db.People.Find(person.MotherID.Value);
				Relatives.Add((Mother, "Mother"));
			}
			if (person.FatherID.HasValue)
			{
				var Siblings = db.People.Where(P => P.FatherID == person.FatherID && P.ID != person.ID);
				foreach (var item in Siblings)
					Relatives.Add((item, "Sibling"));
			}
			if (person.MotherID.HasValue)
			{
				var Siblings = db.People.Where(P => P.MotherID == person.MotherID && P.ID != person.ID);
				foreach (var item in Siblings)
					Relatives.Add((item, "Sibling"));
			}
			var Children = db.People.Where(P => (person.Gender && P.MotherID == person.ID) || (!person.Gender && P.FatherID == person.ID));
			foreach (var item in Children)
				Relatives.Add((item, "Child"));

			Relatives = Relatives.Distinct().ToList();

			return View((person, Relatives));
		}

		// GET: People/Create
		public ActionResult Create()
		{
			InitializeSelectLists();
			return View();
		}

		//private void InitializeSelectLists()
		//{
		//	ViewBag.FatherID = new SelectList(db.People.Where(P => !P.Gender), "ID", "FirstName");
		//	ViewBag.MotherID = new SelectList(db.People.Where(P => P.Gender), "ID", "FirstName");
		//}
		private void InitializeSelectLists(Person person =null)
		{
			ViewBag.FatherID = new SelectList(db.People.Where(P => !P.Gender).ToList().ConvertAll(P => new { ID = P.ID, FullName = $"{P.FirstName} {P.LastName}" }),
				"ID", "FullName", person?.FatherID);
			ViewBag.MotherID = new SelectList(db.People.Where(P => P.Gender).ToList().ConvertAll(P => new { ID = P.ID, FullName = $"{P.FirstName} {P.LastName}" }),
				"ID", "FullName", person?.MotherID);
		}

		// POST: People/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to, for 
		// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create([Bind(Include = "ID,FirstName,LastName,Gender,FatherID,MotherID")] Person person)
		{
			if (ModelState.IsValid)
			{
				db.People.Add(person);
				db.SaveChanges();
				return RedirectToAction("Index");
			}

			InitializeSelectLists(person);
			return View(person);
		}

		// GET: People/Edit/5
		public ActionResult Edit(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			Person person = db.People.Find(id);
			if (person == null)
			{
				return HttpNotFound();
			}
			InitializeSelectLists(person);
			return View(person);
		}

		// POST: People/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to, for 
		// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit([Bind(Include = "ID,FirstName,LastName,Gender,FatherID,MotherID")] Person person)
		{
			if (ModelState.IsValid)
			{
				db.Entry(person).State = EntityState.Modified;
				db.SaveChanges();
				return RedirectToAction("Index");
			}
			InitializeSelectLists(person);
			return View(person);
		}

		// GET: People/Delete/5
		public ActionResult Delete(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			Person person = db.People.Find(id);
			if (person == null)
			{
				return HttpNotFound();
			}
			return View(person);
		}

		// POST: People/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteConfirmed(int id)
		{
			try
			{
				Person person = db.People.Find(id);
				db.People.Remove(person);
				db.SaveChanges();
			}
			catch (Exception)
			{

			}
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
