using AIS.Entities;
using AIS.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AIS.Controllers
{
    [Authorize(Roles = "Администратор")]
    public class AdminAttestationController : Controller
    {
        private readonly AISEntities db = new AISEntities();

        // GET: AdminAttestation
        public ActionResult AdmAttestations(int? idTypeAttestation, int? idGroup, int? idDiscipline)
        {
            IEnumerable<Attestation> attestations;
           

            var typeAttestations = db.TypeAttestation.ToList();
            typeAttestations.Insert(0, new TypeAttestation { Title = "Все", IdTypeAttestation = 0 });

            var group = db.Group.ToList();
            group.Insert(0, new Group { Title = "Все", IdGroup = 0 });

            var disciplineCurrentUser = db.Discipline.ToList();
            disciplineCurrentUser.Insert(0, new Discipline { Title = "Все", IdDiscipline = 0 });

            attestations = db.Attestation.Where(a => a.Deleted == true);


            if (idTypeAttestation != null && idTypeAttestation != 0) // фильтрация аттестаций по типу
            {
                attestations = attestations.Where(a => a.IdTypeAttestation == idTypeAttestation);
            }

            if (idGroup != null && idGroup != 0) // фильтрация аттестаций по типу
            {
                attestations = attestations.Where(a => a.IdGroup == idGroup);
            }

            if (idDiscipline != null && idDiscipline != 0) // фильтрация аттестаций по типу
            {
                attestations = attestations.Where(a => a.IdDiscipline == idDiscipline);
            }


            AttestationListViewModel attestationListViewModel = new AttestationListViewModel
            {
                Attestations = attestations,
                TypeAttestations = new SelectList(typeAttestations, "IdTypeAttestation", "Title"),
                Groups = new SelectList(group, "IdGroup", "Title"),
                Disciplines = new SelectList(disciplineCurrentUser, "IdDiscipline", "Title"),
            };

            return View(attestationListViewModel);
        }

        // POST: Attestations/Delete/5
        public ActionResult DeleteConfirmed(int id)
        {
            Attestation attestation = db.Attestation.Find(id);
            var idCriteriaAttestation = db.Criteria.Where(c => c.IdAttestation == id).Select(c => c.IdCriteria);
            db.Vedomosti.RemoveRange(db.Vedomosti.Where(v => v.IdAttestation == id));
            db.StudentResult.RemoveRange(db.StudentResult.Where(sr => idCriteriaAttestation.Contains(sr.IdCriteria)));
            db.Criteria.RemoveRange(db.Criteria.Where(c => c.IdAttestation == id));
            db.Attestation.Remove(attestation);
            db.SaveChanges();

            return RedirectToAction("AdmAttestations");
        }
    }
}