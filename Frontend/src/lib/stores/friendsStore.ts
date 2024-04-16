import type { FriendPreview } from '$lib/handlers/userHandler';
import { writable } from 'svelte/store';

// friendsStore.ts




export const friendsStore = writable<FriendPreview[]>([]);