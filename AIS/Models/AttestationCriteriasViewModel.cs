using AIS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AIS.Models
{
    public class AttestationCriteriasViewModel
    {
        public Attestation Attestations { get; set; }
        public IEnumerable<Criteria> Criterias { get; set; }
        public IEnumerable<Discipline> Disciplines { get; set; }
        public IEnumerable<Group> Groups { get; set; }
    }
}