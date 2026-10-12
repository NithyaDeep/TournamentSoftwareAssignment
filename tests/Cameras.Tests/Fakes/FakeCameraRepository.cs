using Cameras.Application.Abstractions;
using Cameras.Domain;

namespace Cameras.Tests.Fakes;

/// <summary>
/// An in-memory repository used to test the application layer without touching a file.
/// </summary>
/// <remarks>
/// This class is the practical proof that the data source is genuinely swappable: the
/// search tests exercise the real service against a completely different implementation
/// of <see cref="ICameraRepository"/>. Replacing the CSV with a database would be the
/// same exercise.
/// </remarks>
internal sealed class FakeCameraRepository : ICameraRepository
{
    private readonly IReadOnlyList<Camera> _cameras;

    public FakeCameraRepository(params Camera[] cameras) => _cameras = cameras;

    public IReadOnlyList<Camera> GetAll() => _cameras;
}
