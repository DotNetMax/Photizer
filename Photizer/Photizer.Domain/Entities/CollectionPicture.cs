namespace Photizer.Domain.Entities
{
    public class CollectionPicture
    {
        public int CollectionId { get; set; }
        public int PictureId { get; set; }
        public Collection Collection { get; set; }
        public Picture Picture { get; set; }
    }
}