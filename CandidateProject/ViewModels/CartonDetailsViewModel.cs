using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CandidateProject.ViewModels
{
    public class CartonDetailsViewModel
    {
        public int CartonId { get; set; }
        public string CartonNumber { get; set; }
        public IEnumerable<EquipmentViewModel> Equipment { get; set; }
        public CartonDetailsViewModel()
        {
            Equipment = new List<EquipmentViewModel>();
        }
    }
}