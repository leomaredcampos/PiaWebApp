using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PiaWebApp.Data
{
    [Table("datagroup5")] // Maps sa 'datagroup5' table sa database
    public class PromoDetails
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("promono", TypeName = "BIGINT")]
        public long PromoNo { get; set; }

        [Column("sku")]
        public int Sku { get; set; }

        [Column("monthx")]
        public int monthx { get; set; }

        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("mop")]
        public string Mop { get; set; } = string.Empty;

        // ✅ ADDING DISCOUNT FIELD
        [Column("discount")]
        public double Discount { get; set; }

        // ✅ ADDING EXTENDED FIELD
        [Column("extended")]
        public double Extended { get; set; }
    }
}
