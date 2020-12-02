using System.Collections.Generic;

namespace Photizer.Domain.Entities
{
    public class Person : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get { return $"{FirstName} {LastName}"; } set {; } }
        public List<PicturePerson> PicturePeople { get; set; }
    }
}