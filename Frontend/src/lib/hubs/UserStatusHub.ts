import { fetchUserId, isLoggedIn } from "$lib/Handlers/accountHandler";
import { getToken } from "$lib/Handlers/authHandler";
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { userStatuses } from "$lib/stores/userStatusesStore";

import { Connect } from "vite";

const backendUrl = import.meta.env.VITE_BACKEND_URL;
let connection: HubConnection;
let userId: string;

export async function startConnection() {

    if(await isLoggedIn()){
        userId = await fetchUserId();
        connection = new HubConnectionBuilder()
        .withUrl(`${backendUrl}/accountHub`)
        .build();

        connection.on("UpdateUserStatus", async (userId, status) => {
            console.log("userid" + userId + "status" + status)
            
            userStatuses.update(statuses => ({ ...statuses, [userId]: status }));
        });

        await connection.start();

        userId = userId.toString();


        return connection;
        

    }
    return connection;
}