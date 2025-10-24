export interface NUICallback {
  action?: string;
  id?: number;
}

declare global {
  interface Window {
    GetParentResourceName?: () => string;
  }
}

export interface VehicleColor {
  ID: string;
  Description: string;
  Hex: string;
  RGB: string;
}

export interface VehicleWheel {
  WheelType: number;
  Wheel: string;
  vID: number;
}

export interface WheelTypeInfo {
  id: number;
  name: string;
  wheels: VehicleWheel[];
}