import { Pipe, PipeTransform } from '@angular/core';
import { environment } from '../../environments/environment';

// путь /uploads/… → полный адрес на сервере API
@Pipe({ name: 'photoSrc' })
export class PhotoSrcPipe implements PipeTransform {
  transform(url: string): string {
    return `${environment.apiUrl}${url}`;
  }
}
