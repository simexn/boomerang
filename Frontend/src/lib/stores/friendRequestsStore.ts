import type { FriendRequest } from '$lib/Handlers/userHandler';
import { writable } from 'svelte/store';

// Initialize the stores with empty arrays
export const sentRequestsStore = writable<FriendRequest[]>([]);
export const receivedRequestsStore = writable<FriendRequest[]>([]);
export const blockedUsersStore = writable<FriendRequest[]>([]);