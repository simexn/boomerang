import { writable } from "svelte/store";

type UserStatuses = {
    [key: string]: string;
};

export const userStatuses = writable<UserStatuses>({});