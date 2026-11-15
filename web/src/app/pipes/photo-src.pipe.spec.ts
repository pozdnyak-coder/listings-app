import { PhotoSrcPipe } from './photo-src.pipe';
import { environment } from '../../environments/environment';

describe('PhotoSrcPipe', () => {
  it('добавляет адрес API к пути файла', () => {
    const pipe = new PhotoSrcPipe();
    expect(pipe.transform('/uploads/a.jpg')).toBe(`${environment.apiUrl}/uploads/a.jpg`);
  });
});
