import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { ListingFilter, ListingRequest, Listing } from '../models/listing';

@Injectable({ providedIn: 'root' })
export class ListingApi {
  private http = inject(HttpClient);
  private base = environment.apiUrl;

  // адреса — в одном месте; ими же пользуется httpResource на страницах
  listingsUrl = `${this.base}/api/listings`;
  myListingsUrl = `${this.base}/api/listings/mine`;
  districtsUrl = `${this.base}/api/districts`;
  listingUrl = (id: number | string) => `${this.base}/api/listings/${id}`;

  // превращает фильтр в query-параметры без null
  toParams(filter: ListingFilter): Record<string, string | number> {
    const params: Record<string, string | number> = {};
    if (filter.districtId !== null) params['districtId'] = filter.districtId;
    if (filter.maxPrice !== null) params['maxPrice'] = filter.maxPrice;
    return params;
  }

  create(r: ListingRequest) { return this.http.post<Listing>(this.listingsUrl, r); }
  update(id: number, r: ListingRequest) { return this.http.put<void>(this.listingUrl(id), r); }
  delete(id: number) { return this.http.delete<void>(this.listingUrl(id)); }

  uploadPhoto(id: number, file: File) {
    const form = new FormData();
    form.append('file', file);            // 'file' = имя параметра IFormFile file на сервере
    return this.http.post<{ photoUrl: string }>(`${this.listingUrl(id)}/photo`, form);
  }
}
