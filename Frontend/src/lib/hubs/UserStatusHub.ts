import { fetchUserId, isLoggedIn, type User } from "$lib/Handlers/accountHandler";
import { getToken } from "$lib/Handlers/authHandler";
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { userStatuses } from "$lib/stores/userStatusesStore";
import { friendRequestsStore } from "$lib/stores/friendRequestsStore";
import type { FriendPreview, FriendRequest } from "$lib/Handlers/userHandler";
import { json } from "@sveltejs/kit";
import { friendsStore } from "$lib/stores/friendsStore";
import { userStore } from "$lib/stores/userInfoStore";
import { goto } from "$app/navigation";


const backendUrl = import.meta.env.VITE_BACKEND_URL;
let connection: HubConnection;
let userId: string;

export async function startConnection() {

    if(await isLoggedIn()){
        userId = await fetchUserId();
        let token = await getToken();
        console.log("Token: " + token); 
        connection = new HubConnectionBuilder()
        .withUrl(`${backendUrl}/accountHub?userId=${userId}`, { 
        accessTokenFactory: () => token
        })
        .build();
        

        connection.on("UpdateUserStatus", async (userId, status) => {
            
            userStatuses.update(statuses => ({ ...statuses, [userId]: status }));
        });

        connection.on("NewFriendRequest", async function(data: any) {
            // Update friendRequests array with new friend request
            console.log("New friend request: " + data);
            const friendRequestAdd : FriendRequest ={
                username: data.username,
                requestSentDate: data.requestSentDate
            }
            friendRequestsStore.update(requests => [...requests, friendRequestAdd]);
        });

        connection.on("FriendRequestAccepted", function(data: any) {
            friendRequestsStore.update(requests => requests.filter(request => request.username === data.username));

            let userId;

            userStore.subscribe((user: User) => {
                if (user) {
                    userId = user.id;
                }
            });

            if(userId === data.user.id){
                console.log("Friend added: " + data.friend.userPfp);
                let friendToAdd: FriendPreview = {
                    id: data.friend.id,
                    userPfp: `${backendUrl}${data.friend.profilePictureUrl}?${Date.now()}`,
                    username: data.friend.userName,
                
                    chatId: data.chatId
                }
                friendsStore.update(friends => [...friends, friendToAdd]);
            }
            else{
                let friendToAdd: FriendPreview = {
                    id: data.user.id,
                    userPfp: `${backendUrl}${data.user.profilePictureUrl}?${Date.now()}`,
                    username: data.user.userName,
                   
                    chatId: data.chatId
                }
                friendsStore.update(friends => [...friends, friendToAdd]);
            }

            friendRequestsStore.subscribe(value => {
                console.log("FriendRequestStore", value);
            });
        });

        connection.on("FriendRequestRejected", function(data: any) {
            friendRequestsStore.update(requests => requests.filter(request => request.username === data.username));
        });

        connection.on("FriendRemoved", function(data: any) {          
            console.log("Friend removed: " + data.friendId);
            friendsStore.update(friends => friends.filter(friend => friend.id !== data.friend.id));
            friendsStore.update(friends => friends.filter(friend => friend.id !== data.user.id));
            
            

            
        });


        userId = userId.toString();
        await connection.start();
        


        return connection;
        

    }
    return connection;
}