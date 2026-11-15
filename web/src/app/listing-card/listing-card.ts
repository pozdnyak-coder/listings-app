import { Component, computed, input } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Listing } from '../models/listing';
import { PhotoSrcPipe } from '../pipes/photo-src.pipe';

@Component({
  selector: 'app-listing-card',
  imports: [DatePipe, DecimalPipe, RouterLink, PhotoSrcPipe],
  templateUrl: './listing-card.html',
  styleUrl: './listing-card.css',
})
export class ListingCard {
  listing = input.required<Listing>();

  // производные значения — computed от input-сигнала
  isCheap = computed(() => this.listing().price < 40000);
  roomsLabel = computed(() => {
    const r = this.listing().rooms;
    return r === 1 ? 'студия' : `${r} комн.`;
  });
}
