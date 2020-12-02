using Photizer.Domain.Entities;

namespace Photizer.Domain.EventMessages
{
    public class EditCameraMessage
    {
        public Camera EditedCamera { get; set; }
    }
}