/** A camera as returned by GET /api/cameras. */
export interface Camera {
  number: number;
  name: string;
  latitude: number;
  longitude: number;
}

/**
 * A stable, unique identifier for a camera.
 *
 * The camera number cannot be used on its own: two different cameras in the dataset are
 * both numbered 545, so using the number as a React key or a selection id would make the
 * two indistinguishable. The name disambiguates them.
 */
export function cameraKey(camera: Camera): string {
  return `${camera.number}|${camera.name}`;
}

/**
 * The camera name without its "UTR-CM-501" prefix, for display next to the number.
 * Falls back to the full name if the prefix is missing.
 */
export function readableName(camera: Camera): string {
  return camera.name.replace(/^UTR-CM-\d+[\s-]*/i, '').trim() || camera.name;
}
