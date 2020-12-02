using Photizer.Domain.Entities;

namespace Photizer.Domain.EventMessages
{
    public class OpenDetailPagePictureMessage
    {
        public Picture Picture { get; set; }
        public bool CameFromCollection { get; set; }
        public Collection Collection { get; set; }
    }
}