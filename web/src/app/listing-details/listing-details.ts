import { Component, computed, input } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { Listing } from '../models/listing';
import { PhotoSrcPipe } from '../pipes/photo-src.pipe';

@Component({
  selector: 'app-listing-details',
  imports: [DatePipe, DecimalPipe, PhotoSrcPipe],
  templateUrl: './listing-details.html',
  styleUrl: './listing-details.css',
})
export class ListingDetails {
  listing = input.required<Listing>();
  perRoom = computed(() => Math.round(this.listing().price / this.listing().rooms));
}
