using AIS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AIS.Models
{
    public class AttestationListViewModel
    {
        public IEnumerable<Attestation> Attestations { get; set; }
        public int IdCurentUser { get; set; }
        public SelectList TypeAttestations { get; set; }

    }
}