import type { FriendPreview } from '$lib/Handlers/userHandler';
import { writable } from 'svelte/store';

// friendsStore.ts




export const friendsStore = writable<FriendPreview[]>([]);