import type { FriendRequest } from '$lib/Handlers/userHandler';
import { writable } from 'svelte/store';

// Initialize the store with an empty array
export const friendRequestsStore = writable<FriendRequest[]>([]);