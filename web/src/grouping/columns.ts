import type { Camera } from '../types/camera';

/**
 * One column of the homepage table.
 */
export interface ColumnDefinition {
  id: string;
  label: string;
  /** Inclusive lower bound on the camera number. */
  min: number;
  /** Exclusive upper bound on the camera number; `null` means no upper bound. */
  max: number | null;
}

/**
 * The column layout from the assignment.
 *
 * Two deliberate decisions are encoded here:
 *
 * 1. The assignment asks for "five columns" and then lists four rules. Four columns are
 *    built, matching the rules. Adding a fifth is a matter of adding an entry here.
 *
 * 2. The rules overlap at the boundaries — "up to 600" and "600 to 700" both read as
 *    including 600. Each `max` is treated as exclusive, so a camera numbered 600 would
 *    land in the second column. No camera in the supplied dataset is numbered 600, 700
 *    or 800, so this choice does not change how the given data is displayed.
 */
export const COLUMN_DEFINITIONS: readonly ColumnDefinition[] = [
  { id: 'below-600', label: 'Up to 600', min: Number.NEGATIVE_INFINITY, max: 600 },
  { id: '600-700', label: '600 – 700', min: 600, max: 700 },
  { id: '700-800', label: '700 – 800', min: 700, max: 800 },
  { id: '800-plus', label: '800 and above', min: 800, max: null },
];

export interface CameraColumnData {
  definition: ColumnDefinition;
  cameras: Camera[];
}

/**
 * Distributes cameras across the columns by camera number.
 *
 * Every column is returned, including empty ones. The 600–700 column has no cameras in
 * the supplied dataset, and silently omitting it would misrepresent the data as having
 * three groups.
 */
export function groupCamerasIntoColumns(
  cameras: readonly Camera[],
  definitions: readonly ColumnDefinition[] = COLUMN_DEFINITIONS,
): CameraColumnData[] {
  return definitions.map((definition) => ({
    definition,
    cameras: cameras.filter((camera) => belongsInColumn(camera.number, definition)),
  }));
}

function belongsInColumn(cameraNumber: number, definition: ColumnDefinition): boolean {
  const { min, max } = definition;
  return cameraNumber >= min && (max === null || cameraNumber < max);
}
