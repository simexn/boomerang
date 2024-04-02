import type { HubConnection } from '@microsoft/signalr';
import { writable } from 'svelte/store';

export const connectionStore = writable<HubConnection | null>(null);