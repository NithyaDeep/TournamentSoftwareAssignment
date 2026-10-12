import { useCallback, useState } from 'react';
import { CameraMap } from './components/CameraMap';
import { CameraTable } from './components/CameraTable';
import { SearchBar } from './components/SearchBar';
import { useCameras } from './hooks/useCameras';

export default function App() {
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedKey, setSelectedKey] = useState<string | null>(null);

  const { cameras, isLoading, error } = useCameras(searchTerm);

  // Selecting an already-selected camera clears the selection, so the map and table
  // return to showing everything equally.
  const handleSelect = useCallback((key: string) => {
    setSelectedKey((current) => (current === key ? null : key));
  }, []);

  return (
    <div className="app">
      <header className="app-header">
        <div className="app-identity">
          <h1 className="app-title">Utrecht traffic cameras</h1>
          <p className="app-subtitle">
            Click a marker or a row to link the two.
          </p>
        </div>

        <SearchBar
          value={searchTerm}
          onChange={setSearchTerm}
          resultCount={cameras.length}
          isLoading={isLoading}
        />
      </header>

      <main className="app-main">
        {error !== null ? (
          <div className="notice notice-error" role="alert">
            <p className="notice-title">The camera data could not be loaded.</p>
            <p>{error}</p>
            <p>
              Start the API with <code>dotnet run --project src/Cameras.Api</code>, then
              reload this page.
            </p>
          </div>
        ) : (
          <>
            <section aria-label="Camera locations" className="map-section">
              <CameraMap
                cameras={cameras}
                selectedKey={selectedKey}
                onSelect={handleSelect}
              />
            </section>

            {!isLoading && cameras.length === 0 ? (
              <div className="notice" role="status">
                <p className="notice-title">No cameras match that search.</p>
                <p>Try part of a street or square, for example “Neude” or “Croeselaan”.</p>
              </div>
            ) : (
              <CameraTable
                cameras={cameras}
                selectedKey={selectedKey}
                onSelect={handleSelect}
              />
            )}
          </>
        )}
      </main>
    </div>
  );
}
