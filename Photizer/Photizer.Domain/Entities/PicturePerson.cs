namespace Photizer.Domain.Entities
{
    public class PicturePerson
    {
        public int PictureId { get; set; }
        public int PersonId { get; set; }
        public Picture Picture { get; set; }
        public Person Person { get; set; }
    }
}