using Cameras.Domain;

namespace Cameras.Application.Abstractions;

/// <summary>
/// Supplies cameras to the application layer.
/// </summary>
public interface ICameraRepository
{
    IReadOnlyList<Camera> GetAll();
}
