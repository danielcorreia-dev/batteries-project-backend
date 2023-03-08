using System;

namespace Domain.Entities
{
    public class BaseEntity : IEntity
    {
        public int Id { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
