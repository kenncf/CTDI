namespace CandidateProject.EntityModels
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("CartonDetail")]
    public partial class CartonDetail
    {
        public int Id { get; set; }

        public int CartonId { get; set; }

        public int EquipmentId { get; set; }

        public Carton Carton { get; set; }

        public Equipment Equipment { get; set; }
    }
}
