import { Component, computed, inject, signal } from '@angular/core';
import { httpResource } from '@angular/common/http';
import { District, Listing, ListingFilter as FilterValue, Page } from '../../models/listing';
import { ListingApi } from '../../services/listing-api';
import { ErrorMessagePipe } from '../../pipes/error-message.pipe';
import { ListingFilter } from '../../listing-filter/listing-filter';
import { ListingList } from '../../listing-list/listing-list';

@Component({
  selector: 'app-listings-page',
  imports: [ListingFilter, ListingList, ErrorMessagePipe],
  templateUrl: './listings-page.html',
  styleUrl: './listings-page.css',
})
export class ListingsPage {
  private api = inject(ListingApi);

  filter = signal<FilterValue>({ districtId: null, maxPrice: null });
  page = signal(1);
  sort = signal('new');
  pageSize = signal(6);
  readonly pageSizes = [6, 12, 24];

  // изменился filter, page, sort или pageSize — новый запрос, старый отменяется
  listings = httpResource<Page<Listing>>(() => ({
    url: this.api.listingsUrl,
    params: {
      ...this.api.toParams(this.filter()),
      page: this.page(),
      pageSize: this.pageSize(),
      sort: this.sort(),
    },
  }));

  districts = httpResource<District[]>(() => this.api.districtsUrl, { defaultValue: [] });

  // сколько всего страниц: округляем вверх
  totalPages = computed(() => {
    const p = this.listings.value();
    return p ? Math.max(1, Math.ceil(p.total / p.pageSize)) : 1;
  });

  onFilter(value: FilterValue) {
    this.filter.set(value);
    this.page.set(1);          // новый фильтр — с первой страницы
  }

  onSort(event: Event) {
    this.sort.set((event.target as HTMLSelectElement).value);
    this.page.set(1);
  }

  onPageSize(event: Event) {
    this.pageSize.set(Number((event.target as HTMLSelectElement).value));
    this.page.set(1);
  }

  refresh() {
    this.listings.reload();
    this.districts.reload();
  }
}
