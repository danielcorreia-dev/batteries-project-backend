using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public interface IEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
