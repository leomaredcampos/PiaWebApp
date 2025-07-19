using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PiaWebApp.Models
{
    [Table("datagroup1")] // Maps to your database table
    public class Promo
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("promono", TypeName = "BIGINT")] // Exact column name and type
        public long PromoNo { get; set; } // Uses 'long' for BIGINT

        [Column("description")]
        public string Description { get; set; } = string.Empty;

        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("capacity")]
        public string capacity { get; set; } = string.Empty;

        [Column("subcategory")]
        public string subcategory { get; set; } = string.Empty;

        [Column("storecode")]
        public string storecodex { get; set; } = string.Empty;

        [Column("freebies")]
        public string PromoFreebies { get; set; } = string.Empty;

        [Column("Data18")]
        public string Data18 { get; set; } = string.Empty;

        [Column("Data19")]
        public string Data19 { get; set; } = string.Empty;

        [Column("Data20")]
        public string Data20 { get; set; } = string.Empty;

        [Column("Data21")]
        public string Data21 { get; set; } = string.Empty;

        [Column("Data22")]
        public string Data22 { get; set; } = string.Empty;

        [Column("Data23")]
        public string Data23 { get; set; } = string.Empty;

        [Column("Data31")]
        public string Data31 { get; set; } = string.Empty;

        [Column("startdate")]
        public DateTime StartDate { get; set; }

        [Column("enddate")]
        public DateTime EndDate { get; set; }

        [Column("sku")]
        public int Sku { get; set; }

        [Column("srp")]
        public string Srp { get; set; } = string.Empty;
    }
}