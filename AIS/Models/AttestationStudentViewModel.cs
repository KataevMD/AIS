using AIS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AIS.Models
{
    public class AttestationStudentViewModel
    {
        public Attestation Attestations { get; set; }
        public IEnumerable<Student> Students { get; set; }

        public IEnumerable<Criteria> Criterias { get; set; }
    }
}