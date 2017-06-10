using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace NewgramWeb.Models
{
    public class Seguidor
    {

        [Key, Column(Order = 0)]
        public int SeguidorID { get; set; }

        [Key, Column(Order = 1)]
        public int SeguidoID { get; set; }

    }
}