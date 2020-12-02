using Photizer.Domain.Entities;
using Photizer.Domain.Interfaces;
using Photizer.Infrastructure.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Photizer.Infrastructure.Services
{
    public class PictureSearcher : IPictureSearcher
    {
        private readonly IPhotizerUnitOfWork _photizerUnitOfWork;

        private readonly IPhotizerLogger _logger;

        private ConcurrentDictionary<string, List<Picture>> _searchResults;

        public PictureSearcher(IPhotizerUnitOfWork photizerUnitOfWork, IPhotizerLogger logger)
        {
            _photizerUnitOfWork = photizerUnitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<Picture>> SearchAsync(List<Tag> tags
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
            , CancellationToken cancellationToken)
        {
            try
            {
                _searchResults = new ConcurrentDictionary<string, List<Picture>>();
                if (tags.Count > 0)
                {
                    await SearchTags(tags, cancellationToken).ConfigureAwait(false);
                }
                if (category != null)
                {
                    await SearchCategories(category, cancellationToken).ConfigureAwait(false);
                }
                if (people.Count > 0)
                {
                    await SearchPeople(people, cancellationToken).ConfigureAwait(false);
                }
                if (camera != null)
                {
                    await SearchCameras(camera, cancellationToken).ConfigureAwait(false);
                }
                if (lense != null)
                {
                    await SearchLenses(lense, cancellationToken).ConfigureAwait(false);
                }
                if (location != null)
                {
                    await SearchLocations(location, cancellationToken).ConfigureAwait(false);
                }
                if (rating != 0 && ratingParameter != null)
                {
                    await SearchRating(rating, ratingParameter, cancellationToken).ConfigureAwait(false);
                }
                if (createdFrom != null && createdTo != null)
                {
                    await SearchDateTimeRange(createdFrom, createdTo, cancellationToken).ConfigureAwait(false);
                }
                if (!string.IsNullOrEmpty(title))
                {
                    await SearchTitle(title, cancellationToken).ConfigureAwait(false);
                }
                return ProcessSearchResults(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Search was canceled");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error searching for Pictures", ex);
                return null;
            }
        }

        private IEnumerable<Picture> ProcessSearchResults(CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                token.ThrowIfCancellationRequested();
            }

            try
            {
                var duplicateCounter = _searchResults.Keys.Count;
                if (duplicateCounter > 1)
                {
                    List<Picture> totalResults = new List<Picture>();
                    foreach (var result in _searchResults)
                    {
                        totalResults.AddRange(result.Value);
                    }

                    var group = from picture in totalResults group picture by picture.Id into pictureGroup select new { pictureGroup.Key, Count = pictureGroup.Count() };
                    var duplicateKeys = group.Where(g => g.Count >= duplicateCounter).Select(g => g.Key).ToList();
                    List<Picture> duplicates = totalResults.Where(r => duplicateKeys.Contains(r.Id)).Select(p => p).ToList();
                    duplicates = duplicates.Distinct(new DistinctItemComparer()).ToList();

                    return duplicates;
                }
                else
                {
                    return _searchResults.Values.FirstOrDefault();
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Search was canceled");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating the search result", ex);
                return null;
            }
        }

        private async Task SearchTags(List<Tag> tags, CancellationToken token)
        {
            try
            {
                List<Picture> tagResults = new List<Picture>();
                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }
                var result = await _photizerUnitOfWork.PictureRepository.GetAllByTags(tags).ConfigureAwait(false);
                if (result != null && result.ToList().Count > 0)
                {
                    tagResults.AddRange(result);
                }

                _searchResults.TryAdd("TagResults", tagResults);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Search was canceled");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error seaching in Tags", ex);
            }
        }

        private async Task SearchCategories(Category category, CancellationToken token)
        {
            try
            {
                List<Picture> categoryResults = new List<Picture>();

                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }
                var result = await _photizerUnitOfWork.PictureRepository.GetAllByCategory(category).ConfigureAwait(false);
                if (result != null && result.ToList().Count > 0)
                {
                    categoryResults.AddRange(result);
                }

                _searchResults.TryAdd("CategoryResults", categoryResults);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Search was canceled");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error searching in Category", ex);
            }
        }

        private async Task SearchLocations(Location location, CancellationToken token)
        {
            try
            {
                List<Picture> locationResults = new List<Picture>();

                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }
                var result = await _photizerUnitOfWork.PictureRepository.GetAllByLocation(location).ConfigureAwait(false);
                if (result != null && result.ToList().Count > 0)
                {
                    locationResults.AddRange(result);
                }

                _searchResults.TryAdd("LocationResults", locationResults);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Search was canceled");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error searching in Location", ex);
            }
        }

        private async Task SearchCameras(Camera camera, CancellationToken token)
        {
            try
            {
                List<Picture> cameraResults = new List<Picture>();

                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }
                var result = await _photizerUnitOfWork.PictureRepository.GetAllByCamera(camera).ConfigureAwait(false);
                if (result != null && result.ToList().Count > 0)
                {
                    cameraResults.AddRange(result);
                }

                _searchResults.TryAdd("CameraResults", cameraResults);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Search was canceled");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error searching in Camera", ex);
            }
        }

        private async Task SearchLenses(Lense lense, CancellationToken token)
        {
            try
            {
                List<Picture> lenseResults = new List<Picture>();

                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }
                var result = await _photizerUnitOfWork.PictureRepository.GetAllByLense(lense).ConfigureAwait(false);
                if (result != null && result.ToList().Count > 0)
                {
                    lenseResults.AddRange(result);
                }

                _searchResults.TryAdd("LenseResults", lenseResults);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Search was canceled");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error searching in Lense", ex);
            }
        }

        private async Task SearchPeople(List<Person> people, CancellationToken token)
        {
            try
            {
                List<Picture> personResults = new List<Picture>();
                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }
                var result = await _photizerUnitOfWork.PictureRepository.GetAllByPeople(people).ConfigureAwait(false);
                if (result != null && result.ToList().Count > 0)
                {
                    personResults.AddRange(result);
                }
                _searchResults.TryAdd("PersonResults", personResults);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Search was canceled");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error searching in People", ex);
            }
        }

        private async Task SearchRating(int rating, string ratingParameter, CancellationToken token)
        {
            try
            {
                List<Picture> ratingResults = new List<Picture>();
                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }
                var result = await _photizerUnitOfWork.PictureRepository.GetByRating(rating, ratingParameter).ConfigureAwait(false);
                if (result != null && result.ToList().Count > 0)
                {
                    ratingResults.AddRange(result);
                }
                _searchResults.TryAdd("RatingResults", ratingResults);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Search was canceled");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error searching in Rating", ex);
            }
        }

        private async Task SearchDateTimeRange(DateTime from, DateTime to, CancellationToken token)
        {
            try
            {
                List<Picture> dateTimeRangeResult = new List<Picture>();

                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }
                var result = await _photizerUnitOfWork.PictureRepository.GetInDateTimeRange(from, to).ConfigureAwait(false);
                if (result != null && result.ToList().Count > 0)
                {
                    dateTimeRangeResult.AddRange(result);
                }

                _searchResults.TryAdd("DateTimeRangeResults", dateTimeRangeResult);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Search was canceled");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error searching in DateTimeRange", ex);
            }
        }

        private async Task SearchTitle(string title, CancellationToken token)
        {
            try
            {
                List<Picture> titleResult = new List<Picture>();

                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }
                var result = await _photizerUnitOfWork.PictureRepository.GetByTitle(title).ConfigureAwait(false);
                if (result != null && result.ToList().Count > 0)
                {
                    titleResult.AddRange(result);
                }

                _searchResults.TryAdd("TitleResult", titleResult);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Search was canceled");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error searching in Title", ex);
            }
        }
    }

    public class DistinctItemComparer : IEqualityComparer<Picture>
    {
        public bool Equals(Picture x, Picture y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(Picture obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}