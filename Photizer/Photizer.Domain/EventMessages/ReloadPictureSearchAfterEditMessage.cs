using Photizer.Domain.Entities;

namespace Photizer.Domain.EventMessages
{
    public class ReloadPictureSearchAfterEditMessage
    {
        public Picture Picture { get; set; }
    }
}