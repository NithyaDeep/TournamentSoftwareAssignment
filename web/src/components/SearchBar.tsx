interface SearchBarProps {
  value: string;
  onChange: (value: string) => void;
  resultCount: number;
  isLoading: boolean;
}

export function SearchBar({ value, onChange, resultCount, isLoading }: SearchBarProps) {
  const trimmed = value.trim();

  return (
    <div className="search">
      <label className="search-label" htmlFor="camera-search">
        Search by name
      </label>
      <input
        id="camera-search"
        className="search-input"
        type="search"
        placeholder="e.g. Neude"
        value={value}
        onChange={(event) => onChange(event.target.value)}
        autoComplete="off"
      />
      <p className="search-status" role="status">
        {isLoading
          ? 'Loading…'
          : `${resultCount} ${resultCount === 1 ? 'camera' : 'cameras'}${
              trimmed.length > 0 ? ` matching “${trimmed}”` : ''
            }`}
      </p>
    </div>
  );
}
