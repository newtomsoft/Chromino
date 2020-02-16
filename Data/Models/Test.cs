using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Models
{
    [Table("Test")]
    public class Test
    {
        [Key]
        public int Id { get; set; }

        public int tata { get; set; }
        public int toto { get; set; }
        public bool Bot { get; set; }
    }
}
