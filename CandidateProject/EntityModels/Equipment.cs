namespace CandidateProject.EntityModels
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Equipment")]
    public partial class Equipment
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Equipment()
        {
            CartonDetails = new HashSet<CartonDetail>();
        }

        public int Id { get; set; }

        public int ModelTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string SerialNumber { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public ICollection<CartonDetail> CartonDetails { get; set; }

        public ModelType ModelType { get; set; }
    }
}
