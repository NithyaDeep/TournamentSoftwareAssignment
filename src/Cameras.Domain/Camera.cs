namespace Cameras.Domain;

/// <summary>
/// A traffic camera.
/// </summary>
/// <param name="Number">
/// The numeric part of the camera's identifier, for example 501 for "UTR-CM-501 Neude plein".
/// Not guaranteed to be unique: the source data contains two cameras numbered 545.
/// </param>
/// <param name="Name">The full camera name exactly as published, including the identifier prefix.</param>
/// <param name="Latitude">Latitude in decimal degrees (WGS 84).</param>
/// <param name="Longitude">Longitude in decimal degrees (WGS 84).</param>
public sealed record Camera(int Number, string Name, double Latitude, double Longitude);
