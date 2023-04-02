using System;

namespace Domain.Entities
{
    public abstract class BaseEntity : IEntity
    {
        public int Id { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
    }
}
