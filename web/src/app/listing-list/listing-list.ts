import { Component, input } from '@angular/core';
import { Listing } from '../models/listing';
import { ListingCard } from '../listing-card/listing-card';

@Component({
  selector: 'app-listing-list',
  imports: [ListingCard],
  templateUrl: './listing-list.html',
  styleUrl: './listing-list.css',
})
export class ListingList {
  listings = input.required<Listing[]>();
  total = input<number>(0);              // всего подходящих — для счётчика
}
