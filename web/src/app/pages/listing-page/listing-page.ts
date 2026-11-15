import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { Listing } from '../../models/listing';
import { ListingApi } from '../../services/listing-api';
import { AuthService } from '../../services/auth.service';
import { ErrorMessagePipe, errorMessage } from '../../pipes/error-message.pipe';
import { problemErrors } from '../../utils/problem-details';
import { ListingDetails } from '../../listing-details/listing-details';

@Component({
  selector: 'app-listing-page',
  imports: [ListingDetails, ErrorMessagePipe, RouterLink],
  templateUrl: './listing-page.html',
  styleUrl: './listing-page.css',
})
export class ListingPage {
  private api = inject(ListingApi);
  private router = inject(Router);
  private auth = inject(AuthService);

  id = input.required<string>();     // из маршрута :id — всегда строка
  listing = httpResource<Listing>(() => this.api.listingUrl(this.id()));

  // кнопки изменения — только у автора
  canEdit = computed(() =>
    this.listing.hasValue() &&
    this.auth.userId() !== null &&
    this.listing.value().ownerId === this.auth.userId());

  deleting = signal(false);
  deleteError = signal<string | null>(null);
  uploading = signal(false);
  uploadError = signal<string | null>(null);

  // повторять запрос имеет смысл, кроме случая «объявления нет»
  canRetry(err: unknown): boolean {
    return !(err instanceof HttpErrorResponse && err.status === 404);
  }

  async remove() {
    if (!confirm('Удалить объявление?')) return;
    this.deleting.set(true);
    this.deleteError.set(null);
    try {
      await firstValueFrom(this.api.delete(Number(this.id())));
      this.router.navigate(['/']);
    } catch (e) {
      this.deleteError.set(errorMessage(e));
    } finally {
      this.deleting.set(false);
    }
  }

  async onPhoto(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    if (file.size > 5 * 1024 * 1024) {              // для удобства; защищает сервер
      this.uploadError.set('Файл больше 5 МБ');
      input.value = '';
      return;
    }
    this.uploading.set(true);
    this.uploadError.set(null);
    try {
      await firstValueFrom(this.api.uploadPhoto(Number(this.id()), file));
      this.listing.reload();                         // перечитать — фото появится
    } catch (e) {
      this.uploadError.set(problemErrors(e)?.['file']?.[0] ?? errorMessage(e));
    } finally {
      this.uploading.set(false);
      input.value = '';                              // чтобы тот же файл можно было выбрать снова
    }
  }
}
