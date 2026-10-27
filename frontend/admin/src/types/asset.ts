export type AssetType = 'Desktop' | 'Laptop' | 'Server' | 'Printer' | 'NetworkDevice' | 'Other';
export type AssetStatus = 'Available' | 'Assigned' | 'UnderMaintenance' | 'Retired' | 'Disabled';
export interface CreateAssetRequest { assetCode: string; name: string; type: AssetType; assignedUserId: string | null }
export interface AssetResponse {
  id: string; assetCode: string; name: string; type: AssetType; status: AssetStatus;
  assignedUserId: string | null; assignedUserName: string | null; createdAt: string; updatedAt: string | null;
}
