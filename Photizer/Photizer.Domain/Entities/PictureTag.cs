namespace Photizer.Domain.Entities
{
    public class PictureTag
    {
        public int PictureId { get; set; }
        public int TagId { get; set; }
        public Picture Picture { get; set; }
        public Tag Tag { get; set; }
    }
}