using Photizer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Photizer.Domain.Interfaces
{
    public interface IPictureSearcher
    {
        Task<IEnumerable<Picture>> SearchAsync(List<Tag> tags
            , List<Person> people
            , Category category
            , Camera camera
            , Lense lense
            , Location location
            , int rating
            , string ratingParameter
            , DateTime createdFrom
            , DateTime createdTo
            , string title
            , CancellationToken cancellationToken);
    }
}