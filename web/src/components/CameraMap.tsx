import L from 'leaflet';
import 'leaflet/dist/leaflet.css';
import markerIconRetinaUrl from 'leaflet/dist/images/marker-icon-2x.png';
import markerIconUrl from 'leaflet/dist/images/marker-icon.png';
import markerShadowUrl from 'leaflet/dist/images/marker-shadow.png';
import { useEffect } from 'react';
import { MapContainer, Marker, Popup, TileLayer, useMap } from 'react-leaflet';
import { cameraKey, type Camera } from '../types/camera';

// Leaflet resolves its default marker images from a relative path that does not survive
// bundling. Pointing it at the imported assets is what stops the markers rendering as
// broken images in a Vite build.
L.Icon.Default.mergeOptions({
  iconRetinaUrl: markerIconRetinaUrl,
  iconUrl: markerIconUrl,
  shadowUrl: markerShadowUrl,
});

/** Centre of Utrecht, as suggested by the assignment. */
const UTRECHT_CENTRE: L.LatLngTuple = [52.0914, 5.1115];
const DEFAULT_ZOOM = 13;

interface CameraMapProps {
  cameras: Camera[];
  selectedKey: string | null;
  onSelect: (key: string) => void;
}

export function CameraMap({ cameras, selectedKey, onSelect }: CameraMapProps) {
  return (
    <MapContainer
      className="map"
      center={UTRECHT_CENTRE}
      zoom={DEFAULT_ZOOM}
      scrollWheelZoom
    >
      <TileLayer
        attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
        url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
        maxZoom={19}
      />

      <FitToCameras cameras={cameras} />
      <PanToSelected cameras={cameras} selectedKey={selectedKey} />

      {cameras.map((camera) => {
        const key = cameraKey(camera);
        const isSelected = key === selectedKey;

        return (
          <Marker
            key={key}
            position={[camera.latitude, camera.longitude]}
            zIndexOffset={isSelected ? 1000 : 0}
            opacity={selectedKey === null || isSelected ? 1 : 0.5}
            eventHandlers={{ click: () => onSelect(key) }}
          >
            <Popup>
              <strong>{camera.number}</strong>
              <br />
              {camera.name}
              <br />
              <span className="popup-coords">
                {camera.latitude.toFixed(6)}, {camera.longitude.toFixed(6)}
              </span>
            </Popup>
          </Marker>
        );
      })}
    </MapContainer>
  );
}

/**
 * Keeps every visible camera in frame. Without this, searching for a camera outside the
 * default viewport would appear to return nothing on the map.
 */
function FitToCameras({ cameras }: { cameras: Camera[] }) {
  const map = useMap();

  useEffect(() => {
    if (cameras.length === 0) {
      return;
    }

    const bounds = L.latLngBounds(
      cameras.map((camera) => [camera.latitude, camera.longitude] as L.LatLngTuple),
    );

    map.fitBounds(bounds, { padding: [32, 32], maxZoom: 16 });
  }, [cameras, map]);

  return null;
}

/** Brings the selected camera into view when a table row is clicked. */
function PanToSelected({
  cameras,
  selectedKey,
}: {
  cameras: Camera[];
  selectedKey: string | null;
}) {
  const map = useMap();

  useEffect(() => {
    if (selectedKey === null) {
      return;
    }

    const selected = cameras.find((camera) => cameraKey(camera) === selectedKey);
    if (selected === undefined) {
      return;
    }

    map.panTo([selected.latitude, selected.longitude], { animate: true });
  }, [cameras, selectedKey, map]);

  return null;
}
