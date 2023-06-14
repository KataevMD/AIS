using AIS.Entities;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace AIS.Controllers
{
    public class AdminProcedureForTransferringPointsController : Controller
    {
        private AISEntities db = new AISEntities();

        // GET: ProcedureForTransferringPoints
        public ActionResult Index()
        {
            return View(db.ProcedureForTransferringPoints.ToList());
        }

        // GET: ProcedureForTransferringPoints/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProcedureForTransferringPoints procedureForTransferringPoints = db.ProcedureForTransferringPoints.Find(id);
            if (procedureForTransferringPoints == null)
            {
                return HttpNotFound();
            }
            return View(procedureForTransferringPoints);
        }

        // POST: ProcedureForTransferringPoints/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. 
        // Дополнительные сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "FinalGrade,MinPercents,MaxPersents")] ProcedureForTransferringPoints procedureForTransferringPoints)
        {
            if (ModelState.IsValid)
            {
                db.Entry(procedureForTransferringPoints).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(procedureForTransferringPoints);
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
