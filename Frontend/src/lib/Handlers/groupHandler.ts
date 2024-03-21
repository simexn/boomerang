import { getToken } from './authHandler';
import type { User } from './accountHandler';
const backendUrl = import.meta.env.VITE_BACKEND_URL;

export interface Group {
    id: number;
    name: string;
    isGroup: boolean
    creatorId: number;
    inviteCode: string;
    users: User[];
    admins: User[];
}

function generateInviteCode(length: number) {
    const characters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    let result = '';
    for (let i = 0; i < length; i++) {
        result += characters.charAt(Math.floor(Math.random() * characters.length));
    }
    return result;
}

export async function fetchGroupInfo(groupId: string){
    let token = await getToken();
    
    const response = await fetch(`${backendUrl}/group/getGroupInfo?chatId=${groupId}`, {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        credentials: 'include'
    });
    
    const data = await response.json();
    
    if (response.ok && data.chat) {
        const groupInfo: Group = {
            id: data.chat.id,
            name: data.chat.name,
            isGroup: data.chat.isGroup,
            creatorId: data.chat.creatorId,
            inviteCode: data.chat.inviteCode,
            users: data.users,
            admins: data.admins
        }
        return groupInfo;
    } else {
        console.error('Error fetching chat:', await response.text());
        const groupInfo: Group = {
            id: 0,
            name: '',
            isGroup: false,
            creatorId: 0,
            inviteCode: '',
            users: [],
            admins: []
        };
        return groupInfo;
    }
}

async function fetchGroupUsers(groupId: string){
    let token = await getToken();
    
    const response = await fetch(`${backendUrl}/group/getGroupUsers?chatId=${groupId}`, {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        credentials: 'include'
    });
    
    const data = await response.json();
    
    if (response.ok && data) {
        let users: User[] = data.users;
        return users;
    } else {
        console.error('Error fetching chat:', await response.text());
        return [];
    }
}

export async function fetchChats() {
    try {
        let token = await getToken();
        
        const response = await fetch(`${backendUrl}/chat/getAllChats` , {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            credentials: 'include'
        
        });
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const data = await response.json();
        
        return data.chats;
    } catch (error) {
        console.error('Error fetching chats:', error);
    }
};

export async function handleRoomSubmit(formData: {newGroupName: string, inviteCode: string}) {
    let token = await getToken();
    
    if (formData.inviteCode === '' || formData.inviteCode === null || formData.inviteCode === undefined) {
        formData.inviteCode = generateInviteCode(6);
    }

    console.log(formData)
    const requestBody = JSON.stringify(formData);

    const response = await fetch(`${backendUrl}/chat/createRoom`, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        credentials: 'include',
        body: requestBody 
    });

    if (response.ok){
        var chats = await fetchChats();
        return chats;
    }
    if (!response.ok) {
        return response.text().then(text => { throw new Error(text) });
    }

    const data = await response.text();
}

export async function handleJoinRoom(inviteCode: string) {
    let token = await getToken();
    

    const response = await fetch(`${backendUrl}/group/joinGroup`, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        credentials: 'include',
        body: JSON.stringify(inviteCode)
    });

    if (response.ok){
        var chats = await fetchChats();
        return chats;
    }
    if (!response.ok) {
        console.log("losh otgovor");
        return response.text().then(text => { throw new Error(text) });
    }

    const data = await response.text();
}

export async function handleLeaveGroup(chatId: string) {
    const token = await getToken();

    const response = await fetch(`${backendUrl}/group/leaveGroup/${chatId}`, {
        method: 'DELETE',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
    });

    if (!response.ok) {
        const message = `An error has occurred: ${response.status}`;
        throw new Error(message);
    }

    return response;
}

export async function handleDeleteGroup(chatId: string) {
    const token = await getToken();

    const response = await fetch(`${backendUrl}/group/deleteGroup/${chatId}`, {
        method: 'DELETE',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
    });

    if (!response.ok) {
        const message = `An error has occurred: ${response.status}`;
        throw new Error(message);
    }

    return response;
}

export async function handleKickUser(chatId: string, userId: number) {
    const token = await getToken();

    const response = await fetch(`${backendUrl}/group/kickUser/${chatId}/${userId}`, {
        method: 'DELETE',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
    });

    if (!response.ok) {
        const message = `An error has occurred: ${response.status}`;
        throw new Error(message);
    }

    return response;
}

export async function handlePromoteUser(chatId: string, userId: number){
    const token = await getToken();
    console.log(backendUrl)
    const response = await fetch(`${backendUrl}/group/promoteUser/${chatId}/${userId}`, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
    });

    if (!response.ok) {
        
        const message = `An error has occurred: ${response.json()}`;
        throw new Error(message);
    }
    return response;
}

export async function handleDemoteUser(chatId: string, userId: number){
    const token = await getToken();
    const response = await fetch(`${backendUrl}/group/demoteUser/${chatId}/${userId}`, {
        method: 'DELETE',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
    });

    if (!response.ok) {
        
        const message = `An error has occurred: ${response.json()}`;
        throw new Error(message);
    }
    return response;
}

export async function handleTransferOwnership(chatId: string, userId: number){
    const token = await getToken();
    const response = await fetch(`${backendUrl}/group/transferOwnership/${chatId}/${userId}`, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
    });

    if (!response.ok) {
        
        const message = `An error has occurred: ${response.json()}`;
        throw new Error(message);
    }
    return response;
}


export async function handleGroupUpdate(formData: {chatId: string, newGroupName: string, inviteCode: string}) {
    let token = await getToken();
    
    const requestBody = JSON.stringify(formData);

    const response = await fetch(`${backendUrl}/group/updateGroup`, {
        method: 'PUT',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        credentials: 'include',
        body: requestBody 
    });

    if (response.ok){
        var chats = await fetchChats();
        return chats;
    }
    if (!response.ok) {
        return response.text().then(text => { throw new Error(text) });
    }

    const data = await response.text();
}



