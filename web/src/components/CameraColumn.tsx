import type { CameraColumnData } from '../grouping/columns';
import { cameraKey, readableName } from '../types/camera';

interface CameraColumnProps {
  column: CameraColumnData;
  selectedKey: string | null;
  onSelect: (key: string) => void;
}

export function CameraColumn({ column, selectedKey, onSelect }: CameraColumnProps) {
  const { definition, cameras } = column;
  const headingId = `column-${definition.id}`;

  return (
    <section className="column" aria-labelledby={headingId}>
      <header className="column-header">
        <h2 className="column-title" id={headingId}>
          {definition.label}
        </h2>
        <span className="column-count">{cameras.length}</span>
      </header>

      {cameras.length === 0 ? (
        // The 600–700 column is empty for the supplied dataset. Saying so is more
        // honest than rendering a blank panel.
        <p className="column-empty">No cameras in this range.</p>
      ) : (
        <ul className="camera-list">
          {cameras.map((camera) => {
            const key = cameraKey(camera);
            const isSelected = key === selectedKey;

            return (
              <li key={key}>
                <button
                  type="button"
                  className={`camera${isSelected ? ' is-selected' : ''}`}
                  onClick={() => onSelect(key)}
                  aria-pressed={isSelected}
                  title={camera.name}
                  ref={isSelected ? revealElement : undefined}
                >
                  <span className="camera-number">{camera.number}</span>
                  <span className="camera-name">{readableName(camera)}</span>
                  <span className="camera-coords">
                    {camera.latitude.toFixed(6)}, {camera.longitude.toFixed(6)}
                  </span>
                </button>
              </li>
            );
          })}
        </ul>
      )}
    </section>
  );
}

/** Scrolls a newly selected row into view, for selections made by clicking the map. */
function revealElement(element: HTMLButtonElement | null): void {
  element?.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
}
