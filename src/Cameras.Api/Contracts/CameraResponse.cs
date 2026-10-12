using Cameras.Domain;

namespace Cameras.Api.Contracts;

/// <summary>
/// Camera Response.
/// </summary>
public sealed record CameraResponse(int Number, string Name, double Latitude, double Longitude)
{
    public static CameraResponse From(Camera camera) =>
        new(camera.Number, camera.Name, camera.Latitude, camera.Longitude);
}
