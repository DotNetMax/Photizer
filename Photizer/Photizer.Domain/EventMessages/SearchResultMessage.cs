using Photizer.Domain.Entities;
using System.Collections.Generic;

namespace Photizer.Domain.EventMessages
{
    public class SearchResultMessage
    {
        public List<Picture> Pictures { get; set; }
    }
}