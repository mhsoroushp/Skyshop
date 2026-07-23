import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface PaymentStatusUpdate {
  orderId: string;
  paymentStatus: string;
  orderStatus: string;
  message: string;
  updatedAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class PaymentStatusRealtimeService {
  private hubConnection?: HubConnection;
  private readonly statusUpdatesSubject = new Subject<PaymentStatusUpdate>();
  readonly statusUpdates$ = this.statusUpdatesSubject.asObservable();

  private readonly apiBaseUrl = environment.apiBaseUrl;
  private readonly hubPaymentStatusUrl = environment.hubPamentStatusUrl;

  async connect(): Promise<void> {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      return;
    }

    if (!this.hubConnection) {
      this.hubConnection = new HubConnectionBuilder()
        .withUrl(this.hubPaymentStatusUrl)
        .withAutomaticReconnect()
        .configureLogging(LogLevel.Information)
        .build();

      this.hubConnection.onclose((err) => console.log('[SignalR] Connection closed', err));
      this.hubConnection.onreconnecting((err) => console.log('[SignalR] Reconnecting...', err));
      this.hubConnection.onreconnected((id) => console.log('[SignalR] Reconnected, connectionId:', id));

      this.hubConnection.on('PaymentStatusUpdated', (update: PaymentStatusUpdate) => {
        this.statusUpdatesSubject.next(update);
      });
    }

    if (this.hubConnection.state === HubConnectionState.Disconnected) {
      await this.hubConnection.start();
      console.log('[SignalR] Connected, state:', this.hubConnection.state);
    }
  }

  async joinOrderGroup(orderId: string): Promise<void> {
    await this.connect();
    await this.hubConnection?.invoke('JoinOrderGroup', orderId);
    // console.log('[SignalR] Joined order group:', orderId);
  }

  async leaveOrderGroup(orderId: string): Promise<void> {
    if (!this.hubConnection || this.hubConnection.state !== HubConnectionState.Connected) {
      return;
    }

    await this.hubConnection.invoke('LeaveOrderGroup', orderId);
  }

  async disconnect(): Promise<void> {
    if (!this.hubConnection) {
      return;
    }

    if (this.hubConnection.state !== HubConnectionState.Disconnected) {
      await this.hubConnection.stop();
    }
  }
}