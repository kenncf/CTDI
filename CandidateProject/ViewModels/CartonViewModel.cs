/*
    KF: added EquipmentCount to be used to store the count of cartondetails 
*/
namespace CandidateProject.ViewModels
{
    public class CartonViewModel
    {
        public int Id { get; set; }
        public string CartonNumber { get; set; }
        public int EquipmentCount { get; set; }
    }
}