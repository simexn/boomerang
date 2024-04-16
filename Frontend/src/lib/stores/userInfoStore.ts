import { fetchUserInfo, type User } from '$lib/handlers/accountHandler';
import { writable } from 'svelte/store';

// Create a writable store with initial value of null
export const userStore = writable<User>();

// Function to update the store
export async function updateUserInfo() {
    const userInfo: User = await fetchUserInfo();
    userStore.set(userInfo);
}