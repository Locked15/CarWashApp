//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CarWashApp.Entities
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    
    public partial class Order
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Order()
        {
            this.Service = new HashSet<Service>();
        }
    
        public int ID { get; set; }
        public int EmployeID { get; set; }
        public string CarStateNumber { get; set; }
        public string Address { get; set; }
        public System.DateTime OrderDateTime { get; set; }
    
        public virtual Client Client { get; set; }
        public virtual Employe Employe { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Service> Service { get; set; }


        public Decimal FinalPrice_Bind
        {
            get
            {
                return Service.ToList().Sum(s => s.Price);
            }
        }

        public Decimal TaxFromPrice
        {
            get
            {
                return FinalPrice_Bind * 0.13M;
            }
        }
    }
}
