using CandidateProject.EntityModels;
using CandidateProject.ViewModels;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace CandidateProject.Controllers
{
    public class CartonController : Controller
    {
        private CartonContext db = new CartonContext();

        //KF: I used the appSettings so that we don't need to recompile the app if we need to change the limit
        private int _CartonLimit = int.Parse(System.Configuration.ConfigurationManager.AppSettings["CartonLimit"]);

        // GET: Carton
        public ActionResult Index()
        {
            var cartons = db.Cartons
                .Select(c => new CartonViewModel()
                {
                    Id = c.Id,
                    CartonNumber = c.CartonNumber,
                    EquipmentCount = c.CartonDetails.Count
                })
                .ToList();
            return View(cartons);
        }

        // GET: Carton/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var carton = db.Cartons
                .Where(c => c.Id == id)
                .Select(c => new CartonViewModel()
                {
                    Id = c.Id,
                    CartonNumber = c.CartonNumber
                })
                .SingleOrDefault();
            if (carton == null)
            {
                return HttpNotFound();
            }
            return View(carton);
        }

        // GET: Carton/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Carton/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,CartonNumber")] Carton carton)
        {
            if (ModelState.IsValid)
            {
                db.Cartons.Add(carton);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(carton);
        }

        // GET: Carton/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var carton = db.Cartons
                .Where(c => c.Id == id)
                .Select(c => new CartonViewModel()
                {
                    Id = c.Id,
                    CartonNumber = c.CartonNumber
                })
                .SingleOrDefault();
            if (carton == null)
            {
                return HttpNotFound();
            }
            return View(carton);
        }

        // POST: Carton/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,CartonNumber")] CartonViewModel cartonViewModel)
        {
            if (ModelState.IsValid)
            {
                var carton = db.Cartons.Find(cartonViewModel.Id);
                carton.CartonNumber = cartonViewModel.CartonNumber;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(cartonViewModel);
        }

        // GET: Carton/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Carton carton = db.Cartons.Find(id);
            if (carton == null)
            {
                return HttpNotFound();
            }
            return View(carton);
        }

        /*
            KF:
            2 - Our customers has reported the following bugs:
            1. We can delete empty cartons from the system, but cannot delete cartons that have items. We
            should be able to delete a carton at any time.
        */
        // POST: Carton/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            //KF: Issue 2.1 - change to include the CartonDetails
            Carton carton = db.Cartons.Include(cd=>cd.CartonDetails).FirstOrDefault(cd=>cd.Id==id);

            if (carton != null)
            {
                //KF: Issue 2.1 - change the WillCascadeOnDelete
                try
                {
                    db.Cartons.Remove(carton);
                    db.SaveChanges();
                }
                catch(System.Exception ex)
                {
                    throw new System.Exception(ex.Message);
                }
            }

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        /*
            KF:
            2 - Our customers has reported the following bugs:
                2. We can add the same piece of equipment to 2 different cartons, this doesn’t make sense. We
                should only be able to add a piece of equipment to 1 carton, once a piece of equipment is on a
                carton it should be unavailable to place on another carton.
        */
        public ActionResult AddEquipment(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var carton = db.Cartons
                .Where(c => c.Id == id)
                .Select(c => new CartonDetailsViewModel()
                {
                    CartonNumber = c.CartonNumber,
                    CartonId = c.Id
                })
                .SingleOrDefault();

            if (carton == null)
            {
                return HttpNotFound();
            }

            //KF: solution to issue 2.2: remove the CartonID from the exclusion
            // another solution is to check if the item exists in CartonDetails prior to adding it in AddEquipmentToCarton
            var equipment = db.Equipments
                .Where(e => !db.CartonDetails.Select(cd => cd.EquipmentId).Contains(e.Id))
                //.Where(e => !db.CartonDetails.Where(cd => cd.CartonId == id).Select(cd => cd.EquipmentId).Contains(e.Id) )
                .Select(e => new EquipmentViewModel()
                {
                    Id = e.Id,
                    ModelType = e.ModelType.TypeName,
                    SerialNumber = e.SerialNumber
                })
                .ToList();

            carton.Equipment = equipment;

            //KF: add a carton equipment status count
            TempData["Status"] = cartonStatus(id);

            return View(carton);
        }

        /*
            2 - Our customers has reported the following bugs:
                3. Our cartons can physically hold up to 10 pieces of equipment, but the application allows us to
                put an unlimited number of equipment on the carton, please only allow up to 10 pieces of
                equipment on the carton.

        */
        public ActionResult AddEquipmentToCarton([Bind(Include = "CartonId,EquipmentId")] AddEquipmentViewModel addEquipmentViewModel)
        {
            if (ModelState.IsValid)
            {
                var carton = db.Cartons
                    .Include(c => c.CartonDetails)
                    .Where(c => c.Id == addEquipmentViewModel.CartonId)
                    .SingleOrDefault();
                if (carton == null)
                {
                    return HttpNotFound();
                }
                var equipment = db.Equipments
                    .Where(e => e.Id == addEquipmentViewModel.EquipmentId)
                    .SingleOrDefault();
                if (equipment == null)
                {
                    return HttpNotFound();
                }
                var detail = new CartonDetail()
                {
                    Carton = carton,
                    Equipment = equipment
                };

                //check the carton detail count - must be lesser than 10
                if (carton.CartonDetails.Count < _CartonLimit)
                {
                    carton.CartonDetails.Add(detail);
                    db.SaveChanges();
                }
            }
            return RedirectToAction("AddEquipment", new { id = addEquipmentViewModel.CartonId });
        }

        public ActionResult ViewCartonEquipment(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var carton = db.Cartons
                .Where(c => c.Id == id)
                .Select(c => new CartonDetailsViewModel()
                {
                    CartonNumber = c.CartonNumber,
                    CartonId = c.Id,
                    Equipment = c.CartonDetails
                        .Select(cd => new EquipmentViewModel()
                        {
                            Id = cd.EquipmentId,
                            ModelType = cd.Equipment.ModelType.TypeName,
                            SerialNumber = cd.Equipment.SerialNumber
                        })
                })
                .SingleOrDefault();
            if (carton == null)
            {
                return HttpNotFound();
            }
            //KF: add a carton equipment status count
            TempData["Status"] = cartonStatus(id);
            return View(carton);
        }

        /*
            Last remaining development item for this iteration:
            1. Implement the RemoveEquipmentOnCarton action on the CartonController, right now it is just
            throwing a BadRequest. 
        */
        public ActionResult RemoveEquipmentOnCarton([Bind(Include = "CartonId,EquipmentId")] RemoveEquipmentViewModel removeEquipmentViewModel)
        {
            //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            if (ModelState.IsValid)
            {
                //KF: implement the remove function
                try
                {
                    var equipment = (from cd in db.CartonDetails
                                     where cd.CartonId == removeEquipmentViewModel.CartonId
                                     && cd.EquipmentId == removeEquipmentViewModel.EquipmentId

                                     select cd).FirstOrDefault();
                    if (equipment != null)
                    {
                        db.CartonDetails.Remove(equipment);
                        db.SaveChanges();
                    }
                }
                catch
                {
                    throw new System.Exception();
                }

            }
            return RedirectToAction("ViewCartonEquipment", new { id = removeEquipmentViewModel.CartonId });
        }

        /*
            KF:
            3. Our customers have asked for the following enhancements:
                2. We need a quick way to remove all the items from the carton with 1 click.
        */
        public ActionResult EmptyCarton(int id)
        {
            emptyCarton(id);
            return RedirectToAction("Index");
        }

        /*
            KF: This can be moved to a repository - but for simplicity, it is implemented here
                This function clears the CartonDetails by CartonID
        */
        private int emptyCarton(int id)
        {
            int equipmentsRemoved = 0;


            if (ModelState.IsValid)
            {
                var cds = (from cd in db.CartonDetails
                           where cd.CartonId == id
                           select cd);
                if (cds != null)
                {
                    try
                    {
                        equipmentsRemoved = db.CartonDetails.RemoveRange(cds).Count();
                        db.SaveChanges();
                    }
                    catch
                    {
                        throw new System.Exception();
                    }
                }
            }

            return equipmentsRemoved;
        }

        /*
            KF: This can be moved to a repository - but for simplicity, it is implemented here
            Let's add a status to the view so that the user is aware how much equipment is left to add 
        */
        private string cartonStatus(int? id)
        {
            if (id == null)
            {
                return "";
            }
            try
            {
                return db.CartonDetails.Where(c => c.CartonId == id).Count().ToString() + " of " + _CartonLimit.ToString() + " equipments in the carton";
            }
            catch
            {
                return "";
            }
        }
    }
}
