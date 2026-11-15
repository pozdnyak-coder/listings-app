import { Component, inject } from '@angular/core';
import { httpResource } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { Listing } from '../../models/listing';
import { ListingApi } from '../../services/listing-api';
import { ErrorMessagePipe } from '../../pipes/error-message.pipe';
import { ListingList } from '../../listing-list/listing-list';

@Component({
  selector: 'app-my-listings-page',
  imports: [ListingList, ErrorMessagePipe, RouterLink],
  templateUrl: './my-listings-page.html',
})
export class MyListingsPage {
  private api = inject(ListingApi);
  listings = httpResource<Listing[]>(() => this.api.myListingsUrl);   // токен добавит интерсептор
}
