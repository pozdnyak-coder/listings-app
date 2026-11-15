// Точная копия DTO сервера (camelCase). Даты приходят строкой ISO, не Date.
export interface Listing {
  id: number;
  title: string;
  price: number;
  rooms: number;
  districtId: number;
  districtName: string;
  address?: string | null;
  createdAt: string;
  ownerId: number | null;
  photoUrl: string | null;
}

export interface ListingFilter {
  districtId: number | null;   // null = все районы
  maxPrice: number | null;     // null = без ограничения
}

export interface District {
  id: number;
  name: string;
  listingsCount: number;
}

// то, что уходит на сервер в POST и PUT — совпадает с CreateListingRequest
export interface ListingRequest {
  title: string;
  price: number;
  rooms: number;
  districtId: number;
  address: string | null;
}

export interface Page<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
}
