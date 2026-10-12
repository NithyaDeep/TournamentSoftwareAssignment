import { describe, expect, it } from 'vitest';
import { COLUMN_DEFINITIONS, groupCamerasIntoColumns } from './columns';
import type { Camera } from '../types/camera';

function camera(number: number): Camera {
  return { number, name: `UTR-CM-${number} Test`, latitude: 52.09, longitude: 5.11 };
}

/** Returns the id of the column a camera number lands in. */
function columnIdFor(number: number): string | undefined {
  return groupCamerasIntoColumns([camera(number)]).find((column) => column.cameras.length === 1)
    ?.definition.id;
}

describe('groupCamerasIntoColumns', () => {
  it('returns one entry per column definition', () => {
    const columns = groupCamerasIntoColumns([]);

    expect(columns).toHaveLength(COLUMN_DEFINITIONS.length);
    expect(columns.map((column) => column.definition.id)).toEqual([
      'below-600',
      '600-700',
      '700-800',
      '800-plus',
    ]);
  });

  it('returns empty columns rather than dropping them', () => {
    const columns = groupCamerasIntoColumns([camera(501)]);

    expect(columns[1]?.cameras).toEqual([]);
    expect(columns.every((column) => column.definition !== undefined)).toBe(true);
  });

  // The upper bound of each range is exclusive. These are the cases where an
  // off-by-one would otherwise go unnoticed.
  it.each([
    [1, 'below-600'],
    [501, 'below-600'],
    [599, 'below-600'],
    [600, '600-700'],
    [650, '600-700'],
    [699, '600-700'],
    [700, '700-800'],
    [726, '700-800'],
    [799, '700-800'],
    [800, '800-plus'],
    [803, '800-plus'],
    [9999, '800-plus'],
  ])('puts camera %i in the %s column', (number, expectedColumnId) => {
    expect(columnIdFor(number)).toBe(expectedColumnId);
  });

  it('places every camera in exactly one column', () => {
    const cameras = [1, 599, 600, 699, 700, 799, 800, 1000].map(camera);

    const columns = groupCamerasIntoColumns(cameras);
    const total = columns.reduce((sum, column) => sum + column.cameras.length, 0);

    expect(total).toBe(cameras.length);
  });

  it('preserves the order cameras arrive in', () => {
    const columns = groupCamerasIntoColumns([camera(505), camera(501), camera(503)]);

    expect(columns[0]?.cameras.map((entry) => entry.number)).toEqual([505, 501, 503]);
  });

  it('reproduces the distribution of the supplied dataset', () => {
    // 67 cameras below 600, none between 600 and 700, 18 in the 700s, 3 from 801.
    const cameras = [
      ...range(501, 567),
      ...range(726, 743),
      ...range(801, 803),
    ].map(camera);

    const counts = groupCamerasIntoColumns(cameras).map((column) => column.cameras.length);

    expect(counts).toEqual([67, 0, 18, 3]);
  });

  it('accepts a custom set of definitions', () => {
    const columns = groupCamerasIntoColumns([camera(50), camera(150)], [
      { id: 'low', label: 'Low', min: 0, max: 100 },
      { id: 'high', label: 'High', min: 100, max: null },
    ]);

    expect(columns.map((column) => column.cameras.length)).toEqual([1, 1]);
  });
});

function range(from: number, to: number): number[] {
  return Array.from({ length: to - from + 1 }, (_, index) => from + index);
}
