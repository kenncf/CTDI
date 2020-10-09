namespace CandidateProject
{
    using EntityModels;
    using System.Data.Entity;

    public partial class CartonContext : DbContext
    {
        public CartonContext()
            : base("name=CartonContext")
        {
        }

        public virtual DbSet<Carton> Cartons { get; set; }
        public virtual DbSet<CartonDetail> CartonDetails { get; set; }
        public virtual DbSet<Equipment> Equipments { get; set; }
        public virtual DbSet<ModelType> ModelTypes { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Carton>()
                .HasMany(e => e.CartonDetails)
                .WithRequired(e => e.Carton)
                .WillCascadeOnDelete(true);//KF: change WillCascadeOnDelete to True
                  
            modelBuilder.Entity<Equipment>()
                .HasMany(e => e.CartonDetails)
                .WithRequired(e => e.Equipment)
                .WillCascadeOnDelete(false);
            
            modelBuilder.Entity<ModelType>()
                .HasMany(e => e.Equipments)
                .WithRequired(e => e.ModelType)
                .WillCascadeOnDelete(false);
        }
    }
}
