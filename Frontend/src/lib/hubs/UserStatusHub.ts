import { fetchUserId, isLoggedIn, type User } from "$lib/Handlers/accountHandler";
import { getToken } from "$lib/Handlers/authHandler";
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { userStatuses } from "$lib/stores/userStatusesStore";
import { receivedRequestsStore, sentRequestsStore } from "$lib/stores/friendRequestsStore";
import type { FriendPreview, FriendRequest } from "$lib/Handlers/userHandler";
import { json } from "@sveltejs/kit";
import { friendsStore } from "$lib/stores/friendsStore";
import { userStore } from "$lib/stores/userInfoStore";
import { goto } from "$app/navigation";
import { get } from "svelte/store";


const backendUrl = import.meta.env.VITE_BACKEND_URL;
let connection: HubConnection;
let userId: string;
let user = get(userStore);

export async function startConnection() {

    if(await isLoggedIn()){
        userId = await fetchUserId();
        let token = await getToken();
        connection = new HubConnectionBuilder()
        .withUrl(`${backendUrl}/accountHub?userId=${userId}`, { 
        accessTokenFactory: () => token
        })
        .build();
        

        connection.on("UpdateUserStatus", async (userId, status) => {
            console.log("User status updated: " + userId + " " + status);
            userStatuses.update(statuses => ({ ...statuses, [userId]: status }));
        });

        connection.on("NewFriendRequest", async function(data: any) {
            // Update friendRequests array with new friend request
            console.log("New friend request: " + data);
            const friendRequestAdd : FriendRequest ={
                username: data.username,
                requestSentDate: data.requestSentDate,
                id: data.id,
                userPfp: `${backendUrl}${data.userPfp}`
            }
            receivedRequestsStore.update(requests => [...requests, friendRequestAdd]);
        });

        connection.on("FriendRequestAccepted", function(data: any) {
            

            let userId;

            userStore.subscribe((user: User) => {
                if (user) {
                    userId = user.id;
                }
            });

            if(userId === data.user.id){
                receivedRequestsStore.update(requests => requests.filter(request => request.username === data.username));
                let friendToAdd: FriendPreview = {
                    id: data.friend.id,
                    userPfp: `${backendUrl}${data.friend.profilePictureUrl}?${Date.now()}`,
                    username: data.friend.userName,
                
                    chatId: data.chatId
                }
                friendsStore.update(friends => [...friends, friendToAdd]);
            }
            else{
                sentRequestsStore.update(requests => requests.filter(request => request.username === data.username));
                let friendToAdd: FriendPreview = {
                    id: data.user.id,
                    userPfp: `${backendUrl}${data.user.profilePictureUrl}?${Date.now()}`,
                    username: data.user.userName,
                   
                    chatId: data.chatId
                }
                friendsStore.update(friends => [...friends, friendToAdd]);
            }
        });

        connection.on("FriendRequestRejected", function(data: any) {

                receivedRequestsStore.update(requests => requests.filter(request => request.username === data.friend.username));

                sentRequestsStore.update(requests => requests.filter(request => request.username === data.user.username));

        });

        connection.on("FriendRemoved", function(data: any) {          
            console.log("Friend removed: " + data.friendId);
            friendsStore.update(friends => friends.filter(friend => friend.id !== data.friend.id));
            receivedRequestsStore.update(requests => requests.filter(request => request.id !== data.friend.id));
            sentRequestsStore.update(requests => requests.filter(request => request.id !== data.friend.id));
            friendsStore.update(friends => friends.filter(friend => friend.id !== data.user.id));
            receivedRequestsStore.update(requests => requests.filter(request => request.id !== data.user.id));
            sentRequestsStore.update(requests => requests.filter(request => request.id !== data.user.id));
            
            

            
        });

        connection.on("UserBlocked", function(data: any) {

            receivedRequestsStore.update(requests => requests.filter(request => request.id !== data.friend.id));
            sentRequestsStore.update(requests => requests.filter(request => request.id !== data.friend.id));

            if (userId.toString() === data.friend.id.toString()) {
                friendsStore.update(friends => friends.filter(friend => friend.id.toString() !== data.user.id.toString()));
                goto('/chat/home');
            } else if (userId.toString() === data.user.id.toString()) {
                friendsStore.update(friends => friends.filter(friend => friend.id !== data.friend.id));
            }
        });


        userId = userId.toString();
        await connection.start();
        


        return connection;
        

    }
    return connection;
}