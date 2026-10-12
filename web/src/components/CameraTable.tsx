import { groupCamerasIntoColumns } from '../grouping/columns';
import type { Camera } from '../types/camera';
import { CameraColumn } from './CameraColumn';

interface CameraTableProps {
  cameras: Camera[];
  selectedKey: string | null;
  onSelect: (key: string) => void;
}

export function CameraTable({ cameras, selectedKey, onSelect }: CameraTableProps) {
  const columns = groupCamerasIntoColumns(cameras);

  return (
    <div className="columns">
      {columns.map((column) => (
        <CameraColumn
          key={column.definition.id}
          column={column}
          selectedKey={selectedKey}
          onSelect={onSelect}
        />
      ))}
    </div>
  );
}
