using Cameras.Application.Abstractions;
using Cameras.Domain;

namespace Cameras.Application.Services;

/// <summary>
/// Finds cameras by part of their name.
/// </summary>
/// <remarks>
/// This is the single implementation of the search rule. Both entry points use it.
/// </remarks>
public sealed class CameraSearchService
{
    private readonly ICameraRepository _repository;

    public CameraSearchService(ICameraRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// Returns the cameras whose name contains <paramref name="nameFragment"/>, ignoring case.
    /// A null, empty or whitespace-only fragment matches every camera.
    /// </summary>
    public IReadOnlyList<Camera> Search(string? nameFragment)
    {
        IEnumerable<Camera> cameras = _repository.GetAll();

        if (!string.IsNullOrWhiteSpace(nameFragment))
        {
            var needle = nameFragment.Trim();
            cameras = cameras.Where(camera =>
                camera.Name.Contains(needle, StringComparison.OrdinalIgnoreCase));
        }

        return cameras
            .OrderBy(camera => camera.Number)
            .ThenBy(camera => camera.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
