namespace Photizer.Domain.Models
{
    public class AddPictureSettings
    {
        public bool KeepCategory { get; set; }
        public bool KeepTags { get; set; }
        public bool KeepPeople { get; set; }
        public bool KeepLocation { get; set; }
    }
}