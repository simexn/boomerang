import { blockedUsersStore, receivedRequestsStore, sentRequestsStore } from "$lib/stores/friendRequestsStore";
import { friendsStore } from "$lib/stores/friendsStore";
import { getToken } from "./authHandler";
import type { Group, GroupPreview } from "./groupHandler";


const backendUrl = import.meta.env.VITE_BACKEND_URL;

export interface FriendRequest {
    id: string;
    username: string;
    userPfp: string;
    requestSentDate: string;
    requestRespondedDate?:string;
}

export interface FriendInfo{
    id: string;
    username: string;
    lastMessagePreview: string;
    chatId:string;
    userPfp:string;
    memberSince?: string;
    friendsSince?: string;
    mutualFriends?: FriendPreview[];
    mutualGroups?: GroupPreview[];
}
export interface FriendPreview{
    id: string;
    username: string;
    userPfp: string;
    chatId:string;

}

export async function handleAddFriend(username: string){
    let token = await getToken();

    const response = await fetch(`${backendUrl}/user/addFriend/${username}`, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        credentials: 'include'
    });

    if(response.ok){
        const data = await response.json();
        data.userPfp = `${backendUrl}${data.userPfp}`;
        sentRequestsStore.update(requests => [...requests, data]);
    }
    else{
        return response.text();
    }

}

export async function fetchFriendRequests() {
    let token = await getToken();

    const response = await fetch(`${backendUrl}/user/getFriendships`, {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        credentials: 'include'
    });
    const data = await response.json();

    if(response.ok){
        console.log(data);

        const receivedFriendRequests: FriendRequest[] = data.receivedRequests.map((friendRequest: any) => {
            return {
                id: friendRequest.userId,
                username: friendRequest.username,
                userPfp: `${backendUrl}${friendRequest.profilePictureUrl}`,
                requestSentDate: friendRequest.requestSentDate
            }
        });

        const sentFriendRequests: FriendRequest[] = data.sentRequests.map((friendRequest: any) => {
            return {
                id: friendRequest.userId,
                username: friendRequest.username,
                userPfp: `${backendUrl}${friendRequest.profilePictureUrl}`,
                requestSentDate: friendRequest.requestSentDate
            }
        });

        const blockedUsers: FriendRequest[] = data.blockedUsers.map((friendRequest: any) => {
            return {
                id: friendRequest.userId,
                username: friendRequest.username,
                userPfp: `${backendUrl}${friendRequest.profilePictureUrl}`,
                requestSentDate: friendRequest.requestSentDate,
                requestRespondedDate: friendRequest.blockedOn
            }
        });

        receivedRequestsStore.set(receivedFriendRequests);
        sentRequestsStore.set(sentFriendRequests);
        blockedUsersStore.set(blockedUsers);
        console.log("BlockedUsers" + blockedUsers)

        return receivedFriendRequests;
    }

    return [];
}

export async function fetchFriends(){
    let token = await getToken();

    const response = await fetch(`${backendUrl}/user/getFriends`, {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        credentials: 'include'
    });
    const data = await response.json();

    if(response.ok){
        const friends: FriendPreview[] = await data.friends.map((data: any) => {
            console.log("friends fetched:" + data.profilePictureUrl);
            return {
                id: data.id,
                username: data.username,
                userPfp: `${backendUrl}${data.profilePictureUrl}?${Date.now()}`,
                chatId: data.chatId
                
            }
            
        });

        

        friendsStore.set(friends);

        return friends;
    }
    return [];
}

export async function fetchFriendInfo(chatId:string){
    let token = await getToken();

    const response = await fetch(`${backendUrl}/user/getFriendInfo/${chatId}`, {
        method: `GET`,
        headers:{
            'Authorization' : `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        credentials: 'include'
    });
    const data = await response.json();

    if(response.ok){
        const friend: FriendInfo =  
        {    
            id: data.friend.id,
            username: data.friend.username,
            lastMessagePreview: "",
            chatId: data.friend.chatId,  
            userPfp: `${backendUrl}${data.friend.userPfp}`,
            memberSince: data.friend.memberSince,
            friendsSince: data.friend.friendsSince,   
            mutualFriends: data.friend.mutualFriends.map((friend: FriendPreview) => ({
                ...friend,
                userPfp: `${backendUrl}${friend.userPfp}`
            })),
            mutualGroups: data.friend.mutualGroups       
        };
        return friend
    }
    let friend: FriendInfo = { id: '', username: "", lastMessagePreview: '', chatId: "", userPfp:"",  mutualFriends: [], mutualGroups: []}
    return friend
}

export async function handleAcceptRequest(username: string){
    let token = await getToken();

    const response = await fetch(`${backendUrl}/user/acceptFriendRequest/${username}`, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        credentials: 'include'
    });

}
export async function handleRejectRequest(username: string){
    let token = await getToken();

    const response = await fetch(`${backendUrl}/user/rejectFriendRequest/${username}`, {
        method: 'DELETE',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        credentials: 'include'
    });

}

export async function handleRemoveFriend(friendId: string){
    let token = await getToken();

    const response = await fetch(`${backendUrl}/user/removeFriend/${friendId}`, {
        method: 'DELETE',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        credentials: 'include'
    });
}
export async function handleBlockUser(friendId: string){
    let token = await getToken();

    const response = await fetch(`${backendUrl}/user/blockUser/${friendId}`, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        credentials: 'include'
    });

    if(response.ok){
        const data = await response.json();
        data.userPfp = `${backendUrl}${data.userPfp}`;
        blockedUsersStore.update(users => [...users, data]);
        
    }
    else{
        return response.text();
    }
}

export async function handleUnblockUser(userId: string){
    let token = await getToken();

    const response = await fetch(`${backendUrl}/user/unblockUser/${userId}`, {
        method: 'DELETE',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        credentials: 'include'
    });

    if(response.ok){
        blockedUsersStore.update(users => users.filter(user => user.id !== userId));
    }
}

export async function handleSearchUser(username: string) {
    let token = await getToken()

    const response = await fetch(`${backendUrl}/user/searchUser/${username}`, {
        method: 'GET',
        headers:{
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        credentials: 'include'
    });

    return response;
    
}

