import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from '../layout/header/header.component';
import { FooterComponent } from '../layout/footer/footer.component';
import { LoadingComponent } from './shared/components/loading/loading.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, HeaderComponent, FooterComponent, LoadingComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected title = signal('client Ha');

  constructor(){
    this.title.set('test');
  }

}
