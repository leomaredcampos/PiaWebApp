using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PiaWebApp.Data
{
    [Table("accesstbl")] // Maps to your database table
    public class Accesstbl
    {

        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("ipaddress")]
        public string ipx { get; set; } = string.Empty;

        [Column("StoreCode")]
        public string storez { get; set; } = string.Empty;

        [Column("StoreDescription")]
        public string sdescription { get; set; } = string.Empty;

     
    }
}